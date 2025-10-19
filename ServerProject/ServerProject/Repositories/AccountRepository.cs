using Dapper;
using ServerProject.Common;
using ServerProject.Models;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 帳戶服務實現類，提供帳戶相關的操作方法
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        /// <summary>
        /// 查詢語句：取得指定賬戶ID的賬戶資訊
        /// </summary>
        private readonly string queryGetAccountById = "SELECT * FROM \"Accounts\" WHERE account_id = @AccountId AND status = 0;";

        /// <summary>
        /// 查詢語句：列出指定客戶的所有賬戶資訊
        /// </summary>
        private readonly string queryListAccountsByCustomer = "SELECT * FROM \"Accounts\" WHERE customer_id = @CustomerId AND status = 0;";

        /// <summary>
        /// 插入語句：新增一個賬戶
        /// </summary>
        private readonly string queryInsertNewAccount = "INSERT INTO \"Accounts\" " +
            "(customer_id, account_type, balance, currency, status, open_date) " +
            "VALUES " +
            "(@CustomerId, @AccountType, @Balance, @Currency, @Status, @OpenDate) " +
            "RETURNING account_id;";

        /// <summary>
        /// 更新語句：更新賬戶類型
        /// </summary>
        private readonly string queryUpdateAccountType = "UPDATE \"Accounts\" " +
            "SET account_type = @NewAccountType, update_date = @UpdateDate " +
            "WHERE account_id = @AccountId;";

        /// <summary>
        /// 更新語句：調整賬戶餘額
        /// </summary>
        private readonly string queryAdjustBalance = "UPDATE \"Accounts\" " +
            "SET balance = balance + @Delta, update_date = @UpdateDate " +
            "WHERE account_id = @AccountId AND status = 0;";

        /// <summary>
        /// 更新語句：關閉賬戶
        /// </summary>
        private readonly string queryCloseAccount = "UPDATE \"Accounts\" " +
            "SET status = @Status, close_date = @CloseDate, balance = balance - balance" +
            "WHERE account_id = @AccountId AND status = 0;";

        /// <summary>
        /// 更新語句：更新賬戶狀態
        /// </summary>
        private readonly string queryUpdateStatus = "UPDATE \"Accounts\" " +
            "SET status = @Status, update_date = @UpdateDate " +
            "WHERE account_id = @AccountId;";

        /// <summary>
        /// 更新語句：更新賬戶貨幣類型
        /// </summary>
        private readonly string queryUpdateCurrency = "UPDATE \"Accounts\" " +
            "SET currency = @Currency, update_date = @UpdateDate " +
            "WHERE account_id = @AccountId" +
            "AND balance = 0" +
            "AND status = 0;";

        /// <summary>
        /// 查詢語句：獲取指定帳戶的餘額
        /// </summary>
        private readonly string queryGetBalance = "SELECT balance FROM \"Accounts\" WHERE account_id = @AccountId AND status = 0;";

        /// <summary>
        /// 數據訪問對象，用於執行數據庫操作
        /// </summary>
        private IDataAccess _dataAccess;

        /// <summary>
        /// 數據訪問對象屬性，用於設置和獲取數據訪問對象
        /// </summary>
        public IDataAccess DataAccess
        {
            set => _dataAccess = value ?? throw new ArgumentNullException(nameof(value), "Data access cannot be null.");
        }

        /// <summary>
        /// 帳戶服務構造函數，注入數據訪問對象
        /// </summary>
        /// <param name="dataAccess">數據訪問對象</param>
        public AccountRepository(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// 獲取指定帳戶ID的帳戶資訊
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>賬戶信息</returns>
        public async Task<AccountModel> GetAccountById(long accountId)
        {
            // 執行查詢操作，獲取指定帳戶ID的帳戶資訊
            var account = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<AccountModel>(
                queryGetAccountById, new { AccountId = accountId });

            // 檢查是否找到帳戶，如果沒有找到則拋出異常
            if (account == null)
            {
                throw new InvalidOperationException($"Account with ID {accountId} not found.");
            }

            // 返回找到的帳戶資訊
            return account;
        }

        /// <summary>
        /// 獲取指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>賬戶餘額</returns>
        public async Task<decimal?> GetBalance(long accountId)
        {
            // 執行查詢操作，獲取指定帳戶的餘額
            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<decimal>(
                queryGetBalance, new { AccountId = accountId });
        }

        /// <summary>
        /// 新增一個帳戶
        /// </summary>
        /// <param name="account">新的賬戶信息</param>
        /// <returns>新的賬戶ID</returns>
        public async Task<long> InsertNewAccount(AccountModel account)
        {
            // 執行插入操作，將新的帳戶資訊插入到數據庫中
            var newAccountId = await _dataAccess.DbConnection.QuerySingleAsync(queryInsertNewAccount,account);

            // 返回新插入帳戶的ID
            return newAccountId.account_id;
        }

        /// <summary>
        /// 列出指定客戶的所有帳戶
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>指定客戶的所有賬戶信息列表</returns>
        public async Task<List<AccountModel>> ListAccountsByCustomer(int customerId)
        {
            // 執行查詢操作，獲取指定客戶的所有帳戶資訊
            var accounts = await _dataAccess.DbConnection.QueryAsync<AccountModel>(
                queryListAccountsByCustomer, new { CustomerId = customerId });

            // 檢查是否找到帳戶，如果沒有找到則返回空列表，如果找到則轉換為列表並返回
            return accounts.ToList();
        }

        /// <summary>
        /// 更新帳戶類型
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="newAccountType">新的賬戶類型</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateAccountType(long accountId, AccountType newAccountType, DateTime updateDate)
        {
            // 返回執行更新操作，將指定帳戶的類型更新為新的類型
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateAccountType, new
            {
                AccountId = accountId,
                NewAccountType = newAccountType,
                UpdateDate = updateDate
            }) > 0;
        }

        /// <summary>
        /// 調整指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="delta">餘額變化量（正數表示增加，負數表示減少）</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> AdjustBalance(long accountId, decimal delta, DateTime updateDate)
        {
            // 返回執行更新操作，將指定帳戶的餘額調整為新的值
            return await _dataAccess.DbConnection.ExecuteAsync(queryAdjustBalance, new
            {
                AccountId = accountId,
                Delta = delta,
                UpdateDate = updateDate
            }) > 0;
        }

        /// <summary>
        /// 更新帳戶狀態
        /// </summary>
        /// <param name="accountId">待更新的賬戶ID</param>
        /// <param name="status">待更新的賬戶狀態</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateStatus(long accountId, AccountStatus status, DateTime updateDate)
        {
            // 返回執行更新操作，將指定帳戶的狀態更新為新的狀態
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateStatus, new
            {
                AccountId = accountId,
                Status = status,
                UpdateDate = updateDate
            }) > 0;
        }

        /// <summary>
        /// 更新帳戶貨幣類型
        /// </summary>
        /// <param name="accountId">待更新的賬戶ID</param>
        /// <param name="currency">待更新的貨幣類型</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateCurrency(long accountId, string currency, DateTime updateDate)
        {
            // 返回執行更新操作，將指定帳戶的貨幣類型更新為新的類型
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCurrency, new
            {
                AccountId = accountId,
                Currency = currency,
                UpdateDate = updateDate
            }) > 0;
        }

        /// <summary>
        /// 關閉指定帳戶
        /// </summary>
        /// <param name="accountId">待關閉的賬戶ID</param>
        /// <param name="status">待關閉的賬戶狀態</param>
        /// <param name="closedDate">關閉日期</param>
        /// <returns>true:關閉成功/false:關閉失敗</returns>
        public async Task<bool> CloseAccount(long accountId, AccountStatus status, DateTime closedDate)
        {
            // 返回執行更新操作，將指定帳戶的關閉狀態和關閉日期更新
            return await _dataAccess.DbConnection.ExecuteAsync(queryCloseAccount, new
            {
                AccountId = accountId,
                Status = status,
                CloseDate = closedDate
            }) > 0;
        }
    }

    /// <summary>
    /// 帳戶服務接口，提供帳戶相關的操作方法
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>
        /// 數據訪問對象屬性，用於設置和獲取數據訪問對象
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 獲取指定帳戶ID的帳戶資訊
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>賬戶信息</returns>
        Task<AccountModel> GetAccountById(long accountId);

        /// <summary>
        /// 列出指定客戶的所有帳戶
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>指定客戶的所有賬戶信息列表</returns>
        Task<List<AccountModel>> ListAccountsByCustomer(int customerId);

        /// <summary>
        /// 新增一個帳戶
        /// </summary>
        /// <param name="account">新的賬戶信息</param>
        /// <returns>新的賬戶ID</returns>
        Task<long> InsertNewAccount(AccountModel account);

        /// <summary>
        /// 更新帳戶類型
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="newAccountType">新的賬戶類型</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateAccountType(long accountId, AccountType newAccountType, DateTime updateDate);

        /// <summary>
        /// 調整指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="delta">餘額變化量（正數表示增加，負數表示減少）</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> AdjustBalance(long accountId, decimal delta, DateTime updateDate);

        /// <summary>
        /// 關閉指定帳戶
        /// </summary>
        /// <param name="accountId">待關閉的賬戶ID</param>
        /// <param name="status">待關閉的賬戶狀態</param>
        /// <param name="closedDate">關閉日期</param>
        /// <returns>true:關閉成功/false:關閉失敗</returns>
        Task<bool> CloseAccount(long accountId, AccountStatus status, DateTime closedDate);

        /// <summary>
        /// 更新帳戶狀態
        /// </summary>
        /// <param name="accountId">待更新的賬戶ID</param>
        /// <param name="status">待更新的賬戶狀態</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateStatus(long accountId, AccountStatus status, DateTime updateDate);

        /// <summary>
        /// 更新帳戶貨幣類型
        /// </summary>
        /// <param name="accountId">待更新的賬戶ID</param>
        /// <param name="currency">待更新的貨幣類型</param>
        /// <param name="updateDate">更新日期</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateCurrency(long accountId, string currency, DateTime updateDate);

        /// <summary>
        /// 獲取指定帳戶的餘額
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>賬戶餘額</returns>
        Task<decimal?> GetBalance(long accountId);
    }
}