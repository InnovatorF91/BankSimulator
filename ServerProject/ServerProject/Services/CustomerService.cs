using Microsoft.AspNetCore.Identity.Data;
using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;

namespace ServerProject.Services
{
    /// <summary>
    /// 客戶邏輯類，用於處理客戶相關的業務邏輯
    /// </summary>
    public class CustomerService : ServiceBase, ICustomerService
    {
        /// <summary>
        /// 客戶服務實例，用於處理客戶相關的數據操作
        /// </summary>
        private readonly ICustomerRepository _customerRepository;

        /// <summary>
        /// 客戶認證服務實例，用於處理客戶認證相關的數據操作
        /// </summary>
        private readonly ICustomerAuthRepository _customerAuthRepository;

		/// <summary>
		/// 加密服務實例，用於處理加密相關的操作
		/// </summary>
		private readonly ICryptoRepository _cryptoRepository;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// 客戶邏輯類構造函數
        /// </summary>
        /// <param name="connectionFactory">數據庫連接工廠實例</param>
        /// <param name="customerRepository">客戶服務實例</param>
        /// <param name="customerAuthRepository">客戶認證服務實例</param>
        /// <param name="timeProvider">時間提供者實例</param>
        /// <exception cref="ArgumentNullException">参数空异常</exception>
        public CustomerService(IConnectionFactory connectionFactory, ICustomerRepository customerRepository, ICustomerAuthRepository customerAuthRepository, ICryptoRepository cryptoRepository,ITimeProvider timeProvider) : base(connectionFactory)
        {
            // 確保服務實例不為空，否則拋出異常
            _customerRepository = customerRepository;

            // 確保認證服務實例不為空，否則拋出異常
            _customerAuthRepository = customerAuthRepository;

			// 確保加密服務實例不為空，否則拋出異常
			_cryptoRepository = cryptoRepository;

			// 確保時間提供者不為空，否則拋出異常
			_timeProvider = timeProvider;
        }

		/// <summary>
		/// 獲取客戶信息方法，根據客戶ID獲取對應的客戶信息
		/// </summary>
		/// <param name="id">客戶ID</param>
		/// <returns>返回包含客戶信息的CustomerDto對象</returns>
		public async Task<CustomerDto> GetCustomer(int id)
		{
			// 驗證客戶ID是否有效，如果無效則返回錯誤信息
			if (id <= 0)
			{
				return CustomerDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			try
			{
				// 從數據庫中獲取客戶信息
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(id);
					},
					"GetCustomerById"
				);

				// 如果客戶信息不存在，則返回錯誤信息
				if (customer == null)
				{
					return CustomerDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Invalid Customer ID.");
				}

				// 如果客戶已被刪除，則返回錯誤信息
				if (customer.IsDeleted)
				{
					return CustomerDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer has been removed.");
				}

				// 返回成功的客戶信息
				return CustomerDto.SuccessDto(
					customer.CustomerId,
					customer.Name,
					customer.BirthDate,
					customer.Gender,
					customer.IDType,
					customer.IDNumber,
					customer.Address,
					customer.Phone,
					customer.Email,
					customer.KYCStatus,
					customer.CreatedAt,
					customer.UpdateAt,
					customer.DeletedAt,
					customer.IsDeleted,
					customer.DeletedReason);

			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return CustomerDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}		
		}

		/// <summary>
		/// 註冊客戶方法，根據提供的註冊請求信息創建新的客戶賬戶
		/// </summary>
		/// <param name="request">包含註冊所需信息的RegisterCustomerRequest對象</param>
		/// <returns>返回包含註冊結果的RegisterCustomerResultDto對象</returns>
		public async Task<RegisterCustomerResultDto> RegisterCustomer(RegisterCustomerRequest request)
		{
			// 校验註冊請求中客户名是否完整，如果不完整則返回錯誤信息
			if (string.IsNullOrEmpty(request.Name))
			{
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.CustomerCreationFailed, "Please enter the customer name.");
			}

