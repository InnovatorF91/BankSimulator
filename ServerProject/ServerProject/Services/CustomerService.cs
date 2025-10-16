using Dapper;
using ServerProject.Common;
using ServerProject.Models;
using ShareProject.Common;

namespace ServerProject.Services
{
    /// <summary>
    /// 客戶服務實現類
    /// </summary>
    public class CustomerService : ICustomerService
    {
        /// <summary>
        /// 查詢語句：通過客戶ID獲取客戶信息
        /// </summary>
        private readonly string queryGetCustomerById = "SELECT * FROM \"Customers\" WHERE customer_id = @Id AND is_deleted = false";

        /// <summary>
        /// 插入語句：插入新的客戶信息
        /// </summary>
        private readonly string queryInsertCustomer = "INSERT INTO \"Customers\" " +
            "(name, gender,birth_date,id_type,id_number,address,phone,email,kyc_status,create_at) " +
            "VALUES " +
            "(@Name, @Gender,@BirthDate,@IdType,@IdNumber,@Address,@Phone,@Email,@KYCStatus,@CreateAt) " +
            "RETURNING customer_id";

        /// <summary>
        /// 更新語句：更新客戶信息
        /// </summary>
        private readonly string queryUpdateCustomer = "UPDATE \"Customers\" " +
            "SET name = @Name, gender = @Gender, birth_date = @BirthDate , id_type = @IdType , id_number = @IdNumber , address = @Address , phone = @Phone ,email = @Email , kyc_status = @KYCStatus , update_at = @UpdateAt " +
            "WHERE customer_id = @CustomerId AND is_deleted = false;";

        /// <summary>
        /// 刪除語句：刪除客戶信息
        /// </summary>
        private readonly string queryDeleteCustomer = "UPDATE \"Customers\" " +
            "SET  deleted_at = @DeletedAt, is_deleted = @IsDeleted, deleted_reason = @DeletedReason " +
            "WHERE customer_id = @CustomerId AND is_deleted = false;";

        /// <summary>
        /// 查詢語句：通過客戶電話號碼獲取客戶信息
        /// </summary>
        private readonly string queryGetCustomerByPhone = "SELECT * FROM \"Customers\" WHERE phone = @Phone AND is_deleted = false;";

        /// <summary>
        /// 查詢語句：通過客戶電子郵件獲取客戶信息
        /// </summary>
        private readonly string queryGetCustomerByEmail = "SELECT * FROM \"Customers\" WHERE email = @Email AND is_deleted = false;";

        /// <summary>
        /// 查詢語句：列出所有未刪除的客戶信息
        /// </summary>
        private readonly string queryListAllCustomers = "SELECT * FROM \"Customers\" WHERE is_deleted = false;";

		/// <summary>
		/// 更新語句：更新客戶KYC狀態
		/// </summary>
		private readonly string queryUpdateCustomerKyc = "UPDATE \"Customers\" " +
            "SET kyc_status = @KYCStatus , update_at = @UpdateAt " +
            "WHERE customer_id = @CustomerId AND is_deleted = false;";

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
        /// 客戶服務構造函數
        /// </summary>
        /// <param name="dataAccess">數據訪問對象</param>
        public CustomerService(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess; // 初始化數據訪問對象
        }

        /// <summary>
        /// 刪除客戶信息
        /// </summary>
        /// <param name="customer">客戶信息</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        public async Task<bool> DeleteCustomer(CustomerModel customer)
        {
            CheckDataAccess();

            // 執行刪除操作
            return await _dataAccess.DbConnection.ExecuteAsync(queryDeleteCustomer, customer) > 0;
        }

        /// <summary>
        /// 通過客戶ID獲取客戶信息
        /// </summary>
        /// <param name="id">客戶ID</param>
        /// <returns>客戶信息</returns>
        /// <exception cref="InvalidOperationException">无效操作异常</exception>
        public async Task<CustomerModel> GetCustomerById(int id)
        {
            // 訪問數據庫並執行查詢
            var customer = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerModel>(queryGetCustomerById, new { id });
            if (customer == null)
            {
                // 如果沒有找到客戶，則拋出異常
                throw new InvalidOperationException($"Customer with ID {id} not found.");
            }

            // 返回查詢結果
            return customer;

        }

        /// <summary>
        /// 插入新的客戶信息
        /// </summary>
        /// <param name="customer">新的客戶信息</param>
        /// <returns>客戶ID</returns>
        public async Task<int> InsertCustomer(CustomerModel customer)
        {
            // 執行插入操作
            var customerId = await _dataAccess.DbConnection.QuerySingleAsync(queryInsertCustomer, customer);

            // 如果插入成功，返回新客戶的ID
            return customerId.customer_id;
        }

