using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;

namespace ServerProject.Services
{
	/// <summary>
	/// 賬戶邏輯類別，實現帳戶相關的業務邏輯
	/// </summary>
	public class AccountService : ServiceBase, IAccountService
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
        /// 建構函數，初始化帳戶邏輯類別的實例
        /// </summary>
        /// <param name="connectionFactory">數據庫連接工廠實例</param>
        /// <param name="accountRepository">帳戶服務實例</param>
        /// <param name="customerRepository">客戶服務實例</param>
        /// <param name="timeProvider">時間提供者實例</param>
        public AccountService(
			IConnectionFactory connectionFactory, 
			IAccountRepository accountRepository, 
			ICustomerRepository customerRepository,
			ITransactionRepository transactionRepository ,
			ITimeProvider timeProvider,
			ICurrentCustomer currentCustomer) : base(connectionFactory)
        {
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
            _timeProvider = timeProvider;
			_currentCustomer = currentCustomer;
		}

		/// <summary>
		/// 關閉我的帳戶，根據請求參數關閉當前登入客戶的帳戶並返回結果
		/// </summary>
		/// <param name="request">關閉我的帳戶請求參數</param>
		/// <returns>返回關閉我的帳戶結果</returns>

		public async Task<CloseMyAccountResultDto> CloseMyAccount(CloseMyAccountRequest request)
		{
			// 檢查request是否為null
			if (request == null)
			{
				return CloseMyAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 檢查當前客戶是否已認證
			if (!_currentCustomer.IsAuthenticated)
			{
				return CloseMyAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			// 從當前客戶獲取客戶ID
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return CloseMyAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			// 检查账户ID是否有效
			if (request.AccountId <= 0)
			{
				return CloseMyAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The account ID is invalid.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return CloseMyAccountResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return CloseMyAccountResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 查询账户
				var myAccount = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountByIdAndCustomerId(request.AccountId, customerId);
					}
				);

				// 如果账户不存在，返回错误
				if (myAccount == null)
				{
					return CloseMyAccountResultDto.Failure((int)AccountErrorCode.AccountNotFound, "The account does not exist.");
				}

				// 如果账户已关闭，返回错误
				if (myAccount.Status == AccountStatus.Closed)
				{
					return CloseMyAccountResultDto.Failure(
						(int)AccountErrorCode.AccountAlreadyClosed,
						"The account has already been closed.");
				}

				// 如果账户不是活动状态，返回错误
				if (myAccount.Status != AccountStatus.Active)
				{
					return CloseMyAccountResultDto.Failure(
						(int)AccountErrorCode.AccountNotAvailable,
						"The account is not active.");
				}

				// 如果账户余额不为零，返回错误
				if (myAccount.Balance != 0)
				{
					return CloseMyAccountResultDto.Failure((int)AccountErrorCode.AccountBalanceNotZero, "The account balance is not zero.");
				}

				// 获取当前时间
				var now = _timeProvider.Now();

				// 执行关闭账户操作
				var count = await ExecuteInTxAsync<int>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.UpdateAccountStatusIfCurrentStatus(
							myAccount.AccountId,
							myAccount.CustomerId,
							AccountStatus.Active,
							AccountStatus.Closed,
							now,
							requireZeroBalance: true
						);
					}
				);

				// 如果更新操作未成功，返回错误
				if (count <= 0)
				{
					return CloseMyAccountResultDto.Failure((int)AccountErrorCode.AccountStatusInvalid, "Failed to close the account. The account status or balance may have changed.");
				}

