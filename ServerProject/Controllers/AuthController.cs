using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		/// <summary>
		/// 验证登录凭据并创建会话。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("Login")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			if (request.Type != AuthType.Session)
			{
				return BadRequest(new ErrorResponse
				{
					Success = false,
					Code = (int)AuthErrorCode.AuthTypeInvalid,
					Message = "This endpoint only supports session authentication."
				});
			}

			var result = await _authService.Login(request);

			if (!result.Success)
			{
				var code = result.Code;
				var (statusCode, message) = code switch
				{
					(int)AuthErrorCode.LoginIdOrPasswordRequired
						or (int)AuthErrorCode.AuthTypeInvalid
						=> (StatusCodes.Status400BadRequest, result.Message ?? string.Empty),

					(int)AuthErrorCode.LoginIdNotFound
						or (int)AuthErrorCode.PasswordInvalid
						or (int)AuthErrorCode.AccountLocked
						=> (StatusCodes.Status401Unauthorized, "Invalid login ID or password."),

					(int)AuthErrorCode.TwoFactorRequired
						or (int)AuthErrorCode.TwoFactorCodeInvalid
						=> (StatusCodes.Status401Unauthorized, result.Message ?? string.Empty),

					_ => (StatusCodes.Status500InternalServerError,
						"An internal server error occurred.")
				};

				if (code is (int)AuthErrorCode.LoginIdNotFound
					or (int)AuthErrorCode.AccountLocked)
				{
					code = (int)AuthErrorCode.PasswordInvalid;
				}

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			if (result.SessionId is not Guid sessionId || sessionId == Guid.Empty)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ErrorResponse
					{
						Success = false,
						Code = (int)AuthErrorCode.CreateSessionFailed,
						Message = "An internal server error occurred."
					});
			}

			return Ok(new LoginResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Login succeeded.",
				SessionId = sessionId
			});
		}

		/// <summary>
		/// 注销当前设备的会话。
		/// </summary>
		[HttpPost("LogoutCurrentDevice")]
		public async Task<ActionResult<LogoutCurrentDeviceResponse>> LogoutCurrentDevice()
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0
				|| !Guid.TryParse(User.FindFirst("session_id")?.Value, out var sessionId)
				|| sessionId == Guid.Empty)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			var result = await _authService.LogoutCurrentDevice(
				new LogoutCurrentDeviceRequest
				{
					CustomerId = customerId,
					AuthType = AuthType.Session,
					SessionId = sessionId
				});

			if (!result.Success)
			{
				if (result.Code is (int)AuthErrorCode.SessionNotFound
					or (int)AuthErrorCode.SessionInvalid)
				{
					return Unauthorized(new ErrorResponse
					{
						Success = false,
						Code = (int)CustomerErrorCode.UnauthorizedAccess,
						Message = "A valid session is required."
					});
				}

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = "An internal server error occurred."
					});
			}

			return Ok(new LogoutCurrentDeviceResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The current session was logged out successfully."
			});
		}

		/// <summary>
		/// 登出当前客户，撤销其认证类型对应的凭据。
		/// </summary>
		[HttpPost("Logout")]
		public async Task<ActionResult<LogoutResponse>> Logout()
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			var result = await _authService.Logout(new LogoutRequest
			{
				CustomerId = customerId
			});

			if (!result.Success)
			{
				if (result.Code == (int)AuthErrorCode.LogoutUserNotFound)
				{
					return Unauthorized(new ErrorResponse
					{
						Success = false,
						Code = (int)CustomerErrorCode.UnauthorizedAccess,
						Message = "A valid session is required."
					});
				}

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = "An internal server error occurred."
					});
			}

			return Ok(new LogoutResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Logout succeeded."
			});
		}

		/// <summary>
		/// 修改当前客户的密码，并使现有会话失效。
		/// </summary>
		[HttpPost("ChangePassword")]
		public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			request.CustomerId = customerId;
			var result = await _authService.ChangePassword(request);

			if (!result.Success)
			{
				var (statusCode, code, message) = result.Code switch
				{
					(int)AuthErrorCode.ChangePasswordOldPasswordRequired
						or (int)AuthErrorCode.ChangePasswordOldPasswordInvalid
						or (int)AuthErrorCode.ChangePasswordNewPasswordRequired
						or (int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld
						or (int)AuthErrorCode.ChangePasswordNewPasswordPolicyInvalid
						=> (StatusCodes.Status400BadRequest,
							result.Code,
							result.Message ?? string.Empty),

					(int)AuthErrorCode.ChangePasswordUserNotFound
						=> (StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							"A valid session is required."),

					_ => (StatusCodes.Status500InternalServerError,
						result.Code,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			return Ok(new ChangePasswordResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The password was changed successfully. Please log in again."
			});
		}

		/// <summary>
		/// 为当前客户开始双因素认证设置。
		/// </summary>
		[HttpPost("BeginEnableTwoFactor")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public async Task<ActionResult<BeginEnableTwoFactorResponse>> BeginEnableTwoFactor()
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			var result = await _authService.BeginEnableTwoFactor(
				new BeginEnableTwoFactorRequest
				{
					CustomerId = customerId
				});

			if (!result.Success)
			{
				var (statusCode, message) = result.Code switch
				{
					(int)AuthErrorCode.TwoFactorBeginFailed
						=> (StatusCodes.Status409Conflict,
							"Two-factor setup could not be started for the current account."),

					_ => (StatusCodes.Status500InternalServerError,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = message
				});
			}

			if (string.IsNullOrWhiteSpace(result.Secret)
				|| result.ExpiresAt is not DateTime expiresAt
				|| string.IsNullOrWhiteSpace(result.OtpauthUri))
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ErrorResponse
					{
						Success = false,
						Code = (int)AuthErrorCode.TwoFactorBeginFailed,
						Message = "An internal server error occurred."
					});
			}

			return Ok(new BeginEnableTwoFactorResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Two-factor setup started. Confirm it with a code from your authenticator.",
				Secret = result.Secret,
				ExpiresAt = expiresAt,
				OtpauthUri = result.OtpauthUri
			});
		}

		/// <summary>
		/// 验证当前客户的验证码并正式启用双因素认证。
		/// </summary>
		[HttpPost("ConfirmEnableTwoFactor")]
		public async Task<ActionResult<ConfirmEnableTwoFactorResponse>> ConfirmEnableTwoFactor([FromBody] ConfirmEnableTwoFactorRequest request)
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			request.CustomerId = customerId;
			var result = await _authService.ConfirmEnableTwoFactor(request);

			if (!result.Success)
			{
				var (statusCode, message) = result.Code switch
				{
					(int)AuthErrorCode.TwoFactorCodeInvalid
						or (int)AuthErrorCode.TwoFactorConfirmCodeInvalid
						=> (StatusCodes.Status400BadRequest,
							"Invalid two-factor authentication code."),

					(int)AuthErrorCode.TwoFactorConfirmInvalidState
						=> (StatusCodes.Status409Conflict,
							"There is no valid pending two-factor setup to confirm."),

					_ => (StatusCodes.Status500InternalServerError,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = message
				});
			}

			return Ok(new ConfirmEnableTwoFactorResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Two-factor authentication was enabled successfully."
			});
		}

		/// <summary>
		/// 验证当前验证码，关闭双因素认证并使现有会话失效。
		/// </summary>
		[HttpPost("DisableTwoFactor")]
		public async Task<ActionResult<DisableTwoFactorResponse>> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			request.CustomerId = customerId;
			var result = await _authService.DisableTwoFactor(request);

			if (!result.Success)
			{
				var (statusCode, code, message) = result.Code switch
				{
					(int)AuthErrorCode.TwoFactorCodeInvalid
						=> (StatusCodes.Status400BadRequest,
							result.Code,
							"Invalid two-factor authentication code."),

					(int)AuthErrorCode.DisableTwoFactorUserNotFound
						=> (StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							"A valid session is required."),

					(int)AuthErrorCode.DisableTwoFactorInvalidState
						=> (StatusCodes.Status409Conflict,
							result.Code,
							"Two-factor authentication cannot be disabled in its current state."),

					_ => (StatusCodes.Status500InternalServerError,
						result.Code,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			return Ok(new DisableTwoFactorResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Two-factor authentication was disabled successfully. Please log in again."
			});
		}

		/// <summary>
		/// 受理向账户登记邮箱发送密码重置邮件的请求。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("ResetPassword")]
		public ActionResult<ResetPasswordResponse> ResetPassword([FromBody] ResetPasswordRequest request,[FromServices] PasswordResetQueue queue)
		{
			if (string.IsNullOrWhiteSpace(request.LoginId))
			{
				return BadRequest(new ErrorResponse
				{
					Success = false,
					Code = (int)AuthErrorCode.ResetPasswordLoginIdRequired,
					Message = "Login ID is required."
				});
			}

			var queuedRequest = new ResetPasswordRequest
			{
				LoginId = request.LoginId,
				Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
				Device = Request.Headers["User-Agent"].FirstOrDefault()
			};

			if (!queue.TryEnqueue(queuedRequest))
			{
				return StatusCode(
					StatusCodes.Status503ServiceUnavailable,
					new ErrorResponse
					{
						Success = false,
						Code = (int)AuthErrorCode.ResetPasswordFailed,
						Message = "Password reset is temporarily unavailable. Please try again later."
					});
			}

			return Ok(new ResetPasswordResponse
			{
				Success = true,
				Code = 0,
				Message = "If the account exists, a password reset email will be sent."
			});
		}

		/// <summary>
		/// 使用邮件中的重置令牌设置新密码。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("ConfirmResetPassword")]
		public async Task<ActionResult<ConfirmResetPasswordResponse>> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request)
		{
			var result = await _authService.ConfirmResetPassword(request);

			if (!result.Success)
			{
				var (statusCode, code, message) = result.Code switch
				{
					(int)AuthErrorCode.ConfirmResetPasswordTokenRequired
						or (int)AuthErrorCode.ConfirmResetPasswordNewPasswordRequired
						or (int)AuthErrorCode.ConfirmResetPasswordNewPasswordPolicyInvalid
						or (int)AuthErrorCode.ChangePasswordNewPasswordSameAsOld
						=> (StatusCodes.Status400BadRequest,
							result.Code,
							result.Message ?? string.Empty),

					(int)AuthErrorCode.ConfirmResetPasswordTokenInvalid
						or (int)AuthErrorCode.ConfirmResetPasswordTokenExpired
						or (int)AuthErrorCode.ConfirmResetPasswordUserNotFound
						=> (StatusCodes.Status400BadRequest,
							(int)AuthErrorCode.ConfirmResetPasswordTokenInvalid,
							"The password reset token is invalid or has expired."),

					_ => (StatusCodes.Status500InternalServerError,
						result.Code,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			return Ok(new ConfirmResetPasswordResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The password was reset successfully. Please log in again."
			});
		}

		/// <summary>
		/// 更新当前会话的最后活动时间。
		/// </summary>
		[HttpPost("TouchSession")]
		public async Task<ActionResult<TouchSessionResponse>> TouchSession()
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !Guid.TryParse(User.FindFirst("session_id")?.Value, out var sessionId)
				|| sessionId == Guid.Empty)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			var result = await _authService.TouchSession(new TouchSessionRequest
			{
				SessionId = sessionId
			});

			if (!result.Success)
			{
				if (result.Code == (int)AuthErrorCode.SessionTouchRejected)
				{
					return Unauthorized(new ErrorResponse
					{
						Success = false,
						Code = (int)CustomerErrorCode.UnauthorizedAccess,
						Message = "A valid session is required."
					});
				}

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = "An internal server error occurred."
					});
			}

			return Ok(new TouchSessionResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The current session was updated successfully."
			});
		}

		/// <summary>
		/// 将当前客户的认证类型设置为 Session。
		/// </summary>
		[HttpPost("UpdateAuthType")]
		public async Task<ActionResult<UpdateAuthTypeResponse>> UpdateAuthType([FromBody] UpdateAuthTypeRequest request)
		{
			if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
				|| !int.TryParse(User.FindFirst("uid")?.Value, out var customerId)
				|| customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "A valid session is required."
				});
			}

			if (request.AuthType != AuthType.Session)
			{
				return BadRequest(new ErrorResponse
				{
					Success = false,
					Code = (int)AuthErrorCode.AuthTypeInvalid,
					Message = "This endpoint only supports session authentication."
				});
			}

			request.CustomerId = customerId;
			var result = await _authService.UpdateAuthType(request);

			if (!result.Success)
			{
				var (statusCode, code, message) = result.Code switch
				{
					(int)AuthErrorCode.AuthTypeInvalid
						=> (StatusCodes.Status400BadRequest,
							result.Code,
							"Invalid authentication type."),

					(int)AuthErrorCode.UpdateAuthTypeUserNotFound
						=> (StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							"A valid session is required."),

					_ => (StatusCodes.Status500InternalServerError,
						result.Code,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			return Ok(new UpdateAuthTypeResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The account authentication type is set to Session."
			});
		}

		/// <summary>
		/// 使用现有刷新令牌换取新的访问令牌和刷新令牌。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("RefreshToken")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.RefreshToken))
			{
				return BadRequest(new ErrorResponse
				{
					Success = false,
					Code = (int)AuthErrorCode.RefreshTokenInvalid,
					Message = "A refresh token is required."
				});
			}

			var result = await _authService.RefreshToken(request);

			if (!result.Success)
			{
				var (statusCode, code, message) = result.Code switch
				{
					(int)AuthErrorCode.RefreshTokenInvalid
						or (int)AuthErrorCode.RefreshTokenNotFound
						or (int)AuthErrorCode.RefreshTokenExpired
						or (int)AuthErrorCode.RefreshTokenRevoked
						or (int)AuthErrorCode.RefreshTokenUserNotFound
						or (int)AuthErrorCode.RefreshTokenAuthInvalid
						or (int)AuthErrorCode.RefreshTokenTokenVersionMismatch
						=> (StatusCodes.Status401Unauthorized,
							(int)AuthErrorCode.RefreshTokenInvalid,
							"The refresh token is invalid or has expired."),

					_ => (StatusCodes.Status500InternalServerError,
						result.Code,
						"An internal server error occurred.")
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = code,
					Message = message
				});
			}

			return Ok(new RefreshTokenResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The tokens were refreshed successfully.",
				AccessToken = result.AccessToken,
				RefreshToken = result.RefreshToken
			});
		}
	}
}