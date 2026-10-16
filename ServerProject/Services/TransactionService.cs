using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;

namespace ServerProject.Services
{
	/// <summary>
	/// 交易服务实例
	/// </summary>
	public class TransactionService : ServiceBase, ITransactionService
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
		/// 交易服務實例，用於處理交易相關的操作
		/// </summary>
		private readonly ITransactionRepository _transactionRepository;

		/// <summary>
		/// 當前客戶實例，用於獲取當前登入的客戶信息
		/// </summary>
		private readonly ICurrentCustomer _currentCustomer;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 最大允許的交易金額，超過此金額的交易將被拒絕
		/// </summary>
		private const decimal MaxAmount = 9999999999999.99m;

		/// <summary>
		/// 建構函數，初始化帳戶邏輯類別的實例
		/// </summary>
		/// <param name="connectionFactory">數據庫連接工廠實例</param>
		/// <param name="accountRepository">帳戶服務實例</param>
		/// <param name="customerRepository">客戶服務實例</param>
		/// <param name="transactionRepository">交易服務實例</param>
		/// <param name="currentCustomer">当前登录客户信息</param>
		/// <param name="timeProvider">時間提供者實例</param>
		public TransactionService(
			IConnectionFactory connectionFactory,
			IAccountRepository accountRepository,
			ICustomerRepository customerRepository,
			ITransactionRepository transactionRepository,
			ICurrentCustomer currentCustomer,
			ITimeProvider timeProvider
		) : base(connectionFactory)
		{
			_accountRepository = accountRepository;
			_customerRepository = customerRepository;
			_transactionRepository = transactionRepository;
			_currentCustomer = currentCustomer;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// 存款操作
		/// </summary>
		/// <param name="request">存款请求</param>
		/// <returns>存款结果Dto</returns>
		public async Task<DepositResultDto> Deposit(DepositRequest request)
		{
			int customerId;

			//request 判空
			if (request == null)
			{
				return DepositResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "Request cannot be null.");
			}

			//AccountId > 0
			if (request.AccountId <= 0)
			{
				return DepositResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AccountId must be greater than 0.");
			}

			//Amount > 0
			if (request.AmountDelta <= 0)
			{
				return DepositResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AmountDelta must be greater than 0.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return DepositResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			//取得当前登录用户
			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return DepositResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				//开始事务
				var transactionResult = await ExecuteInTxAsync<DepositResultDto>(
					async dataAccess =>
					{
						_transactionRepository.DataAccess = dataAccess;
						_accountRepository.DataAccess = dataAccess;

						//查询账户
						var account = await _accountRepository.GetAccountByIdAndCustomerIdForUpdate(request.AccountId, customerId);

						// 验证该账户是否存在
						if (account == null)
						{
							return DepositResultDto.Failure((int)TransactionErrorCode.SourceAccountNotFound, "The specified account does not exist.");
						}

						//确认账户属于当前用户
						if (account.CustomerId != customerId)
						{
							return DepositResultDto.Failure((int)TransactionErrorCode.SourceAccountAccessDenied, "The specified account does not belong to the current customer.");
						}

						//确认账户状态为 Active
						if (account.Status != AccountStatus.Active)
						{
							return DepositResultDto.Failure((int)TransactionErrorCode.AccountNotAvailable, "The specified account is not available for transactions.");
						}

						// 转换账户货币字符串为枚举类型，如果转换失败，返回错误
						if (!Enum.TryParse<CurrencyCode>(account.Currency, out var currency))
						{
							return DepositResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The account currency is invalid.");
						}

						// AmountDelta 最多允许两位小数
						if (request.AmountDelta != decimal.Round(
								request.AmountDelta, 2))
						{
							return DepositResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta cannot have more than two decimal places.");
						}

						// 检查 AmountDelta 是否超过最大允许金额
						if (request.AmountDelta > MaxAmount)
						{
							return DepositResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta exceeds the maximum allowed amount.");
						}

						//计算交易前后余额
						var newBalance = account.Balance + request.AmountDelta;

						// 检查账户余额是否超过最大允许金额
						if (newBalance > MaxAmount)
						{
							return DepositResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"Destination account balance exceeds the maximum allowed amount.");
						}

						// 获取当前时间
						var now = _timeProvider.Now();

						//更新账户余额
						var updateResult = await _accountRepository.IncreaseBalance(account.AccountId, request.AmountDelta, now);

						//检查更新结果,如果更新失败，抛出异常以触发事务回滚
						if (updateResult != 1)
						{
							throw new TransactionOperationException(TransactionErrorCode.AccountBalanceUpdateFailed, "Failed to update the account balance.");
						}

						//插入 Transactions
						var transactionEntity = new TransactionEntity
						{
							AccountId = account.AccountId,
							TransactionType = TransactionType.Deposit,
							AmountDelta = request.AmountDelta,
							RelatedAccount = null,
							CreateAt = now,
							Status = TransactionStatus.Completed,
							GroupId = null,
							Note = request.Note,
							Currency = account.Currency,
							BalanceBefore = account.Balance,
							BalanceAfter = newBalance
						};

						// 插入交易记录
						var transactionId = await _transactionRepository.InsertTransaction(transactionEntity);

						if (transactionId <= 0)
						{
							throw new TransactionOperationException(TransactionErrorCode.TransactionInsertFailed, "Failed to insert the transaction record.");
						}

						//返回成功
						return DepositResultDto.SuccessDto(
							transactionId,
							account.AccountId,
							request.AmountDelta,
							account.Balance,
							newBalance,
							TransactionStatus.Completed,
							currency,
							now
						);
					}
				);

				return transactionResult;
			}
			catch (TransactionOperationException ex)
			{
				//发生异常则 Rollback
				return DepositResultDto.Failure((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return DepositResultDto.Failure((int)TransactionErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 转账操作
		/// </summary>
		/// <param name="request">转账请求</param>
		/// <returns>转账结果Dto</returns>
		public async Task<TransferResultDto> Transfer(TransferRequest request)
		{
			int customerId;

			//request 判空
			if (request == null)
			{
				return TransferResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "Request cannot be null.");
			}

			//SourceAccountId > 0
			if (request.SourceAccountId <= 0)
			{
				return TransferResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "SourceAccountId must be greater than 0.");
			}

			//DestinationAccountId > 0
			if (request.DestinationAccountId <= 0)
			{
				return TransferResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "DestinationAccountId must be greater than 0.");
			}

			// SourceAccountId != DestinationAccountId
			if (request.SourceAccountId == request.DestinationAccountId)
			{
				return TransferResultDto.Failure((int)TransactionErrorCode.SameAccountTransfer, "SourceAccountId and DestinationAccountId cannot be the same.");
			}

			//Amount > 0
			if (request.AmountDelta <= 0)
			{
				return TransferResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AmountDelta must be greater than 0.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return TransferResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			//取得当前登录用户
			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return TransferResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				// 开始事务
				var transactionResult = await ExecuteInTxAsync<TransferResultDto>(
					async dataAccess =>
					{
						_transactionRepository.DataAccess = dataAccess;
						_accountRepository.DataAccess = dataAccess;

						// 按 AccountId 升序取得行锁
						var firstId = Math.Min(
							request.SourceAccountId,
							request.DestinationAccountId);

						var secondId = Math.Max(
							request.SourceAccountId,
							request.DestinationAccountId);

						var firstAccount = await _accountRepository
							.GetAccountByIdForUpdate(firstId);

						var secondAccount = await _accountRepository
							.GetAccountByIdForUpdate(secondId);

						// 还原源账户和目标账户
						var sourceAccount =
							request.SourceAccountId == firstId
								? firstAccount
								: secondAccount;

						var destinationAccount =
							request.DestinationAccountId == firstId
								? firstAccount
								: secondAccount;

						// 验证源账户是否存在
						if (sourceAccount == null)
						{
							return TransferResultDto.Failure((int)TransactionErrorCode.SourceAccountNotFound, "The specified account does not exist.");
						}

						//确认源账户属于当前用户
						if (sourceAccount.CustomerId != customerId)
						{
							return TransferResultDto.Failure((int)TransactionErrorCode.SourceAccountAccessDenied, "The specified account does not belong to the current customer.");
						}

						//确认源账户状态为 Active
						if (sourceAccount.Status != AccountStatus.Active)
						{
							return TransferResultDto.Failure((int)TransactionErrorCode.AccountNotAvailable, "The specified account is not available for transactions.");
						}

						// 转换源账户货币字符串为枚举类型，如果转换失败，返回错误
						if (!Enum.TryParse<CurrencyCode>(sourceAccount.Currency, out var sourceCurrency))
						{
							return TransferResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The sourceAccount currency is invalid.");
						}

						// 验证目标账户是否存在
						if (destinationAccount == null)
						{
							return TransferResultDto.Failure((int)TransactionErrorCode.TargetAccountNotFound, "The destination account does not exist.");
						}

						// 验证目标账户状态为 Active
						if (destinationAccount.Status != AccountStatus.Active)
						{
							return TransferResultDto.Failure((int)TransactionErrorCode.AccountNotAvailable, "The destination account is not available for transactions.");
						}

						// 转换目标账户货币字符串为枚举类型，如果转换失败，返回错误
						if (!Enum.TryParse<CurrencyCode>(destinationAccount.Currency, out var destinationCurrency))
						{
							return TransferResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The destinationAccount currency is invalid.");
						}

						// 验证源账户和目标账户的货币是否匹配
						if (sourceCurrency != destinationCurrency)
						{
							return TransferResultDto.Failure(
								(int)TransactionErrorCode.CurrencyMismatch,
								"The source and destination account currencies do not match.");
						}

						// AmountDelta 最多允许两位小数
						if (request.AmountDelta != decimal.Round(
								request.AmountDelta, 2))
						{
							return TransferResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta cannot have more than two decimal places.");
						}

						// 检查 AmountDelta 是否超过最大允许金额
						if (request.AmountDelta > MaxAmount)
						{
							return TransferResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta exceeds the maximum allowed amount.");
						}

						// 检查源账户余额是否足够进行转账操作
						if (sourceAccount.Balance < request.AmountDelta)
						{
							return TransferResultDto.Failure(
								(int)TransactionErrorCode.InsufficientBalance,
								"Insufficient balance in the source account.");
						}

						// 计算交易前后余额
						var sourceNewBalance = sourceAccount.Balance - request.AmountDelta;
						var destinationNewBalance = destinationAccount.Balance + request.AmountDelta;

						// 检查目标账户余额是否超过最大允许金额
						if (destinationNewBalance > MaxAmount)
						{
							return TransferResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"Destination account balance exceeds the maximum allowed amount.");
						}

						// 获取当前时间
						var now = _timeProvider.Now();

						// 生成交易组ID，用于关联源账户和目标账户的交易记录
						var groupId = Guid.NewGuid();

						// 更新源账户余额
						var sourceUpdateResult = await _accountRepository.DecreaseBalanceIfEnoughBalance(sourceAccount.AccountId, request.AmountDelta, now);

						//检查更新结果,如果更新失败，抛出异常以触发事务回滚
						if (sourceUpdateResult != 1)
						{
							throw new TransactionOperationException(TransactionErrorCode.AccountBalanceUpdateFailed, "Failed to update the sourceAccount balance.");
						}

						// 更新目标账户余额
						var destinationUpdateResult = await _accountRepository.IncreaseBalance(destinationAccount.AccountId, request.AmountDelta, now);

						//检查更新结果,如果更新失败，抛出异常以触发事务回滚
						if (destinationUpdateResult != 1)
						{
							throw new TransactionOperationException(TransactionErrorCode.AccountBalanceUpdateFailed, "Failed to update the destinationAccount balance.");
						}

						// 插入原账户的交易记录
						var sourceTransactionEntity = new TransactionEntity
						{
							AccountId = sourceAccount.AccountId,
							TransactionType = TransactionType.Transfer,
							AmountDelta = request.AmountDelta,
							RelatedAccount = destinationAccount.AccountId,
							CreateAt = now,
							Status = TransactionStatus.Completed,
							GroupId = groupId,
							Note = request.Note,
							Currency = sourceAccount.Currency,
							BalanceBefore = sourceAccount.Balance,
							BalanceAfter = sourceNewBalance
						};
						var sourceTransactionId = await _transactionRepository.InsertTransaction(sourceTransactionEntity);

						// 检查插入结果，如果插入失败，抛出异常以触发事务回滚
						if (sourceTransactionId <= 0)
						{
							throw new TransactionOperationException(TransactionErrorCode.TransactionInsertFailed, "Failed to insert the source transaction record.");
						}

						// 插入目标账户的交易记录
						var destinationTransactionEntity = new TransactionEntity
						{
							AccountId = destinationAccount.AccountId,
							TransactionType = TransactionType.Transfer,
							AmountDelta = request.AmountDelta,
							RelatedAccount = sourceAccount.AccountId,
							CreateAt = now,
							Status = TransactionStatus.Completed,
							GroupId = groupId,
							Note = request.Note,
							Currency = destinationAccount.Currency,
							BalanceBefore = destinationAccount.Balance,
							BalanceAfter = destinationNewBalance
						};
						var destinationTransactionId = await _transactionRepository.InsertTransaction(destinationTransactionEntity);

						// 检查插入结果，如果插入失败，抛出异常以触发事务回滚
						if (destinationTransactionId <= 0)
						{
							throw new TransactionOperationException(TransactionErrorCode.TransactionInsertFailed, "Failed to insert the destination transaction record.");
						}

						// 返回成功
						return TransferResultDto.SuccessDto(
							groupId,
							sourceTransactionId,
							destinationTransactionId,
							sourceAccount.AccountId,
							destinationAccount.AccountId,
							request.AmountDelta,
							sourceAccount.Balance,
							sourceNewBalance,
							TransactionStatus.Completed,
							sourceCurrency,
							now
						);
					}
				);

				return transactionResult;
			}
			catch (TransactionOperationException ex)
			{
				//发生异常则 Rollback
				return TransferResultDto.Failure((int)ex.ErrorCode, ex.Message);
			}
			catch (DbDeadlockException)
			{
				return TransferResultDto.Failure(
					(int)TransactionErrorCode.UnableToOperateDb,
					"Database deadlock detected. Please retry the transaction.");
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return TransferResultDto.Failure((int)TransactionErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 取款操作
		/// </summary>
		/// <param name="request">取款请求</param>
		/// <returns>取款结果Dto</returns>
		public async Task<WithdrawResultDto> Withdraw(WithdrawRequest request)
		{
			int customerId;

			//request 判空
			if (request == null)
			{
				return WithdrawResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "Request cannot be null.");
			}

			//AccountId > 0
			if (request.AccountId <= 0)
			{
				return WithdrawResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AccountId must be greater than 0.");
			}

			//Amount > 0
			if (request.AmountDelta <= 0)
			{
				return WithdrawResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AmountDelta must be greater than 0.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return WithdrawResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			//取得当前登录用户
			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return WithdrawResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				//开始事务
				var transactionResult = await ExecuteInTxAsync<WithdrawResultDto>(
					async dataAccess =>
					{
						_transactionRepository.DataAccess = dataAccess;
						_accountRepository.DataAccess = dataAccess;

						//查询账户
						var account = await _accountRepository.GetAccountByIdAndCustomerIdForUpdate(request.AccountId, customerId);

						// 验证该账户是否存在
						if (account == null)
						{
							return WithdrawResultDto.Failure((int)TransactionErrorCode.SourceAccountNotFound, "The specified account does not exist.");
						}

						//确认账户属于当前用户
						if (account.CustomerId != customerId)
						{
							return WithdrawResultDto.Failure((int)TransactionErrorCode.SourceAccountAccessDenied, "The specified account does not belong to the current customer.");
						}

						//确认账户状态为 Active
						if (account.Status != AccountStatus.Active)
						{
							return WithdrawResultDto.Failure((int)TransactionErrorCode.AccountNotAvailable, "The specified account is not available for transactions.");
						}

						// 转换账户货币字符串为枚举类型，如果转换失败，返回错误
						if (!Enum.TryParse<CurrencyCode>(account.Currency, out var currency))
						{
							return WithdrawResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The account currency is invalid.");
						}

						// AmountDelta 最多允许两位小数
						if (request.AmountDelta != decimal.Round(
								request.AmountDelta, 2))
						{
							return WithdrawResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta cannot have more than two decimal places.");
						}

						// 检查 AmountDelta 是否超过最大允许金额
						if (request.AmountDelta > MaxAmount)
						{
							return WithdrawResultDto.Failure(
								(int)TransactionErrorCode.ValidationFailed,
								"AmountDelta exceeds the maximum allowed amount.");
						}

						// 检查账户余额是否足够进行取款操作
						if (account.Balance < request.AmountDelta)
						{
							return WithdrawResultDto.Failure(
								(int)TransactionErrorCode.InsufficientBalance,
								"Insufficient account balance.");
						}

						//计算交易前后余额
						var newBalance = account.Balance - request.AmountDelta;

						// 获取当前时间
						var now = _timeProvider.Now();

						//更新账户余额
						var updateResult = await _accountRepository.DecreaseBalanceIfEnoughBalance(account.AccountId, request.AmountDelta, now);

						//检查更新结果,如果更新失败，抛出异常以触发事务回滚
						if (updateResult != 1)
						{
							throw new TransactionOperationException(TransactionErrorCode.AccountBalanceUpdateFailed, "Failed to update the account balance.");
						}

						//插入 Transactions
						var transactionEntity = new TransactionEntity
						{
							AccountId = account.AccountId,
							TransactionType = TransactionType.Withdrawal,
							AmountDelta = request.AmountDelta,
							RelatedAccount = null,
							CreateAt = now,
							Status = TransactionStatus.Completed,
							GroupId = null,
							Note = request.Note,
							Currency = account.Currency,
							BalanceBefore = account.Balance,
							BalanceAfter = newBalance
						};

						// 插入交易记录
						var transactionId = await _transactionRepository.InsertTransaction(transactionEntity);

						if (transactionId <= 0)
						{
							throw new TransactionOperationException(TransactionErrorCode.TransactionInsertFailed, "Failed to insert the transaction record.");
						}

						//返回成功
						return WithdrawResultDto.SuccessDto(
							transactionId,
							account.AccountId,
							request.AmountDelta,
							account.Balance,
							newBalance,
							TransactionStatus.Completed,
							currency,
							now
						);
					}
				);

				return transactionResult;
			}
			catch (TransactionOperationException ex)
			{
				//发生异常则 Rollback
				return WithdrawResultDto.Failure((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return WithdrawResultDto.Failure((int)TransactionErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 获取当前用户的交易记录
		/// </summary>
		/// <param name="request">获取交易记录请求</param>
		/// <returns>获取交易记录结果Dto</returns>
		public async Task<GetMyTransactionResultDto> GetMyTransactions(GetMyTransactionRequest request)
		{
			int customerId;

			//request 判空
			if (request == null)
			{
				return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "Request cannot be null.");
			}

			// 如果 StartDate 和 EndDate 都有值，检查 StartDate 是否晚于 EndDate
			if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value.Date > request.EndDate.Value.Date)
			{
				return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "StartDate cannot be later than EndDate.");
			}

			// 开始或结束日期的边界判断
			if( (request.StartDate.HasValue && request.StartDate.Value.Date == DateTime.MinValue.Date)
				|| (request.EndDate.HasValue && request.EndDate.Value.Date == DateTime.MaxValue.Date))
			{
				return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "StartDate or EndDate exceeds the supported date range.");
			}

			// 获取东京时区信息
			var jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

			// 开始日期：当天 00:00
			DateTime? fromUtc = request.StartDate.HasValue ? TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(request.StartDate.Value.Date,DateTimeKind.Unspecified),jst) : null;

			// 结束日期：次日 00:00（不包含）
			DateTime? toUtcExclusive = request.EndDate.HasValue ? TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1),DateTimeKind.Unspecified),jst) : null;

