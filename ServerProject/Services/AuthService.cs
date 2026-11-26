using System.Diagnostics;
using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;
using System.Security.Cryptography;

namespace ServerProject.Services
{
	public class AuthService : ServiceBase, IAuthService
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
		/// 客戶會話服務實例，用於處理客戶會話相關的數據操作
		/// </summary>
		private readonly ICustomerSessionRepository _customerSessionRepository;

		/// <summary>
		/// 刷新令牌服務實例，用於處理刷新令牌相關的數據操作
		/// </summary>
		private readonly IRefreshTokenRepository _refreshTokenRepository;

		/// <summary>
		/// 加密服務實例，用於處理加密和哈希操作
		/// </summary>
		private readonly ICryptoRepository _cryptoRepository;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// JWT 提供者實例，用於生成和驗證 JWT 令牌
		/// </summary>
		private readonly IJwtProvider _jwtProvider;

		/// <summary>
		/// 會話選項實例，用於配置會話相關的設置
		/// </summary>
		private readonly ISessionOption _sessionOption;

		/// <summary>
		/// 雙重認證選項實例，用於配置雙重認證相關的設置
		/// </summary>
		private readonly ITwoFactorOption _twoFactorOption;

		/// <summary>
		/// 密碼驗證器實例，用於驗證密碼是否符合安全規範
		/// </summary>
		private readonly IPasswordValidator _passwordValidator;

		/// <summary>
		/// 密碼重置令牌服務實例，用於處理密碼重置令牌相關的數據操作
		/// </summary>
		private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;

		/// <summary>
		/// 電子郵件發送器實例，用於發送密碼重置等相關的電子郵件通知
		/// </summary>
		private readonly IEmailSender _emailSender;


		private readonly ILogger<AuthService> _logger;

		public AuthService(
			IConnectionFactory connectionFactory,
			ICustomerRepository customerRepository,
			ICustomerAuthRepository customerAuthRepository,
			ICustomerSessionRepository customerSessionRepository,
			IRefreshTokenRepository refreshTokenRepository,
			ICryptoRepository cryptoRepository,
			ITimeProvider timeProvider,
			IJwtProvider jwtProvider,
			ISessionOption sessionOption,
			ITwoFactorOption twoFactorOption,
			IPasswordValidator passwordValidator,
			IPasswordResetTokenRepository passwordResetTokenRepository,
			IEmailSender emailSender,
			ILogger<AuthService> logger)
			: base(connectionFactory, logger)
		{
			// 初始化依賴的服務實例
			_customerRepository = customerRepository;
			_customerAuthRepository = customerAuthRepository;
			_customerSessionRepository = customerSessionRepository;
			_refreshTokenRepository = refreshTokenRepository;
			_cryptoRepository = cryptoRepository;
			_timeProvider = timeProvider;
			_jwtProvider = jwtProvider;
			_sessionOption = sessionOption;
			_twoFactorOption = twoFactorOption;
			_passwordValidator = passwordValidator;
			_passwordResetTokenRepository = passwordResetTokenRepository;
			_emailSender = emailSender;
			_logger = logger;
		}

