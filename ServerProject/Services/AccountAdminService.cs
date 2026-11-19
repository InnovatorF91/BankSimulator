using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;

namespace ServerProject.Services
{
	public class AccountAdminService : ServiceBase, IAccountAdminService
	{
		/// <summary>
		/// 帳戶服務實例，用於處理帳戶相關的數據操作
		/// </summary>
		private readonly IAccountRepository _accountRepository;

		/// <summary>
		/// 客戶服務實例，用於處理客戶相關的操作
		/// </summary>
		private readonly ICustomerRepository _customerRepository;

		/// <summary>
		/// 卡片服務實例，用於處理卡片相關的操作
		/// </summary>
		private readonly ICardRepository _cardRepository;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 建構函數，初始化帳戶邏輯類別的實例
		/// </summary>
		/// <param name="connectionFactory">數據庫連接工廠實例</param>
		/// <param name="accountRepository">帳戶服務實例</param>
		/// <param name="customerRepository">客戶服務實例</param>
		/// <param name="timeProvider">時間提供者實例</param>

		public AccountAdminService(
			IConnectionFactory connectionFactory,
			IAccountRepository accountRepository,
			ICustomerRepository customerRepository,
			ITimeProvider timeProvider,
			ICardRepository cardRepository
		) : base(connectionFactory)
		{
			_accountRepository = accountRepository;
			_customerRepository = customerRepository;
			_timeProvider = timeProvider;
			_cardRepository = cardRepository;
		}

		/// <summary>
		/// 管理员强制关闭指定账户
		/// </summary>
		/// <param name="request">包含账户ID和强制关闭理由的请求对象</param>
		/// <returns>强制关闭账户结果 DTO</returns>
		/// <summary>
		/// 强制关闭指定账户，并在同一事务内停用卡片、写入操作日志。
		/// </summary>
		public async Task<ForceCloseAccountResultDto> ForceCloseAccount(ForceCloseAccountRequest request)
		{
			// 验证请求对象是否为 null，如果是，则返回失败结果
			if (request == null)
			{
				return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The request is null.");
			}

			// 验证账户ID是否有效，如果无效，则返回失败结果
			if (request.AccountId <= 0)
			{
				return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The account ID is invalid.");
			}

			try
			{
				// 获取账户信息，并检查账户是否存在
				var account = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountById(request.AccountId);
					},
					"GetAccountById");

				// 如果账户不存在，则返回失败结果
				if (account == null)
				{
					return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.AccountNotFound,"The account does not exist.");
				}

