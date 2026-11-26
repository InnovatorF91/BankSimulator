using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerProject.Common;
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
		private const string InternalErrorMessage = "An internal server error occurred.";
		private const string ValidSessionRequiredMessage = "A valid session is required.";
		private const string SessionOnlyMessage = "This endpoint only supports session authentication.";

		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthService authService, ILogger<AuthController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		/// <summary>
		/// 验证登录凭据并创建会话。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("Login")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			return ExecuteWithLoggingAsync<LoginResponse>(nameof(Login), async () =>
			{
				if (request.Type != AuthType.Session)
				{
					return CreateErrorResult<LoginResponse>(
						StatusCodes.Status400BadRequest,
						(int)AuthErrorCode.AuthTypeInvalid,
						SessionOnlyMessage);
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

						_ => (StatusCodes.Status500InternalServerError, InternalErrorMessage)
					};

					if (code is (int)AuthErrorCode.LoginIdNotFound
						or (int)AuthErrorCode.AccountLocked)
					{
						code = (int)AuthErrorCode.PasswordInvalid;
					}

					return CreateErrorResult<LoginResponse>(statusCode, code, message);
				}

				if (result.SessionId is not Guid sessionId || sessionId == Guid.Empty)
				{
					return CreateErrorResult<LoginResponse>(
						StatusCodes.Status500InternalServerError,
						(int)AuthErrorCode.CreateSessionFailed,
						InternalErrorMessage);
				}

				var response = new LoginResponse
				{
					Success = true,
					Code = result.Code,
					Message = "Login succeeded.",
					SessionId = sessionId
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 注销当前设备的会话。
		/// </summary>
		[HttpPost("LogoutCurrentDevice")]
		public Task<ActionResult<LogoutCurrentDeviceResponse>> LogoutCurrentDevice()
		{
			return ExecuteWithLoggingAsync<LogoutCurrentDeviceResponse>(
				nameof(LogoutCurrentDevice),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId)
						|| !Guid.TryParse(User.FindFirst("session_id")?.Value, out var sessionId)
						|| sessionId == Guid.Empty)
					{
						return CreateInvalidSessionResult<LogoutCurrentDeviceResponse>();
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
							return CreateInvalidSessionResult<LogoutCurrentDeviceResponse>();
						}

						return CreateErrorResult<LogoutCurrentDeviceResponse>(
							StatusCodes.Status500InternalServerError,
							result.Code,
							InternalErrorMessage);
					}

					var response = new LogoutCurrentDeviceResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The current session was logged out successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 登出当前客户，撤销其认证类型对应的凭据。
		/// </summary>
		[HttpPost("Logout")]
		public Task<ActionResult<LogoutResponse>> Logout()
		{
			return ExecuteWithLoggingAsync<LogoutResponse>(nameof(Logout), async () =>
			{
				if (!TryGetSessionCustomerId(out var customerId))
				{
					return CreateInvalidSessionResult<LogoutResponse>();
				}

				var result = await _authService.Logout(new LogoutRequest
				{
					CustomerId = customerId
				});

				if (!result.Success)
				{
					if (result.Code == (int)AuthErrorCode.LogoutUserNotFound)
					{
						return CreateInvalidSessionResult<LogoutResponse>();
					}

					return CreateErrorResult<LogoutResponse>(
						StatusCodes.Status500InternalServerError,
						result.Code,
						InternalErrorMessage);
				}

				var response = new LogoutResponse
				{
					Success = true,
					Code = result.Code,
					Message = "Logout succeeded."
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 修改当前客户的密码，并使现有会话失效。
		/// </summary>
		[HttpPost("ChangePassword")]
		public Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			return ExecuteWithLoggingAsync<ChangePasswordResponse>(
				nameof(ChangePassword),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId))
					{
						return CreateInvalidSessionResult<ChangePasswordResponse>();
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
									ValidSessionRequiredMessage),

							_ => (StatusCodes.Status500InternalServerError,
								result.Code,
								InternalErrorMessage)
						};

						return CreateErrorResult<ChangePasswordResponse>(statusCode, code, message);
					}

					var response = new ChangePasswordResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The password was changed successfully. Please log in again."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 为当前客户开始双因素认证设置。
		/// </summary>
		[HttpPost("BeginEnableTwoFactor")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public Task<ActionResult<BeginEnableTwoFactorResponse>> BeginEnableTwoFactor()
		{
			return ExecuteWithLoggingAsync<BeginEnableTwoFactorResponse>(
				nameof(BeginEnableTwoFactor),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId))
					{
						return CreateInvalidSessionResult<BeginEnableTwoFactorResponse>();
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

							_ => (StatusCodes.Status500InternalServerError, InternalErrorMessage)
						};

						return CreateErrorResult<BeginEnableTwoFactorResponse>(
							statusCode, result.Code, message);
					}

					if (string.IsNullOrWhiteSpace(result.Secret)
						|| result.ExpiresAt is not DateTime expiresAt
						|| string.IsNullOrWhiteSpace(result.OtpauthUri))
					{
						return CreateErrorResult<BeginEnableTwoFactorResponse>(
							StatusCodes.Status500InternalServerError,
							(int)AuthErrorCode.TwoFactorBeginFailed,
							InternalErrorMessage);
					}

					var response = new BeginEnableTwoFactorResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Two-factor setup started. Confirm it with a code from your authenticator.",
						Secret = result.Secret,
						ExpiresAt = expiresAt,
						OtpauthUri = result.OtpauthUri
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 验证当前客户的验证码并正式启用双因素认证。
		/// </summary>
		[HttpPost("ConfirmEnableTwoFactor")]
		public Task<ActionResult<ConfirmEnableTwoFactorResponse>> ConfirmEnableTwoFactor([FromBody] ConfirmEnableTwoFactorRequest request)
		{
			return ExecuteWithLoggingAsync<ConfirmEnableTwoFactorResponse>(
				nameof(ConfirmEnableTwoFactor),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId))
					{
						return CreateInvalidSessionResult<ConfirmEnableTwoFactorResponse>();
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

							_ => (StatusCodes.Status500InternalServerError, InternalErrorMessage)
						};

						return CreateErrorResult<ConfirmEnableTwoFactorResponse>(
							statusCode, result.Code, message);
					}

					var response = new ConfirmEnableTwoFactorResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Two-factor authentication was enabled successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 验证当前验证码，关闭双因素认证并使现有会话失效。
		/// </summary>
		[HttpPost("DisableTwoFactor")]
		public Task<ActionResult<DisableTwoFactorResponse>> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
		{
			return ExecuteWithLoggingAsync<DisableTwoFactorResponse>(
				nameof(DisableTwoFactor),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId))
					{
						return CreateInvalidSessionResult<DisableTwoFactorResponse>();
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
									ValidSessionRequiredMessage),

							(int)AuthErrorCode.DisableTwoFactorInvalidState
								=> (StatusCodes.Status409Conflict,
									result.Code,
									"Two-factor authentication cannot be disabled in its current state."),

							_ => (StatusCodes.Status500InternalServerError,
								result.Code,
								InternalErrorMessage)
						};

						return CreateErrorResult<DisableTwoFactorResponse>(
							statusCode, code, message);
					}

					var response = new DisableTwoFactorResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Two-factor authentication was disabled successfully. Please log in again."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 受理向账户登记邮箱发送密码重置邮件的请求。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("ResetPassword")]
		public ActionResult<ResetPasswordResponse> ResetPassword([FromBody] ResetPasswordRequest request,[FromServices] PasswordResetQueue queue)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;
			int? errorCode = null;

			_logger.LogStart();

			try
			{
				if (string.IsNullOrWhiteSpace(request.LoginId))
				{
					var error = CreateErrorResult<ResetPasswordResponse>(
						StatusCodes.Status400BadRequest,
						(int)AuthErrorCode.ResetPasswordLoginIdRequired,
						"Login ID is required.");

					outcome = error.Outcome;
					errorCode = error.ErrorCode;
					return error.Response;
				}

				var queuedRequest = new ResetPasswordRequest
				{
					LoginId = request.LoginId,
					Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
					Device = Request.Headers["User-Agent"].FirstOrDefault()
				};

				if (!queue.TryEnqueue(queuedRequest))
				{
					var error = CreateErrorResult<ResetPasswordResponse>(
						StatusCodes.Status503ServiceUnavailable,
						(int)AuthErrorCode.ResetPasswordFailed,
						"Password reset is temporarily unavailable. Please try again later.");

					outcome = error.Outcome;
					errorCode = error.ErrorCode;
					return error.Response;
				}

				var response = new ResetPasswordResponse
				{
					Success = true,
					Code = 0,
					Message = "If the account exists, a password reset email will be sent."
				};

				// 当前请求在入队后结束，邮件由后台 Worker 发送。
				outcome = LogOutcome.Completed;
				return Ok(response);
			}
			finally
			{
				stopwatch.Stop();
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, errorCode);
			}
		}

		/// <summary>
		/// 使用邮件中的重置令牌设置新密码。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("ConfirmResetPassword")]
		public Task<ActionResult<ConfirmResetPasswordResponse>> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request)
		{
			return ExecuteWithLoggingAsync<ConfirmResetPasswordResponse>(
				nameof(ConfirmResetPassword),
				async () =>
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
								InternalErrorMessage)
						};

						return CreateErrorResult<ConfirmResetPasswordResponse>(
							statusCode, code, message);
					}

					var response = new ConfirmResetPasswordResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The password was reset successfully. Please log in again."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 更新当前会话的最后活动时间。
		/// </summary>
		[HttpPost("TouchSession")]
		public Task<ActionResult<TouchSessionResponse>> TouchSession()
		{
			return ExecuteWithLoggingAsync<TouchSessionResponse>(nameof(TouchSession), async () =>
			{
				if (User.Identity is not { IsAuthenticated: true, AuthenticationType: "Session" }
					|| !Guid.TryParse(User.FindFirst("session_id")?.Value, out var sessionId)
					|| sessionId == Guid.Empty)
				{
					return CreateInvalidSessionResult<TouchSessionResponse>();
				}

				var result = await _authService.TouchSession(new TouchSessionRequest
				{
					SessionId = sessionId
				});

				if (!result.Success)
				{
					if (result.Code == (int)AuthErrorCode.SessionTouchRejected)
					{
						return CreateInvalidSessionResult<TouchSessionResponse>();
					}

					return CreateErrorResult<TouchSessionResponse>(
						StatusCodes.Status500InternalServerError,
						result.Code,
						InternalErrorMessage);
				}

				var response = new TouchSessionResponse
				{
					Success = true,
					Code = result.Code,
					Message = "The current session was updated successfully."
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 将当前客户的认证类型设置为 Session。
		/// </summary>
		[HttpPost("UpdateAuthType")]
		public Task<ActionResult<UpdateAuthTypeResponse>> UpdateAuthType([FromBody] UpdateAuthTypeRequest request)
		{
			return ExecuteWithLoggingAsync<UpdateAuthTypeResponse>(
				nameof(UpdateAuthType),
				async () =>
				{
					if (!TryGetSessionCustomerId(out var customerId))
					{
						return CreateInvalidSessionResult<UpdateAuthTypeResponse>();
					}

					if (request.AuthType != AuthType.Session)
					{
						return CreateErrorResult<UpdateAuthTypeResponse>(
							StatusCodes.Status400BadRequest,
							(int)AuthErrorCode.AuthTypeInvalid,
							SessionOnlyMessage);
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
									ValidSessionRequiredMessage),

							_ => (StatusCodes.Status500InternalServerError,
								result.Code,
								InternalErrorMessage)
						};

						return CreateErrorResult<UpdateAuthTypeResponse>(statusCode, code, message);
					}

					var response = new UpdateAuthTypeResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account authentication type is set to Session."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 使用现有刷新令牌换取新的访问令牌和刷新令牌。
		/// </summary>
		[AllowAnonymous]
		[HttpPost("RefreshToken")]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
		{
			return ExecuteWithLoggingAsync<RefreshTokenResponse>(
				nameof(RefreshToken),
				async () =>
				{
					if (string.IsNullOrWhiteSpace(request.RefreshToken))
					{
						return CreateErrorResult<RefreshTokenResponse>(
							StatusCodes.Status400BadRequest,
							(int)AuthErrorCode.RefreshTokenInvalid,
							"A refresh token is required.");
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
								InternalErrorMessage)
						};

						return CreateErrorResult<RefreshTokenResponse>(statusCode, code, message);
					}

					var response = new RefreshTokenResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The tokens were refreshed successfully.",
						AccessToken = result.AccessToken,
						RefreshToken = result.RefreshToken
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 从已认证的 Session 身份中读取有效的客户 ID。
		/// </summary>
		private bool TryGetSessionCustomerId(out int customerId)
		{
			customerId = 0;

			return User.Identity is { IsAuthenticated: true, AuthenticationType: "Session" }
				&& int.TryParse(User.FindFirst("uid")?.Value, out customerId)
				&& customerId > 0;
		}

		/// <summary>
		/// 构造需要有效会话时的统一错误响应。
		/// </summary>
		private (ActionResult<TResponse> Response, LogOutcome Outcome, int? ErrorCode)
			CreateInvalidSessionResult<TResponse>()
		{
			return CreateErrorResult<TResponse>(
				StatusCodes.Status401Unauthorized,
				(int)CustomerErrorCode.UnauthorizedAccess,
				ValidSessionRequiredMessage);
		}

		/// <summary>
		/// 构造错误响应，并按 HTTP 状态码确定日志结果。
		/// </summary>
		private (ActionResult<TResponse> Response, LogOutcome Outcome, int? ErrorCode)CreateErrorResult<TResponse>(int statusCode, int errorCode, string message)
		{
			var outcome = statusCode >= StatusCodes.Status500InternalServerError
				? LogOutcome.Failed
				: LogOutcome.Rejected;

			var response = new ErrorResponse
			{
				Success = false,
				Code = errorCode,
				Message = message
			};

			return (StatusCode(statusCode, response), outcome, errorCode);
		}

		/// <summary>
		/// 执行异步 Controller 操作，记录 START、END、执行耗时和操作结果。
		/// </summary>
		/// <typeparam name="TResponse">操作成功时的响应类型。</typeparam>
		/// <param name="operation">写入日志的 Action 名称。</param>
		/// <param name="execute">返回 Action 响应、日志结果和可选错误码的异步委托。</param>
		/// <returns>异步操作任务，其结果为委托返回的 Action 响应。</returns>
		/// <remarks>
		/// 如果委托抛出异常，结束日志会将结果记为 <see cref="LogOutcome.Failed"/>，
		/// 异常继续向调用方传播。
		/// </remarks>
		private async Task<ActionResult<TResponse>> ExecuteWithLoggingAsync<TResponse>(
			string operation,
			Func<Task<(ActionResult<TResponse> Response, LogOutcome Outcome, int? ErrorCode)>> execute)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;
			int? errorCode = null;

			_logger.LogStart(operation: operation);

			try
			{
				var result = await execute();
				outcome = result.Outcome;
				errorCode = result.ErrorCode;
				return result.Response;
			}
			finally
			{
				stopwatch.Stop();
				_logger.LogEnd(
					outcome,
					stopwatch.ElapsedMilliseconds,
					errorCode,
					operation: operation);
			}
		}
	}
}