			// 如果AccountId有值,检查 AccountId 是否小于等于 0
			if (request.AccountId.HasValue && request.AccountId <= 0)
			{
				return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.ValidationFailed, "AccountId must be greater than 0.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return GetMyTransactionResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			//取得当前登录用户
			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return GetMyTransactionResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				if (request.AccountId.HasValue)
				{
					// 如果指定了 AccountId，检查该账户是否属于当前客户
					var account = await ExecuteDbAsync<AccountEntity?>(
						async dataAccess =>
						{
							_accountRepository.DataAccess = dataAccess;
							return await _accountRepository.GetAccountByIdAndCustomerId(request.AccountId.Value, customerId);
						});

					// 如果账户不存在或不属于当前客户，返回访问被拒绝的错误
					if (account == null)
					{
						return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.SourceAccountAccessDenied, "The specified account does not belong to the current customer.");
					}

					// 如果账户状态为关闭，返回账户不可用的错误
					if (account.Status == AccountStatus.Closed)
					{
						return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.AccountNotAvailable, "The specified account is not available for transactions.");
					}

					// 查询该账户的交易记录
					var transactions = await ExecuteDbAsync<List<TransactionEntity>>(
						async dataAccess =>
						{
							_transactionRepository.DataAccess = dataAccess;
							return await _transactionRepository.GetTransactionsByAccountId(account.AccountId, fromUtc, toUtcExclusive, null, null);
						});