				// 如果账户已关闭，则返回失败结果
				if (account.Status == AccountStatus.Closed)
				{
					return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.AccountAlreadyClosed,"The account has already been closed.");
				}

				// 如果账户余额不为零，则返回失败结果
				if (account.Balance != 0)
				{
					return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.AccountBalanceNotZero,"The account balance is not zero.");
				}

				// 如果账户状态不是活跃或冻结，则返回失败结果
				if (account.Status != AccountStatus.Active && account.Status != AccountStatus.Frozen)
				{
					return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid,"The account status is invalid for force closing.");
				}

				var now = _timeProvider.Now();

				// 执行强制关闭账户操作，并在同一事务中停用卡片、写入操作日志
				return await ExecuteInTxAsync<ForceCloseAccountResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						_cardRepository.DataAccess = dataAccess;

						// 更新账户状态为关闭，并检查当前状态是否为活跃或冻结
						var updateCount =
							await _accountRepository.UpdateAccountStatusIfCurrentStatus(
								account.AccountId,
								account.CustomerId,
								account.Status,
								AccountStatus.Closed,
								now,
								requireZeroBalance: true);

						// 如果更新操作未成功，抛出异常
						if (updateCount <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.AccountStatusInvalid,
								"Failed to force close the account. The account status or balance may have changed.");
						}

						// 没有需要停用的卡片时，影响零行也属于正常情况。
						await _cardRepository.DeactivateAllCards(
							account.AccountId,
							CardStatus.Inactive,
							now,
							now);

						// 在同一事务中插入账户操作日志
						var operationLogId = await _accountRepository.InsertAccountOperationLog(
							new AccountOperationLogEntity
							{
								AccountId = account.AccountId,
								CustomerId = account.CustomerId,
								OperationType = AccountOperationType.ForceClose,
								OldStatus = account.Status,
								NewStatus = AccountStatus.Closed,
								Reason = request.Reason,
								OperatedBy = null,
								OperatedAt = now
							});

						// 如果插入操作日志失败，抛出异常
						if (operationLogId <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.UnableToOperateDb,
								"Failed to insert the account operation log.");
						}

						// 返回成功的强制关闭账户结果 DTO
						return ForceCloseAccountResultDto.SuccessDto(
							account.AccountId,
							AccountStatus.Closed,
							now,
							now);
					},
					"ForceCloseAccount");
			}
			catch (AccountOperationException ex)
			{
				// 捕获账户操作异常，返回相应的失败结果 DTO
				return ForceCloseAccountResultDto.Failure((int)ex.ErrorCode,ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库操作异常，返回数据库操作失败的结果 DTO
				return ForceCloseAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb,"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 凍結指定帳戶，僅限管理員使用
		/// </summary>
		/// <param name="request">包含账户ID和冻结原因的请求物件<param/>
		/// <returns>包含冻结账户结果的DTO<returns/>
		/// <summary>
		/// 冻结指定账户，并在同一事务中记录操作日志。
		/// </summary>
		public async Task<FreezeAccountResultDto> FreezeAccount(FreezeAccountRequest request)
		{
			// 验证请求对象是否为 null，如果是，则返回失败结果
			if (request == null)
			{
				return FreezeAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The request is null.");
			}

			// 验证账户ID是否有效，如果无效，则返回失败结果
			if (request.AccountId <= 0)
			{
				return FreezeAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The account ID is invalid.");
			}

			try
			{
				// 获取账户信息，并检查账户是否存在
				var account = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountById(request.AccountId);
					},
					"GetAccountById");

				// 检查账户是否存在，如果不存在，则返回失败结果
				if (account == null)
				{
					return FreezeAccountResultDto.Failure((int)AccountErrorCode.AccountNotFound,"The account does not exist.");
				}

				// 检查账户状态是否已关闭，如果已关闭，则返回失败结果
				if (account.Status == AccountStatus.Closed)
				{
					return FreezeAccountResultDto.Failure((int)AccountErrorCode.AccountAlreadyClosed,"The account has already been closed.");
				}

				// 检查账户状态是否已冻结，如果已冻结，则返回失败结果
				if (account.Status == AccountStatus.Frozen)
				{
					return FreezeAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid,"The account is already frozen.");
				}

				// 检查账户状态是否为活跃状态，如果不是，则返回失败结果
				if (account.Status != AccountStatus.Active)
				{
					return FreezeAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid,"The account status is invalid for freezing.");
				}

				var now = _timeProvider.Now();

				// 执行冻结账户操作，并在同一事务中记录操作日志
				return await ExecuteInTxAsync<FreezeAccountResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;

						// 更新账户状态为冻结，并检查当前状态是否为活跃
						var updateCount = await _accountRepository.UpdateAccountStatusIfCurrentStatus(
							account.AccountId,
							account.CustomerId,
							AccountStatus.Active,
							AccountStatus.Frozen,
							now,
							requireZeroBalance: false);

						// 如果更新操作未成功，抛出异常
						if (updateCount <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.AccountStatusInvalid,
								"Failed to freeze the account. The account status may have changed.");
						}

						// 在同一事务中插入账户操作日志
						var operationLogId = await _accountRepository.InsertAccountOperationLog(
							new AccountOperationLogEntity
							{
								AccountId = account.AccountId,
								CustomerId = account.CustomerId,
								OperationType = AccountOperationType.Freeze,
								OldStatus = AccountStatus.Active,
								NewStatus = AccountStatus.Frozen,
								Reason = request.Reason,
								OperatedBy = null,
								OperatedAt = now
							});

						// 如果插入操作日志失败，抛出异常
						if (operationLogId <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.UnableToOperateDb,
								"Failed to insert the account operation log.");
						}

						// 返回成功的冻结账户结果 DTO
						return FreezeAccountResultDto.SuccessDto(
							account.AccountId,
							AccountStatus.Frozen,
							now);
					},
					"FreezeAccount");
			}
			catch (AccountOperationException ex)
			{
				// 捕获账户操作异常，返回相应的失败结果 DTO
				return FreezeAccountResultDto.Failure((int)ex.ErrorCode,ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库操作异常，返回数据库操作失败的结果 DTO
				return FreezeAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb,"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 取得指定帳戶的詳細資訊，僅限管理員使用。
		/// </summary>
		/// <param name="request">包含帳戶ID的請求物件</param>
		/// <returns>包含帳戶詳細資訊的結果 DTO</returns>
		public async Task<GetAccountForAdminResultDto> GetAccount(GetAccountForAdminRequest request)
		{
			// 驗證請求物件是否為 null, 如果是，返回失敗結果
			if (request == null)
			{
				return GetAccountForAdminResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The request is null.");
			}

			// 驗證帳戶ID是否有效, 如果無效，返回失敗結果
			if (request.AccountId <= 0)
			{
				return GetAccountForAdminResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The account ID is invalid.");
			}

			try
			{
				// 取得帳戶資料，並檢查帳戶是否存在
				var account = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountById(request.AccountId);
					},
					"GetAccountById");

				// 檢查帳戶是否存在，如果不存在，返回失敗結果
				if (account == null)
				{
					return GetAccountForAdminResultDto.Failure(
						(int)AccountErrorCode.AccountNotFound,
						"The account does not exist.");
				}

				// 檢查帳戶的貨幣代碼是否有效，如果無效，返回失敗結果
				if (!Enum.TryParse<CurrencyCode>(account.Currency, out var currency))
				{
					return GetAccountForAdminResultDto.Failure(
						(int)AccountErrorCode.InvalidCurrency,
						"The account currency is invalid.");
				}

				// 返回成功的結果 DTO，包含帳戶詳細資訊
				return GetAccountForAdminResultDto.SuccessDto(
					account.AccountId,
					account.CustomerId,
					account.AccountType,
					account.Balance,
					currency,
					account.Status,
					account.OpenDate,
					account.CloseDate,
					account.UpdateDate);
			}
			catch (UnableToOperateDBException)
			{
				// 返回失敗的結果 DTO，表示資料庫操作失敗
				return GetAccountForAdminResultDto.Failure(
					(int)AccountErrorCode.UnableToOperateDb,
					"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 取得指定的操作日志
		/// </summary>
		/// <param name="request">取得指定操作日志的请求</param>
		/// <returns>取得指定操作日志DTO</returns>
		public async Task<GetAccountOperationLogResultDto> GetAccountOperationLog(GetAccountOperationLogRequest request)
		{
			// 检查request是否为空，如果为空，则返回失败的Dto
			if (request == null)
			{
				return GetAccountOperationLogResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The request is null.");
			}

			// 检查操作日志ID是否有效，如果无效，则返回失败Dto
			if (request.OperationLogId <= 0)
			{
				return GetAccountOperationLogResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The operation log ID is invalid.");
			}

			try
			{
				// 执行根据操作日志ID查找该条日志的操作
				var log = await ExecuteDbAsync<AccountOperationLogEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;

						return await _accountRepository.GetAccountOperationLogById(
							request.OperationLogId);
					},
					"GetAccountOperationLogById");

				// 如果取得的日志为空，则返回失败Dto
				if (log == null)
				{
					return GetAccountOperationLogResultDto.Failure(
						(int)AccountErrorCode.AccountOperationLogNotFound,
						"The account operation log does not exist.");
				}

				// 将操作日志实体里的信息转换到操作日志Dto里
				var logDto = new AccountOperationLogDto()
				{
					OperationLogId = log.OperationLogId,
					AccountId = log.AccountId,
					CustomerId = log.CustomerId,
					OperationType = log.OperationType,
					OldStatus = log.OldStatus,
					NewStatus = log.NewStatus,
					Reason = log.Reason,
					OperatedBy = log.OperatedBy,
					OperatedAt = log.OperatedAt
				};

				// 返回成功的Dto
				return GetAccountOperationLogResultDto.SuccessDto(
					logDto);
			}
			catch (UnableToOperateDBException)
			{
				// 返回失敗的結果 DTO，表示資料庫操作失敗
				return GetAccountOperationLogResultDto.Failure(
					(int)AccountErrorCode.UnableToOperateDb,
					"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 取得指定账户下所有操作日志
		/// </summary>
		/// <param name="request">取得指定账户下所有操作日志的请求</param>
		/// <returns>取得所有账户操作日志的DTO</returns>
		public async Task<GetAccountOperationLogsResultDto> GetAccountOperationLogs(GetAccountOperationLogsRequest request)
		{
			// 检查request是否为空，如果为空，则返回失败的Dto
			if (request == null)
			{
				return GetAccountOperationLogsResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The request is null.");
			}

			// 检查账户ID是否有效，如果无效，则返回失败Dto
			if (request.AccountId <= 0)
			{
				return GetAccountOperationLogsResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The account ID is invalid.");
			}

			try
			{
				// 执行取得账户信息和账户相关的所有操作日志的操作
				var result = await ExecuteDbAsync<(AccountEntity? Account, List<AccountOperationLogEntity> Logs)>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;

						var account = await _accountRepository.GetAccountById(request.AccountId);

						if (account == null)
						{
							return (null, new List<AccountOperationLogEntity>());
						}

						var logs = await _accountRepository.GetAccountOperationLogsByAccountId(
							request.AccountId);

						return (account, logs);
					},
					"GetAccountOperationLogs");

				// 检查账户是否存在，如果不存在，则返回失败Dto
				if (result.Account == null)
				{
					return GetAccountOperationLogsResultDto.Failure(
						(int)AccountErrorCode.AccountNotFound,
						"The account does not exist.");
				}

				var logDtos = new List<AccountOperationLogDto>();

				// 将操作日志实体里的信息转换到操作日志Dto里
				foreach (var log in result.Logs)
				{
					logDtos.Add(new AccountOperationLogDto()
					{
						OperationLogId = log.OperationLogId,
						AccountId = log.AccountId,
						CustomerId = log.CustomerId,
						OperationType = log.OperationType,
						OldStatus = log.OldStatus,
						NewStatus = log.NewStatus,
						Reason = log.Reason,
						OperatedBy = log.OperatedBy,
						OperatedAt = log.OperatedAt
					});
				}

				// 返回成功的Dto
				return GetAccountOperationLogsResultDto.SuccessDto(
					request.AccountId,
					logDtos);
			}
			catch (UnableToOperateDBException)
			{
				// 返回失敗的結果 DTO，表示資料庫操作失敗
				return GetAccountOperationLogsResultDto.Failure(
					(int)AccountErrorCode.UnableToOperateDb,
					"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 取得指定客戶的帳戶列表。
		/// </summary>
		/// <param name="request">包含客戶ID和是否包含已關閉帳戶的請求物件</param>
		/// <returns>包含客戶帳戶列表的結果 DTO</returns>
		public async Task<GetCustomerAccountsResultDto> GetCustomerAccounts(GetCustomerAccountsRequest request)
		{
			// 驗證請求物件是否為 null, 如果是，返回失敗結果
			if (request == null)
			{
				return GetCustomerAccountsResultDto.Failure(
					(int)AccountErrorCode.ValidationFailed,
					"The request is null.");
			}

			// 驗證客戶ID是否有效, 如果無效，返回失敗結果
			if (request.CustomerId <= 0)
			{
				return GetCustomerAccountsResultDto.Failure(
					(int)CustomerErrorCode.InvalidCustomerData,
					"The customer ID is invalid.");
			}

			try
			{
				// 取得客戶資料，並檢查客戶是否存在或已刪除
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerById");

				// 檢查客戶是否存在，如果不存在，返回失敗結果
				if (customer == null)
				{
					return GetCustomerAccountsResultDto.Failure(
						(int)CustomerErrorCode.CustomerNotFound,
						"The customer does not exist.");
				}

				// 檢查客戶是否已刪除，如果已刪除，返回失敗結果
				if (customer.IsDeleted)
				{
					return GetCustomerAccountsResultDto.Failure(
						(int)CustomerErrorCode.InvalidCustomerData,
						"The customer has been deleted.");
				}

				// 根據請求參數，取得客戶的帳戶列表，包含或不包含已關閉的帳戶
				var accounts = await ExecuteDbAsync<List<AccountEntity>>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;

						return request.IncludeClosedAccounts
							? await _accountRepository.GetAllAccountsByCustomerId(request.CustomerId)
							: await _accountRepository.GetNonClosedAccountsByCustomerId(request.CustomerId);
					},
					"GetCustomerAccounts");

				var accountDtos = new List<AccountDto>();

				// 將帳戶實體轉換為 DTO，並檢查貨幣代碼是否有效
				foreach (var account in accounts)
				{
					if (!Enum.TryParse<CurrencyCode>(account.Currency, out var currency))
					{
						return GetCustomerAccountsResultDto.Failure(
							(int)AccountErrorCode.InvalidCurrency,
							"One or more accounts have invalid currency codes.");
					}

					accountDtos.Add(new AccountDto
					{
						AccountId = account.AccountId,
						AccountType = account.AccountType,
						Balance = account.Balance,
						Currency = currency,
						Status = account.Status,
						OpenDate = account.OpenDate,
						CloseDate = account.CloseDate,
						UpdateDate = account.UpdateDate
					});
				}

				// 返回成功的結果 DTO，包含客戶ID和帳戶列表
				return GetCustomerAccountsResultDto.SuccessDto(
					request.CustomerId,
					accountDtos);
			}
			catch (UnableToOperateDBException)
			{
				// 返回失敗的結果 DTO，表示資料庫操作失敗
				return GetCustomerAccountsResultDto.Failure(
					(int)AccountErrorCode.UnableToOperateDb,
					"A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 解冻指定账户，僅限管理員使用
		/// </summary>
		/// <param name="request">包含账户ID和解冻原因的请求物件</param>
		/// <returns>包含解冻账户结果的DTO</returns>
		/// <summary>
		/// 解冻指定账户，并在同一事务中记录操作日志。
		/// </summary>
		public async Task<UnfreezeAccountResultDto> UnfreezeAccount(UnfreezeAccountRequest request)
		{
			// 验证请求对象是否为 null，如果是，则返回失败结果
			if (request == null)
			{
				return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The request is null.");
			}

			// 验证账户ID是否有效，如果无效，则返回失败结果
			if (request.AccountId <= 0)
			{
				return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed,"The account ID is invalid.");
			}

			try
			{
				// 取得账户信息，并检查账户是否存在
				var account = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountById(request.AccountId);
					},
					"GetAccountById");

				// 如果账户不存在，则返回失败结果
				if (account == null)
				{
					return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.AccountNotFound,"The account does not exist.");
				}

				// 如果账户已关闭，则返回失败结果
				if (account.Status == AccountStatus.Closed)
				{
					return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.AccountAlreadyClosed,"The account has already been closed.");
				}

				// 如果账户已激活，则返回失败结果
				if (account.Status == AccountStatus.Active)
				{
					return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid,"The account is already active.");
				}

				// 如果账户状态不是冻结状态，则返回失败结果
				if (account.Status != AccountStatus.Frozen)
				{
					return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid,"The account status is invalid for unfreezing.");
				}

				var now = _timeProvider.Now();

				// 执行解冻账户操作，并在同一事务中记录操作日志
				return await ExecuteInTxAsync<UnfreezeAccountResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;

						// 更新账户状态为激活，并检查当前状态是否为冻结
						var updateCount = await _accountRepository.UpdateAccountStatusIfCurrentStatus(
							account.AccountId,
							account.CustomerId,
							AccountStatus.Frozen,
							AccountStatus.Active,
							now,
							requireZeroBalance: false);

						// 如果更新操作未成功，抛出异常
						if (updateCount <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.AccountStatusInvalid,
								"Failed to unfreeze the account. The account status may have changed.");
						}

						// 在同一事务中插入账户操作日志
						var operationLogId = await _accountRepository.InsertAccountOperationLog(
							new AccountOperationLogEntity
							{
								AccountId = account.AccountId,
								CustomerId = account.CustomerId,
								OperationType = AccountOperationType.Unfreeze,
								OldStatus = AccountStatus.Frozen,
								NewStatus = AccountStatus.Active,
								Reason = request.Reason,
								OperatedBy = null,
								OperatedAt = now
							});

						// 如果插入操作日志失败，抛出异常
						if (operationLogId <= 0)
						{
							throw new AccountOperationException(
								AccountErrorCode.UnableToOperateDb,
								"Failed to insert the account operation log.");
						}

						// 返回成功的解冻账户结果 DTO
						return UnfreezeAccountResultDto.SuccessDto(
							account.AccountId,
							AccountStatus.Active,
							now);
					},
					"UnfreezeAccount");
			}
			catch (AccountOperationException ex)
			{
				// 捕获账户操作异常，返回相应的失败结果 DTO
				return UnfreezeAccountResultDto.Failure((int)ex.ErrorCode,ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库操作异常，返回数据库操作失败的结果 DTO
				return UnfreezeAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb,"A database error occurred during the operation.");
			}
		}
	}

	public interface IAccountAdminService
	{
		/// <summary>
		/// 取得指定客戶的帳戶列表。
		/// </summary>
		/// <param name="request">包含客戶ID和是否包含已關閉帳戶的請求物件</param>
		/// <returns>包含客戶帳戶列表的結果 DTO</returns>
		Task<GetCustomerAccountsResultDto> GetCustomerAccounts(GetCustomerAccountsRequest request);

		/// <summary>
		/// 取得指定帳戶的詳細資訊，僅限管理員使用。
		/// </summary>
		/// <param name="request">包含帳戶ID的請求物件</param>
		/// <returns>包含帳戶詳細資訊的結果 DTO</returns>
		Task<GetAccountForAdminResultDto> GetAccount(GetAccountForAdminRequest request);

		/// <summary>
		/// 凍結指定帳戶，僅限管理員使用
		/// </summary>
		/// <param name="request">包含账户ID和冻结原因的请求物件<param/>
		/// <returns>包含冻结账户结果的DTO<returns/>
		Task<FreezeAccountResultDto> FreezeAccount(FreezeAccountRequest request);

		/// <summary>
		/// 解冻指定账户，僅限管理員使用
		/// </summary>
		/// <param name="request">包含账户ID和解冻原因的请求物件</param>
		/// <returns>包含解冻账户结果的DTO</returns>
		Task<UnfreezeAccountResultDto> UnfreezeAccount(UnfreezeAccountRequest request);

		/// <summary>
		/// 取得指定账户下所有操作日志
		/// </summary>
		/// <param name="request">取得指定账户下所有操作日志的请求</param>
		/// <returns>取得所有账户操作日志的DTO</returns>
		Task<GetAccountOperationLogsResultDto> GetAccountOperationLogs(GetAccountOperationLogsRequest request);

		/// <summary>
		/// 取得指定的操作日志
		/// </summary>
		/// <param name="request">取得指定操作日志的请求</param>
		/// <returns>取得指定操作日志DTO</returns>
		Task<GetAccountOperationLogResultDto> GetAccountOperationLog(GetAccountOperationLogRequest request);

		/// <summary>
		/// 管理员强制关闭指定账户
		/// </summary>
		/// <param name="request">包含账户ID和强制关闭理由的请求对象</param>
		/// <returns>强制关闭账户结果 DTO</returns>
		Task<ForceCloseAccountResultDto> ForceCloseAccount(ForceCloseAccountRequest request);
	}
}