				// 返回关闭账户成功结果
				return CloseMyAccountResultDto.SuccessDto(
					myAccount.AccountId,
					AccountStatus.Closed,
					now,
					now
				);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return CloseMyAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 獲取我的帳戶信息，根據請求參數返回當前登入客戶的帳戶信息
		/// </summary>
		/// <param name="request">獲取我的帳戶請求參數</param>
		/// <returns>返回我的帳戶信息結果</returns>
		public async Task<GetMyAccountResultDto> GetMyAccount(GetMyAccountRequest request)
		{
			// 檢查request是否為null
			if (request == null)
			{
				return GetMyAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 檢查當前客戶是否已認證
			if (!_currentCustomer.IsAuthenticated)
			{
				return GetMyAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			// 從當前客戶獲取客戶ID
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return GetMyAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			// 检查账户ID是否有效
			if (request.AccountId <= 0)
			{
				return GetMyAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The account ID is invalid.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return GetMyAccountResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return GetMyAccountResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 查询账户
				var myAccount = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountByIdAndCustomerId(request.AccountId, customerId);
					}
				);

				// 如果账户不存在，返回错误
				if (myAccount == null)
				{
					return GetMyAccountResultDto.Failure((int)AccountErrorCode.AccountNotFound, "The account does not exist.");
				}

				// 转换账户货币字符串为枚举类型，如果转换失败，返回错误
				if (!Enum.TryParse<CurrencyCode>(myAccount.Currency, out var currency))
				{
					return GetMyAccountResultDto.Failure(
						(int)AccountErrorCode.InvalidCurrency,
						"The account currency is invalid.");
				}

				// 返回账户信息
				return GetMyAccountResultDto.SuccessDto(
					myAccount.AccountId,
					myAccount.AccountType,
					myAccount.Balance,
					currency,
					myAccount.Status,
					myAccount.OpenDate,
					myAccount.CloseDate,
					myAccount.UpdateDate
				);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return GetMyAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 獲取我的帳戶餘額，根據請求參數返回當前登入客戶的帳戶餘額信息
		/// </summary>
		/// <param name="request">獲取我的帳戶餘額請求參數</param>
		/// <returns>返回我的帳戶餘額信息結果</returns>
		public async Task<GetMyAccountBalanceResultDto> GetMyAccountBalance(GetMyAccountBalanceRequest request)
		{
			// 檢查request是否為null
			if (request == null)
			{
				return GetMyAccountBalanceResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 檢查當前客戶是否已認證
			if (!_currentCustomer.IsAuthenticated)
			{
				return GetMyAccountBalanceResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			// 從當前客戶獲取客戶ID
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return GetMyAccountBalanceResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			// 检查账户ID是否有效
			if (request.AccountId <= 0)
			{
				return GetMyAccountBalanceResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The account ID is invalid.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return GetMyAccountBalanceResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return GetMyAccountBalanceResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 查询账户
				var myAccount = await ExecuteDbAsync<AccountEntity?>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.GetAccountByIdAndCustomerId(request.AccountId, customerId);
					}
				);

				// 如果账户不存在，返回错误
				if (myAccount == null)
				{
					return GetMyAccountBalanceResultDto.Failure((int)AccountErrorCode.AccountNotFound, "The account does not exist.");
				}

				// 如果账户已关闭，返回错误
				if (myAccount.Status == AccountStatus.Closed)
				{
					return GetMyAccountBalanceResultDto.Failure(
						(int)AccountErrorCode.AccountAlreadyClosed,
						"The account has already been closed.");
				}

				// 转换账户货币字符串为枚举类型，如果转换失败，返回错误
				if (!Enum.TryParse<CurrencyCode>(myAccount.Currency, out var currency))
				{
					return GetMyAccountBalanceResultDto.Failure(
						(int)AccountErrorCode.InvalidCurrency,
						"The account currency is invalid.");
				}

				// 返回账户余额信息
				return GetMyAccountBalanceResultDto.SuccessDto(
					myAccount.AccountId,
					myAccount.Balance,
					currency,
					myAccount.Status,
					myAccount.UpdateDate
				);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return GetMyAccountBalanceResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 獲取我的所有帳戶信息，根據請求參數返回當前登入客戶的所有帳戶信息
		/// </summary>
		/// <param name="request">獲取我的所有帳戶請求參數</param>
		/// <returns>返回我的所有帳戶信息結果</returns>
		public async Task<GetMyAccountsResultDto> GetMyAccounts(GetMyAccountsRequest request)
		{
			// 檢查request是否為null
			if (request == null)
			{
				return GetMyAccountsResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 檢查當前客戶是否已認證
			if (!_currentCustomer.IsAuthenticated)
			{
				return GetMyAccountsResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			// 從當前客戶獲取客戶ID
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return GetMyAccountsResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			var accounts = new List<AccountEntity>();

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return GetMyAccountsResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return GetMyAccountsResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				if (request.IncludeClosedAccounts)
				{
					// 如果請求包含已關閉的帳戶，則獲取所有帳戶
					accounts = await ExecuteDbAsync<List<AccountEntity>>(
							async dataAccess =>
							{
								_accountRepository.DataAccess = dataAccess;
								return await _accountRepository.GetAllAccountsByCustomerId(customerId);
							}
						);
				}
				else
				{
					// 否則僅獲取未關閉的帳戶
					accounts = await ExecuteDbAsync<List<AccountEntity>>(
							async dataAccess =>
							{
								_accountRepository.DataAccess = dataAccess;
								return await _accountRepository.GetNonClosedAccountsByCustomerId(customerId);
							}
						);
				}
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return GetMyAccountsResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}

			var accountDtos = new List<AccountDto>();

			// 將帳戶實體轉換為帳戶DTO
			foreach (var account in accounts)
			{
				if (!Enum.TryParse<CurrencyCode>(account.Currency, out var currency))
				{
					return GetMyAccountsResultDto.Failure(
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

			// 返回帳戶DTO列表
			return GetMyAccountsResultDto.SuccessDto(accountDtos);
		}

		/// <summary>
		/// 開戶操作，根據請求參數創建新的帳戶並返回結果
		/// </summary>
		/// <param name="request">開戶請求參數</param>
		/// <returns>返回開戶結果</returns>
		public async Task<OpenAccountResultDto> OpenAccount(OpenAccountRequest request)
		{
			int customerId;

			// 检查request是否为空
			if (request == null)
			{
				return OpenAccountResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return OpenAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return OpenAccountResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			// 验证AccountType是否有效
			if (!Enum.IsDefined(typeof(AccountType), request.AccountType))
			{
				return OpenAccountResultDto.Failure((int)AccountErrorCode.InvalidAccountType, "The account type is invalid.");
			}

			// 验证CurrencyCode是否有效
			if (!Enum.IsDefined(typeof(CurrencyCode), request.Currency))
			{
				return OpenAccountResultDto.Failure((int)AccountErrorCode.InvalidCurrency, "The currency code is invalid.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return OpenAccountResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return OpenAccountResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 检查同一客户同一币种是否有未关闭的账户
				var isExistsActiveAccount = await ExecuteDbAsync<bool>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.ExistsNotClosedAccountByCustomerIdAndCurrency(customerId, request.Currency.ToString());
					}
				);

				// 如果存在同一客户同一币种的未关闭账户，则返回错误
				if (isExistsActiveAccount)
				{
					return OpenAccountResultDto.Failure((int)AccountErrorCode.AccountAlreadyExists, "The customer already has an available account with the same currency.");
				}

				// 获取当前时间
				var now = _timeProvider.Now();

				// 生成开户初始数据
				var newAccount = new AccountEntity()
				{
					CustomerId = customerId,
					AccountType = request.AccountType,
					Balance = 0,
					Currency = request.Currency.ToString(),
					Status = AccountStatus.Active,
					OpenDate = now,
					CloseDate = null,
					UpdateDate = now
				};

				// 执行开户操作
				var accountId = await ExecuteInTxAsync<long>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.InsertAccount(newAccount);
					}
				);

				// 返回开户结果
				return OpenAccountResultDto.SuccessDto(
					accountId,
					newAccount.AccountType,
					newAccount.Balance,
					request.Currency,
					newAccount.Status,
					newAccount.OpenDate
				);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return OpenAccountResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 開戶並進行初始存款操作，根據請求參數創建新的帳戶並進行初始存款，返回結果
		/// </summary>
		/// <param name="request">開戶並初始存款請求參數</param>
		/// <returns>返回開戶並初始存款結果</returns>
		public async Task<OpenAccountWithInitialDepositResultDto> OpenAccountWithInitialDeposit(OpenAccountWithInitialDepositRequest request)
		{
			int customerId;

			// 检查request是否为空
			if (request == null)
			{
				return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.ValidationFailed, "The request is null.");
			}

			// 检查初始存款金额是否大于零
			if (request.InitialDepositAmount <= 0)
			{
				return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.InvalidInitialDepositAmount, "The initial deposit amount must be greater than zero.");
			}

			// 检查当前客户是否已认证
			if (!_currentCustomer.IsAuthenticated)
			{
				return OpenAccountWithInitialDepositResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "The current customer is not authenticated.");
			}

			try
			{
				// 从当前客户获取客户ID
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果获取客户ID失败，返回未授权访问错误
				return OpenAccountWithInitialDepositResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			// 验证AccountType是否有效
			if (!Enum.IsDefined(typeof(AccountType), request.AccountType))
			{
				return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.InvalidAccountType, "The account type is invalid.");
			}

			// 验证CurrencyCode是否有效
			if (!Enum.IsDefined(typeof(CurrencyCode), request.Currency))
			{
				return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.InvalidCurrency, "The currency code is invalid.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return OpenAccountWithInitialDepositResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return OpenAccountWithInitialDepositResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 检查同一客户同一币种是否有未关闭的账户
				var isExistsActiveAccount = await ExecuteDbAsync<bool>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						return await _accountRepository.ExistsNotClosedAccountByCustomerIdAndCurrency(customerId, request.Currency.ToString());
					}
				);

				// 如果存在同一客户同一币种的未关闭账户，则返回错误
				if (isExistsActiveAccount)
				{
					return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.AccountAlreadyExists, "The customer already has an available account with the same currency.");
				}

				// 获取当前时间
				var now = _timeProvider.Now();

				// 生成开户初始数据
				var newAccount = new AccountEntity()
				{
					CustomerId = customerId,
					AccountType = request.AccountType,
					Balance = 0,
					Currency = request.Currency.ToString(),
					Status = AccountStatus.Active,
					OpenDate = now,
					CloseDate = null,
					UpdateDate = now
				};

				// 执行开户操作
				var (accountId, transactionId) = await ExecuteInTxAsync<(long, long)>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						_transactionRepository.DataAccess = dataAccess;
						// 插入新账户
						var accountId = await _accountRepository.InsertAccount(newAccount);

						// 创建新的交易实体对象
						var newTransaction = new TransactionEntity()
						{
							AccountId = accountId,
							TransactionType = TransactionType.Deposit,
							AmountDelta = request.InitialDepositAmount,
							RelatedAccount = null,
							CreateAt = now,
							Status = TransactionStatus.Completed,
							GrouppId = null,
							Note = "Initial deposit"
						};

						// 插入交易记录
						var transactionId = await _transactionRepository.InsertTransaction(newTransaction);

						await _accountRepository.IncreaseBalance(accountId, request.InitialDepositAmount, now);

						return (accountId, transactionId);
					}
				);

				// 返回开户并初始存款结果
				return OpenAccountWithInitialDepositResultDto.SuccessDto(
					accountId,
					newAccount.AccountType,
					request.InitialDepositAmount,
					request.Currency,
					newAccount.Status,
					newAccount.OpenDate,
					transactionId,
					request.InitialDepositAmount,
					TransactionStatus.Completed,
					now
				);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return OpenAccountWithInitialDepositResultDto.Failure((int)AccountErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}
	}

	/// <summary>
	/// 賬戶邏輯接口，定義帳戶相關的業務邏輯方法
	/// </summary>
	public interface IAccountService
    {
		/// <summary>
		/// 開戶操作，根據請求參數創建新的帳戶並返回結果
		/// </summary>
		/// <param name="request">開戶請求參數</param>
		/// <returns>返回開戶結果</returns>
		Task<OpenAccountResultDto> OpenAccount(OpenAccountRequest request);

		/// <summary>
		/// 開戶並進行初始存款操作，根據請求參數創建新的帳戶並進行初始存款，返回結果
		/// </summary>
		/// <param name="request">開戶並初始存款請求參數</param>
		/// <returns>返回開戶並初始存款結果</returns>
		Task<OpenAccountWithInitialDepositResultDto> OpenAccountWithInitialDeposit(OpenAccountWithInitialDepositRequest request);

		/// <summary>
		/// 獲取我的帳戶信息，根據請求參數返回當前登入客戶的帳戶信息
		/// </summary>
		/// <param name="request">獲取我的帳戶請求參數</param>
		/// <returns>返回我的帳戶信息結果</returns>
		Task<GetMyAccountResultDto> GetMyAccount(GetMyAccountRequest request);

		/// <summary>
		/// 獲取我的所有帳戶信息，根據請求參數返回當前登入客戶的所有帳戶信息
		/// </summary>
		/// <param name="request">獲取我的所有帳戶請求參數</param>
		/// <returns>返回我的所有帳戶信息結果</returns>
		Task<GetMyAccountsResultDto> GetMyAccounts(GetMyAccountsRequest request);

		/// <summary>
		/// 獲取我的帳戶餘額，根據請求參數返回當前登入客戶的帳戶餘額信息
		/// </summary>
		/// <param name="request">獲取我的帳戶餘額請求參數</param>
		/// <returns>返回我的帳戶餘額信息結果</returns>
		Task<GetMyAccountBalanceResultDto> GetMyAccountBalance(GetMyAccountBalanceRequest request);

		/// <summary>
		/// 關閉我的帳戶，根據請求參數關閉當前登入客戶的帳戶並返回結果
		/// </summary>
		/// <param name="request">關閉我的帳戶請求參數</param>
		/// <returns>返回關閉我的帳戶結果</returns>
		Task<CloseMyAccountResultDto> CloseMyAccount(CloseMyAccountRequest request);
	}
}