		/// <summary>
		/// 開始啟用雙重認證
		/// </summary>
		/// <param name="request">開始啟用雙重認證請求，包含用戶ID</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public Task<BeginTwoFactorResultDto> BeginEnableTwoFactor(BeginEnableTwoFactorRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(BeginEnableTwoFactor),
				() => BeginEnableTwoFactorCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<BeginTwoFactorResultDto> BeginEnableTwoFactorCoreAsync(BeginEnableTwoFactorRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return BeginTwoFactorResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			var now = _timeProvider.Now();

			// 生成一個隨機的 Base64 編碼的密鑰，用於雙重認證
			var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));

			// 計算雙重認證待確認的過期時間，根據當前時間加上配置的過期時間
			var expiresAt = now.Add(_twoFactorOption.TwoFactorPendingExpiresAt);

			// 生成 otpauth URI，用於在雙重認證應用中掃描和配置雙重認證
			var otpauth = Totp.BuildTotpUri(
				issuer: "BankSimulator",
				accountName: request.CustomerId.ToString(),
				base64Secret: secret);

			try
			{
				// 在一個事務中執行以下操作：
				return await ExecuteInTxAsync<BeginTwoFactorResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;

						// 更新用戶認證信息中的雙重認證狀態為待確認，並設置密鑰和過期時間
						var updatedRows = await _customerAuthRepository.BeginEnableTwoFactor(
							request.CustomerId,
							secret,
							now,
							expiresAt);

						// 如果更新用戶認證信息的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.TwoFactorBeginFailed,
								"Failed to begin enabling two-factor authentication.");
						}

						return BeginTwoFactorResultDto.SuccessDto(secret, expiresAt, otpauth);
					},
					"BeginEnableTwoFactor");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(BeginEnableTwoFactor), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return BeginTwoFactorResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(BeginEnableTwoFactor), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return BeginTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to begin enabling two-factor authentication due to a database error.");
			}
		}

		/// <summary>
		/// 更改密碼
		/// </summary>
		/// <param name="request">更改密碼請求，包含用戶ID、舊密碼和新密碼</param>
		/// <returns>更改密碼結果DTO，包含操作結果和相關信息</returns>
		public Task<ChangePasswordResultDto> ChangePassword(ChangePasswordRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(ChangePassword),
				() => ChangePasswordCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<ChangePasswordResultDto> ChangePasswordCoreAsync(ChangePasswordRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			// 檢查舊密碼是否為空或僅包含空白字符，如果是，則返回舊密碼為必填的錯誤DTO
			if (string.IsNullOrWhiteSpace(request.OldPassword))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordOldPasswordRequired, "Old password is required.");
			}

			// 檢查新密碼是否為空或僅包含空白字符，如果是，則返回新密碼為必填的錯誤DTO
			if (string.IsNullOrWhiteSpace(request.NewPassword))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordRequired, "New password is required.");
			}

			// 檢查新密碼是否與舊密碼相同，如果是，則返回新密碼不能與舊密碼相同的錯誤DTO
			if (string.Equals(request.OldPassword, request.NewPassword, StringComparison.Ordinal))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
			}

			// 使用密碼驗證器檢查新密碼是否符合安全規範，如果不符合，則返回新密碼策略無效的錯誤DTO
			var passwordValidation = _passwordValidator.Validate(request.NewPassword);
			if (!passwordValidation.IsValid)
			{
				return ChangePasswordResultDto.Fail(
					(int)AuthErrorCode.ChangePasswordNewPasswordPolicyInvalid,
					passwordValidation.Message);
			}

			try
			{
				// 根據用戶ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							request.CustomerId);
					},
					"GetAuthByCustomerId");

				// 如果找不到對應的用戶認證信息，則返回用戶未找到的錯誤DTO
				if (auth == null)
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
				}

				// 比较舊密碼的哈希值與數據庫中存儲的哈希值是否匹配，如果不匹配，則返回舊密碼無效的錯誤DTO
				if (!_cryptoRepository.Verify(auth.PasswordHash, request.OldPassword))
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordOldPasswordInvalid, "Old password is invalid.");
				}

				// 比較新密碼的哈希值與數據庫中存儲的哈希值是否匹配，如果匹配，則返回新密碼不能與舊密碼相同的錯誤DTO
				if (_cryptoRepository.Verify(auth.PasswordHash, request.NewPassword))
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
				}

				// 生成新密碼的哈希值，以便在數據庫中更新用戶認證信息
				var newPasswordHash = _cryptoRepository.Hash(request.NewPassword, HashProfile.UserPassword);
				var now = _timeProvider.Now();

				// 在一個事務中執行以下操作：
				return await ExecuteInTxAsync<ChangePasswordResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						_refreshTokenRepository.DataAccess = dataAccess;
						_customerSessionRepository.DataAccess = dataAccess;

						// 使用戶的所有會話無效，以防止舊的會話被濫用
						await _customerSessionRepository.InvalidateAllSessions(
							request.CustomerId,
							now);

						// 撤銷用戶的所有刷新令牌，以防止舊的刷新令牌被濫用
						await _refreshTokenRepository.RevokeAllRefreshTokens(
							request.CustomerId,
							now);

						// 更新用戶認證信息中的密碼哈希值和令牌版本，以完成密碼更改操作
						var updatedRows =
							await _customerAuthRepository.UpdatePasswordHashAndBumpTokenVersion(
								request.CustomerId,
								newPasswordHash,
								now);

						// 如果更新用戶認證信息的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.ChangePasswordTransactionFailed,
								"Failed to change password due to a transaction error.");
						}

						// 如果所有操作成功，則返回成功的更改密碼結果DTO
						return ChangePasswordResultDto.SuccessDto();
					},
					"ChangePasswordTransaction");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(ChangePassword), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return ChangePasswordResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(ChangePassword), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to change password due to a database error.");
			}
		}

		/// <summary>
		/// 確認重置密碼
		/// </summary> 
		/// <param name="request">確認重置密碼請求，包含用戶ID、重置令牌和新密碼</param>
		/// <returns>確認重置密碼結果DTO，包含操作結果和相關信息</returns>
		public Task<ConfirmResetPasswordResultDto> ConfirmResetPassword(ConfirmResetPasswordRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(ConfirmResetPassword),
				() => ConfirmResetPasswordCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<ConfirmResetPasswordResultDto> ConfirmResetPasswordCoreAsync(ConfirmResetPasswordRequest request)
		{
			// 檢查輸入的重置令牌是否為空或僅包含空白字符，如果是，則返回錯誤DTO
			if (request == null || string.IsNullOrWhiteSpace(request.ResetToken))
			{
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenRequired, "Reset token is required.");
			}

			// 檢查輸入的新密碼是否為空或僅包含空白字符，如果是，則返回錯誤DTO
			if (string.IsNullOrWhiteSpace(request.NewPassword))
			{
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordNewPasswordRequired, "New password is required.");
			}

			var now = _timeProvider.Now();

			// 檢查新密碼是否符合安全規範，如果不符合，則返回錯誤DTO
			var passwordValidation = _passwordValidator.Validate(request.NewPassword);
			if (!passwordValidation.IsValid)
			{
				// 如果新密碼不符合安全規範，則返回錯誤DTO
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordNewPasswordPolicyInvalid, passwordValidation.Message);
			}

			// 使用 JWT 提供者生成重置令牌的哈希值，以便在數據庫中查找對應的重置令牌記錄
			var resetTokenHash = _jwtProvider.CreatePasswordResetTokenHash(request.ResetToken);

			try
			{
				// 根據重置令牌的哈希值從數據庫中查找對應的重置令牌記錄
				var resetToken = await ExecuteDbAsync<PasswordResetTokenEntity?>(
					async dataAccess =>
					{
						_passwordResetTokenRepository.DataAccess = dataAccess;
						return await _passwordResetTokenRepository.GetResetTokenByTokenHash(
							resetTokenHash);
					},
					"GetPasswordResetTokenByHash");

				// 如果找不到對應的重置令牌記錄，則返回錯誤DTO
				if (resetToken == null)
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenInvalid, "Invalid reset token.");
				}

				// 如果重置令牌已經被使用，則返回錯誤DTO
				if (resetToken.UsedAt != null)
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenInvalid, "Reset token has already been used.");
				}

				// 如果重置令牌已經過期，則返回錯誤DTO
				if (resetToken.ExpiresAt < now)
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenExpired, "Reset token has expired.");
				}

				// 根據重置令牌的用戶ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							resetToken.CustomerId);
					},
					"GetAuthByCustomerId");

				// 如果找不到對應的用戶認證信息，或者該用戶已被刪除，則返回錯誤DTO
				if (auth == null || auth.IsDeleted)
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordUserNotFound, "User not found.");
				}

				// 如果新密碼與舊密碼相同，則返回錯誤DTO
				if (_cryptoRepository.Verify(auth.PasswordHash, request.NewPassword))
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
				}

				// 生成新密碼的哈希值，以便在數據庫中更新用戶認證信息
				var newPasswordHash = _cryptoRepository.Hash(request.NewPassword, HashProfile.UserPassword);

				// 在一個事務中執行以下操作：
				return await ExecuteInTxAsync<ConfirmResetPasswordResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						_passwordResetTokenRepository.DataAccess = dataAccess;
						_customerSessionRepository.DataAccess = dataAccess;
						_refreshTokenRepository.DataAccess = dataAccess;

						// 將重置令牌標記為已使用，並更新其使用時間
						var markedRows = await _passwordResetTokenRepository.MarkResetTokenAsUsed(
							resetToken.TokenId,
							now);

						// 如果標記重置令牌為已使用的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (markedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.ConfirmResetPasswordFailed,
								"Failed to mark the reset token as used.");
						}

						// 使用戶的所有會話無效，以防止舊的會話被濫用
						await _customerSessionRepository.InvalidateAllSessions(
							resetToken.CustomerId,
							now);

						// 撤銷用戶的所有刷新令牌，以防止舊的刷新令牌被濫用
						await _refreshTokenRepository.RevokeAllRefreshTokens(
							resetToken.CustomerId,
							now);

						// 更新用戶認證信息中的密碼哈希值和令牌版本，以完成密碼重置操作
						var updatedRows =
							await _customerAuthRepository.UpdatePasswordHashAndBumpTokenVersion(
								resetToken.CustomerId,
								newPasswordHash,
								now);

						// 如果更新用戶認證信息的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.ConfirmResetPasswordFailed,
								"Failed to update the password.");
						}

						// 如果所有操作成功，則返回成功的確認重置密碼結果DTO
						return ConfirmResetPasswordResultDto.SuccessDto();
					},
					"ConfirmResetPasswordTransaction");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(ConfirmResetPassword), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return ConfirmResetPasswordResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(ConfirmResetPassword), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to confirm reset password due to a database error.");
			}
		}

		/// <summary>
		/// 確認啟用雙重認證
		/// </summary>
		/// <param name="request">確認啟用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public Task<ConfirmEnableTwoFactorResultDto> ConfirmEnableTwoFactor(ConfirmEnableTwoFactorRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(ConfirmEnableTwoFactor),
				() => ConfirmEnableTwoFactorCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<ConfirmEnableTwoFactorResultDto> ConfirmEnableTwoFactorCoreAsync(ConfirmEnableTwoFactorRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			// 檢查雙重認證代碼是否為空或僅包含空白字符，如果是，則返回無效的雙重認證代碼的錯誤DTO
			if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
			{
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
			}

			var now = _timeProvider.Now();

			try
			{
				// 根據用戶ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							request.CustomerId);
					},
					"GetAuthByCustomerId");

				// 如果找不到對應的用戶認證信息，則返回無效的雙重認證確認狀態的錯誤DTO
				if (auth == null)
				{
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmInvalidState, "Authentication record not found.");
				}

				// 檢查用戶認證信息的雙重認證狀態是否為待確認，密鑰是否存在，以及待確認過期時間是否有效，如果不符合條件，則返回無效的雙重認證確認狀態的錯誤DTO
				if (auth.TwoFactorStatus != (short)TwoFactorStatus.Pending ||
					string.IsNullOrWhiteSpace(auth.TwoFactorSecret) ||
					auth.TwoFactorPendingExpiresAt == null ||
					auth.TwoFactorPendingExpiresAt < now)
				{
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmInvalidState, "Invalid state for confirming two-factor authentication.");
				}

				// 使用 Totp 驗證用戶輸入的雙重認證代碼是否有效
				var codeIsValid = Totp.VerifyCodeFromBase64Secret(
					base64Secret: auth.TwoFactorSecret,
					code: request.TwoFactorCode,
					now: now);

				// 如果驗證失敗，則返回無效的雙重認證代碼的錯誤DTO
				if (!codeIsValid)
				{
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmCodeInvalid, "Invalid two-factor authentication code.");
				}

				// 在一個事務中執行以下操作：
				return await ExecuteInTxAsync<ConfirmEnableTwoFactorResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;

						// 更新用戶認證信息中的雙重認證狀態為啟用，並清除待確認過期時間，以完成雙重認證的啟用操作
						var updatedRows = await _customerAuthRepository.ConfirmEnableTwoFactor(
							request.CustomerId,
							auth.TwoFactorSecret!,
							now);

						// 如果更新用戶認證信息的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.TwoFactorConfirmUpdateFailed,
								"Failed to confirm enabling two-factor authentication.");
						}

						// 如果所有操作成功，則返回成功的確認啟用雙重認證結果DTO
						return ConfirmEnableTwoFactorResultDto.SuccessDto();
					},
					"ConfirmEnableTwoFactor");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(ConfirmEnableTwoFactor), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return ConfirmEnableTwoFactorResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(ConfirmEnableTwoFactor), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to confirm enabling two-factor authentication due to a database error.");
			}
		}

		/// <summary>
		/// 禁用雙重認證
		/// </summary>
		/// <param name="request">禁用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含操作結果和相關信息</returns>
		public Task<DisableTwoFactorResultDto> DisableTwoFactor(DisableTwoFactorRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(DisableTwoFactor),
				() => DisableTwoFactorCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<DisableTwoFactorResultDto> DisableTwoFactorCoreAsync(DisableTwoFactorRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorUserNotFound, "User not found.");
			}

			// 檢查雙重認證代碼是否為空或僅包含空白字符，如果是，則返回無效的雙重認證代碼的錯誤DTO
			if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
			{
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
			}

			try
			{
				// 根據用戶ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							request.CustomerId);
					},
					"GetAuthByCustomerId");

				// 如果找不到對應的用戶認證信息，則返回用戶未找到的錯誤DTO
				if (auth == null)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorUserNotFound, "User not found.");
				}

				// 如果雙重認證未啟用，則返回無效的雙重認證狀態的錯誤DTO
				if (auth.TwoFactorStatus != (short)TwoFactorStatus.Enabled)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorInvalidState, "Two-factor authentication is not enabled.");
				}

				// 如果雙重認證的 secret 為空，則返回無效的雙重認證狀態的錯誤DTO
				if (string.IsNullOrEmpty(auth.TwoFactorSecret))
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorInvalidState, "Two-factor authentication secret is missing.");
				}

				var now = _timeProvider.Now();

				// 使用 Totp 驗證用戶輸入的雙重認證代碼是否有效
				var codeIsValid = Totp.VerifyCodeFromBase64Secret(
					base64Secret: auth.TwoFactorSecret,
					code: request.TwoFactorCode,
					now: now);

				// 如果驗證失敗，則返回無效的雙重認證代碼的錯誤DTO
				if (!codeIsValid)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
				}

				// 在一個事務中執行以下操作：
				return await ExecuteInTxAsync<DisableTwoFactorResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						_customerSessionRepository.DataAccess = dataAccess;
						_refreshTokenRepository.DataAccess = dataAccess;

						// 更新用戶認證信息中的雙重認證狀態為禁用，並清除雙重認證的 secret
						var updatedRows = await _customerAuthRepository.DisableTwoFactor(
							request.CustomerId,
							now);

						// 如果更新用戶認證信息的操作未成功（返回的受影響行數不等於1），則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.DisableTwoFactorFailed,
								"Failed to disable two-factor authentication.");
						}

						// 使所有用戶會話無效，以防止舊的會話被濫用
						await _customerSessionRepository.InvalidateAllSessions(
							request.CustomerId,
							now);

						// 撤銷用戶的所有刷新令牌，以防止舊的刷新令牌被濫用
						await _refreshTokenRepository.RevokeAllRefreshTokens(
							request.CustomerId,
							now);

						// 如果所有操作成功，則返回成功的禁用雙重認證結果DTO
						return DisableTwoFactorResultDto.SuccessDto();
					},
					"DisableTwoFactor");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(DisableTwoFactor), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return DisableTwoFactorResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(DisableTwoFactor), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to disable two-factor authentication due to a database error.");
			}
		}

		/// <summary>
		/// 用戶登錄
		/// </summary>
		/// <param name="loginRequest">登錄模型</param>
		/// <returns>登錄結果DTO</returns>
		public Task<LoginResultDto> Login(LoginRequest loginRequest)
		{
			return ExecuteWithLoggingAsync(
				nameof(Login),
				() => LoginCoreAsync(loginRequest),
				result => (result.Success, result.Code));
		}

		private async Task<LoginResultDto> LoginCoreAsync(LoginRequest loginRequest)
		{
			// 檢查登錄請求是否為空，或者登錄ID或密碼是否為空或僅包含空白字符，如果是，則返回登錄ID和密碼為必填的錯誤DTO
			if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.LoginId) || string.IsNullOrWhiteSpace(loginRequest.Password))
			{
				return LoginResultDto.Fail((int)AuthErrorCode.LoginIdOrPasswordRequired, "Login ID and password are required.");
			}

			var now = _timeProvider.Now();

			try
			{
				// 根據登錄ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByLoginId(loginRequest.LoginId);
					},
					"GetAuthByLoginId");

				// 如果找不到對應的用戶認證信息，則返回登錄ID無效的錯誤DTO
				if (auth == null)
				{
					return LoginResultDto.Fail((int)AuthErrorCode.LoginIdNotFound, "Invalid login ID.");
				}

				// 如果用戶帳號已被鎖定，則返回帳號已鎖定的錯誤DTO
				if (auth.LockedUntil > _timeProvider.Now())
				{
					return LoginResultDto.Fail((int)AuthErrorCode.AccountLocked, "Account is locked.");
				}

				// 比较輸入的密碼與數據庫中存儲的密碼哈希值是否匹配，如果不匹配，則進行失敗計數和鎖定處理
				if (!_cryptoRepository.Verify(auth.PasswordHash, loginRequest.Password))
				{
					// 如果密碼驗證失敗，則增加失敗計數
					if (auth.FailedCount >= 5)
					{
						// 执行鎖定帳號的操作，將帳號鎖定一段時間（例如1天）
						var lockedRows = await ExecuteInTxAsync<int>(
							async dataAccess =>
							{
								_customerAuthRepository.DataAccess = dataAccess;
								return await _customerAuthRepository.Lock(
									auth.CustomerId, now, now.AddDays(1));
							},
							"Lock");

						// 如果密码验证失败但是锁定操作未成功（返回的受影响行数为0），则返回锁定账户失败的错误DTO
						if (lockedRows == 0)
						{
							_logger.LogError("{Operation} could not lock the account. ErrorCode={ErrorCode}",nameof(Login),(int)AuthErrorCode.LockAccountFailed);
							return LoginResultDto.Fail((int)AuthErrorCode.LockAccountFailed, "Failed to lock account after multiple failed login attempts.");
						}
					}
					else
					{
						// 执行更新失敗計數的操作，將失敗計數增加1
						var updatedRows = await ExecuteInTxAsync<int>(
							async dataAccess =>
							{
								_customerAuthRepository.DataAccess = dataAccess;
								return await _customerAuthRepository.UpdateFailedCount(
									auth.CustomerId, now);
							},
							"UpdateFailedCount");

						// 如果更新失敗計數的操作未成功（返回的受影響行數為0），則返回更新失敗計數失敗的錯誤DTO
						if (updatedRows == 0)
						{
							_logger.LogError("{Operation} could not update the failed login count. ErrorCode={ErrorCode}",nameof(Login),(int)AuthErrorCode.FailedCountUpdateFailed);
							return LoginResultDto.Fail((int)AuthErrorCode.FailedCountUpdateFailed, "Failed to update failed login attempt count.");
						}
					}

					// 返回密碼無效的錯誤DTO
					return LoginResultDto.Fail((int)AuthErrorCode.PasswordInvalid, "Invalid password.");
				}

				// 如果密碼驗證成功，則重置失敗計數和鎖定狀態
				if (auth.TwoFactorStatus == (int)TwoFactorStatus.Enabled)
				{
					// 如果雙重認證已啟用，則檢查是否提供了雙重認證代碼，如果未提供，則返回需要雙重認證的結果DTO
					if (string.IsNullOrWhiteSpace(loginRequest.TwoFactorCode))
					{
						return LoginResultDto.RequireTwoFactor((int)AuthErrorCode.TwoFactorRequired);
					}

					// 如果提供了雙重認證代碼，則使用 Totp 驗證用戶輸入的雙重認證代碼是否有效
					var codeIsValid = Totp.VerifyCodeFromBase64Secret(
						base64Secret: auth.TwoFactorSecret!,
						code: loginRequest.TwoFactorCode,
						now: now);

					// 如果驗證失敗，則返回無效的雙重認證代碼的錯誤DTO
					if (!codeIsValid)
					{
						return LoginResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
					}
				}

				switch (loginRequest.Type)
				{
					case AuthType.Session:
						{
							// 当使用的是 Session 类型的认证时，创建一个新的会话，并返回包含会话ID的成功结果DTO
							return await ExecuteInTxAsync<LoginResultDto>(
								async dataAccess =>
								{
									_customerSessionRepository.DataAccess = dataAccess;

									var idleExpiresAt = now.Add(_sessionOption.IdleTimeout);
									var absoluteExpiresAt = now.Add(_sessionOption.AbsoluteLifetime);
									var expiresAt = idleExpiresAt <= absoluteExpiresAt
										? idleExpiresAt
										: absoluteExpiresAt;

									// 创建新的会话
									var sessionId = await _customerSessionRepository.CreateSession(
										auth.CustomerId,
										loginRequest.Device,
										loginRequest.Ip,
										now,
										expiresAt);

									// 如果创建会话失败（返回的会话ID为null或空），则抛出异常
									if (sessionId == null || sessionId == Guid.Empty)
									{
										throw new AuthOperationException(
											AuthErrorCode.CreateSessionFailed,
											"Failed to create session.");
									}

									// 返回包含会话ID的成功结果DTO
									return LoginResultDto.SuccessWithSession(sessionId.Value);
								},
								"CreateSession");
						}

					case AuthType.JWT:
						{
							// 创建访问令牌和刷新令牌，以及刷新令牌的哈希值，以便在数据库中存储
							var accessToken = _jwtProvider.CreateAccessToken(auth.CustomerId, auth.TokenVersion);
							var refreshToken = _jwtProvider.CreateRefreshToken();
							var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(refreshToken);

							// 当使用的是 JWT 类型的认证时，存储刷新令牌，并返回包含访问令牌和刷新令牌的成功结果DTO
							return await ExecuteInTxAsync<LoginResultDto>(
								async dataAccess =>
								{
									_refreshTokenRepository.DataAccess = dataAccess;

									// 存储刷新令牌，设置刷新令牌的绝对过期时间为当前时间加上配置的刷新令牌生命周期
									var tokenId = await _refreshTokenRepository.StoreRefreshToken(
										auth.CustomerId,
										refreshTokenHash,
										now,
										now.Add(_jwtProvider.RefreshTokenLifetime),
										loginRequest.Device,
										loginRequest.Ip,
										auth.TokenVersion);

									// 如果存储刷新令牌失败（返回的令牌ID为null或空），则抛出异常
									if (tokenId == null || tokenId == Guid.Empty)
									{
										throw new AuthOperationException(
											AuthErrorCode.CreateRefreshTokenFailed,
											"Failed to create refresh token.");
									}

									// 返回包含访问令牌和刷新令牌的成功结果DTO
									return LoginResultDto.SuccessWithJwt(accessToken, refreshToken);
								},
								"StoreRefreshToken");
						}

					// 如果认证类型无效，则返回无效的认证类型的错误DTO
					default:
						return LoginResultDto.Fail((int)AuthErrorCode.AuthTypeInvalid, "Invalid authentication type.");
				}
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(Login), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return LoginResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(Login), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return LoginResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Login failed due to a database error.");
			}
		}

		/// <summary>
		/// 用戶登出
		/// </summary>
		/// <param name="request">登出請求，包含用戶ID</param>
		/// <returns>取消的会话或令牌数量，null表示用户不存在</returns>
		public Task<LogoutResultDto> Logout(LogoutRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(Logout),
				() => LogoutCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<LogoutResultDto> LogoutCoreAsync(LogoutRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return LogoutResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound, "User not found.");
			}

			try
			{
				// 根據用戶ID從數據庫中查找對應的用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							request.CustomerId);
					},
					"GetAuthByCustomerId");

				// 如果找不到對應的用戶認證信息或是用户认证信息已被删除，則返回用戶未找到的錯誤DTO
				if (auth == null || auth.IsDeleted)
				{
					return LogoutResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound, "User not found.");
				}

				var now = _timeProvider.Now();

				return await ExecuteInTxAsync<LogoutResultDto>(
					async dataAccess =>
					{
						_customerSessionRepository.DataAccess = dataAccess;
						_refreshTokenRepository.DataAccess = dataAccess;

						// 使所有用戶會話無效，以防止舊的會話被濫用
						await _customerSessionRepository.InvalidateAllSessions(
							request.CustomerId,
							now);

						// 撤銷用戶的所有刷新令牌，以防止舊的刷新令牌被濫用
						await _refreshTokenRepository.RevokeAllRefreshTokens(
							request.CustomerId,
							now);

						// 如果所有操作成功，則返回成功的登出結果DTO
						return LogoutResultDto.SuccessDto();
					},
					"Logout");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(Logout), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return LogoutResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(Logout), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return LogoutResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to logout due to a database error.");
			}
		}

		/// <summary>
		/// 登出當前設備（使當前會話或令牌無效）
		/// </summary>
		/// <param name="request">登出當前設備請求，包含用戶ID和當前設備的識別信息（如會話ID或刷新令牌）</param>
		/// <returns>任务结果DTO，包含操作结果和相關信息</returns>
		public Task<LogoutCurrentDeviceResultDto> LogoutCurrentDevice(LogoutCurrentDeviceRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(LogoutCurrentDevice),
				() => LogoutCurrentDeviceCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<LogoutCurrentDeviceResultDto> LogoutCurrentDeviceCoreAsync(LogoutCurrentDeviceRequest request)
		{
			// 檢查請求是否為空或用戶ID是否小於等於0，如果是，則返回用戶未找到的錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound, "User not found.");
			}

			var now = _timeProvider.Now();

			try
			{
				switch (request.AuthType)
				{
					case AuthType.Session:
						{
							// 如果使用的是 Session 類型的認證，則檢查請求中是否包含有效的會話ID，如果無效，則返回會話未找到的錯誤DTO
							if (request.SessionId is not Guid sessionId || sessionId == Guid.Empty)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionNotFound, "Invalid session ID.");
							}

							// 根據會話ID從數據庫中查找對應的會話實體
							var session = await ExecuteDbAsync<CustomerSessionEntity?>(
								async dataAccess =>
								{
									_customerSessionRepository.DataAccess = dataAccess;
									return await _customerSessionRepository.GetSessionById(sessionId);
								},
								"GetSessionById");

							// 如果找不到對應的會話實體，則返回會話未找到的錯誤DTO
							if (session == null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionNotFound, "Session not found.");
							}

							// 如果會話不屬於請求中的用戶ID，則返回會話無效的錯誤DTO
							if (session.UserId != request.CustomerId)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionInvalid, "Session does not belong to the user.");
							}

							// 如果會話已經無效，則返回會話已經無效的錯誤DTO
							if (!session.IsValid)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionInvalid, "Session is already invalid.");
							}

							// 在一個事務中執行以下操作：
							return await ExecuteInTxAsync<LogoutCurrentDeviceResultDto>(
								async dataAccess =>
								{
									_customerSessionRepository.DataAccess = dataAccess;

									// 使當前會話無效，以防止舊的會話被濫用
									var affectedRows = await _customerSessionRepository.InvalidateSession(
										sessionId, now);

									// 如果無法使會話無效，則拋出異常
									if (affectedRows != 1)
									{
										throw new AuthOperationException(
											AuthErrorCode.SessionInvalid,
											"Failed to invalidate session.");
									}

									// 如果所有操作成功，則返回成功的登出當前設備結果DTO
									return LogoutCurrentDeviceResultDto.SuccessDto();
								},
								"InvalidateSession");
						}

					case AuthType.JWT:
						{
							// 如果使用的是 JWT 類型的認證，則檢查請求中是否包含有效的刷新令牌，如果無效，則返回刷新令牌無效的錯誤DTO
							if (string.IsNullOrWhiteSpace(request.RefreshToken))
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Invalid refresh token.");
							}

							// 生成刷新令牌的哈希值，以便在數據庫中查找對應的刷新令牌實體
							var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(request.RefreshToken);

							// 根據刷新令牌的哈希值從數據庫中獲取對應的刷新令牌實體
							var refreshToken = await ExecuteDbAsync<RefreshTokenEntity?>(
								async dataAccess =>
								{
									_refreshTokenRepository.DataAccess = dataAccess;
									return await _refreshTokenRepository.GetRefreshTokenByTokenHash(
										refreshTokenHash);
								},
								"GetRefreshTokenByTokenHash");

							// 如果找不到對應的刷新令牌實體，則返回刷新令牌未找到的錯誤DTO
							if (refreshToken == null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenNotFound, "Refresh token not found.");
							}

							// 如果刷新令牌不屬於請求中的用戶ID，則返回刷新令牌無效的錯誤DTO
							if (refreshToken.UserId != request.CustomerId)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Refresh token does not belong to the user.");
							}

							// 如果刷新令牌已經被撤銷，則返回刷新令牌已經被撤銷的錯誤DTO
							if (refreshToken.RevokedAt != null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenRevoked, "Refresh token is already revoked.");
							}

							// 如果刷新令牌已經過期，則返回刷新令牌已經過期的錯誤DTO
							if (refreshToken.ExpiresAt < now)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenExpired, "Refresh token has expired.");
							}

							// 在一個事務中執行以下操作：
							return await ExecuteInTxAsync<LogoutCurrentDeviceResultDto>(
								async dataAccess =>
								{
									_refreshTokenRepository.DataAccess = dataAccess;

									// 撤銷當前刷新令牌，以防止舊的刷新令牌被濫用
									var affectedRows = await _refreshTokenRepository.RevokeRefreshToken(
										refreshToken.TokenId, now);

									// 如果無法撤銷刷新令牌，則拋出異常
									if (affectedRows != 1)
									{
										throw new AuthOperationException(
											AuthErrorCode.RefreshTokenInvalid,
											"Failed to revoke refresh token.");
									}

									// 如果所有操作成功，則返回成功的登出當前設備結果DTO
									return LogoutCurrentDeviceResultDto.SuccessDto();
								},
								"RevokeRefreshToken");
						}

					// 如果認證類型無效，則返回無效的認證類型的錯誤DTO
					default:
						return LogoutCurrentDeviceResultDto.Fail(
							(int)AuthErrorCode.AuthTypeInvalid,
							"Invalid authentication type.");
				}
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(LogoutCurrentDevice), (int)ex.ErrorCode);
				// 如果在執行操作時發生自定義的身份驗證操作異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return LogoutCurrentDeviceResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(LogoutCurrentDevice), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，包含無法操作資料庫的錯誤代碼和消息
				return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to logout current device due to a database error.");
			}
		}

		/// <summary>
		/// 刷新令牌
		/// </summary>
		/// <param name="request">刷新令牌请求，包含刷新令牌字符串</param>
		/// <returns>刷新结果DTO，包含新的访问令牌和刷新令牌</returns>
		public Task<RefreshTokenDto> RefreshToken(RefreshTokenRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(RefreshToken),
				() => RefreshTokenCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<RefreshTokenDto> RefreshTokenCoreAsync(RefreshTokenRequest request)
		{
			// 驗證輸入的刷新令牌是否為空或僅包含空白字符，如果是，則返回錯誤DTO
			if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
			{
				return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Invalid refresh token.");
			}

			var now = _timeProvider.Now();

			// 生成刷新令牌的哈希值，以便在數據庫中查找對應的刷新令牌實體
			var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(request.RefreshToken);

			try
			{
				// 根據刷新令牌的哈希值從數據庫中獲取對應的刷新令牌實體
				var refreshToken = await ExecuteDbAsync<RefreshTokenEntity?>(
					async dataAccess =>
					{
						_refreshTokenRepository.DataAccess = dataAccess;
						return await _refreshTokenRepository.GetRefreshTokenByTokenHash(
							refreshTokenHash);
					},
					"GetRefreshTokenByTokenHash");

				// 驗證刷新令牌實體是否存在，如果不存在，則返回錯誤DTO
				if (refreshToken == null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenNotFound, "Refresh token not found.");
				}

				// 验证刷新令牌是否已经过期，如果已经过期，则撤销该刷新令牌并返回错误DTO
				if (refreshToken.ExpiresAt < now)
				{
					// 撤销过期的刷新令牌
					await ExecuteInTxAsync<int>(
						async dataAccess =>
						{
							_refreshTokenRepository.DataAccess = dataAccess;
							return await _refreshTokenRepository.RevokeRefreshToken(
								refreshToken.TokenId,
								now);
						},
						"RevokeExpiredRefreshToken");

					// 返回刷新令牌过期的错误DTO
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenExpired, "Refresh token has expired.");
				}

				// 验证刷新令牌是否已经被撤销，如果已经被撤销，则返回错误DTO
				if (refreshToken.RevokedAt != null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenRevoked, "Refresh token has been revoked.");
				}

				// 根据刷新令牌的用户ID从数据库中获取对应的用户认证信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							refreshToken.UserId);
					},
					"GetAuthByCustomerId");

				// 验证用户认证信息是否存在，如果不存在，则返回错误DTO
				if (auth == null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenUserNotFound, "User not found for the refresh token.");
				}

				// 验证用户账户是否已被删除，如果已被删除，则返回错误DTO
				if (auth.IsDeleted)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenAuthInvalid, "User account has been deleted.");
				}

				// 验证用户认证信息的认证类型是否为JWT，如果不是，则返回错误DTO
				if (auth.AuthType != (short)AuthType.JWT)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenAuthInvalid, "Invalid authentication type for the refresh token.");
				}

				// 验证刷新令牌的版本是否与用户认证信息的版本匹配，如果不匹配，则返回错误DTO
				if (auth.TokenVersion != refreshToken.TokenVersion)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenTokenVersionMismatch, "Token version mismatch. The refresh token is no longer valid due to password change or other security events.");
				}

				// 生成新的访问令牌和刷新令牌
				var newAccessToken = _jwtProvider.CreateAccessToken(
					auth.CustomerId,
					auth.TokenVersion);

				// 生成新的刷新令牌和其哈希值
				var newRefreshToken = _jwtProvider.CreateRefreshToken();

				// 生成新的刷新令牌的哈希值
				var newRefreshTokenHash = _jwtProvider.CreateRefreshTokenHash(newRefreshToken);

				// 在一个事务中撤销旧的刷新令牌并存储新的刷新令牌
				return await ExecuteInTxAsync<RefreshTokenDto>(
					async dataAccess =>
					{
						_refreshTokenRepository.DataAccess = dataAccess;

						// 撤销旧的刷新令牌
						var revokedRows = await _refreshTokenRepository.RevokeRefreshToken(
							refreshToken.TokenId,
							now);

						// 验证撤销操作是否成功，如果不成功，则抛出异常
						if (revokedRows != 1)
						{
							throw new AuthOperationException(AuthErrorCode.RefreshTokenRotationFailed, "Failed to rotate refresh token.");
						}

						// 存储新的刷新令牌
						var newTokenId = await _refreshTokenRepository.StoreRefreshToken(
							refreshToken.UserId,
							newRefreshTokenHash,
							now,
							now.Add(_jwtProvider.RefreshTokenLifetime),
							refreshToken.MetaDevice,
							refreshToken.MetaIP,
							refreshToken.TokenVersion);

						// 验证存储操作是否成功，如果不成功，则抛出异常
						if (newTokenId == null || newTokenId == Guid.Empty)
						{
							throw new AuthOperationException(AuthErrorCode.RefreshTokenRotationFailed, "Failed to rotate refresh token.");
						}

						// 返回成功的刷新令牌DTO，包含新的访问令牌和刷新令牌
						return RefreshTokenDto.SuccessDto(newAccessToken, newRefreshToken);
					},
					"RotateRefreshToken");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(RefreshToken), (int)ex.ErrorCode);
				// 如果在刷新令牌操作中发生自定义异常，则返回错误DTO，包含异常的错误代码和消息
				return RefreshTokenDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(RefreshToken), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在执行数据库操作时发生异常，则返回错误DTO，表示无法操作数据库
				return RefreshTokenDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to refresh token due to a database error.");
			}
		}

		/// <summary>
		/// 重置密碼
		/// </summary>
		/// <param name="request">重置密碼請求，包含用戶ID和ip地址或是装置信息</param>
		/// <returns>重置密碼結果DTO，包含操作結果和相關信息</returns>
		public Task<ResetPasswordResultDto> ResetPassword(ResetPasswordRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(ResetPassword),
				() => ResetPasswordCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<ResetPasswordResultDto> ResetPasswordCoreAsync(ResetPasswordRequest request)
		{
			// 驗證輸入的請求是否為空或登錄ID是否為空白，如果是，則返回錯誤DTO
			if (request == null || string.IsNullOrWhiteSpace(request.LoginId))
			{
				return ResetPasswordResultDto.Fail(
					(int)AuthErrorCode.ResetPasswordLoginIdRequired,
					"Login ID is required.");
			}

			// 成功消息的常量字符串，用於在操作成功時返回給用戶
			const string successMessage =
				"If the account exists, a password reset email has been sent.";

			try
			{
				// 根據登錄ID從數據庫中獲取用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByLoginId(request.LoginId);
					},
					"GetAuthByLoginId");

				// 如果用戶認證信息不存在或已被刪除，則返回成功DTO，避免洩露用戶是否存在的信息
				if (auth == null || auth.IsDeleted)
				{
					_logger.LogDebug(
						"{Operation} completed without submitting a reset email.",
						nameof(ResetPassword));

					return ResetPasswordResultDto.SuccessDto(successMessage);
				}

				// 根據用戶ID從數據庫中獲取用戶信息
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(auth.CustomerId);
					},
					"GetCustomerById");

				// 如果用戶信息不存在或電子郵件為空，則返回成功DTO，避免洩露用戶是否存在的信息
				if (customer == null || string.IsNullOrWhiteSpace(customer.Email))
				{
					_logger.LogDebug(
						"{Operation} completed without submitting a reset email.",
						nameof(ResetPassword));

					return ResetPasswordResultDto.SuccessDto(successMessage);
				}

				// 生成新的密碼重置令牌及其哈希值
				var passwordResetToken = _jwtProvider.CreatePasswordResetToken();
				var passwordResetTokenHash =
					_jwtProvider.CreatePasswordResetTokenHash(passwordResetToken);

				// 計算令牌的過期時間
				var now = _timeProvider.Now();
				var expiresAt = now.Add(_jwtProvider.PasswordResetTokenLifetime);

				// 在一個事務中撤銷舊的密碼重置令牌並存儲新的密碼重置令牌
				await ExecuteInTxAsync(
					async dataAccess =>
					{
						_passwordResetTokenRepository.DataAccess = dataAccess;

						// 没有旧令牌时，撤销操作影响零行是正常的。
						await _passwordResetTokenRepository.RevokeActiveResetTokens(
							auth.CustomerId,
							now);

						// 存储新的密码重置令牌
						var tokenId = await _passwordResetTokenRepository.StoreResetToken(
							auth.CustomerId,
							passwordResetTokenHash,
							now,
							expiresAt,
							request.Ip,
							request.Device);

						// 驗證存儲操作是否成功，如果不成功，則拋出異常
						if (tokenId == null || tokenId == Guid.Empty)
						{
							throw new AuthOperationException(
								AuthErrorCode.ResetPasswordFailed,
								"Failed to generate password reset token.");
						}
					},
					"StorePasswordResetToken");

				try
				{
					// 發送密碼重置電子郵件給用戶
					await _emailSender.SendResetPasswordEmailAsync(
						customer.Email,
						passwordResetToken,
						expiresAt);
				}
				catch (Exception)
				{
					_logger.LogError(
						"{Operation} could not send the reset email. ErrorCode={ErrorCode}",
						nameof(ResetPassword),
						(int)AuthErrorCode.ResetPasswordFailed);

					// 如果發送電子郵件失敗，則返回錯誤DTO
					return ResetPasswordResultDto.Fail(
						(int)AuthErrorCode.ResetPasswordFailed,
						"Failed to send password reset email.");
				}

				_logger.LogInformation(
					"{Operation} submitted a reset email successfully.",
					nameof(ResetPassword));

				// 返回成功DTO，告知用戶已發送密碼重置電子郵件
				return ResetPasswordResultDto.SuccessDto(successMessage);
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(ResetPassword), (int)ex.ErrorCode);

				// 如果在重置密碼操作中發生自定義異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return ResetPasswordResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError(
					"{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(ResetPassword),
					(int)AuthErrorCode.UnableToOperateDb);

				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，表示無法操作資料庫
				return ResetPasswordResultDto.Fail(
					(int)AuthErrorCode.UnableToOperateDb,
					"Failed to reset password due to a database error.");
			}
		}
		/// <summary>
		/// 更新會話的最後活動時間
		/// </summary>
		/// <param name="request">更新會話請求，包含會話ID</param>
		/// <returns>true:有效/false:无效</returns>
		public Task<TouchSessionResultDto> TouchSession(TouchSessionRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(TouchSession),
				() => TouchSessionCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<TouchSessionResultDto> TouchSessionCoreAsync(TouchSessionRequest request)
		{
			// 驗證輸入的請求是否為空或會話ID是否為空，如果是，則返回錯誤DTO
			if (request == null || request.SessionId == Guid.Empty)
			{
				return TouchSessionResultDto.Fail((int)AuthErrorCode.SessionTouchRejected, "Failed to touch session.");
			}

			// 計算新的過期時間，將當前時間加上配置的閒置超時值
			var now = _timeProvider.Now();

			try
			{
				// 在一個事務中更新會話的最後活動時間和過期時間
				return await ExecuteInTxAsync<TouchSessionResultDto>(
					async dataAccess =>
					{
						_customerSessionRepository.DataAccess = dataAccess;

						// 更新會話的最後活動時間和過期時間
						var affectedRows = await _customerSessionRepository.Touch(
							request.SessionId,
							now,
							_sessionOption.IdleTimeout,
							_sessionOption.AbsoluteLifetime);

						// 驗證更新操作是否成功，如果不成功，則拋出異常
						if (affectedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.SessionTouchRejected,
								"Failed to touch session.");
						}

						// 返回成功DTO，表示會話已成功更新
						return TouchSessionResultDto.SuccessDto();
					},
					"Touch");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(TouchSession), (int)ex.ErrorCode);
				// 如果在更新會話操作中發生自定義異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return TouchSessionResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(TouchSession), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，表示無法操作資料庫
				return TouchSessionResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to touch session due to a database error.");
			}
		}

		/// <summary>
		/// 更新認證類型
		/// </summary>
		/// <param name="request">更新認證類型請求，包含用戶ID和新的認證類型</param>
		/// <returns>更新認證類型結果DTO，包含操作結果和相關信息</returns>
		public Task<UpdateAuthTypeResultDto> UpdateAuthType(UpdateAuthTypeRequest request)
		{
			return ExecuteWithLoggingAsync(
				nameof(UpdateAuthType),
				() => UpdateAuthTypeCoreAsync(request),
				result => (result.Success, result.Code));
		}

		private async Task<UpdateAuthTypeResultDto> UpdateAuthTypeCoreAsync(UpdateAuthTypeRequest request)
		{
			// 驗證輸入的請求是否為空或用戶ID是否有效，如果是，則返回錯誤DTO
			if (request == null || request.CustomerId <= 0)
			{
				return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UpdateAuthTypeUserNotFound, "User not found.");
			}

			// 驗證輸入的認證類型是否有效，如果無效，則返回錯誤DTO
			if (!Enum.IsDefined(typeof(AuthType), request.AuthType))
			{
				return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.AuthTypeInvalid, "Invalid authentication type.");
			}

			try
			{
				// 根據用戶ID從數據庫中獲取用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(
							request.CustomerId);
					},
					"GetAuthByCustomerId");

				// 驗證用戶認證信息是否存在，如果不存在，則返回錯誤DTO
				if (auth == null)
				{
					return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UpdateAuthTypeUserNotFound, "User not found.");
				}

				// 驗證用戶認證信息的認證類型是否已經是新的認證類型，如果是，則返回成功DTO
				var newAuthType = (short)request.AuthType;
				if (auth.AuthType == newAuthType)
				{
					return UpdateAuthTypeResultDto.SuccessDto();
				}

				var now = _timeProvider.Now();

				// 在一個事務中更新用戶的認證類型，並使所有會話和刷新令牌無效
				return await ExecuteInTxAsync<UpdateAuthTypeResultDto>(
					async dataAccess =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						_refreshTokenRepository.DataAccess = dataAccess;
						_customerSessionRepository.DataAccess = dataAccess;

						// 更新用戶的認證類型
						var updatedRows = await _customerAuthRepository.UpdateAuthType(
							request.CustomerId,
							newAuthType,
							now);

						// 驗證更新操作是否成功，如果不成功，則拋出異常
						if (updatedRows != 1)
						{
							throw new AuthOperationException(
								AuthErrorCode.UpdateAuthTypeFailed,
								"Failed to update authentication type due to a transaction error.");
						}

						// 使所有會話無效
						await _customerSessionRepository.InvalidateAllSessions(
							request.CustomerId,
							now);

						// 使所有刷新令牌無效
						await _refreshTokenRepository.RevokeAllRefreshTokens(
							request.CustomerId,
							now);

						// 返回成功DTO
						return UpdateAuthTypeResultDto.SuccessDto();
					},
					"UpdateAuthTypeTransaction");
			}
			catch (AuthOperationException ex)
			{
				LogCaughtAuthFailure(nameof(UpdateAuthType), (int)ex.ErrorCode);
				// 如果在更新認證類型操作中發生自定義異常，則返回錯誤DTO，包含異常的錯誤代碼和消息
				return UpdateAuthTypeResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				_logger.LogError("{Operation} encountered a database error. ErrorCode={ErrorCode}",
					nameof(UpdateAuthType), (int)AuthErrorCode.UnableToOperateDb);
				// 如果在執行資料庫操作時發生異常，則返回錯誤DTO，表示無法操作資料庫
				return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to update authentication type due to a database error.");
			}
		}
		/// <summary>
		/// 根据认证错误码区分系统故障和业务拒绝。
		/// </summary>
		private static LogOutcome GetFailureOutcome(int errorCode)
		{
			return errorCode switch
			{
				(int)AuthErrorCode.UnableToOperateDb
					or (int)AuthErrorCode.ChangePasswordTransactionFailed
					or (int)AuthErrorCode.ConfirmResetPasswordFailed
					or (int)AuthErrorCode.TwoFactorBeginFailed
					or (int)AuthErrorCode.TwoFactorConfirmUpdateFailed
					or (int)AuthErrorCode.DisableTwoFactorFailed
					or (int)AuthErrorCode.LockAccountFailed
					or (int)AuthErrorCode.FailedCountUpdateFailed
					or (int)AuthErrorCode.CreateSessionFailed
					or (int)AuthErrorCode.CreateRefreshTokenFailed
					or (int)AuthErrorCode.RefreshTokenRotationFailed
					or (int)AuthErrorCode.ResetPasswordFailed
					or (int)AuthErrorCode.UpdateAuthTypeFailed => LogOutcome.Failed,
				_ => LogOutcome.Rejected
			};
		}

		/// <summary>
		/// 记录已捕获的认证操作异常。
		/// </summary>
		private void LogCaughtAuthFailure(string operation, int errorCode)
		{
			var level = GetFailureOutcome(errorCode) == LogOutcome.Failed
				? LogLevel.Error
				: LogLevel.Debug;

			_logger.Log(level, "{Operation} caught an authentication operation exception. ErrorCode={ErrorCode}",
				operation, errorCode);
		}

		/// <summary>
		/// 执行认证操作并记录开始、结束、耗时和结果。
		/// </summary>
		private async Task<TResult> ExecuteWithLoggingAsync<TResult>(
			string operation,
			Func<Task<TResult>> execute,
			Func<TResult, (bool Success, int Code)> getResult)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;
			int? errorCode = null;

			_logger.LogStart(operation: operation);

			try
			{
				var result = await execute();
				var (success, code) = getResult(result);

				if (success)
				{
					outcome = LogOutcome.Completed;
				}
				else
				{
					errorCode = code;
					outcome = GetFailureOutcome(code);
				}

				return result;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode, operation: operation);
			}
		}

	}

	public interface IAuthService
	{
		/// <summary>
		/// 用戶登出
		/// </summary>
		/// <param name="request">登出請求，包含用戶ID</param>
		/// <returns>取消的会话或令牌数量，null表示用户不存在</returns>
		Task<LogoutResultDto> Logout(LogoutRequest request);

		/// <summary>
		/// 用戶登錄
		/// </summary>
		/// <param name="loginModel">登錄模型</param>
		/// <returns>登錄結果DTO</returns>
		Task<LoginResultDto> Login(LoginRequest loginModel);

		/// <summary>
		/// 開始啟用雙重認證
		/// </summary>
		/// <param name="request">開始啟用雙重認證請求，包含用戶ID</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		Task<BeginTwoFactorResultDto> BeginEnableTwoFactor(BeginEnableTwoFactorRequest request);

		/// <summary>
		/// 確認啟用雙重認證
		/// </summary>
		/// <param name="request">確認啟用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		Task<ConfirmEnableTwoFactorResultDto> ConfirmEnableTwoFactor(ConfirmEnableTwoFactorRequest request);

		/// <summary>
		/// 更新會話的最後活動時間
		/// </summary>
		/// <param name="request">更新會話請求，包含會話ID</param>
		/// <returns>true:有效/false:无效</returns>
		Task<TouchSessionResultDto> TouchSession(TouchSessionRequest request);

		/// <summary>
		/// 刷新令牌
		/// </summary>
		/// <param name="request">刷新令牌请求，包含刷新令牌字符串</param>
		/// <returns>刷新结果DTO，包含新的访问令牌和刷新令牌</returns>
		Task<RefreshTokenDto> RefreshToken(RefreshTokenRequest request);

		/// <summary>
		/// 更改密碼
		/// </summary>
		/// <param name="request">更改密碼請求，包含用戶ID、舊密碼和新密碼</param>
		/// <returns>更改密碼結果DTO，包含操作結果和相關信息</returns>
		Task<ChangePasswordResultDto> ChangePassword(ChangePasswordRequest request);

		/// <summary>
		/// 更新認證類型
		/// </summary>
		/// <param name="request">更新認證類型請求，包含用戶ID和新的認證類型</param>
		/// <returns>更新認證類型結果DTO，包含操作結果和相關信息</returns>
		Task<UpdateAuthTypeResultDto> UpdateAuthType(UpdateAuthTypeRequest request);

		/// <summary>
		/// 禁用雙重認證
		/// </summary>
		/// <param name="request">禁用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含操作結果和相關信息</returns>
		Task<DisableTwoFactorResultDto> DisableTwoFactor(DisableTwoFactorRequest request);

		/// <summary>
		/// 登出當前設備（使當前會話或令牌無效）
		/// </summary>
		/// <param name="request">登出當前設備請求，包含用戶ID和當前設備的識別信息（如會話ID或刷新令牌）</param>
		/// <returns>任务结果DTO，包含操作结果和相關信息</returns>
		Task<LogoutCurrentDeviceResultDto> LogoutCurrentDevice(LogoutCurrentDeviceRequest request);

		/// <summary>
		/// 重置密碼
		/// </summary>
		/// <param name="request">重置密碼請求，包含用戶ID和ip地址或是装置信息</param>
		/// <returns>重置密碼結果DTO，包含操作結果和相關信息</returns>
		Task<ResetPasswordResultDto> ResetPassword(ResetPasswordRequest request);

		/// <summary>
		/// 確認重置密碼
		/// </summary>
		/// <param name="request">確認重置密碼請求，包含用戶ID、重置令牌和新密碼</param>
		/// <returns>確認重置密碼結果DTO，包含操作結果和相關信息</returns>
		Task<ConfirmResetPasswordResultDto> ConfirmResetPassword(ConfirmResetPasswordRequest request);
	}
}