					// 如果没有交易记录，返回一个空的交易记录列表
					if (transactions == null || transactions.Count == 0)
					{
						return GetMyTransactionResultDto.SuccessDto(new List<TransactionRecordDto>());
					}

					var transactionDtos = new List<TransactionRecordDto>();

					// 将交易实体转换为交易记录Dto
					foreach (var transaction in transactions)
					{
						if (!Enum.TryParse<CurrencyCode>(transaction.Currency, out var transactionCurrency) || !Enum.IsDefined(transactionCurrency))
						{
							return GetMyTransactionResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The transaction currency is invalid.");
						}

						transactionDtos.Add(new TransactionRecordDto
						{
							TransactionId = transaction.TransactionId,
							AccountId = transaction.AccountId,
							TransactionType = transaction.TransactionType,
							AmountDelta = transaction.AmountDelta,
							RelatedAccount = transaction.RelatedAccount,
							CreatedAt = transaction.CreateAt,
							Status = transaction.Status,
							GroupId = transaction.GroupId,
							Note = transaction.Note,
							Currency = transactionCurrency,
							BalanceBefore = transaction.BalanceBefore,
							BalanceAfter = transaction.BalanceAfter
						});
					}

					// 返回成功的交易记录Dto列表
					return GetMyTransactionResultDto.SuccessDto(transactionDtos);
				}

				// 如果没有指定 AccountId，则查询当前客户的所有非关闭账户
				var accounts = await ExecuteDbAsync<List<AccountEntity>>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetNonClosedAccountsByCustomerId(customerId);
					});

				// 如果没有账户，返回一个空的交易记录列表
				if (accounts == null || accounts.Count == 0)
				{
					return GetMyTransactionResultDto.SuccessDto(new List<TransactionRecordDto>());
				}

				// 创建一个列表来存储所有账户的交易记录Dto
				var allTransactionDtos = new List<TransactionRecordDto>();

				foreach (var account in accounts)
				{
					// 查询每个账户的交易记录
					var transactions = await ExecuteDbAsync<List<TransactionEntity>>(
						async dataAccess =>
						{
							_transactionRepository.DataAccess = dataAccess;
							return await _transactionRepository.GetTransactionsByAccountId(account.AccountId, fromUtc, toUtcExclusive, null, null);
						});

					var transactionDtos = new List<TransactionRecordDto>();

					// 将交易实体转换为交易记录Dto
					foreach (var transaction in transactions)
					{
						if (!Enum.TryParse<CurrencyCode>(transaction.Currency, out var transactionCurrency) || !Enum.IsDefined(transactionCurrency))
						{
							return GetMyTransactionResultDto.Failure(
								(int)AccountErrorCode.InvalidCurrency,
								"The transaction currency is invalid.");
						}

						transactionDtos.Add(new TransactionRecordDto
						{
							TransactionId = transaction.TransactionId,
							AccountId = transaction.AccountId,
							TransactionType = transaction.TransactionType,
							AmountDelta = transaction.AmountDelta,
							RelatedAccount = transaction.RelatedAccount,
							CreatedAt = transaction.CreateAt,
							Status = transaction.Status,
							GroupId = transaction.GroupId,
							Note = transaction.Note,
							Currency = transactionCurrency,
							BalanceBefore = transaction.BalanceBefore,
							BalanceAfter = transaction.BalanceAfter
						});
					}

					allTransactionDtos.AddRange(transactionDtos);	
				}

				// 按创建时间降序排序
				allTransactionDtos = allTransactionDtos.OrderByDescending (t => t.CreatedAt).ThenByDescending(t => t.TransactionId).ToList();

				// 返回所有账户的交易记录Dto列表
				return GetMyTransactionResultDto.SuccessDto(allTransactionDtos);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return GetMyTransactionResultDto.Failure((int)TransactionErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}
	}

	/// <summary>
	/// 交易服务接口
	/// </summary>
	public interface ITransactionService
	{
		/// <summary>
		/// 存款操作
		/// </summary>
		/// <param name="request">存款请求</param>
		/// <returns>存款结果Dto</returns>
		Task<DepositResultDto> Deposit(DepositRequest request);

		/// <summary>
		/// 取款操作
		/// </summary>
		/// <param name="request">取款请求</param>
		/// <returns>取款结果Dto</returns>
		Task<WithdrawResultDto> Withdraw(WithdrawRequest request);

		/// <summary>
		/// 转账操作
		/// </summary>
		/// <param name="request">转账请求</param>
		/// <returns>转账结果Dto</returns>

		Task<TransferResultDto> Transfer(TransferRequest request);

		/// <summary>
		/// 获取当前用户的交易记录
		/// </summary>
		/// <param name="request">获取交易记录请求</param>
		/// <returns>获取交易记录结果Dto</returns>
		Task<GetMyTransactionResultDto> GetMyTransactions(GetMyTransactionRequest request);
	}
}
