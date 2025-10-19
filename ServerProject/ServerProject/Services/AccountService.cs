using ServerProject.Common;
using ServerProject.Models;
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
        /// <exception cref="ArgumentNullException">参数空异常</exception>
        public AccountService(IConnectionFactory connectionFactory, IAccountRepository accountRepository, ICustomerRepository customerRepository,ITransactionRepository transactionRepository ,ITimeProvider timeProvider) : base(connectionFactory)
        {
            // 確保帳戶服務不為空，否則拋出異常
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository), "Account service cannot be null.");

            // 確保客戶服務不為空，否則拋出異常
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository), "Customer service cannot be null.");

            // 確保交易服務不為空，否則拋出異常
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository), "Transaction service cannot be null.");

            // 確保時間提供者不為空，否則拋出異常
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider), "Time provider cannot be null.");
        }

        /// <summary>
        /// 關閉帳戶的邏輯方法，根據提供的關閉帳戶請求關閉指定帳戶
        /// </summary>
        /// <param name="closeAccountRequest">關戶請求</param>
        /// <returns>true:關閉成功/關閉失敗</returns>
        public async Task<bool> CloseAccount(CloseAccountRequest closeAccountRequest)
        {
            // 使用數據庫連接工廠創建一個新的數據庫連接，並啟用交易
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
                    // 將數據訪問對象分配給帳戶服務
                    _accountRepository.DataAccess = dataAccess;

                    // 將數據訪問對象分配給交易服務
                    _transactionRepository.DataAccess = dataAccess;

                    // 獲得關閉帳戶的餘額
                    var balance = await _accountRepository.GetBalance(Convert.ToInt64(closeAccountRequest.AccountId));

                    // 創建一個新的交易記錄，表示從關閉的帳戶中扣除餘額
                    var transactionFrom = new TransactionModel
                    {
                        AccountId = Convert.ToInt64(closeAccountRequest.AccountId),
                        TransactionType = closeAccountRequest.TransactionType,
                        AmountDelta = -(balance ?? throw new UnableToOperateDBException("Balance is null") { Result = false }), // 扣除餘額
						RelatedAccount = Convert.ToInt64(closeAccountRequest.PayoutTargetAccountId),
                        CreateAt = _timeProvider.Now(),
                        Status = TransactionStatus.Completed,
                        Note = "Close Account"
                    };

                    // 插入交易記錄並獲取交易ID
                    var transactionIdFrom = await _transactionRepository.InsertTransaction(transactionFrom);
                    if (transactionIdFrom <= 0)
                    {
                        // 如果交易插入失敗，拋出異常
                        var e = new UnableToOperateDBException("Failed to create a transaction for closing account.");
                        e.Result = false;
                        throw e;
                    }

                    // 呼叫帳戶服務的方法來關閉帳戶
                    var result = await _accountRepository.CloseAccount(Convert.ToInt64(closeAccountRequest.AccountId),AccountStatus.Closed ,_timeProvider.Now());

                    if (!result)
                    {
                        // 如果關閉帳戶失敗，拋出異常
                        var e = new UnableToOperateDBException("Failed to close the account.");
                        e.Result = result;
                        throw e;
                    }

                    // 如果是轉帳，則在目標帳戶創建一個接收交易
                    if (closeAccountRequest.TransactionType == TransactionType.Transfer)
                    {
                        // 創建一個新的交易記錄，表示在目標帳戶中接收從關閉的帳戶轉出的餘額
                        var transactionTo = new TransactionModel
                        {
                            AccountId = Convert.ToInt64(closeAccountRequest.PayoutTargetAccountId),
                            TransactionType = closeAccountRequest.TransactionType,
                            AmountDelta = balance ?? throw new UnableToOperateDBException("Balance is null") { Result = false },
                            RelatedAccount = Convert.ToInt64(closeAccountRequest.AccountId),
                            CreateAt = _timeProvider.Now(),
                            Status = TransactionStatus.Completed,
                            Note = "Receive from closed account"
                        };

                        // 插入交易記錄並獲取交易ID
                        var transactionIdTo = await _transactionRepository.InsertTransaction(transactionTo);

                        if (transactionIdTo <= 0)
                        {
                            // 如果交易插入失敗，拋出異常
                            var e = new UnableToOperateDBException("Failed to create a transaction for receiving from closed account.");
                            e.Result = false;
                            throw e;
                        }
                    }

                    // 提交交易
                    dataAccess.Commit();

                    // 返回成功關閉帳戶的結果
                    return true;
                }
                catch (InvalidOperationException)
                {
                    dataAccess.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// 獲取指定帳戶的詳細信息
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>銀行賬戶傳輸物件</returns>
        public async Task<AccountDto> GetAccount(long accountId)
        {
            // 使用數據庫連接工廠創建一個新的數據庫連接
            using (var dataAccess = await CreateConnectionAsync())
            {
                // 將數據訪問對象分配給帳戶服務
                _accountRepository.DataAccess = dataAccess;

                // 從帳戶服務中獲取指定ID的帳戶
                var account = await _accountRepository.GetAccountById(accountId);

                // 如果帳戶不存在，拋出異常
                if (account == null)
                {
                    return new AccountDto();
                }

                // 將帳戶模型轉換為帳戶DTO並返回
                return new AccountDto
                {
                    AccountId = account.AccountId,
                    CustomerId = account.CustomerId,
                    AccountType = account.AccountType,
                    Balance = account.Balance,
                    // 嘗試將字串轉換為CurrencyCode枚舉，若失敗則預設為JPY
                    Currency = Enum.TryParse<CurrencyCode>(account.Currency, out var currency) ? currency : CurrencyCode.JPY,
                    Status = account.Status,
                    OpenDate = account.OpenDate,
                    CloseDate = account.CloseDate,
                    IsClosed = account.IsClosed,
                    UpdateDate = account.UpdateDate
                };
            }
        }

        /// <summary>
        /// 獲取指定客戶的所有帳戶列表
        /// </summary>
        /// <param name="customer">客戶資料傳輸物件</param>
        /// <returns>帳戶列表</returns>
        public async Task<List<AccountDto>> GetAccounts(int customerId)
        {
            // 使用數據庫連接工廠創建一個新的數據庫連接
            using (var dataAccess = await CreateConnectionAsync())
            {
                // 將數據訪問對象分配給帳戶服務
                _accountRepository.DataAccess = dataAccess;

                // 將數據訪問對象分配給客戶服務
                var accounts = await _accountRepository.ListAccountsByCustomer(customerId);

                // 創建一個帳戶DTO列表來存儲轉換後的帳戶資料
                var accountDtos = new List<AccountDto>();

                // 遍歷每個帳戶模型並轉換為帳戶DTO
                foreach (var account in accounts)
                {
                    var accountDto = new AccountDto
                    {
                        AccountId = account.AccountId,
                        CustomerId = account.CustomerId,
                        AccountType = account.AccountType,
                        Balance = account.Balance,
                        // 嘗試將字串轉換為CurrencyCode枚舉，若失敗則預設為JPY
                        Currency = Enum.TryParse<CurrencyCode>(account.Currency, out var currency) ? currency : CurrencyCode.JPY,
                        Status = account.Status,
                        OpenDate = account.OpenDate,
                        CloseDate = account.CloseDate,
                        IsClosed = account.IsClosed,
                        UpdateDate = account.UpdateDate
                    };

                    // 將轉換後的帳戶DTO添加到列表中
                    accountDtos.Add(accountDto);
                }

                // 返回帳戶DTO列表
                return accountDtos;
            }
        }

        /// <summary>
        /// 獲取指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>帳戶餘額</returns>
        public async Task<decimal?> GetBalance(long accountId)
        {
            // 使用數據庫連接工廠創建一個新的數據庫連接
            using (var dataAccess = await CreateConnectionAsync())
            {
                // 將數據訪問對象分配給帳戶服務
                var accountRepository = _accountRepository;

                // 將數據訪問對象分配給帳戶服務
                accountRepository.DataAccess = dataAccess;

				// 從帳戶服務中獲取指定ID的帳戶餘額
				var balance = await accountRepository.GetBalance(accountId);

				// 如果無法提出賬戶餘額，賬戶不存在，則返回空值
				if (balance == null)
                {
                    return null;
                }

                // 否則返回餘額
                return balance;
            }
        }

        /// <summary>
        /// 修改帳戶的邏輯方法，根據提供的修改帳戶請求更新帳戶信息
        /// </summary>
        /// <param name="accountRequest">修改賬戶請求</param>
        /// <returns>true:修改成功/false:修改失敗</returns>
        public async Task<bool> ModifyAccount(ModifyAccountRequest accountRequest)
        {
			// 使用數據庫連接工廠創建一個新的數據庫連接，並啟用交易
			using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
					// 將數據訪問對象分配給帳戶服務
					_accountRepository.DataAccess = dataAccess;

					// 將數據訪問對象分配給交易服務
					_transactionRepository.DataAccess = dataAccess;

					// 獲取交易的金額變動
					var amountDelta = await _transactionRepository.GetAmountDeltaByTransactionId(accountRequest.TransactionId);

					// 如果金額變動不為零，代表有一筆交易需要處理，則調整帳戶餘額
					if (amountDelta != 0)
                    {
						// 調整帳戶餘額
						var result = await _accountRepository.AdjustBalance(accountRequest.Account.AccountId, amountDelta, _timeProvider.Now());

						// 如果調整餘額失敗，拋出異常
						if (!result)
                        {
                            var e = new UnableToOperateDBException("Failed to adjust account balance.");
                            e.Result = result;
                            throw e;
						}
					}

					// 獲取當前帳戶信息
					var currentAccount = await _accountRepository.GetAccountById(accountRequest.Account.AccountId);

					// 如果帳戶不存在，拋出異常
					if (currentAccount == null)
                    {
                        var e = new UnableToOperateDBException("Account not found.");
                        e.Result = false;
                        throw e;
					}

					// 比較並更新帳戶的貨幣、狀態和類型
					if (accountRequest.Account.Currency.ToString() != currentAccount.Currency)
                    {
						// 嘗試將CurrencyCode枚舉轉換為字串,並更新帳戶的貨幣
						var res1 = await _accountRepository.UpdateCurrency(accountRequest.Account.AccountId, accountRequest.Account.Currency.ToString(), _timeProvider.Now());

						// 如果更新失敗，拋出異常
						if (!res1)
                        {
                            var e = new UnableToOperateDBException("Failed to update account currency.");
                            e.Result = res1;
                            throw e;
						}
					}

					// 更新帳戶狀態
					if (accountRequest.Account.Status != currentAccount.Status)
                    {
						// 更新帳戶的狀態
						var res2 = await _accountRepository.UpdateStatus(accountRequest.Account.AccountId, accountRequest.Account.Status, _timeProvider.Now());

						// 如果更新失敗，拋出異常
						if (!res2)
                        {
                            var e = new UnableToOperateDBException("Failed to update account status.");
                            e.Result = res2;
                            throw e;
                        }
					}

					// 更新帳戶類型
					if (accountRequest.Account.AccountType != currentAccount.AccountType)
                    {
                        // 更新帳戶的類型
                        var res3 = await _accountRepository.UpdateAccountType(accountRequest.Account.AccountId, accountRequest.Account.AccountType, _timeProvider.Now());

						// 如果更新失敗，拋出異常
						if (!res3)
                        {
                            var e = new UnableToOperateDBException("Failed to update account type.");
                            e.Result = res3;
                            throw e;
                        }
					}

					// 提交交易
					dataAccess.Commit();

					// 返回成功的修改帳戶結果
					return true;
                }
                catch (UnableToOperateDBException e)
                {
					// 在發生異常時回滾交易並重新拋出異常
					dataAccess.Rollback();
                    return e.Result;
                }
            }
        }

		/// <summary>
		/// 開立新帳戶的邏輯方法，根據提供的帳戶請求創建新帳戶
		/// </summary>
		/// <param name="accountRequest">開戶請求</param>
		/// <returns>新帳戶的ID</returns>
		public async Task<long> OpenAccount(OpenAccountRequest accountRequest)
        {
			// 使用數據庫連接工廠創建一個新的數據庫連接，並啟用交易
			using (var dataAccess = await CreateConnectionAsync(transaction: true))
			{
				try
				{
					// 將數據訪問對象分配給帳戶服務
					_accountRepository.DataAccess = dataAccess;

					// 將數據訪問對象分配給客戶服務
					_customerRepository.DataAccess = dataAccess;
					// 檢查客戶是否存在，根據提供的電子郵件或電話號碼
					var existingCustomer = await _customerRepository.GetCustomerByEmailOrPhone(accountRequest.Email, accountRequest.Phone);
					// 如果客戶不存在，拋出異常
					if (existingCustomer == null)
					{
						var e = new UnableToOperateDBException("Failed to create a new account.");
						e.Result = false;
						e.ErrorCode = -1;
						throw e;
					}
					// 將客戶ID設置到帳戶請求中
					accountRequest.Account.CustomerId = existingCustomer.CustomerId;

					// 將數據訪問對象分配給客戶服務
					var accountModel = new AccountModel
					{
						CustomerId = accountRequest.Account.CustomerId,
						AccountType = accountRequest.Account.AccountType,
						Balance = accountRequest.Account.Balance,
						Currency = accountRequest.Account.Currency.ToString(),
						Status = AccountStatus.Active,
						OpenDate = _timeProvider.Now(),
						IsClosed = false,
					};

					// 呼叫帳戶服務的方法來插入新帳戶，並獲取新帳戶的ID
					var accountId = await _accountRepository.InsertNewAccount(accountModel);

					// 如果帳戶ID無效，拋出異常
					if (accountId <= 0)
					{
						var e = new UnableToOperateDBException("Failed to create a new account.");
						e.Result = false;
						e.ErrorCode = -1;
						throw e;
					}

					// 提交交易
					dataAccess.Commit();

					// 返回成功的開戶回應，包含新帳戶的詳細信息
					return accountId;
				}
				catch (UnableToOperateDBException e)
				{
					// 在發生異常時回滾交易並重新拋出異常
					dataAccess.Rollback();
					return e.ErrorCode;
				}
			}
		}

		/// <summary>
		/// 設定帳戶的貨幣類型
		/// </summary>
		/// <param name="accountId">賬戶ID</param>
		/// <param name="currency">貨幣類型</param>
		/// <returns></returns>
		public async Task<bool> SetCurrency(long accountId, CurrencyCode currency)
        {
            // 使用數據庫連接工廠創建一個新的數據庫連接
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
                    // 將數據訪問對象分配給帳戶服務
                    _accountRepository.DataAccess = dataAccess;

                    // 嘗試將CurrencyCode枚舉轉換為字串
                    var currencyString = currency.ToString();

                    // 獲取當前時間作為更新日期
                    var updateDate = _timeProvider.Now();

                    // 呼叫帳戶服務的方法來更新帳戶的貨幣類型
                    var result = await _accountRepository.UpdateCurrency(accountId, currencyString, updateDate);

                    if (!result)
                    {
                        // 如果更新失敗，拋出異常
                        var e = new UnableToOperateDBException("Failed to update account currency.");
                        e.Result = result;
                        throw e;
                    }

                    // 如果更新成功，提交交易, 且返回結果
                    dataAccess.Commit();
                    return result;
                }
                catch (UnableToOperateDBException e)
                {
                    dataAccess.Rollback();
                    return e.Result;
                } 
            }
        }
    }

	/// <summary>
	/// 賬戶邏輯接口，定義帳戶相關的業務邏輯方法
	/// </summary>
	public interface IAccountService
    {
		/// <summary>
		/// 開立新帳戶的邏輯方法，根據提供的帳戶請求創建新帳戶
		/// </summary>
		/// <param name="accountRequest">開戶請求</param>
		/// <returns>新帳戶的ID</returns>
		Task<long> OpenAccount(OpenAccountRequest accountRequest);

        /// <summary>
        /// 關閉帳戶的邏輯方法，根據提供的關閉帳戶請求關閉指定帳戶
        /// </summary>
        /// <param name="closeAccountRequest">關戶請求</param>
        /// <returns>true:關閉成功/關閉失敗</returns>
        Task<bool> CloseAccount(CloseAccountRequest closeAccountRequest);

        /// <summary>
        /// 修改帳戶的邏輯方法，根據提供的修改帳戶請求更新帳戶信息
        /// </summary>
        /// <param name="accountRequest">修改賬戶請求</param>
        /// <returns>true:修改成功/false:修改失敗</returns>
        Task<bool> ModifyAccount(ModifyAccountRequest accountRequest);

        /// <summary>
        /// 獲取指定帳戶的詳細信息
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>銀行賬戶傳輸物件</returns>
        Task<AccountDto> GetAccount(long accountId);

        /// <summary>
        /// 獲取指定客戶的所有帳戶列表
        /// </summary>
        /// <param name="customer">客戶資料傳輸物件</param>
        /// <returns>帳戶列表</returns>
        Task<List<AccountDto>> GetAccounts(int customerId);

        /// <summary>
        /// 獲取指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>帳戶餘額</returns>
        Task<decimal?> GetBalance(long accountId);

        /// <summary>
        /// 設定帳戶的貨幣類型
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="currency">貨幣類型</param>
        /// <returns></returns>
        Task<bool> SetCurrency(long accountId, CurrencyCode currency);
    }
}