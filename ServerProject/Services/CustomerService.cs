using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;
using System.Diagnostics;

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
		/// 帳戶服務實例，用於處理帳戶相關的數據操作
		/// </summary>
		private readonly IAccountRepository _accountRepository;

		/// <summary>
		/// 加密服務實例，用於處理加密相關的操作
		/// </summary>
		private readonly ICryptoRepository _cryptoRepository;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 注册密码验证器。
		/// </summary>
		private readonly IPasswordValidator _passwordValidator;

		/// <summary>
		/// 日志接口
		/// </summary>
		private readonly ILogger<CustomerService> _logger;

		/// <summary>
		/// 客戶邏輯類構造函數
		/// </summary>
		/// <param name="connectionFactory">數據庫連接工廠實例</param>
		/// <param name="customerRepository">客戶服務實例</param>
		/// <param name="customerAuthRepository">客戶認證服務實例</param>
		/// <param name="timeProvider">時間提供者實例</param>
		/// <param name="logger">日志實例</param>
		/// <exception cref="ArgumentNullException">参数空异常</exception>
		public CustomerService(IConnectionFactory connectionFactory, ICustomerRepository customerRepository, ICustomerAuthRepository customerAuthRepository, ICryptoRepository cryptoRepository, ITimeProvider timeProvider, IAccountRepository accountRepository, IPasswordValidator passwordValidator, ILogger<CustomerService> logger) : base(connectionFactory, logger)
		{
			_customerRepository = customerRepository;
			_customerAuthRepository = customerAuthRepository;
			_cryptoRepository = cryptoRepository;
			_timeProvider = timeProvider;
			_accountRepository = accountRepository;
			_passwordValidator = passwordValidator;
			_logger = logger;
		}

		/// <summary>
		/// 根据客户操作的错误码区分系统故障和业务拒绝。
		/// </summary>
		private static LogOutcome GetFailureOutcome(int errorCode)
		{
			switch (errorCode)
			{
				case (int)CustomerErrorCode.UnableToOperateDb:
				case (int)CustomerErrorCode.CustomerCreationFailed:
				case (int)CustomerErrorCode.CustomerDeletionFailed:
				case (int)CustomerErrorCode.CustomerUpdateFailed:
					return LogOutcome.Failed;

				default:
					return LogOutcome.Rejected;
			}
		}

		/// <inheritdoc />
		public async Task<CustomerDto> GetCustomer(GetCustomerRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await GetCustomerCoreAsync(request);

				if (result.Success)
				{
					//查询完成后记录成功，GetCustomerCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 根據提供的請求信息獲取客戶信息的方法，返回包含客戶詳細信息的CustomerDto對象
		/// </summary>
		/// <param name="request">包含獲取客戶所需信息的GetCustomerRequest對象</param>
		/// <returns>返回包含客戶詳細信息的CustomerDto對象</returns>
		private async Task<CustomerDto> GetCustomerCoreAsync(GetCustomerRequest request)
		{
			if (request == null || request.Id <= 0)
			{
				return CustomerDto.Fail(
					(int)CustomerErrorCode.InvalidCustomerData,
					"Customer ID must be a positive integer.");
			}

			try
			{
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.Id);
					},
					"GetCustomerById");

				if (customer == null)
				{
					return CustomerDto.Fail(
						(int)CustomerErrorCode.CustomerNotFound,
						"Invalid Customer ID.");
				}

				if (customer.IsDeleted)
				{
					return CustomerDto.Fail(
						(int)CustomerErrorCode.CustomerNotFound,
						"Customer has been removed.");
				}

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
					customer.UpdatedAt,
					customer.DeletedAt,
					customer.IsDeleted,
					customer.DeletedReason);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(GetCustomer), (int)CustomerErrorCode.UnableToOperateDb);
				return CustomerDto.Fail(
					(int)CustomerErrorCode.UnableToOperateDb,
					"Unable to operate the database.");
			}
		}

		/// <inheritdoc />
		public async Task<RegisterCustomerResultDto> RegisterCustomer(RegisterCustomerRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await RegisterCustomerCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，RegisterCustomerCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 註冊客戶方法，根據提供的註冊請求信息創建新的客戶賬戶
		/// </summary>
		/// <param name="request">包含註冊所需信息的RegisterCustomerRequest對象</param>
		/// <returns>返回包含註冊結果的RegisterCustomerResultDto對象</returns>
		private async Task<RegisterCustomerResultDto> RegisterCustomerCoreAsync(RegisterCustomerRequest request)
		{
			// 校验註冊請求中客户名是否完整，如果不完整則返回錯誤信息
			if (string.IsNullOrEmpty(request.Name))
			{
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter the customer name.");
			}

			// 校验註冊請求中出生日期是否有效，如果無效則返回錯誤信息
			if (request.BirthDate == default)
			{
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter the birth date.");
			}

			// 校验密码是否符合要求，如果不符合則返回錯誤信息
			var passwordValidation = _passwordValidator.Validate(request.Password);
			if (!passwordValidation.IsValid)
			{
				return RegisterCustomerResultDto.Fail(
					(int)CustomerErrorCode.InvalidCustomerData,
					passwordValidation.Message);
			}

			try
			{
				// 检查註冊請求中是否提供了至少一種聯繫方式（電話或電子郵件），如果都沒有則返回錯誤信息
				if (string.IsNullOrEmpty(request.Phone) && string.IsNullOrEmpty(request.Email))
				{
					return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter at least one contact information (phone or email).");
				}

				if (!string.IsNullOrEmpty(request.Phone))
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

				if (!string.IsNullOrEmpty(request.Email))
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
				var now = _timeProvider.Now();

				// 創建新的客戶實體，並將註冊請求中的信息填充到客戶實體中
				var newCustomer = new CustomerEntity()
				{
					Name = request.Name,
					Gender = request.Gender,
					BirthDate = request.BirthDate,
					IDType = (IDType?)request.IDType,
					IDNumber = request.IDNumber,
					Address = request.Address,
					Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone,
					Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
					KYCStatus = KYCStatus.Unreviewed,
					CreatedAt = now,
					UpdatedAt = now,
					IsDeleted = false
				};

				// 在一個事務中同時創建客戶和客戶認證信息，確保數據的一致性
				var newCustomerId = await ExecuteInTxAsync<int>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						_customerAuthRepository.DataAccess = dataAccess;

						var customerId = await _customerRepository.InsertCustomer(newCustomer);

						if (customerId <= 0)
						{
							throw new CustomerOperationException(CustomerErrorCode.CustomerCreationFailed, "Failed to create customer account.");
						}

						var authRows = await _customerAuthRepository.InsertAuthEntry(
							new CustomerAuthEntity
							{
								CustomerId = customerId,
								LoginId = !string.IsNullOrEmpty(request.Email)
									? request.Email!
									: request.Phone!,
								PasswordHash = _cryptoRepository.Hash(
									request.Password,
									HashProfile.UserPassword),
								CreatedAt = now
							});

						if (authRows != 1)
						{
							throw new CustomerOperationException(CustomerErrorCode.CustomerCreationFailed, "Failed to create customer authentication data.");
						}

						return customerId;
					},
					"CreateCustomerAndCustomerAuth");

				// 返回成功的註冊結果，包含新創建的客戶ID
				return RegisterCustomerResultDto.SuccessDto(newCustomerId);
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(RegisterCustomer), errorCode);
				}

				return RegisterCustomerResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(RegisterCustomer), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return RegisterCustomerResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <inheritdoc/>
		public async Task<RemoveCustomerResultDto> RemoveCustomer(RemoveCustomerRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await RemoveCustomerCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，RemoveCustomerCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 刪除客戶方法，根據提供的刪除請求信息將客戶賬戶標記為已刪除狀態
		/// </summary>
		/// <param name="request">包含刪除所需信息的RemoveCustomerRequest對象</param>
		/// <returns>返回包含刪除結果的RemoveCustomerResultDto對象</returns>
		private async Task<RemoveCustomerResultDto> RemoveCustomerCoreAsync(RemoveCustomerRequest request)
		{
			// 驗證刪除請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request == null || request.Id <= 0)
			{
				return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			try
			{
				// 從數據庫中獲取要刪除的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.Id);
					},
					"GetCustomerByIdForRemoval");

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 從數據庫中獲取客戶的認證信息，確保該客戶存在認證數據
				var customerAuth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(request.Id);
					},
					"GetCustomerAuthByCustomerIdForRemoval");

				// 如果客戶的認證信息不存在，則返回錯誤信息
				if (customerAuth == null)
				{
					_logger.LogError("{Operation} failed because customer authentication data was missing. ErrorCode={ErrorCode}",
						nameof(RemoveCustomer), (int)CustomerErrorCode.CustomerDeletionFailed);
					return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.CustomerDeletionFailed, "Customer authentication data was not found.");
				}

				var now = _timeProvider.Now();

				// 在一個事務中同時刪除客戶和客戶認證信息，確保數據的一致性
				return await ExecuteInTxAsync<RemoveCustomerResultDto>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						_customerAuthRepository.DataAccess = dataAccess;
						_accountRepository.DataAccess = dataAccess;

						// 鎖定客戶記錄以進行更新，確保在刪除操作期間不會有其他操作修改該客戶的數據
						var lockedCustomer = await _customerRepository.GetCustomerByIdForUpdate(
							request.Id);

						// 如果鎖定的客戶記錄不存在或已經被刪除，則拋出異常，提示客戶不存在或已經被刪除
						if (lockedCustomer == null || lockedCustomer.IsDeleted)
						{
							throw new CustomerOperationException(CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
						}

						// 檢查客戶是否有未關閉的帳戶，如果有則拋出異常，提示所有帳戶必須在刪除客戶之前關閉
						var nonClosedAccounts = await _accountRepository.GetNonClosedAccountsByCustomerId(request.Id);

						// 如果客戶有未關閉的帳戶，則拋出異常，提示所有帳戶必須在刪除客戶之前關閉
						if (nonClosedAccounts.Count > 0)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerHasNonClosedAccounts,
								"All accounts must be closed before removing the customer.");
						}

						// 刪除客戶的認證信息，如果刪除操作失敗，則拋出異常，提示刪除客戶認證數據失敗
						var authRows = await _customerAuthRepository.RemoveAuthEntry(customerAuth);

						// 如果刪除客戶認證信息的影響行數不等於1，則拋出異常，提示刪除客戶認證數據失敗
						if (authRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerDeletionFailed,
								"Failed to remove customer authentication data.");
						}

						// 刪除客戶的基本信息，將客戶標記為已刪除狀態，如果刪除操作失敗，則拋出異常，提示刪除客戶賬戶失敗
						var affectedRows = await _customerRepository.RemoveCustomer(
							request.Id,
							request.IsDeleted,
							request.DeletedReason,
							now);

						// 如果刪除客戶基本信息的影響行數不等於1，則拋出異常，提示刪除客戶賬戶失敗
						if (affectedRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerDeletionFailed,
								"Failed to remove customer account.");
						}

						// 返回成功的刪除結果
						return RemoveCustomerResultDto.SuccessDto();
					},
					"MarkCustomerAsDeleted");
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(RemoveCustomer), errorCode);
				}

				// 如果在刪除客戶過程中發生業務邏輯異常，則返回錯誤信息
				return RemoveCustomerResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(RemoveCustomer), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return RemoveCustomerResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <inheritdoc/>
		public async Task<UpdateBasicProfileResultDto> UpdateBasicProfile(UpdateBasicProfileRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await UpdateBasicProfileCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，UpdateBasicProfileCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 更新客戶基本信息方法，根據提供的更新請求信息更新客戶的基本信息
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateBasicProfileRequest對象</param>
		/// <returns>返回包含更新結果的UpdateBasicProfileResultDto對象</returns>
		private async Task<UpdateBasicProfileResultDto> UpdateBasicProfileCoreAsync(UpdateBasicProfileRequest request)
		{
			// 如果更新請求中客戶ID無效，則返回錯誤信息，提示客戶ID必須是正整數
			if (request == null || request.Id <= 0)
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 如果更新請求中新的客戶名稱為空，則返回錯誤信息，提示請輸入客戶名稱
			if (string.IsNullOrEmpty(request.NewName))
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter the customer name.");
			}

			var now = _timeProvider.Now();

			// 如果更新請求中新的出生日期為默認值或晚於當前時間，則返回錯誤信息，提示請輸入有效的出生日期
			if (request.NewBirthDate == default || request.NewBirthDate > now)
			{
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter a valid birth date.");
			}

			try
			{
				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.Id);
					},
					"GetCustomerByIdForUpdate");

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 如果更新請求中的基本信息與現有的客戶信息完全相同，則返回錯誤信息，提示沒有檢測到任何變化
				if (existingCustomer.Name == request.NewName &&
					existingCustomer.BirthDate == request.NewBirthDate &&
					existingCustomer.Gender == request.Transgender)
				{
					return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "No changes detected in the basic profile information.");
				}

				// 在一個事務中更新客戶的基本信息，確保數據的一致性
				return await ExecuteInTxAsync<UpdateBasicProfileResultDto>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;

						// 更新客戶的基本信息，包括名稱、性別、出生日期和更新時間
						var affectedRows = await _customerRepository.UpdateBasicProfile(
							request.Id,
							request.NewName,
							(short)request.Transgender,
							DateOnly.FromDateTime(request.NewBirthDate),
							now);

						// 如果更新操作失敗，則拋出異常，提示更新客戶基本信息失敗
						if (affectedRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerUpdateFailed,
								"Failed to update customer basic profile.");
						}

						// 返回成功的更新結果
						return UpdateBasicProfileResultDto.SuccessDto();
					},
					"UpdateCustomerBasicProfile");
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(UpdateBasicProfile), errorCode);
				}

				// 如果在更新客戶基本信息過程中發生業務邏輯異常，則返回錯誤信息
				return UpdateBasicProfileResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(UpdateBasicProfile), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateBasicProfileResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <inheritdoc/>
		public async Task<UpdateContactInfoResultDto> UpdateContactInfo(UpdateContactInfoRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await UpdateContactInfoCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，UpdateContactInfoCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 更新客戶聯繫信息方法，根據提供的更新請求信息更新客戶的聯繫信息，包括電話號碼和電子郵件地址
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateContactInfoRequest對象</param>
		/// <returns>返回包含更新結果的UpdateContactInfoResultDto對象</returns>
		private async Task<UpdateContactInfoResultDto> UpdateContactInfoCoreAsync(UpdateContactInfoRequest request)
		{
			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request == null || request.CustomerId <= 0)
			{
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 电话和邮箱的空字符串统一按 null 处理。
			var phone = string.IsNullOrEmpty(request.NewPhoneNumber)
				? null
				: request.NewPhoneNumber;
			var email = string.IsNullOrEmpty(request.NewEmail)
				? null
				: request.NewEmail;

			// 如果更新請求中沒有提供任何聯繫信息（電話、電子郵件或地址），則返回錯誤信息，提示至少需要提供一種聯繫方式
			if (phone == null && email == null && string.IsNullOrEmpty(request.NewAddress))
			{
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter at least one contact information (phone, email or address).");
			}

			try
			{
				// 客户可以保留自己的号码，但不能使用其他客户的号码。
				if (phone != null)
				{
					// 如果更新請求中提供了電話號碼，則檢查該電話號碼是否已經被其他客戶註冊，如果已經被其他客戶註冊則返回錯誤信息
					var customerByPhone = await ExecuteDbAsync<CustomerEntity?>(
						async dataAccess =>
						{
							_customerRepository.DataAccess = dataAccess;
							return await _customerRepository.GetCustomerByPhone(phone);
						},
						"GetCustomerByPhoneForContactInfoUpdate");

					// 如果查詢到的客戶不為空，且該客戶的ID與當前更新請求中的客戶ID不同，則表示該電話號碼已經被其他客戶註冊，返回錯誤信息
					if (customerByPhone != null && customerByPhone.CustomerId != request.CustomerId)
					{
						return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The phone number is already registered by another customer.");
					}
				}

				// 邮箱采用和电话号码相同的归属检查。
				if (email != null)
				{
					// 如果更新請求中提供了電子郵件地址，則檢查該電子郵件地址是否已經被其他客戶註冊，如果已經被其他客戶註冊則返回錯誤信息
					var customerByEmail = await ExecuteDbAsync<CustomerEntity?>(
						async dataAccess =>
						{
							_customerRepository.DataAccess = dataAccess;
							return await _customerRepository.GetCustomerByEmail(email);
						},
						"GetCustomerByEmailForContactInfoUpdate");

					// 如果查詢到的客戶不為空，且該客戶的ID與當前更新請求中的客戶ID不同，則表示該電子郵件地址已經被其他客戶註冊，返回錯誤信息
					if (customerByEmail != null && customerByEmail.CustomerId != request.CustomerId)
					{
						return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.DuplicateCustomer, "The email address is already registered by another customer.");
					}
				}

				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForContactInfoUpdate");

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 比较前也把数据库里的旧空字符串转换为 null，避免把等价的值判为变更。
				var existingPhone = string.IsNullOrEmpty(existingCustomer.Phone)
					? null
					: existingCustomer.Phone;
				var existingEmail = string.IsNullOrEmpty(existingCustomer.Email)
					? null
					: existingCustomer.Email;

				// 如果更新請求中的聯繫信息與現有的客戶信息完全相同，則返回錯誤信息，提示沒有檢測到任何變化
				if (existingPhone == phone &&
					existingEmail == email &&
					existingCustomer.Address == request.NewAddress)
				{
					return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "No changes detected in the contact information.");
				}

				var now = _timeProvider.Now();

				// 在一個事務中更新客戶的聯繫信息，確保數據的一致性
				return await ExecuteInTxAsync<UpdateContactInfoResultDto>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;

						// 更新客戶的聯繫信息，包括地址、電話號碼、電子郵件地址和更新時間
						var affectedRows = await _customerRepository.UpdateContactInfo(
							request.CustomerId,
							request.NewAddress,
							phone,
							email,
							now);

						// 只允许更新一条客户记录。抛出异常后，事务封装会回滚。
						if (affectedRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerUpdateFailed,
								"Failed to update customer contact information.");
						}

						// 返回成功的更新結果
						return UpdateContactInfoResultDto.SuccessDto();
					},
					"UpdateCustomerContactInfo");
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(UpdateContactInfo), errorCode);
				}

				// 保留业务错误码，包括数据库唯一约束转换出的 DuplicateCustomer。
				return UpdateContactInfoResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(UpdateContactInfo), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作數據庫時發生異常，則返回錯誤信息
				return UpdateContactInfoResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <inheritdoc/>
		public async Task<UpdateIdentityDocumentResultDto> UpdateIdentityDocument(UpdateIdentityDocumentRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await UpdateIdentityDocumentCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，UpdateIdentityDocumentCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 更新客户的身份证件信息。
		/// </summary>
		/// <param name="request">身份证件更新请求。</param>
		/// <returns>身份证件更新结果。</returns>
		private async Task<UpdateIdentityDocumentResultDto> UpdateIdentityDocumentCoreAsync(UpdateIdentityDocumentRequest request)
		{
			// 验证请求中客户ID是否有效，如果无效则返回错误信息
			if (request == null || request.CustomerId <= 0)
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 验证请求中新的身份证件类型是否有效，如果无效则返回错误信息
			var idType = (IDType)request.NewIdType;
			if (!Enum.IsDefined(typeof(IDType), idType))
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Invalid ID type.");
			}

			// 验证请求中新的身份证件号码是否为空或仅包含空白字符，如果是则返回错误信息
			if (string.IsNullOrWhiteSpace(request.NewIdNumber))
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Please enter the ID number.");
			}

			// 按指定的证件类型校验证件号码格式。
			if (!IdNumberValidator.IsValid(idType, request.NewIdNumber))
			{
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Invalid ID number format for the specified ID type.");
			}

			try
			{
				// 从数据库中获取要更新的客户信息，确保该客户存在且未被删除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForIdentityDocumentUpdate");

				// 如果客户信息不存在或已经被删除，则返回错误信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "Customer not found or already removed.");
				}

				// 证件类型和号码都相同时，直接返回无变更错误。
				if (existingCustomer.IDType == idType &&
					existingCustomer.IDNumber == request.NewIdNumber)
				{
					return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "No changes detected in the identity document information.");
				}

				var now = _timeProvider.Now();

				// 在一个事务中更新客户的身份证件信息，确保数据的一致性
				return await ExecuteInTxAsync<UpdateIdentityDocumentResultDto>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;

						// 更新客户的身份证件信息，包括证件类型、证件号码和更新时间
						var affectedRows = await _customerRepository.UpdateIdentityDocument(
							request.CustomerId,
							request.NewIdType,
							request.NewIdNumber,
							now);

						// 必须恰好更新一条客户记录，否则抛出异常，让事务封装回滚。
						if (affectedRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerUpdateFailed,
								"Failed to update customer identity document information.");
						}

						// 返回成功的更新结果
						return UpdateIdentityDocumentResultDto.SuccessDto();
					},
					"UpdateCustomerIdentityDocument");
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(UpdateIdentityDocument), errorCode);
				}

				// 保留业务错误码，包括数据库唯一约束转换出的 DuplicateCustomer。
				return UpdateIdentityDocumentResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(UpdateIdentityDocument), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作数据库时发生异常，则返回错误信息
				return UpdateIdentityDocumentResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}

		/// <inheritdoc/>
		public async Task<UpdateKycStatusResultDto> UpdateKycStatus(UpdateKycStatusRequest request)
		{
			var stopwatch = Stopwatch.StartNew();

			// 如果有未处理异常向上传播，finally 会以 Failed 记录本次操作。
			var outcome = LogOutcome.Failed;

			int? errorCode = null;

			_logger.LogStart();

			try
			{
				var result = await UpdateKycStatusCoreAsync(request);

				if (result.Success)
				{
					// 客户操作事务提交后记录成功，UpdateKycStatusCoreAsync 才会返回成功结果。
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = result.Code;
					outcome = GetFailureOutcome(result.Code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 更新客戶KYC狀態方法，根據提供的更新請求信息更新客戶的KYC狀態，包括KYC審核結果和審核時間
		/// </summary>
		/// <param name="request">包含更新所需信息的UpdateKycStatusRequest對象</param>
		/// <returns>返回包含更新結果的UpdateKycStatusResultDto對象</returns>
		private async Task<UpdateKycStatusResultDto> UpdateKycStatusCoreAsync(UpdateKycStatusRequest request)
		{
			// 驗證更新請求中客戶ID是否有效，如果無效則返回錯誤信息
			if (request == null || request.CustomerId <= 0)
			{
				return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Customer ID must be a positive integer.");
			}

			// 驗證更新請求中KYC狀態是否有效，如果無效則返回錯誤信息
			if (!Enum.IsDefined(typeof(KYCStatus), request.KycStatus))
			{
				return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "Invalid KYC status.");
			}

			try
			{
				// 從數據庫中獲取要更新的客戶信息，確保該客戶存在且未被刪除
				var existingCustomer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(request.CustomerId);
					},
					"GetCustomerByIdForKycStatusUpdate");

				// 如果客戶信息不存在或已經被刪除，則返回錯誤信息
				if (existingCustomer == null || existingCustomer.IsDeleted)
				{
					return UpdateKycStatusResultDto.Fail(
						(int)CustomerErrorCode.CustomerNotFound,
						"Customer not found or already removed.");
				}

				// KYC 审核要求证件资料完整，且证件号码符合对应类型的格式。
				if (existingCustomer.IDType == null || string.IsNullOrEmpty(existingCustomer.IDNumber))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's identity document information is incomplete.");
				}

				// KYC 审核要求证件号码符合对应类型的格式。
				if (!IdNumberValidator.IsValid(
					existingCustomer.IDType.Value,
					existingCustomer.IDNumber))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's identity document information is invalid.");
				}

				// 电话和电子邮件至少需要提供一种。
				if (string.IsNullOrWhiteSpace(existingCustomer.Phone) &&
					string.IsNullOrWhiteSpace(existingCustomer.Email))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's contact information is incomplete.");
				}

				// 取得KYC状态的当前值，如果当前值为 null，则返回错误信息，提示无法更新KYC状态，因为客户的当前KYC状态未定义
				var currentKycStatus = existingCustomer.KYCStatus;
				if (currentKycStatus == null)
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, "Cannot update KYC status because the customer's current KYC status is undefined.");
				}

				// 按业务规则检查当前状态是否允许转换到目标状态。
				if (!KycStatusRule.CanTransit((KYCStatus)currentKycStatus, request.KycStatus))
				{
					return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.KYCVerificationFailed, $"Cannot update KYC status from {currentKycStatus} to {request.KycStatus} due to invalid status transition.");
				}

				var now = _timeProvider.Now();

				// 在一个事务中更新客户的KYC状态，确保数据的一致性
				return await ExecuteInTxAsync<UpdateKycStatusResultDto>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;

						// 更新客户的KYC状态，包括新的KYC状态和更新时间
						var affectedRows = await _customerRepository.UpdateCustomerKycStatus(
							request.CustomerId,
							request.KycStatus,
							now);

						// 必须恰好更新一条客户记录，否则抛出异常触发回滚。
						if (affectedRows != 1)
						{
							throw new CustomerOperationException(
								CustomerErrorCode.CustomerUpdateFailed,
								"Failed to update customer KYC status.");
						}

						// 返回成功的更新结果
						return UpdateKycStatusResultDto.SuccessDto();
					},
					"UpdateCustomerKycStatus");
			}
			catch (CustomerOperationException ex)
			{
				var errorCode = (int)ex.ErrorCode;
				if (GetFailureOutcome(errorCode) == LogOutcome.Failed)
				{
					_logger.LogError("{Operation} failed. ErrorCode={ErrorCode}", nameof(UpdateKycStatus), errorCode);
				}

				// 保留业务错误码，包括数据库唯一约束转换出的 DuplicateCustomer。
				return UpdateKycStatusResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(UpdateKycStatus), (int)CustomerErrorCode.UnableToOperateDb);
				// 如果操作数据库时发生异常，则返回错误信息
				return UpdateKycStatusResultDto.Fail((int)CustomerErrorCode.UnableToOperateDb, "Unable to operate the database.");
			}
		}
	}

	/// <summary>
	/// 客戶邏輯接口，用於定義客戶相關的業務邏輯方法
	/// </summary>
	public interface ICustomerService
	{
		/// <summary>
		/// 根據提供的請求信息獲取客戶信息的方法，返回包含客戶詳細信息的CustomerDto對象
		/// </summary>
		/// <param name="request">包含獲取客戶所需信息的GetCustomerRequest對象</param>
		/// <returns>返回包含客戶詳細信息的CustomerDto對象</returns>
		Task<CustomerDto> GetCustomer(GetCustomerRequest request);

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