			// 校验註冊請求中出生日期是否有效，如果無效則返回錯誤信息
			if (request.BirthDate == default)
			{
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.CustomerCreationFailed, "Please enter the birth date.");
			}

			try
			{
				// 检查註冊請求中是否提供了至少一種聯繫方式（電話或電子郵件），如果都沒有則返回錯誤信息
				if (string.IsNullOrEmpty(request.Phone) && string.IsNullOrEmpty(request.Email))
				{
					return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.CustomerCreationFailed, "Please enter at least one contact information (phone or email).");
				}
				else if (!string.IsNullOrEmpty(request.Phone))
				{
					// 如果註冊請求中提供了電話號碼，則檢查該電話號碼是否已經被註冊，如果已經被註冊則返回錯誤信息
					var currentCustomerWithPhone = await ExecuteDbAsync<CustomerEntity?>(
						async (dataAccess) =>
						{
							_customerRepository.DataAccess = dataAccess;
							return await _customerRepository.GetCustomerByPhone(request.Phone);
						},
						"GetCustomerByPhone"
					);

					if (currentCustomerWithPhone != null)
					{
						return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The phone number is already registered.");
					}
				}
				else if (!string.IsNullOrEmpty(request.Email))
				{
					// 如果註冊請求中提供了電子郵件地址，則檢查該電子郵件地址是否已經被註冊，如果已經被註冊則返回錯誤信息
					var currentCustomerWithEmail = await ExecuteDbAsync<CustomerEntity?>(
						async (dataAccess) =>
						{
							_customerRepository.DataAccess = dataAccess;
							return await _customerRepository.GetCustomerByEmail(request.Email);
						},
						"GetCustomerByEmail"
					);
					if (currentCustomerWithEmail != null)
					{
						return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The email address is already registered.");
					}
				}

				// 獲取當前時間，用於設置客戶的創建和更新時間
				var now = _timeProvider.UtcNow();

				// 創建新的客戶實體，並將註冊請求中的信息填充到客戶實體中
				var newCustomer = new CustomerEntity()
				{
					Name = request.Name,
					Gender = request.Gender,
					BirthDate = request.BirthDate,
					IDType = (IDType?)request.IDType,
					IDNumber = request.IDNumber,
					Address = request.Address,
					Phone = request.Phone,
					Email = request.Email,
					KYCStatus = request.KYCStatus,
					CreatedAt = now,
					UpdateAt = now,
					IsDeleted = false
				};

				// 在一個事務中同時創建客戶和客戶認證信息，確保數據的一致性
				var newCustomerId = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						_customerAuthRepository.DataAccess = dataAccess;

						// 插入新的客戶信息到數據庫中，並獲取新創建的客戶ID
						var customerId = await _customerRepository.InsertCustomer(newCustomer);

						// 插入新的客戶認證信息到數據庫中，使用新創建的客戶ID作為外鍵，並將註冊請求中的登錄ID和密碼哈希存儲到客戶認證表中
						await _customerAuthRepository.InsertAuthEntry(new CustomerAuthEntity()
						{
							CustomerId = customerId,
							LoginId = !string.IsNullOrEmpty(request.Email) ? request.Email! : request.Phone!,
							PasswordHash = _cryptoRepository.Hash(request.Password, HashProfile.UserPassword),
							CreatedAt = now,
						});
						return customerId;
					},
					"CreateCustomerAndCustomerAuth"
				);

				// 如果新創建的客戶ID無效，則返回錯誤信息
				if (newCustomerId <= 0)
				{
					return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.CustomerCreationFailed, "Failed to create customer account.");
				}

				// 返回成功的註冊結果，包含新創建的客戶ID
				return RegisterCustomerResultDto.SuccessDto(newCustomerId);
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <summary>
		/// 刪除客戶方法，根據提供的刪除請求信息將客戶賬戶標記為已刪除狀態
		/// </summary>
		/// <param name="request">包含刪除所需信息的RemoveCustomerRequest對象</param>
		/// <returns>返回包含刪除結果的RemoveCustomerResultDto對象</returns>
		public async Task<RemoveCustomerResultDto> RemoveCustomer(RemoveCustomerRequest request)
		{
			// 驗證刪除請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request.Id <= 0)
			{
				return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			try
			{
				// 從數據庫中獲取要刪除的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
				async (dataAccess) =>
				{
					_customerRepository.DataAccess = dataAccess;
					return await _customerRepository.GetCustomerById(request.Id);
				},
				"GetCustomerByIdForRemoval"
				);

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 從數據庫中獲取與要刪除的客戶相關的認證信息，確保在刪除客戶時也能刪除相關的認證信息
				var customerAuthWaitingForRemoval = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(request.Id);
					},
					"GetCustomerAuthByCustomerIdForRemoval"
				);

				// TODO: 如果客户的所有账户中还有账户的状态为“啟用”，或者客户的所有交易中还有交易的状态为“待處理”，则不允许删除客户账户，并返回错误信息，返回“未經授權的訪問”

				// 獲取當前時間，用於設置客戶的刪除時間
				var now = _timeProvider.UtcNow();

				// 在一個事務中同時刪除客戶和相關的認證信息，確保數據的一致性
				var isRemoved = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						_customerAuthRepository.DataAccess = dataAccess;

						// 刪除與客戶相關的認證信息，確保在刪除客戶時也能刪除相關的認證信息
						_ = await _customerAuthRepository.RemoveAuthEntry(customerAuthWaitingForRemoval!);

						// 將客戶賬戶標記為已刪除狀態，並設置刪除原因和刪除時間
						return await _customerRepository.RemoveCustomer(request.Id, request.IsDeleted, request.DeletedReason, now);
					},
					"MarkCustomerAsDeleted"
				);

				// 如果刪除操作失敗，則返回錯誤信息
				if (isRemoved <= 0)
				{
					return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.CustomerDeletionFailed, "Failed to remove customer account.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}

			// 返回成功的刪除結果
			return RemoveCustomerResultDto.SuccessDto();
		}

		/// <summary>
		/// 更新客戶基本信息方法，根據提供的更新請求信息更新客戶的基本信息
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateBasicProfileRequest對象</param>
		/// <returns>返回包含更新結果的UpdateBasicProfileResultDto對象</returns>
		public async Task<UpdateBasicProfileResultDto> UpdateBasicProfile(UpdateBasicProfileRequest request)
		{
			// 獲取當前時間，用於設置客戶的更新時間
			var now = _timeProvider.UtcNow();

			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request.Id <= 0)
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 驗證更新請求中客戶名是否完整，如果不完整則返回錯誤信息
			if (string.IsNullOrEmpty(request.NewName))
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter the customer name.");
			}

			// 驗證更新請求中出生日期是否有效，如果無效則返回錯誤信息
			if (request.NewBirthDate == default || request.NewBirthDate > now)
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter a valid birth date.");
			}

			try
			{
				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.Id);
					},
					"GetCustomerByIdForUpdate"
				);

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 如果更新請求中的基本信息與現有的客戶信息完全相同，則返回錯誤信息，提示沒有檢測到任何變化
				if (existingCustomer.Name == request.NewName && existingCustomer.BirthDate == request.NewBirthDate && existingCustomer.Gender == request.Transgender)
				{
					return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "No changes detected in the basic profile information.");
				}

				// 在一個事務中更新客戶的基本信息，確保數據的一致性
				var isUpdated = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.UpdateBasicProfile(
							request.Id,
							request.NewName,
							(short)request.Transgender,
							DateOnly.FromDateTime(request.NewBirthDate),
							now
						);
					},
					"UpdateCustomerBasicProfile"
				);

				// 如果更新操作失敗，則返回錯誤信息
				if (isUpdated <= 0)
				{
					return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "Failed to update customer basic profile.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}

			// 返回成功的更新結果
			return UpdateBasicProfileResultDto.SuccessDto();
		}

		/// <summary>
		/// 更新客戶聯繫信息方法，根據提供的更新請求信息更新客戶的聯繫信息，包括電話號碼和電子郵件地址
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateContactInfoRequest對象</param>
		/// <returns>返回包含更新結果的UpdateContactInfoResultDto對象</returns>
		public async Task<UpdateContactInfoResultDto> UpdateContactInfo(UpdateContactInfoRequest request)
		{
			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request.CustomerId <= 0)
			{
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 驗證更新請求中是否提供了至少一種聯繫方式（電話、電子郵件或地址），如果都沒有則返回錯誤信息
			if (string.IsNullOrEmpty(request.NewPhoneNumber) && string.IsNullOrEmpty(request.NewEmail) && string.IsNullOrEmpty(request.NewAddress))
			{
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter at least one contact information (phone, email or address).");
			}

			try
			{
				// 如果更新請求中提供了電話號碼，則檢查該電話號碼是否已經被註冊給其他客戶，如果已經被註冊則返回錯誤信息
				var existingCustomerByPhone = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerByPhone(request.NewPhoneNumber);
					},
					"GetCustomerByPhoneForContactInfoUpdate"
				);

				// 如果找到的客戶信息不為空，且該客戶ID與更新請求中的客戶ID不同，則說明該電話號碼已經被註冊給其他客戶，返回錯誤信息
				if (existingCustomerByPhone != null && existingCustomerByPhone.CustomerId != request.CustomerId)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The phone number is already registered by another customer.");
				}

				// 如果更新請求中提供了電子郵件地址，則檢查該電子郵件地址是否已經被註冊給其他客戶，如果已經被註冊則返回錯誤信息
				var existingCustomerByEmail = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerByEmail(request.NewEmail);
					},
					"GetCustomerByEmailForContactInfoUpdate"
				);

				// 如果找到的客戶信息不為空，且該客戶ID與更新請求中的客戶ID不同，則說明該電子郵件地址已經被註冊給其他客戶，返回錯誤信息
				if (existingCustomerByEmail != null && existingCustomerByEmail.CustomerId != request.CustomerId)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The email address is already registered by another customer.");
				}

				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForContactInfoUpdate"
				);

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 如果更新請求中的聯繫信息與現有的客戶信息完全相同，則返回錯誤信息，提示沒有檢測到任何變化
				if (existingCustomer!.Phone == request.NewPhoneNumber && existingCustomer!.Email == request.NewEmail && existingCustomer!.Address == request.NewAddress)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "No changes detected in the contact information.");
				}

				// 獲取當前時間，用於設置客戶的更新時間
				var now = _timeProvider.UtcNow();

				// 在一個事務中更新客戶的聯繫信息，確保數據的一致性
				var isUpdated = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.UpdateContactInfo(
							request.CustomerId,
							request.NewPhoneNumber,
							request.NewEmail,
							request.NewAddress,
							now
						);
					},
					"UpdateCustomerContactInfo"
				);

				// 如果更新操作失敗，則返回錯誤信息
				if (isUpdated <= 0)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "Failed to update customer contact information.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}

			// 返回成功的更新結果
			return UpdateContactInfoResultDto.SuccessDto();
		}

		/// <summary>
		/// 更新客戶身份證明文件方法，根據提供的更新請求信息更新客戶的身份證明文件信息，包括證件類型和證件號碼
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateIdentityDocumentRequest對象</param>
		/// <returns>返回包含更新結果的UpdateIdentityDocumentResultDto對象</returns>
		public async Task<UpdateIdentityDocumentResultDto> UpdateIdentityDocument(UpdateIdentityDocumentRequest request)
		{
			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request.CustomerId <= 0)
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 驗證更新請求中是否提供了至少一種聯繫方式（電話、電子郵件），如果都沒有則返回錯誤信息
			if (string.IsNullOrEmpty(request.NewPhoneNumber) && string.IsNullOrEmpty(request.NewEmail))
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter at least one contact information (phone, email or address).");
			}

			// 驗證更新請求中提供的證件類型是否有效，如果無效則返回錯誤信息
			if (request.NewIdType < (short)IDType.IDCard || request.NewIdType > (short)IDType.DriverLicense)
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Invalid ID type.");
			}

			// 驗證更新請求中提供的證件號碼是否符合指定證件類型的格式要求，如果不符合則返回錯誤信息
			if (!IdNumberValidator.IsValid((IDType)request.NewIdType, request.NewIdNumber))
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Invalid ID number format for the specified ID type.");
			}

			try
			{
				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForIdentityDocumentUpdate"
				);

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 如果更新請求中提供了電話號碼，則檢查該電話號碼是否已經被註冊給其他客戶，如果已經被註冊則返回錯誤信息
				var existingCustomerByPhone = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerByPhone(request.NewPhoneNumber);
					},
					"GetCustomerByPhoneForContactInfoUpdate"
				);

				// 如果找到的客戶信息不為空，且該客戶ID與更新請求中的客戶ID不同，則說明該電話號碼已經被註冊給其他客戶，返回錯誤信息
				if (existingCustomerByPhone != null && existingCustomerByPhone.CustomerId != request.CustomerId)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The phone number is already registered by another customer.");
				}

				// 如果更新請求中提供了電子郵件地址，則檢查該電子郵件地址是否已經被註冊給其他客戶，如果已經被註冊則返回錯誤信息
				var existingCustomerByEmail = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerByEmail(request.NewEmail);
					},
					"GetCustomerByEmailForContactInfoUpdate"
				);

				// 如果找到的客戶信息不為空，且該客戶ID與更新請求中的客戶ID不同，則說明該電子郵件地址已經被註冊給其他客戶，返回錯誤信息
				if (existingCustomerByEmail != null && existingCustomerByEmail.CustomerId != request.CustomerId)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The email address is already registered by another customer.");
				}

				// 獲取當前時間，用於設置客戶的更新時間
				var now = _timeProvider.UtcNow();

				// 在一個事務中更新客戶的身份證明文件信息，確保數據的一致性
				var isUpdated = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.UpdateIdentityDocument(
							request.CustomerId,
							(short)request.NewIdType,
							request.NewIdNumber,
							now
						);
					},
					"UpdateCustomerIdentityDocument"
				);

				// 如果更新操作失敗，則返回錯誤信息
				if (isUpdated <= 0)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "Failed to update customer identity document information.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}

			// 返回成功的更新結果
			return UpdateIdentityDocumentResultDto.SuccessDto();
		}

		/// <summary>
		/// 更新客戶KYC狀態方法，根據提供的更新請求信息更新客戶的KYC狀態，包括KYC審核結果和審核時間
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateKycStatusRequest對象</param>
		/// <returns>返回包含更新結果的UpdateKycStatusResultDto對象</returns>
		public async Task<UpdateKycStatusResultDto> UpdateKycStatus(UpdateKycStatusRequest request)
		{
			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request.CustomerId <= 0)
			{
				return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			try
			{
				// 驗證更新請求中提供的KYC狀態是否有效，如果無效則返回錯誤信息
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForKycStatusUpdate"
				);

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 如果所获得的客户信息中缺少必要的身份证明文件信息（证件类型或证件号码），则无法进行KYC状态更新，返回错误信息
				if (existingCustomer.IDType == null || string.IsNullOrEmpty(existingCustomer.IDNumber))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's identity document information is incomplete.");
				}

				// 如果所获得的客户信息中的身份证明文件信息（证件类型和证件号码）无效，则无法进行KYC状态更新，返回错误信息
				if (IdNumberValidator.IsValid(existingCustomer.IDType.Value, existingCustomer.IDNumber))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's identity document information is invalid.");
				}

				// 如果所获得的客户信息中既缺少聯繫電話又缺少電子郵件地址，則無法進行KYC狀態更新，返回錯誤信息
				if (existingCustomer.Phone == null && existingCustomer.Email == null)
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's contact information is incomplete.");
				}

				// 如果KYC狀態的更新請求中的KYC狀態與現有的客戶信息中的KYC狀態进行比较后不合法，則返回錯誤信息，提示無法從現有的KYC狀態轉換到請求中的KYC狀態
				if (!KycStatusRule.CanTransit((KYCStatus)existingCustomer.KYCStatus!, request.KycStatus))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, $"Cannot update KYC status from {existingCustomer.KYCStatus} to {request.KycStatus} due to invalid status transition.");
				}

				// 獲取當前時間，用於設置客戶的更新時間
				var now = _timeProvider.UtcNow();

				// 在一個事務中更新客戶的KYC狀態，確保數據的一致性
				var isUpdated = await ExecuteInTxAsync<int>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.UpdateCustomerKycStatus(
							request.CustomerId,
							request.KycStatus,
							now
						);
					},
					"UpdateCustomerKycStatus"
				);

				// 如果更新操作失敗，則返回錯誤信息
				if (isUpdated <= 0)
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.CustomerUpdateFailed, "Failed to update customer KYC status.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}

			// 返回成功的更新結果
			return UpdateKycStatusResultDto.SuccessDto();
		}
	}

    /// <summary>
    /// 客戶邏輯接口，用於定義客戶相關的業務邏輯方法
    /// </summary>
    public interface ICustomerService
    {
		/// <summary>
		/// 獲取客戶信息方法，根據客戶ID獲取對應的客戶信息
		/// </summary>
		/// <param name="id">客戶ID</param>
		/// <returns>返回包含客戶信息的CustomerDto對象</returns>
		Task<CustomerDto> GetCustomer(int id);

		/// <summary>
		/// 註冊客戶方法，根據提供的註冊請求信息創建新的客戶賬戶
		/// </summary>
		/// <param name="request">包含註冊所需信息的RegisterCustomerRequest對象</param>
		/// <returns>返回包含註冊結果的RegisterCustomerResultDto對象</returns>
		Task<RegisterCustomerResultDto> RegisterCustomer(RegisterCustomerRequest request);

		/// <summary>
		/// 更新客戶基本信息方法，根據提供的更新請求信息更新客戶的基本信息
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateBasicProfileRequest對象</param>
		/// <returns>返回包含更新結果的UpdateBasicProfileResultDto對象</returns>
		Task<UpdateBasicProfileResultDto> UpdateBasicProfile(UpdateBasicProfileRequest request);

		/// <summary>
		/// 更新客戶身份證明文件方法，根據提供的更新請求信息更新客戶的身份證明文件信息，包括證件類型和證件號碼
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateIdentityDocumentRequest對象</param>
		/// <returns>返回包含更新結果的UpdateIdentityDocumentResultDto對象</returns>
		Task<UpdateIdentityDocumentResultDto> UpdateIdentityDocument(UpdateIdentityDocumentRequest request);

		/// <summary>
		/// 更新客戶聯繫信息方法，根據提供的更新請求信息更新客戶的聯繫信息，包括電話號碼和電子郵件地址
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateContactInfoRequest對象</param>
		/// <returns>返回包含更新結果的UpdateContactInfoResultDto對象</returns>
		Task<UpdateContactInfoResultDto> UpdateContactInfo(UpdateContactInfoRequest request);

		/// <summary>
		/// 更新客戶KYC狀態方法，根據提供的更新請求信息更新客戶的KYC狀態，包括KYC審核結果和審核時間
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateKycStatusRequest對象</param>
		/// <returns>返回包含更新結果的UpdateKycStatusResultDto對象</returns>
		Task<UpdateKycStatusResultDto> UpdateKycStatus(UpdateKycStatusRequest request);

		/// <summary>
		/// 刪除客戶方法，根據提供的刪除請求信息將客戶賬戶標記為已刪除狀態
		/// </summary>
		/// <param name="request">包含刪除所需信息的RemoveCustomerRequest對象</param>
		/// <returns>返回包含刪除結果的RemoveCustomerResultDto對象</returns>
		Task<RemoveCustomerResultDto> RemoveCustomer(RemoveCustomerRequest request);
	}
}
