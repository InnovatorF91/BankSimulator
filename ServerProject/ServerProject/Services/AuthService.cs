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
			IEmailSender emailSender)
			: base(connectionFactory)
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
		}

		/// <summary>
		/// 開始啟用雙重認證
		/// </summary>
		/// <param name="request">開始啟用雙重認證請求，包含用戶ID</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public async Task<BeginTwoFactorResultDto> BeginEnableTwoFactor(BeginEnableTwoFactorRequest request)
		{
			if (request.CustomerId <= 0)
			{
				// ❌ 無效的用戶ID -> 返回錯誤 DTO
				return BeginTwoFactorResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			var now = _timeProvider.Now();

			// ✅ 服务端生成 secret
			string secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));

			var expiresAt = now.Add(_twoFactorOption.TwoFactorPendingExpiresAt);

			try
			{
				var rows = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
		        	// 更新資料庫中的雙重認證狀態為 Pending，並存儲 secret 和過期時間
		        	return await _customerAuthRepository.BeginEnableTwoFactor(request.CustomerId, secret, now, expiresAt);
		        }, "BeginEnableTwoFactor");

				if (rows == 0)
				{
					// 如果未成功更新資料庫，则抛出异常
					return BeginTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorBeginFailed, "Failed to begin enabling two-factor authentication.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回錯誤 DTO
				return BeginTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to begin enabling two-factor authentication due to a database error.");
			}

			//生成 otpauth URI（用于前端生成二维码）
			var otpauth = Totp.BuildTotpUri
				(
				issuer: "BankSimulator",
				accountName: request.CustomerId.ToString(),
				base64Secret: secret
				);

			return BeginTwoFactorResultDto.SuccessDto(secret, expiresAt, otpauth);
		}

		/// <summary>
		/// 更改密碼
		/// </summary>
		/// <param name="request">更改密碼請求，包含用戶ID、舊密碼和新密碼</param>
		/// <returns>更改密碼結果DTO，包含操作結果和相關信息</returns>
		public async Task<ChangePasswordResultDto> ChangePassword(ChangePasswordRequest request)
		{
			// 检查输入的用户ID是否有效，如果无效，则返回错误DTO
			if (request.CustomerId <= 0)
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			// 检查输入的旧密码是否为空，如果是，则返回错误DTO
			if (string.IsNullOrWhiteSpace(request.OldPassword))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordOldPasswordRequired, "Old password is required.");
			}

			// 检查输入的新密码是否为空，如果是，则返回错误DTO
			if (string.IsNullOrWhiteSpace(request.NewPassword))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordRequired, "New password is required.");
			}

			// 检查新旧密码是否相同，如果是，则返回错误DTO
			if (request.OldPassword.Equals(request.NewPassword))
			{
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
			}

			// 根据用户ID从数据库中获取用户认证信息
			var passwordValidation = _passwordValidator.Validate(request.NewPassword);
			if (!passwordValidation.IsValid)
			{
				// 如果新密码不符合安全规范，则返回错误DTO
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordPolicyInvalid, passwordValidation.Message);
			}

			try
			{
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
			async (dataAccess) =>
			{
				_customerAuthRepository.DataAccess = dataAccess;
				return await _customerAuthRepository.GetAuthByCustomerId(request.CustomerId);
			},
			"GetAuthByCustomerId"
		);

				// 验证用户认证信息是否存在，如果不存在，则返回错误DTO
				if (auth == null)
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
				}

				// 验证旧密码是否正确，如果验证失败，则返回错误DTO
				if (!_cryptoRepository.Verify(auth.PasswordHash, request.OldPassword))
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordOldPasswordInvalid, "Old password is invalid.");
				}

				// 验证新密码是否与旧密码相同，如果是，则返回错误DTO
				if (_cryptoRepository.Verify(auth.PasswordHash, request.NewPassword))
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
				}

				// 生成新密码的哈希值
				var newPasswordHash = _cryptoRepository.Hash(request.NewPassword, HashProfile.UserPassword);

				// 取得当前时间
				var now = _timeProvider.Now();

				var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
					_refreshTokenRepository.DataAccess = dataAccess;
					_customerSessionRepository.DataAccess = dataAccess;
					// 在一个事务中执行以下操作：
					// 撤销用户的所有刷新令牌
					_ = await _refreshTokenRepository.RevokeAllRefreshTokens(request.CustomerId, now);
					// 使用户的所有会话无效
					_ = await _customerSessionRepository.InvalidateAllSessions(request.CustomerId, now);
					// 更新用户认证信息中的密码哈希值和令牌版本
					return await _customerAuthRepository.UpdatePasswordHashAndBumpTokenVersion(request.CustomerId, newPasswordHash, now);
				}, "ChangePasswordTransaction");

				// 如果事务中的任何一个操作未成功（返回0行），则返回错误DTO
				if (count == 0)
				{
					return ChangePasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordTransactionFailed, "Failed to change password due to a transaction error.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return ChangePasswordResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to change password due to a database error.");
			}

			// 如果所有操作成功，则返回成功DTO
			return ChangePasswordResultDto.SuccessDto();
		}

		/// <summary>
		/// 確認重置密碼
		/// </summary> 
		/// <param name="request">確認重置密碼請求，包含用戶ID、重置令牌和新密碼</param>
		/// <returns>確認重置密碼結果DTO，包含操作結果和相關信息</returns>
		public async Task<ConfirmResetPasswordResultDto> ConfirmResetPassword(ConfirmResetPasswordRequest request)
		{
			// 驗證輸入的重置令牌是否為空或僅包含空白字符，如果是，則返回錯誤DTO
			if (string.IsNullOrWhiteSpace(request.ResetToken))
			{
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenRequired, "Reset token is required.");
			}

			// 驗證輸入的新密碼是否為空或僅包含空白字符，如果是，則返回錯誤DTO
			if (string.IsNullOrWhiteSpace(request.NewPassword))
			{
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordNewPasswordRequired, "New password is required.");
			}

			// 取得當前時間
			var now = _timeProvider.Now();

			// 对新密码进行安全规范验证，如果不符合规范，则返回错误DTO
			var passwordValidation = _passwordValidator.Validate(request.NewPassword);
			if (!passwordValidation.IsValid)
			{
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordNewPasswordPolicyInvalid, passwordValidation.Message);
			}

			// 生成输入的重置令牌的哈希值，以便与数据库中存储的哈希值进行比较
			var inputResetTokenHash = _jwtProvider.CreatePasswordResetTokenHash(request.ResetToken);

			try
			{
				// 根据输入的重置令牌哈希值从数据库中获取对应的密码重置令牌实体
				var resetTokenEntity = await ExecuteDbAsync<PasswordResetTokenEntity?>(
					async (dataAccess) =>
					{
						_passwordResetTokenRepository.DataAccess = dataAccess;
						return await _passwordResetTokenRepository.GetResetTokenByTokenHash(inputResetTokenHash);
					},
					"GetPasswordResetTokenByHash"
				);

				if (resetTokenEntity == null)
				{
					// 如果未找到对应的密码重置令牌实体，则返回错误DTO
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenInvalid, "Invalid reset token.");
				}

				if (resetTokenEntity.UsedAt != null)
				{
					// 如果密码重置令牌已被使用，则返回错误DTO
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenInvalid, "Reset token has already been used.");
				}

				if (resetTokenEntity.ExpiresAt < now)
				{
					// 如果密码重置令牌已过期，则返回错误DTO
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordTokenExpired, "Reset token has expired.");
				}

				// 根据客户ID从数据库中获取对应的客户认证实体，以便验证新密码是否与旧密码相同
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(resetTokenEntity.CustomerId);
					},
					"GetAuthByCustomerId"
				);

				// 验证客户认证实体是否存在且未被删除，如果验证失败，则返回错误DTO
				if (auth == null || auth.IsDeleted)
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordUserNotFound, "User not found.");
				}

				// 验证新密码是否与旧密码相同，如果是，则返回错误DTO
				if (_cryptoRepository.Verify(auth.PasswordHash, request.NewPassword))
				{
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld, "New password cannot be the same as the old password.");
				}

				// 生成新密码的哈希值，以便更新数据库中的客户认证信息
				var newPasswordHash = _cryptoRepository.Hash(request.NewPassword, HashProfile.UserPassword);

				// 在一个事务中执行以下操作：
				// 1. 将密码重置令牌标记为已使用
				// 2. 撤销用户的所有刷新令牌
				// 3. 使用户的所有会话无效
				// 4. 更新用户认证信息中的密码哈希值和令牌版本
				var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
					_passwordResetTokenRepository.DataAccess = dataAccess;
					_customerSessionRepository.DataAccess = dataAccess;
					_refreshTokenRepository.DataAccess = dataAccess;

					var markUsedCount = await _passwordResetTokenRepository.MarkResetTokenAsUsed(resetTokenEntity.TokenId, now);
					if (markUsedCount == 0)
					{
						return 0;
					}
					_ = await _refreshTokenRepository.RevokeAllRefreshTokens(resetTokenEntity.CustomerId, now);
					_ = await _customerSessionRepository.InvalidateAllSessions(resetTokenEntity.CustomerId, now);
					return await _customerAuthRepository.UpdatePasswordHashAndBumpTokenVersion(resetTokenEntity.CustomerId, newPasswordHash, now);

				}, "ConfirmResetPasswordTransaction");

				if (count == 0)
				{
					// 如果事务中的任何一个操作未成功（返回0行），则返回错误DTO
					return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.ConfirmResetPasswordFailed, "Failed to confirm reset password due to a transaction error.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return ConfirmResetPasswordResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to confirm reset password due to a database error.");
			}
			// 如果所有操作成功，则返回成功DTO
			return ConfirmResetPasswordResultDto.SuccessDto();
		}

		/// <summary>
		/// 確認啟用雙重認證
		/// </summary>
		/// <param name="request">確認啟用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public async Task<ConfirmEnableTwoFactorResultDto> ConfirmEnableTwoFactor(ConfirmEnableTwoFactorRequest request)
		{
			if (request.CustomerId <= 0)
			{
				// ❌ 無效的用戶ID -> 返回錯誤 DTO
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.ChangePasswordUserNotFound, "User not found.");
			}

			// 輸入驗證
			if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
			{
				// ❌ 無效的 code -> 返回錯誤 DTO
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
			}

			// 取得當前時間
			var now = _timeProvider.Now();

			// 取出 auth（你项目里已经有 GetAuthByCustomerId 在 Logout 用过）
			try
			{
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
			async (dataAccess) =>
			{
				_customerAuthRepository.DataAccess = dataAccess;
				return await _customerAuthRepository.GetAuthByCustomerId(request.CustomerId);
			},
			"GetAuthByCustomerId"
		);

				// ✅ auth 不存在 -> 返回錯誤 DTO
				if (auth == null)
				{
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmInvalidState, "Authentication record not found.");
				}

				// ✅ Pending + 未过期 + secret存在 才允许确认
				if (auth.TwoFactorStatus != 1 || // Pending
					string.IsNullOrWhiteSpace(auth.TwoFactorSecret) ||
					auth.TwoFactorPendingExpiresAt == null ||
					auth.TwoFactorPendingExpiresAt < now)
				{
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmInvalidState, "Invalid state for confirming two-factor authentication.");
				}

				// ✅ 用 DB 的 Base64 secret 验证用户输入的 code
				var ok = Totp.VerifyCodeFromBase64Secret(
					base64Secret: auth.TwoFactorSecret,
					code: request.TwoFactorCode,
					now: now
				);

				if (!ok)
				{
					// ❌ 驗證失敗 -> 返回 錯誤 DTO
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmCodeInvalid, "Invalid two-factor authentication code.");
				}

				// ✅ 通过 -> DB status: Pending -> Enabled
				var rows = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
					// 更新資料庫中的雙重認證狀態為 Enabled
					return await _customerAuthRepository.ConfirmEnableTwoFactor(request.CustomerId, now);
				}, "ConfirmEnableTwoFactor");

				if (rows == 0)
				{
					// 如果未成功更新資料庫, 则返回 錯誤 DTO
					return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorConfirmUpdateFailed, "Failed to confirm enabling two-factor authentication.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回 錯誤 DTO
				return ConfirmEnableTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to confirm enabling two-factor authentication due to a database error.");
			}
			// ✅ 成功 -> 返回 成功 DTO
			return ConfirmEnableTwoFactorResultDto.SuccessDto();
		}

		/// <summary>
		/// 禁用雙重認證
		/// </summary>
		/// <param name="request">禁用雙重認證請求，包含用戶ID和雙重認證代碼</param>
		/// <returns>任務結果，包含操作結果和相關信息</returns>
		public async Task<DisableTwoFactorResultDto> DisableTwoFactor(DisableTwoFactorRequest request)
		{
			// 验证输入的用户ID是否有效，如果无效，则返回错误DTO
			if (request.CustomerId <= 0)
			{
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorUserNotFound, "User not found.");
			}

			// 验证输入的双重认证代码是否为空或仅包含空白字符，如果是，则返回错误DTO
			if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
			{
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
			}

			try
			{
				// 根据用户ID从数据库中获取用户认证信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(request.CustomerId);
					},
					"GetAuthByCustomerId"
				);

				// 验证用户认证信息是否存在，如果不存在，则返回错误DTO
				if (auth == null)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorUserNotFound, "User not found.");
				}

				// 验证双重认证是否已启用，如果不满足条件，则返回错误DTO
				if (auth.TwoFactorStatus != (short)TwoFactorStatus.Enabled)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorInvalidState, "Two-factor authentication is not enabled.");
				}

				// 验证双重认证代码是否为空，如果验证失败，则返回错误DTO
				if (string.IsNullOrEmpty(auth.TwoFactorSecret))
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorInvalidState, "Two-factor authentication secret is missing.");
				}

				// 取得当前时间
				var now = _timeProvider.Now();

				// 使用数据库中的 Base64 secret 验证用户输入的双重认证代码
				var ok = Totp.VerifyCodeFromBase64Secret(
					base64Secret: auth.TwoFactorSecret,
					code: request.TwoFactorCode,
					now: now
				);

				// 如果验证失败，则返回错误DTO
				if (!ok)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
				}

				var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
					_customerSessionRepository.DataAccess = dataAccess;
					_refreshTokenRepository.DataAccess = dataAccess;

					// 先关闭 2FA
					var updateCount = await _customerAuthRepository.DisableTwoFactor(request.CustomerId, now);
					if (updateCount == 0)
					{
						return 0;
					}

					// 再清理登录态
					_ = await _customerSessionRepository.InvalidateAllSessions(request.CustomerId, now);
					_ = await _refreshTokenRepository.RevokeAllRefreshTokens(request.CustomerId, now);

					return updateCount;
				}, "DisableTwoFactor");

				// 如果执行的事务中任何一个操作未成功（返回0行），则返回错误DTO
				if (count == 0)
				{
					return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.DisableTwoFactorFailed, "Failed to disable two-factor authentication.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return DisableTwoFactorResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to disable two-factor authentication due to a database error.");
			}
			// 如果所有操作成功，则返回成功DTO
			return DisableTwoFactorResultDto.SuccessDto();
		}

		/// <summary>
		/// 用戶登錄
		/// </summary>
		/// <param name="loginRequest">登錄模型</param>
		/// <returns>登錄結果DTO</returns>
		public async Task<LoginResultDto> Login(LoginRequest loginRequest)
		{
			if (string.IsNullOrWhiteSpace(loginRequest.LoginId) || string.IsNullOrWhiteSpace(loginRequest.Password))
			{
				// 如果登录ID或密码为空，则返回登录失败
				return LoginResultDto.Fail((int)AuthErrorCode.LoginIdOrPasswordRequired,"Login ID and password are required.");
			}

			// 取得当前時間
			var now = _timeProvider.Now();

			try
			{
				// 設置各個存儲庫的數據訪問對象
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByLoginId(loginRequest.LoginId);
					},
					"GetAuthByLoginId"
				);

				// 驗證用戶憑據
				if (auth == null)
				{
					// 如果用戶認證信息不存在，則返回登錄失敗
					return LoginResultDto.Fail((int)AuthErrorCode.LoginIdNotFound, "Invalid login ID.");
				}
				else if (auth.LockedUntil > _timeProvider.Now())
				{
					// 如果賬戶被鎖定，則返回登錄失敗
					return LoginResultDto.Fail((int)AuthErrorCode.AccountLocked, "Account is locked.");
				}
				else if (!_cryptoRepository.Verify(auth.PasswordHash, loginRequest.Password))
				{
					// 如果密碼驗證失敗，則記錄失敗嘗試並返回登錄失敗
					var failedCount = auth.FailedCount;

					if (failedCount >= 5)
					{
						var lockResult = await ExecuteInTxAsync<int>(async (dataAccess) =>
						{
							_customerAuthRepository.DataAccess = dataAccess;
							return await _customerAuthRepository.Lock(auth.CustomerId, now, now.AddDays(1));
						}, "Lock");

						// 如果失敗次數達到 5 次，鎖定賬戶 1 天
						if (lockResult == 0)
						{
							// 如果無法鎖定賬戶，则返回登錄失敗
							return LoginResultDto.Fail((int)AuthErrorCode.LockAccountFailed, "Failed to lock account after multiple failed login attempts.");
						}
					}
					else
					{
						var updateResult = await ExecuteInTxAsync<int>(async (dataAccess) =>
						{
							_customerAuthRepository.DataAccess = dataAccess;
							return await _customerAuthRepository.UpdateFailedCount(auth.CustomerId, now);
						}, "UpdateFailedCount");

						// 否则，僅增加失敗次數
						if (updateResult == 0)
						{
							// 如果無法增加失敗次數，则返回登錄失敗
							return LoginResultDto.Fail((int)AuthErrorCode.FailedCountUpdateFailed, "Failed to update failed login attempt count.");
						}
					}

					return LoginResultDto.Fail((int)AuthErrorCode.PasswordInvalid, "Invalid password.");
				}

				// 查看是否需要雙重認證
				var needTwoFactor = auth.TwoFactorStatus == (int)TwoFactorStatus.Enabled; // Enabled

				if (needTwoFactor && string.IsNullOrWhiteSpace(loginRequest.TwoFactorCode))
				{
					// 如果需要雙重認證但未提供雙重認證代碼，則返回需要雙重認證
					return LoginResultDto.RequireTwoFactor((int)AuthErrorCode.TwoFactorRequired);
				}

				if (needTwoFactor)
				{
					// 如果提供了雙重認證代碼，則驗證該代碼
					var ok = Totp.VerifyCodeFromBase64Secret(
						base64Secret: auth.TwoFactorSecret!,
						code: loginRequest.TwoFactorCode!,
						now: now
					);

					if (!ok)
					{
						// 如果雙重認證代碼驗證失敗，則返回登錄失敗
						return LoginResultDto.Fail((int)AuthErrorCode.TwoFactorCodeInvalid, "Invalid two-factor authentication code.");
					}
				}

				switch (loginRequest.Type)
				{
					case AuthType.Session:
						// 創建新的會話
						var session = await ExecuteInTxAsync<Guid?>(async (dataAccess) =>
						{
							_customerSessionRepository.DataAccess = dataAccess;
							return await _customerSessionRepository.CreateSession(
							auth.CustomerId,
							loginRequest.Device,
							loginRequest.Ip,
							now,
							now.Add(_sessionOption.AbsoluteLifetime));
						}, "CreateSession");

						if (session == null)
						{
							// 如果無法創建會話，则返回登錄失敗
							return LoginResultDto.Fail((int)AuthErrorCode.CreateSessionFailed, "Failed to create session.");
						}

						// 返回成功的會話ID
						return LoginResultDto.SuccessWithSession((Guid)session);

					case AuthType.JWT:
						var accessToken = _jwtProvider.CreateAccessToken(auth.CustomerId, auth.TokenVersion);

						// 生成刷新令牌的明文和哈希值
						var refreshToken = _jwtProvider.CreateRefreshToken();
						var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(refreshToken);

						// 創建新的訪問令牌和刷新令牌
						var tokenId = await ExecuteInTxAsync<Guid?>(async (dataAccess) =>
						{
							_refreshTokenRepository.DataAccess = dataAccess;
							return await _refreshTokenRepository.StoreRefreshToken(
							auth.CustomerId,
							refreshTokenHash,
							now,
							now.Add(_jwtProvider.RefreshTokenLifetime),
							loginRequest.Device,
							loginRequest.Ip,
							auth.TokenVersion);
						}, "StoreRefreshToken");

						if (tokenId == null)
						{
							// 如果無法創建刷新令牌，則返回登錄失敗
							return LoginResultDto.Fail((int)AuthErrorCode.CreateRefreshTokenFailed, "Failed to create refresh token.");
						}

						return LoginResultDto.SuccessWithJwt(accessToken, refreshToken);
					default:
						// 無效的認證類型
						return LoginResultDto.Fail((int)AuthErrorCode.AuthTypeInvalid, "Invalid authentication type.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回登錄失敗
				return LoginResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Login failed due to a database error.");
			}
		}

		/// <summary>
		/// 用戶登出
		/// </summary>
		/// <param name="request">登出請求，包含用戶ID</param>
		/// <returns>取消的会话或令牌数量，null表示用户不存在</returns>
		public async Task<LogoutResultDto> Logout(LogoutRequest request)
		{
			if (request.CustomerId <= 0)
			{
				// 如果输入的用户ID无效，则返回错误DTO
				return LogoutResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound,"User not found.");
			}

			try
			{
				// 獲取用戶認證信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(request.CustomerId);
					},
					"GetAuthByCustomerId"
				);

				if (auth == null)
				{
					// 如果用戶認證信息不存在,則返回 错误 DTO
					return LogoutResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound, "User not found.");
				}

				// 獲取當前時間
				var now = _timeProvider.Now();

				// 初始化取消的會話或令牌數量
				int count = 0;

				if (auth.AuthType == (short)AuthType.Session)
				{
					// 使所有會話無效
					count = await ExecuteInTxAsync<int>(async (dataAccess) =>
					{
						_customerSessionRepository.DataAccess = dataAccess;
						return await _customerSessionRepository.InvalidateAllSessions(request.CustomerId, now);
					}, "InvalidateAllSessions");

					if (count == 0)
					{
						// 如果無法更新會話狀態，则返回 错误 DTO
						return LogoutResultDto.Fail((int)AuthErrorCode.ValidationFailed, "Failed to invalidate sessions.");
					}
				}
				else
				{
					// 使所有刷新令牌無效
					count = await ExecuteInTxAsync<int>(async (dataAccess) =>
					{
						_refreshTokenRepository.DataAccess = dataAccess;
						return await _refreshTokenRepository.RevokeAllRefreshTokens(request.CustomerId, now);
					}, "RevokeAllRefreshTokens");

					if (count == 0)
					{
						// 如果無法更新刷新令牌狀態,则返回 错误 DTO
						return LogoutResultDto.Fail((int)AuthErrorCode.ValidationFailed, "Failed to revoke refresh tokens.");
					}
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回 错误 DTO
				return LogoutResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to logout due to a database error.");
			}
			// 返回成功 DTO
			return LogoutResultDto.SuccessDto();
		}

		/// <summary>
		/// 登出當前設備（使當前會話或令牌無效）
		/// </summary>
		/// <param name="request">登出當前設備請求，包含用戶ID和當前設備的識別信息（如會話ID或刷新令牌）</param>
		/// <returns>任务结果DTO，包含操作结果和相關信息</returns>

		public async Task<LogoutCurrentDeviceResultDto> LogoutCurrentDevice(LogoutCurrentDeviceRequest request)
		{
			// 验证输入的用户ID是否有效，如果无效，则返回错误DTO
			if (request.CustomerId <= 0)
			{
				return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.LogoutUserNotFound,"User not found.");
			}

			// 验证输入的认证类型是否有效，如果无效，则返回错误DTO
			if (!Enum.IsDefined(typeof(AuthType), request.AuthType))
			{
				return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.AuthTypeInvalid,"Invalid authentication type.");
			}

			// 获取当前时间
			var now = _timeProvider.Now();

			try
			{
				switch (request.AuthType)
				{
					case AuthType.Session:
						{
							// 验证输入的会话ID是否有效，如果无效，则返回错误DTO
							if (request.SessionId == null || request.SessionId == Guid.Empty)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionNotFound, "Invalid session ID.");
							}

							// 根据会话ID从数据库中获取会话信息
							var session = await ExecuteDbAsync<CustomerSessionEntity?>(async (dataAccess) =>
							{
								_customerSessionRepository.DataAccess = dataAccess;
								return await _customerSessionRepository.GetSessionById((Guid)request.SessionId);
							}, "GetSessionById");

							// 验证会话信息是否存在，如果不存在，则返回错误DTO
							if (session == null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionNotFound, "Session not found.");
							}

							// 验证会话是否属于用户，如果不属于，则返回错误DTO
							if (session.UserId != request.CustomerId)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionInvalid, "Session does not belong to the user.");
							}

							// 验证会话是否已经无效，如果已经无效，则返回错误DTO
							if (!session.IsValid)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionInvalid, "Session is already invalid.");
							}

							// 执行事务，使当前会话无效
							var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
							{
								_customerSessionRepository.DataAccess = dataAccess;
								return await _customerSessionRepository.InvalidateSession((Guid)request.SessionId, now);
							}, "InvalidateSession");

							// 如果无法更新会话状态，则返回错误DTO
							if (count == 0)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.SessionInvalid, "Failed to invalidate session.");
							}

							// 返回成功DTO
							return LogoutCurrentDeviceResultDto.SuccessDto();
						}

					case AuthType.JWT:
						{
							// 验证输入的刷新令牌是否有效，如果无效，则返回错误DTO
							if (string.IsNullOrWhiteSpace(request.RefreshToken))
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Invalid refresh token.");
							}

							// 根据输入的刷新令牌生成哈希值
							var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(request.RefreshToken);

							// 根据哈希值从数据库中获取刷新令牌实体
							var refreshTokenEntity = await ExecuteDbAsync<RefreshTokenEntity?>(async (dataAccess) =>
							{
								_refreshTokenRepository.DataAccess = dataAccess;
								return await _refreshTokenRepository.GetRefreshTokenByTokenHash(refreshTokenHash);
							}, "GetRefreshTokenByTokenHash");

							// 验证刷新令牌实体是否存在，如果不存在，则返回错误DTO
							if (refreshTokenEntity == null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenNotFound, "Refresh token not found.");
							}

							// 验证刷新令牌是否属于用户，如果不属于，则返回错误DTO
							if (refreshTokenEntity.UserId != request.CustomerId)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Refresh token does not belong to the user.");
							}

							// 验证刷新令牌是否已经被撤销，如果已经被撤销，则返回错误DTO
							if (refreshTokenEntity.RevokedAt != null)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenRevoked, "Refresh token is already revoked.");
							}

							// 验证刷新令牌是否已经过期，如果已经过期，则返回错误DTO
							if (refreshTokenEntity.ExpiresAt < now)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenExpired, "Refresh token has expired.");
							}

							// 执行事务，撤销当前刷新令牌
							var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
							{
								_refreshTokenRepository.DataAccess = dataAccess;
								return await _refreshTokenRepository.RevokeRefreshToken(refreshTokenEntity.TokenId, now);
							}, "RevokeRefreshToken");

							// 如果无法更新刷新令牌状态，则返回错误DTO
							if (count == 0)
							{
								return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.RefreshTokenInvalid, "Failed to revoke refresh token.");
							}

							// 返回成功DTO
							return LogoutCurrentDeviceResultDto.SuccessDto();
						}

					default:
						// 无效的认证类型
						return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.AuthTypeInvalid, "Invalid authentication type.");
				}

			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return LogoutCurrentDeviceResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to logout current device due to a database error.");
			}		
		}

		/// <summary>
		/// 刷新令牌
		/// </summary>
		/// <param name="request">刷新令牌请求，包含刷新令牌字符串</param>
		/// <returns>刷新结果DTO，包含新的访问令牌和刷新令牌</returns>
		public async Task<RefreshTokenDto> RefreshToken(RefreshTokenRequest request)
		{
			// 检查输入的刷新令牌是否为空或仅包含空白字符，如果是，则返回错误DTO
			if (string.IsNullOrWhiteSpace(request.RefreshToken))
			{
				return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenInvalid,"Invalid refresh token.");
			}

			// 获取当前时间
			var now = _timeProvider.Now();

			// 生成输入刷新令牌的哈希值
			var refreshTokenHash = _jwtProvider.CreateRefreshTokenHash(request.RefreshToken);

			try
			{
				// 根据哈希值从数据库中获取刷新令牌实体
				var refreshTokenEntity = await ExecuteDbAsync<RefreshTokenEntity?>(
					async (dataAccess) =>
					{
						_refreshTokenRepository.DataAccess = dataAccess;
						return await _refreshTokenRepository.GetRefreshTokenByTokenHash(refreshTokenHash);
					},
					"GetRefreshTokenByTokenHash"
				);

				// 如果未找到刷新令牌实体,则返回错误DTO
				if (refreshTokenEntity == null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenNotFound, "Refresh token not found.");
				}

				// 检查刷新令牌是否已过期，如果是，则返回错误DTO,并在一个事务撤销该令牌ID下的旧的刷新令牌
				if (refreshTokenEntity.ExpiresAt < now)
				{
					var revokeCount = await ExecuteInTxAsync<int>(async (dataAccess) =>
					{
						_refreshTokenRepository.DataAccess = dataAccess;
						return await _refreshTokenRepository.RevokeRefreshToken(refreshTokenEntity.TokenId, now);
					}, "RevokeExpiredRefreshToken");
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenExpired, "Refresh token has expired.");
				}

				// 检查刷新令牌是否已被撤销,如果是，则返回错误DTO,并在一个事务中提升用户的令牌版本
				if (refreshTokenEntity.RevokedAt != null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenRevoked, "Refresh token has been revoked.");
				}

				// 获取与刷新令牌关联的用户认证信息
				var customerEntity = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(refreshTokenEntity.UserId);
					},
					"GetAuthByCustomerId"
				);

				// 如果未找到用户认证信息,则返回错误DTO
				if (customerEntity == null)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenUserNotFound, "User not found for the refresh token.");
				}

				// 检查用户认证信息是否被标记为已删除，如果是，则返回错误DTO
				if (customerEntity.IsDeleted)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenAuthInvalid, "User account has been deleted.");
				}

				// 检查用户认证的认证类型是否为 JWT，如果不是，则返回错误DTO
				if (customerEntity.AuthType != (short)AuthType.JWT)
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenAuthInvalid, "Invalid authentication type for the refresh token.");
				}

				// 检查用户认证里的令牌版本是否与刷新令牌实体里的令牌版本一致，如果不一致，则返回错误DTO
				if (!customerEntity.TokenVersion.Equals(refreshTokenEntity.TokenVersion))
				{
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenTokenVersionMismatch, "Token version mismatch. The refresh token is no longer valid due to password change or other security events.");
				}

				// 创建新的访问令牌
				var newAccessToken = _jwtProvider.CreateAccessToken(customerEntity.CustomerId, customerEntity.TokenVersion);

				// 生成新的刷新令牌的明文
				var newRefreshToken = _jwtProvider.CreateRefreshToken();

				// 创建新的刷新令牌哈希值
				var newRefreshTokenHash = _jwtProvider.CreateRefreshTokenHash(newRefreshToken);

				// 在一个事务中撤销旧的刷新令牌并存储新的刷新令牌
				var tokenId = await ExecuteInTxAsync<Guid?>(async (dataAccess) =>
				{
					_refreshTokenRepository.DataAccess = dataAccess;
					// 撤销旧的刷新令牌
					var revokeCount = await _refreshTokenRepository.RevokeRefreshToken(refreshTokenEntity.TokenId, now);
					if (revokeCount == 0)
					{
						// 如果无法撤销旧的刷新令牌，则返回 null 以指示失败
						return null;
					}
					// 存储新的刷新令牌
					return await _refreshTokenRepository.StoreRefreshToken(
						refreshTokenEntity.UserId,
						newRefreshTokenHash,
						now,
						now.Add(_jwtProvider.RefreshTokenLifetime),
						refreshTokenEntity.MetaDevice,
						refreshTokenEntity.MetaIP,
						refreshTokenEntity.TokenVersion);
				}, "RotateRefreshToken");


				if (tokenId == null)
				{
					// 如果在事务中发生任何错误，则返回错误DTO
					return RefreshTokenDto.Fail((int)AuthErrorCode.RefreshTokenRotationFailed, "Failed to rotate refresh token.");
				}

				// 返回成功的访问令牌和新的刷新令牌
				return RefreshTokenDto.SuccessDto(newAccessToken, newRefreshToken);
			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return RefreshTokenDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to refresh token due to a database error.");
			}
		}

		/// <summary>
		/// 重置密碼
		/// </summary>
		/// <param name="request">重置密碼請求，包含用戶ID和ip地址或是装置信息</param>
		/// <returns>重置密碼結果DTO，包含操作結果和相關信息</returns>
		public async Task<ResetPasswordResultDto> ResetPassword(ResetPasswordRequest request)
		{
			// 驗證輸入的用戶ID是否为空，如果为空，則返回錯誤DTO
			if (string.IsNullOrWhiteSpace(request.LoginId))
			{
				return ResetPasswordResultDto.Fail((int)AuthErrorCode.ResetPasswordLoginIdRequired, "Login ID is required.");
			}

			// 预定义成功消息，避免在多个地方重复编写相同的消息文本
			const string successMessage = "If the account exists, a password reset email has been sent.";

			try
			{
				// 获取用户认证信息
				var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByLoginId(request.LoginId);
					},
					"GetAuthByLoginId"
				);

				// 验证用户认证信息是否存在或已被标记为删除，如果是，则返回成功DTO（为了安全起见，不透露账户是否存在的信息）
				if (auth == null || auth.IsDeleted)
				{
					return ResetPasswordResultDto.SuccessDto(successMessage);
				}

				// 获取与用户认证信息关联的客户信息
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async (dataAccess) =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(auth.CustomerId);
					},
					"GetCustomerById"
				);

				// 验证客户信息的电子邮件是否存在，如果不存在或为空，则返回成功DTO（为了安全起见，不透露账户是否存在的信息）
				if (customer == null || string.IsNullOrWhiteSpace(customer.Email))
				{
					return ResetPasswordResultDto.SuccessDto(successMessage);
				}

				// 生成密码重置令牌
				var passwordResetToken = _jwtProvider.CreatePasswordResetToken();

				// 生成密码重置令牌的哈希值
				var passwordResetTokenHash = _jwtProvider.CreatePasswordResetTokenHash(passwordResetToken);

				// 获取当前时间
				var now = _timeProvider.Now();

				// 计算密码重置令牌的过期时间
				var expiresAt = now.Add(_jwtProvider.PasswordResetTokenLifetime);

				// 在一个事务中首先撤销用户的所有活动密码重置令牌，然后存储新的密码重置令牌
				var tokenId = await ExecuteInTxAsync<Guid?>(async (dataAccess) =>
				{
					_passwordResetTokenRepository.DataAccess = dataAccess;

					_ = await _passwordResetTokenRepository.RevokeActiveResetTokens(auth.CustomerId, now);

					return await _passwordResetTokenRepository.StoreResetToken(
						auth.CustomerId,
						passwordResetTokenHash,
						now,
						expiresAt,
						request.Ip,
						request.Device);
				}, "StorePasswordResetToken");

				if (tokenId == null)
				{
					// 如果在事务中发生任何错误，则返回错误DTO
					return ResetPasswordResultDto.Fail((int)AuthErrorCode.ResetPasswordFailed, "Failed to generate password reset token.");
				}

				try
				{
					// 发送密码重置邮件
					await _emailSender.SendResetPasswordEmailAsync(customer.Email, passwordResetToken, expiresAt);
				}
				catch (Exception)
				{
					// 如果发送邮件失败，则返回错误DTO
					return ResetPasswordResultDto.Fail((int)AuthErrorCode.ResetPasswordFailed, "Failed to send password reset email.");
				}

				// 返回成功DTO
				return ResetPasswordResultDto.SuccessDto(successMessage);
			}
			catch (UnableToOperateDBException)
			{
				// 如果在执行数据库操作时发生异常，则返回错误DTO
				return ResetPasswordResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to reset password due to a database error.");
			}
		}

		/// <summary>
		/// 更新會話的最後活動時間
		/// </summary>
		/// <param name="request">更新會話請求，包含會話ID</param>
		/// <returns>true:有效/false:无效</returns>
		public async Task<TouchSessionResultDto> TouchSession(TouchSessionRequest request)
		{
			var now = _timeProvider.Now();
			var newExpiredAt = now.Add(_sessionOption.IdleTimeout);

			try
			{
				// 更新會話的最後活動時間和過期時間
				var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					_customerSessionRepository.DataAccess = dataAccess;
					return await _customerSessionRepository.Touch(request.SessionId, now, newExpiredAt);
				}, "Touch");

				if (count == 0)
				{
					// 如果無法更新會話狀態，则返回 錯誤 DTO
					return TouchSessionResultDto.Fail((int)AuthErrorCode.SessionTouchRejected, "Failed to touch session.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回 錯誤 DTO
				return TouchSessionResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to touch session due to a database error.");
			}

			return TouchSessionResultDto.SuccessDto();
		}

		/// <summary>
		/// 更新認證類型
		/// </summary>
		/// <param name="request">更新認證類型請求，包含用戶ID和新的認證類型</param>
		/// <returns>更新認證類型結果DTO，包含操作結果和相關信息</returns>
		public async Task<UpdateAuthTypeResultDto> UpdateAuthType(UpdateAuthTypeRequest request)
		{
			// 驗證輸入的用戶ID是否有效，如果無效，則返回錯誤DTO
			if (request.CustomerId <= 0)
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
					async (dataAccess) =>
					{
						_customerAuthRepository.DataAccess = dataAccess;
						return await _customerAuthRepository.GetAuthByCustomerId(request.CustomerId);
					},
					"GetAuthByCustomerId"
				);

				// 驗證用戶認證信息是否存在，如果不存在，則返回錯誤DTO
				if (auth == null)
				{
					return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UpdateAuthTypeUserNotFound, "User not found.");
				}

				// 驗證新的認證類型是否與當前認證類型相同，如果相同，則返回成功DTO
				if (auth.AuthType == (short)request.AuthType)
				{
					return UpdateAuthTypeResultDto.SuccessDto();
				}

				// 获取当前时间
				var now = _timeProvider.Now();

				var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
				{
					// 在一个事务中执行以下操作：
					_customerAuthRepository.DataAccess = dataAccess;
					_refreshTokenRepository.DataAccess = dataAccess;
					_customerSessionRepository.DataAccess = dataAccess;

					// 更新用户认证类型
					var updateCount = await _customerAuthRepository.UpdateAuthType(request.CustomerId, (short)request.AuthType, now);
					if (updateCount == 0)
					{
						return 0;
					}

					// 使用户的所有会话无效
					_ = await _customerSessionRepository.InvalidateAllSessions(request.CustomerId, now);

					// 撤销用户的所有刷新令牌
					_ = await _refreshTokenRepository.RevokeAllRefreshTokens(request.CustomerId, now);

					return updateCount;
				}, "UpdateAuthTypeTransaction");

				// 如果事务中的任何一个操作未成功或是更新用户认证类型时返回0行，则返回错误DTO
				if (count == 0)
				{
					return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UpdateAuthTypeFailed, "Failed to update authentication type due to a transaction error.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回 錯誤 DTO
				return UpdateAuthTypeResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to update authentication type due to a database error.");
			}
			// 如果所有操作成功，则返回成功DTO
			return UpdateAuthTypeResultDto.SuccessDto();
		}

		/// <summary>
		/// 驗證會話是否有效
		/// </summary>
		/// <param name="request">驗證會話請求，包含會話ID</param>
		/// <returns>true:有效/false:无效</returns>
		public async Task<ValidateSessionResultDto> ValidateSession(ValidateSessionRequest request)
		{
			try
			{
				// 獲取會話信息
				var session = await ExecuteDbAsync<CustomerSessionEntity?>(
					async (dataAccess) =>
					{
						_customerSessionRepository.DataAccess = dataAccess;
						return await _customerSessionRepository.GetSessionById(request.SessionId);
					},
					"GetSessionById"
				);

				if (session == null || !session.IsValid)
				{
					// 如果會話不存在或無效，則返回 錯誤 DTO
					return ValidateSessionResultDto.Fail((int)AuthErrorCode.SessionNotFound, "Session not found or invalid.");
				}

				// 獲取當前時間
				var now = _timeProvider.Now();

				// 檢查會話是否過期
				if (session.ExpiredAt < now)
				{
					// 使會話無效
					var count = await ExecuteInTxAsync<int>(async (dataAccess) =>
					{
						_customerSessionRepository.DataAccess = dataAccess;
						return await _customerSessionRepository.InvalidateSession(request.SessionId, now);
					}, "InvalidateSession");

					if (count == 0)
					{
						// 如果無法更新會話狀態,则返回 錯誤 DTO
						return ValidateSessionResultDto.Fail((int)AuthErrorCode.ValidationFailed, "Failed to invalidate expired session.");
					}

					// 返回 會話過期 錯誤 DTO
					return ValidateSessionResultDto.Fail((int)AuthErrorCode.SessionExpired, "Session has expired.");
				}
			}
			catch (UnableToOperateDBException)
			{
				// 如果在執行資料庫操作時發生異常，則返回 錯誤 DTO
				return ValidateSessionResultDto.Fail((int)AuthErrorCode.UnableToOperateDb, "Failed to validate session due to a database error.");
			}

			// 會話有效,返回 成功 DTO
			return ValidateSessionResultDto.SuccessDto();
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
		/// 驗證會話是否有效
		/// </summary>
		/// <param name="request">驗證會話請求，包含會話ID</param>
		/// <returns>true:有效/false:无效</returns>
		Task<ValidateSessionResultDto> ValidateSession(ValidateSessionRequest request);

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
