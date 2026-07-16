using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 客戶服務實現類
    /// </summary>
    public class CustomerRepository : RepositoryBase, ICustomerRepository
    {
		public CustomerRepository(IDataAccess dataAccess) : base(dataAccess)
		{
		}

        /// <summary>
        /// 通過客戶ID獲取客戶信息
        /// </summary>
        /// <param name="id">客戶ID</param>
        /// <returns>客戶信息</returns>
        public async Task<CustomerEntity?> GetCustomerById(int id)
        {
            var sql = LoadSql("Customer.GetCustomerById");

            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerEntity>(sql, new { CustomerId = id }, _dataAccess.DbTransaction);

		}

        /// <summary>
        /// 插入新的客戶信息
        /// </summary>
        /// <param name="customer">新的客戶信息</param>
        /// <returns>客戶ID</returns>
        public async Task<int> InsertCustomer(CustomerEntity customer)
        {
            var sql = LoadSql("Customer.InsertCustomer");

            return await _dataAccess.DbConnection.ExecuteScalarAsync<int>(sql, customer, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 通過客戶電話號碼獲取客戶信息
        /// </summary>
        /// <param name="phone">電話號碼</param>
        /// <returns>客戶信息</returns>
        public async Task<CustomerEntity?> GetCustomerByPhone(string phone)
        {
            var sql = LoadSql("Customer.GetCustomerByPhone");

            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerEntity>(sql, new { Phone = phone }, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 通過客戶電子郵件獲取客戶信息
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>客戶信息</returns>
        public async Task<CustomerEntity?> GetCustomerByEmail(string email)
        {
            var sql = LoadSql("Customer.GetCustomerByEmail");

            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerEntity>(sql, new { Email = email }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新客戶KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="status">客戶KYC狀態</param>
		/// <param name="updateAt">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		public async Task<int> UpdateCustomerKycStatus(int customerId, KYCStatus status, DateTime updateAt)
		{
            var sql = LoadSql("Customer.UpdateCustomerKycStatus");

            return await _dataAccess.DbConnection.ExecuteAsync(sql, new { CustomerId = customerId, KYCStatus = status, UpdateAt = updateAt }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新客戶基本資料
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="name">客戶名稱</param>
		/// <param name="gender">性別</param>
		/// <param name="birthDate">出生日期</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		public Task<int> UpdateBasicProfile(int customerId, string name, short? gender, DateOnly birthDate, DateTime updatedAt)
		{
			var sql = LoadSql("Customer.UpdateBasicProfile");

			return _dataAccess.DbConnection.ExecuteAsync(sql, new { CustomerId = customerId, Name = name, Gender = gender, BirthDate = birthDate, UpdateAt = updatedAt}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新客戶身份證明文件信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="idType">身份證明類型</param>
		/// <param name="idNumber">身份證號碼</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		public Task<int> UpdateIdentityDocument(int customerId, short? idType, string idNumber, DateTime updatedAt)
		{
			var sql = LoadSql("Customer.UpdateIdentityDocument");

			return _dataAccess.DbConnection.ExecuteAsync(sql, new { CustomerId = customerId, IdType = idType, IdNumber = idNumber, UpdateAt = updatedAt }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新客戶聯繫信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="address">地址</param>
		/// <param name="phone">電話號碼</param>
		/// <param name="email">電子郵件</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		public Task<int> UpdateContactInfo(int customerId, string? address, string? phone, string? email, DateTime updatedAt)
		{
			var sql = LoadSql("Customer.UpdateContactInfo");

			return _dataAccess.DbConnection.ExecuteAsync(sql, new { CustomerId = customerId, Address = address, Phone = phone, Email = email, UpdateAt = updatedAt }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 刪除客戶信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="isDeleted">是否已刪除</param>
		/// <param name="deletedReason">刪除原因</param>
		/// <param name="deletedAt">刪除時間</param>
		/// <returns>刪除結果數量</returns>
		public Task<int> RemoveCustomer(int customerId, bool isDeleted, string? deletedReason, DateTime deletedAt)
		{
			var sql = LoadSql("Customer.RemoveCustomer");

			return _dataAccess.DbConnection.ExecuteAsync(sql, new { CustomerId = customerId, IsDeleted = isDeleted, DeletedReason = deletedReason, DeletedAt = deletedAt }, _dataAccess.DbTransaction);
		}
	}

    /// <summary>
    /// 客戶服務接口
    /// </summary>
    public interface ICustomerRepository
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
        Task<CustomerEntity?> GetCustomerById(int id);

        /// <summary>
        /// 插入新的客戶信息
        /// </summary>
        /// <param name="customer">新的客戶信息</param>
        /// <returns>客戶ID</returns>
        Task<int> InsertCustomer(CustomerEntity customer);

		/// <summary>
		/// 刪除客戶信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="isDeleted">是否已刪除</param>
		/// <param name="deletedReason">刪除原因</param>
		/// <param name="deletedAt">刪除時間</param>
		/// <returns>刪除結果數量</returns>
		Task<int> RemoveCustomer(int customerId, bool isDeleted, string? deletedReason , DateTime deletedAt);

        /// <summary>
        /// 通過客戶電話號碼獲取客戶信息
        /// </summary>
        /// <param name="phone">電話號碼</param>
        /// <returns>客戶信息</returns>
        Task<CustomerEntity?> GetCustomerByPhone(string phone);

        /// <summary>
        /// 通過客戶電子郵件獲取客戶信息
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>客戶信息</returns>
        Task<CustomerEntity?> GetCustomerByEmail(string email);

		/// <summary>
		/// 更新客戶KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="status">客戶KYC狀態</param>
		/// <param name="updateAt">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> UpdateCustomerKycStatus(int customerId, KYCStatus status, DateTime updateAt);

		/// <summary>
		/// 更新客戶基本資料
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="name">客戶名稱</param>
		/// <param name="gender">性別</param>
		/// <param name="birthDate">出生日期</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> UpdateBasicProfile(
			int customerId,
			string name,
			short? gender,
			DateOnly birthDate,
			DateTime updatedAt);

		/// <summary>
		/// 更新客戶身份證明文件信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="idType">身份證明類型</param>
		/// <param name="idNumber">身份證號碼</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> UpdateIdentityDocument(
			int customerId,
			short? idType,
			string idNumber,
			DateTime updatedAt);

		/// <summary>
		/// 更新客戶聯繫信息
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="address">地址</param>
		/// <param name="phone">電話號碼</param>
		/// <param name="email">電子郵件</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> UpdateContactInfo(
			int customerId,
			string? address,
			string? phone,
			string? email,
			DateTime updatedAt);
	}
}
