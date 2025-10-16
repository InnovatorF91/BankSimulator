using Dapper;
using ServerProject.Common;
using ServerProject.Models;

namespace ServerProject.Services
{
    /// <summary>
    /// 客戶認證服務實現類
    /// </summary>
    public class CustomerAuthService : ICustomerAuthService
    {
        /// <summary>
        /// 查詢語句：根據客戶ID獲取客戶認證信息
        /// </summary>
        private readonly string queryGetAuthByCustomerId = "SELECT * FROM \"CustomerAuth\" WHERE customer_id = @CustomerId AND is_deleted = false";

        /// <summary>
        /// 插入語句：插入新的客戶認證信息
        /// </summary>
        private readonly string queryInsertAuthEntry = "INSERT INTO \"CustomerAuth\" " +
            "(customer_id, login_id, password_hash, two_factor_enabled, created_at) " +
            "VALUES " +
            "(@CustomerId, @LoginId, @PasswordHash, @TwoFactorEnabled, @CreateAt) ";

        /// <summary>
        /// 更新語句：更新客戶認證信息
        /// </summary>
        private readonly string queryUpdateAuthEntry = "UPDATE \"CustomerAuth\" " +
            "SET login_id = @LoginId, password_hash = @PasswordHash, two_factor_enabled = @TwoFactorEnabled, updated_at = @UpdateAt " +
            "WHERE customer_id = @CustomerId AND is_deleted = false;";

        /// <summary>
        /// 刪除語句：刪除客戶認證信息
        /// </summary>
        private readonly string queryDeleteAuthEntry = "UPDATE \"CustomerAuth\" " +
            "SET deleted_at = @DeletedAt, is_deleted = @IsDeleted " +
            "WHERE customer_id = @CustomerId AND is_deleted = false;";

        /// <summary>
        /// 查詢語句：根據登入ID獲取客戶認證信息
        /// </summary>
        private readonly string queryGetAuthByLoginId = "SELECT * FROM \"CustomerAuth\" WHERE login_id = @LoginId AND is_deleted = false";

        /// <summary>
        /// 查詢語句：獲取所有客戶認證信息
        /// </summary>
        private readonly string queryListAllAuthEntries = "SELECT * FROM \"CustomerAuth\" WHERE is_deleted = false;";

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
        /// 客戶認證服務構造函數
        /// </summary>
        /// <param name="dataAccess">數據訪問對象</param>
        public CustomerAuthService(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess; // 初始化數據訪問對象
        }

        /// <summary>
        /// 刪除客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要刪除的客戶信息</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        public async Task<bool> DeleteAuthEntry(CustomerAuthModel customerAuth)
        {
            // 執行刪除操作
            return await _dataAccess.DbConnection.ExecuteAsync(queryDeleteAuthEntry, customerAuth) > 0;
        }

        /// <summary>
        /// 獲取所有客戶認證信息
        /// </summary>
        /// <returns>客戶認證信息列表</returns>
        public async Task<List<CustomerAuthModel>> GetAllAuthEntries()
        {
            // 執行查詢並獲取所有客戶認證信息
            var authEntries = await _dataAccess.DbConnection.QueryAsync<CustomerAuthModel>(queryListAllAuthEntries);

            // 將查詢結果轉換為列表並返回
            return authEntries.ToList();
        }

        /// <summary>
        /// 根據客戶ID獲取客戶認證信息
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>客戶認證信息</returns>
        public async Task<CustomerAuthModel> GetAuthByCustomerId(int customerId)
        {
            // 構建查詢並執行
            var auth = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerAuthModel>(queryGetAuthByCustomerId, new { customerId });
            if (auth == null)
            {
                // 如果沒有找到對應的客戶認證信息，則拋出異常
                throw new InvalidOperationException($"CustomerAuth with CustomerId {customerId} not found.");
            }

            // 返回查詢結果
            return auth;

        }

        /// <summary>
        /// 插入新的客戶認證信息
        /// </summary>
        /// <param name="customerAuth">新的客戶認證信息</param>
        /// <returns>true:插入成功/false:插入失敗</returns>
        public async Task<bool> InsertAuthEntry(CustomerAuthModel customerAuth)
        {
            // 執行插入操作
            return await _dataAccess.DbConnection.ExecuteAsync(queryInsertAuthEntry, customerAuth) > 0;
        }

        /// <summary>
        /// 更新客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要更新的客戶信息</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateAuthEntry(CustomerAuthModel customerAuth)
        {
            // 執行更新操作
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateAuthEntry, customerAuth) > 0;
        }

        /// <summary>
        /// 根據登入ID獲取客戶認證信息
        /// </summary>
        /// <param name="loginId">登入ID</param>
        /// <returns>客戶認證信息</returns>
        public async Task<CustomerAuthModel> GetAuthByLoginId(string loginId)
        {
            // 構建查詢並執行
            var auth = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerAuthModel>(queryGetAuthByLoginId, new { loginId });

            if (auth == null)
            {
                // 如果沒有找到對應的客戶認證信息，則拋出異常
                throw new InvalidOperationException($"CustomerAuth with LoginId {loginId} not found.");
            }

            // 返回查詢結果
            return auth;
        }
    }

    /// <summary>
    /// 客戶認證服務接口
    /// </summary>
    public interface ICustomerAuthService
    {
        /// <summary>
        /// 數據訪問對象，用於執行數據庫操作
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 根據客戶ID獲取客戶認證信息
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>客戶認證信息</returns>
        Task<CustomerAuthModel> GetAuthByCustomerId(int customerId);

        /// <summary>
        /// 插入新的客戶認證信息
        /// </summary>
        /// <param name="customerAuth">新的客戶認證信息</param>
        /// <returns>true:插入成功/false:插入失敗</returns>
        Task<bool> InsertAuthEntry(CustomerAuthModel customerAuth);

        /// <summary>
        /// 更新客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要更新的客戶信息</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateAuthEntry(CustomerAuthModel customerAuth);

        /// <summary>
        /// 刪除客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要刪除的客戶信息</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        Task<bool> DeleteAuthEntry(CustomerAuthModel customerAuth);

        /// <summary>
        /// 獲取所有客戶認證信息
        /// </summary>
        /// <returns>客戶認證信息列表</returns>
        Task<List<CustomerAuthModel>> GetAllAuthEntries();

        /// <summary>
        /// 根據登入ID獲取客戶認證信息
        /// </summary>
        /// <param name="loginId">登入ID</param>
        /// <returns>客戶認證信息</returns>
        Task<CustomerAuthModel> GetAuthByLoginId(string loginId);
    }
}