        /// <summary>
        /// 列出所有客戶信息
        /// </summary>
        /// <returns>客戶信息列表</returns>
        public async Task<List<CustomerModel>> ListAll()
        {
            // 執行查詢操作，獲取所有未刪除的客戶信息
            var customers = await _dataAccess.DbConnection.QueryAsync<CustomerModel>(queryListAllCustomers);

            // 將查詢結果轉換為列表並返回
            return customers.ToList();
        }

        /// <summary>
        /// 更新客戶信息
        /// </summary>
        /// <param name="customer">客戶信息</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateCustomer(CustomerModel customer)
        {
            // 執行更新操作
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCustomer, customer) > 0;
        }

        /// <summary>
        /// 檢查數據訪問對象是否已設置
        /// </summary>
        /// <exception cref="InvalidOperationException">无效操作异常</exception>
        private void CheckDataAccess()
        {
            // 如果數據訪問對象未設置，則拋出異常
            if (_dataAccess == null)
            {
                throw new InvalidOperationException("Data access is not set.");
            }
        }

        /// <summary>
        /// 通過客戶電話號碼獲取客戶信息
        /// </summary>
        /// <param name="phone">電話號碼</param>
        /// <returns>客戶信息</returns>
        public async Task<CustomerModel?> GetCustomerByPhone(string phone)
        {
            // 執行查詢操作，獲取指定電話號碼的客戶信息
            var customer = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerModel>(queryGetCustomerByPhone, new { Phone = phone });

            // 如果沒有找到客戶，則返回null
            return customer ?? null;
        }

        /// <summary>
        /// 通過客戶電子郵件獲取客戶信息
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>客戶信息</returns>
        public async Task<CustomerModel?> GetCustomerByEmail(string email)
        {
            // 執行查詢操作，獲取指定電子郵件的客戶信息
            var customer = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerModel>(queryGetCustomerByEmail, new { Email = email });

            // 如果沒有找到客戶，則返回null
            return customer ?? null;
        }

		/// <summary>
		/// 更新客戶KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="status">客戶KYC狀態</param>
		/// <param name="updateAt">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		public async Task<bool> UpdateCustomerKycStatus(int customerId, KYCStatus status, DateTime updateAt)
		{
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCustomerKyc, new { CustomerId = customerId, KYCStatus = status, UpdateAt = updateAt }) > 0;
		}

		/// <summary>
		/// 通過客戶電子郵件或電話號碼獲取客戶信息
		/// </summary>
		/// <param name="email">電子郵件</param>
		/// <param name="phone">電話號碼</param>
		/// <returns>客戶信息</returns>
		public Task<CustomerModel?> GetCustomerByEmailOrPhone(string? email, string? phone)
		{
			if (!string.IsNullOrEmpty(email))
            {
                return GetCustomerByEmail(email);
            }
            else if (!string.IsNullOrEmpty(phone))
            {
                return GetCustomerByPhone(phone);
            }
            else
            {
                return Task.FromResult<CustomerModel?>(null);
			}
		}
	}

    /// <summary>
    /// 客戶服務接口
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// 數據訪問對象，用於執行數據庫操作
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 通過客戶ID獲取客戶信息
        /// </summary>
        /// <param name="id">客戶ID</param>
        /// <returns>客戶信息</returns>
        Task<CustomerModel> GetCustomerById(int id);

        /// <summary>
        /// 插入新的客戶信息
        /// </summary>
        /// <param name="customer">新的客戶信息</param>
        /// <returns>客戶ID</returns>
        Task<int> InsertCustomer(CustomerModel customer);

        /// <summary>
        /// 更新客戶信息
        /// </summary>
        /// <param name="customer">客戶信息</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateCustomer(CustomerModel customer);

        /// <summary>
        /// 刪除客戶信息
        /// </summary>
        /// <param name="customer">客戶信息</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        Task<bool> DeleteCustomer(CustomerModel customer);

        /// <summary>
        /// 列出所有客戶信息
        /// </summary>
        /// <returns>客戶信息列表</returns>
        Task<List<CustomerModel>> ListAll();

        /// <summary>
        /// 通過客戶電話號碼獲取客戶信息
        /// </summary>
        /// <param name="phone">電話號碼</param>
        /// <returns>客戶信息</returns>
        Task<CustomerModel?> GetCustomerByPhone(string phone);

        /// <summary>
        /// 通過客戶電子郵件獲取客戶信息
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>客戶信息</returns>
        Task<CustomerModel?> GetCustomerByEmail(string email);

		/// <summary>
		/// 通過客戶電子郵件或電話號碼獲取客戶信息
		/// </summary>
		/// <param name="email">電子郵件</param>
		/// <param name="phone">電話號碼</param>
		/// <returns>客戶信息</returns>
		Task<CustomerModel?> GetCustomerByEmailOrPhone(string? email, string? phone);

		/// <summary>
		/// 更新客戶KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="status">客戶KYC狀態</param>
		/// <param name="updateAt">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<bool> UpdateCustomerKycStatus(int customerId, KYCStatus status, DateTime updateAt);
    }
}
