using ServerProject.Common;
using ServerProject.Models;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;

namespace ServerProject.Logics
{
    /// <summary>
    /// 客戶邏輯類，用於處理客戶相關的業務邏輯
    /// </summary>
    public class CustomerLogic : LogicBase, ICustomerLogic
    {
        /// <summary>
        /// 客戶服務實例，用於處理客戶相關的數據操作
        /// </summary>
        private readonly ICustomerService _customerService;

        /// <summary>
        /// 客戶認證服務實例，用於處理客戶認證相關的數據操作
        /// </summary>
        private readonly ICustomerAuthService _customerAuthService;

        /// <summary>
        /// 時間提供者實例，用於獲取當前時間
        /// </summary>
        private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// 客戶邏輯類構造函數
        /// </summary>
        /// <param name="connectionFactory">數據庫連接工廠實例</param>
        /// <param name="customerService">客戶服務實例</param>
        /// <param name="customerAuthService">客戶認證服務實例</param>
        /// <param name="timeProvider">時間提供者實例</param>
        /// <exception cref="ArgumentNullException">参数空异常</exception>
        public CustomerLogic(IConnectionFactory connectionFactory, ICustomerService customerService, ICustomerAuthService customerAuthService, ITimeProvider timeProvider) : base(connectionFactory)
        {
            // 確保服務實例不為空，否則拋出異常
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService), "Customer service cannot be null.");

            // 確保認證服務實例不為空，否則拋出異常
            _customerAuthService = customerAuthService ?? throw new ArgumentNullException(nameof(customerAuthService), "Customer auth service cannot be null.");

            // 確保時間提供者不為空，否則拋出異常
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider), "Time provider cannot be null.");
        }

        /// <summary>
        /// 獲取客戶信息
        /// </summary>
        /// <param name="id">客戶ID</param>
        /// <returns>客戶資料傳輸物件</returns>
        public async Task<CustomerDto> GetCustomerInfo(int id)
        {
            // 生成數據訪問對象，並設置客戶服務的數據訪問對象
            using (var dataAccess = await CreateConnectionAsync())
            {
                // 設置數據訪問對象到客戶服務
                _customerService.DataAccess = dataAccess;

                // 獲取客戶信息
                var customer = await _customerService.GetCustomerById(id);

                // 如果客戶不存在，則拋出異常
                if (customer == null)
                {
                    return new CustomerDto();
                }

                // 將客戶模型轉換為客戶資料傳輸物件
                return new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    Name = customer.Name,
                    Gender = customer.Gender,
                    BirthDate = customer.BirthDate,
                    IDType = customer.IDType,
                    IDNumber = customer.IDNumber,
                    Address = customer.Address,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    KYCStatus = customer.KYCStatus,
                    CreatedAt = customer.CreatedAt,
                    UpdateAt = customer.UpdateAt,
                    DeletedAt = customer.DeletedAt,
                    IsDeleted = customer.IsDeleted,
                    DeletedReason = customer.DeletedReason
                };
            }
        }

        /// <summary>
        /// 列出所有客戶信息
        /// </summary>
        /// <returns>客戶資料傳輸物件列表</returns>
        public async Task<List<CustomerDto>> ListAllCustomers()
        {
            using (var dataAccess = await CreateConnectionAsync())
            {
                _customerService.DataAccess = dataAccess;

                // 獲取所有客戶信息
                var customers = await _customerService.ListAll();

                // 創建客戶資料傳輸物件列表
                var customerDtos = new List<CustomerDto>();

                // 將客戶模型列表轉換為客戶資料傳輸物件列表
                foreach (var customer in customers)
                {
                    var customerDto = new CustomerDto
                    {
                        CustomerId = customer.CustomerId,
                        Name = customer.Name,
                        Gender = customer.Gender,
                        BirthDate = customer.BirthDate,
                        IDType = customer.IDType,
                        IDNumber = customer.IDNumber,
                        Address = customer.Address,
                        Phone = customer.Phone,
                        Email = customer.Email,
                        KYCStatus = customer.KYCStatus,
                        CreatedAt = customer.CreatedAt,
                        UpdateAt = customer.UpdateAt,
                        DeletedAt = customer.DeletedAt,
                        IsDeleted = customer.IsDeleted,
                        DeletedReason = customer.DeletedReason
                    };

                    customerDtos.Add(customerDto);
                }

                // 返回客戶資料傳輸物件列表
                return customerDtos;
            }
        }

        /// <summary>
        /// 修改客戶信息
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>true:修改成功/false:修改失敗</returns>
        public async Task<bool> ModifyCustomer(CustomerRequest customerRequest)
        {
            // 創建數據訪問對象，並開始事務
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
                    // 設置數據訪問對象到客戶服務和客戶認證服務
                    _customerService.DataAccess = dataAccess;
                    _customerAuthService.DataAccess = dataAccess;

                    // 創建客戶模型，並從請求中填充數據
                    var customer = new CustomerModel
                    {
                        CustomerId = customerRequest.Customer.CustomerId,
                        Name = customerRequest.Customer.Name,
                        Gender = customerRequest.Customer.Gender,
                        BirthDate = customerRequest.Customer.BirthDate,
                        IDType = customerRequest.Customer.IDType,
                        IDNumber = customerRequest.Customer.IDNumber,
                        Address = customerRequest.Customer.Address,
                        Phone = customerRequest.Customer.Phone,
                        Email = customerRequest.Customer.Email,
                        KYCStatus = customerRequest.Customer.KYCStatus,
                        UpdateAt = _timeProvider.Now(),
                    };

                    // 更新客戶信息，如果更新失敗，則拋出異常
                    if (!await _customerService.UpdateCustomer(customer))
                    {
                        var e = new UnableToOperateDBException("Failed to update customer.");
                        e.Result = false;
                        throw e;
                    }

                    // 創建客戶認證模型，並從請求中填充數據
                    var customerAuth = new CustomerAuthModel
                    {
                        CustomerId = customerRequest.Customer.CustomerId,
                        LoginId = customerRequest.Customer.Name,
                        PasswordHash = customerRequest.PasswordHash,
                        TwoFactorEnabled = customerRequest.TwoFactorEnabled,
                        UpdateAt = _timeProvider.Now()
                    };

                    // 獲取原有客戶認證信息
                    var currentAuth = await _customerAuthService.GetAuthByCustomerId(customer.CustomerId);

                    // 計算是否需要異動認證（避免空密碼覆蓋）
                    var loginIdChanged = currentAuth == null || customerRequest.Customer.Name != currentAuth.LoginId;
                    var tfaChanged = currentAuth == null || customerRequest.TwoFactorEnabled != currentAuth.TwoFactorEnabled;
                    var pwdChanged = !string.IsNullOrWhiteSpace(customerRequest.PasswordHash) &&
                                          (currentAuth == null || customerRequest.PasswordHash != currentAuth.PasswordHash);

                    if (currentAuth == null)
                    {
                        // 沒有就補建（只在必要欄位齊備時）
                        var newAuth = new CustomerAuthModel
                        {
                            CustomerId = customer.CustomerId,
                            LoginId = customerRequest.Customer.Name,
                            PasswordHash = customerRequest.PasswordHash,
                            TwoFactorEnabled = customerRequest.TwoFactorEnabled,
                            CreatedAt = _timeProvider.Now(),
                        };
                        if (!await _customerAuthService.InsertAuthEntry(newAuth))
                        {
                            var e = new UnableToOperateDBException("Failed to insert customer authentication.");
                            e.Result = false;
                            throw e;
                        }                           
                    }
                    else if (loginIdChanged || tfaChanged || pwdChanged)
                    {
                        // 只帶需要變更的欄位；密碼僅在有新值時更新
                        var authToUpdate = new CustomerAuthModel
                        {
                            CustomerId = customer.CustomerId,
                            LoginId = loginIdChanged ? customerRequest.Customer.Name : currentAuth.LoginId,
                            TwoFactorEnabled = tfaChanged ? customerRequest.TwoFactorEnabled : currentAuth.TwoFactorEnabled,
                            PasswordHash = pwdChanged ? customerRequest.PasswordHash : currentAuth.PasswordHash,
                            UpdateAt = _timeProvider.Now()
                        };
                        if (!await _customerAuthService.UpdateAuthEntry(authToUpdate))
                        {
                            var e = new UnableToOperateDBException("Failed to update customer authentication.");
                            e.Result = false;
                            throw e;
                        }
                    }

                    // 提交事務
                    dataAccess.Commit();

                    // 如果所有操作成功，提交事務並返回true
                    return true;
                }
                catch (UnableToOperateDBException e)
                {
                    // 如果發生無效操作異常，回滾事務並返回false
                    dataAccess.Rollback();
                    return e.Result;
                }
            }
        }

        /// <summary>
        /// 註冊新客戶
        /// </summary>
        /// <param name="customerRequest">新的客戶請求</param>
        /// <returns>true:注冊成功/false:注冊失敗</returns>
        public async Task<bool> RegisterCustomer(CustomerRequest customerRequest)
        {
            // 創建數據訪問對象，並開始事務
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
                    // 設置數據訪問對象到客戶服務和客戶認證服務
                    _customerService.DataAccess = dataAccess;
                    _customerAuthService.DataAccess = dataAccess;

                    // 創建客戶模型，並從請求中填充數據
                    var customer = new CustomerModel
                    {
                        Name = customerRequest.Customer.Name,
                        Gender = customerRequest.Customer.Gender,
                        BirthDate = customerRequest.Customer.BirthDate,
                        IDType = customerRequest.Customer.IDType,
                        IDNumber = customerRequest.Customer.IDNumber,
                        Address = customerRequest.Customer.Address,
                        Phone = customerRequest.Customer.Phone,
                        Email = customerRequest.Customer.Email,
                        KYCStatus = customerRequest.Customer.KYCStatus,
                        CreatedAt = _timeProvider.Now(),
                        IsDeleted = false,
                    };

                    // 插入新客戶信息並獲取客戶ID
                    customer.CustomerId = await _customerService.InsertCustomer(customer);

                    // 如果客戶ID小於等於0，則拋出異常
                    if (customer.CustomerId <= 0)
                    {
                        var e = new UnableToOperateDBException("Failed to insert or update customer.");
                        e.Result = false;
                        throw e;
                    }

                    // 創建客戶認證模型，並從請求中填充數據
                    var customerAuth = new CustomerAuthModel
                    {
                        CustomerId = customer.CustomerId,
                        LoginId = customer.Name,
                        PasswordHash = customerRequest.PasswordHash,
                        TwoFactorEnabled = customerRequest.TwoFactorEnabled,
                        CreatedAt = _timeProvider.Now(),
                        IsDeleted = false,
                    };

                    // 插入客戶認證信息，如果插入失敗，則拋出異常
                    if (!await _customerAuthService.InsertAuthEntry(customerAuth))
                    {
                        var e = new UnableToOperateDBException("Failed to insert or update customer authentication.");
                        e.Result = false;
                        throw e;
                    }

                    // 提交事務
                    dataAccess.Commit();

                    // 如果所有操作成功，提交事務並返回true
                    return true;
                }
                catch (UnableToOperateDBException e)
                {
                    // 如果發生無效操作異常，回滾事務並返回false
                    dataAccess.Rollback();
                    return e.Result;
                }
            }
        }

        /// <summary>
        /// 刪除客戶信息
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        public async Task<bool> RemoveCustomer(CustomerRequest customerRequest)
        {
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
                    // 設置數據訪問對象到客戶服務和客戶認證服務
                    _customerService.DataAccess = dataAccess;
                    _customerAuthService.DataAccess = dataAccess;

                    // 創建客戶模型，並從請求中填充數據
                    var customer = new CustomerModel
                    {
                        CustomerId = customerRequest.Customer.CustomerId,
                        DeletedAt = _timeProvider.Now(),
                        IsDeleted = true,
                        DeletedReason = customerRequest.Customer.DeletedReason
                    };

                    // 刪除客戶信息，如果刪除失敗，則拋出異常
                    if (!await _customerService.DeleteCustomer(customer))
                    {
                        var e = new UnableToOperateDBException("Failed to delete customer.");
                        e.Result = false;
                        throw e;
                    }

                    // 創建客戶認證模型，並從請求中填充數據
                    var customerAuth = new CustomerAuthModel
                    {
                        CustomerId = customerRequest.Customer.CustomerId,
                        DeletedAt = _timeProvider.Now(),
                        IsDeleted = true,
                    };

                    // 刪除客戶認證信息，如果刪除失敗，則拋出異常
                    if (!await _customerAuthService.DeleteAuthEntry(customerAuth))
                    {
                        var e = new UnableToOperateDBException("Failed to delete customer authentication.");
                        e.Result = false;
                        throw e;
                    }

                    // 提交事務
                    dataAccess.Commit();

                    // 如果所有操作成功，提交事務並返回true
                    return true;
                }
                catch (UnableToOperateDBException e)
                {
                    // 如果發生無效操作異常，回滾事務並返回false
                    dataAccess.Rollback();
                    return e.Result;
                }
            }
        }

		/// <summary>
		/// 更新KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="kycStatus">KYC狀態</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		public async Task<bool> UpdateKycStatus(int customerId, KYCStatus kycStatus)
		{
            using (var dataAccess = await CreateConnectionAsync(transaction: true))
            {
                try
                {
					// 設置數據訪問對象到客戶服務
					_customerService.DataAccess = dataAccess;

					// 更新客戶KYC狀態，如果更新失敗，則拋出異常
					var success = await _customerService.UpdateCustomerKycStatus(customerId, kycStatus, _timeProvider.Now());

					// 如果更新失敗，則拋出異常
					if (!success)
                    {
                        var e = new UnableToOperateDBException("Failed to update customer KYC status.");
                        e.Result = false;
                        throw e;
					}

					// 提交事務
					dataAccess.Commit();

					// 如果所有操作成功，提交事務並返回true
					return true;
				}
                catch (UnableToOperateDBException e)
                {
					// 如果發生無效操作異常，回滾事務並返回false
					dataAccess.Rollback();
					return e.Result;
				}
            }
		}
	}

    /// <summary>
    /// 客戶邏輯接口，用於定義客戶相關的業務邏輯方法
    /// </summary>
    public interface ICustomerLogic
    {
        /// <summary>
        /// 獲取客戶信息
        /// </summary>
        /// <param name="id">客戶ID</param>
        /// <returns>客戶資料傳輸物件</returns>
        Task<CustomerDto> GetCustomerInfo(int id);

        /// <summary>
        /// 註冊新客戶
        /// </summary>
        /// <param name="customerRequest">新的客戶請求</param>
        /// <returns>true:注冊成功/false:注冊失敗</returns>
        Task<bool> RegisterCustomer(CustomerRequest customerRequest);

        /// <summary>
        /// 修改客戶信息
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>true:修改成功/false:修改失敗</returns>
        Task<bool> ModifyCustomer(CustomerRequest customerRequest);

        /// <summary>
        /// 刪除客戶信息
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        Task<bool> RemoveCustomer(CustomerRequest customerRequest);

        /// <summary>
        /// 列出所有客戶信息
        /// </summary>
        /// <returns>客戶資料傳輸物件列表</returns>
        Task<List<CustomerDto>> ListAllCustomers();

		/// <summary>
		/// 更新KYC狀態
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="kycStatus">KYC狀態</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<bool> UpdateKycStatus(int customerId, KYCStatus kycStatus);
	}
}
