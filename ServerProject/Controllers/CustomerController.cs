using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServerProject.Common;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/customers")]
	public class CustomerController : ControllerBase
	{
		private readonly ICustomerService _customerService;
		private readonly ICurrentCustomer _currentCustomer;
		private readonly ILogger<CustomerController> _logger;

		public CustomerController(
			ICustomerService customerService,
			ICurrentCustomer currentCustomer,
			ILogger<CustomerController> logger)
		{
			_customerService = customerService;
			_currentCustomer = currentCustomer;
			_logger = logger;
		}

		/// <summary>
		/// 注册新客户。
		/// </summary>
		[HttpPost("RegisterCustomer")]
		public Task<ActionResult<RegisterCustomerResponse>> RegisterCustomer([FromBody] RegisterCustomerRequest request)
		{
			return ExecuteWithLoggingAsync<RegisterCustomerResponse>(
				nameof(RegisterCustomer),
				async () =>
				{
					var result = await _customerService.RegisterCustomer(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.DuplicateCustomer
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<RegisterCustomerResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new RegisterCustomerResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer was registered successfully.",
						CustomerId = result.CustomerId
					};

					return (
						StatusCode(StatusCodes.Status201Created, response),
						LogOutcome.Completed,
						null);
				});
		}

		/// <summary>
		/// 查询当前登录客户的资料。
		/// </summary>
		[HttpPost("GetMyProfile")]
		public Task<ActionResult<GetMyProfileResponse>> GetMyProfile()
		{
			return ExecuteWithLoggingAsync<GetMyProfileResponse>(
				nameof(GetMyProfile),
				async () =>
				{
					if (!TryGetCurrentCustomerId(out var customerId, out var errorMessage))
					{
						return CreateErrorResult<GetMyProfileResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							errorMessage);
					}

					var result = await _customerService.GetCustomer(
						new GetCustomerRequest { Id = customerId });

					if (!result.Success)
					{
						var statusCode = result.Code == (int)CustomerErrorCode.CustomerNotFound
							? StatusCodes.Status404NotFound
							: StatusCodes.Status500InternalServerError;

						return CreateErrorResult<GetMyProfileResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetMyProfileResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer profile was retrieved successfully.",
						CustomerId = result.CustomerId,
						Name = result.Name,
						Gender = result.Gender,
						BirthDate = result.BirthDate,
						IDType = result.IDType,
						IDNumber = result.IDNumber,
						Address = result.Address,
						Phone = result.Phone,
						Email = result.Email,
						KYCStatus = result.KYCStatus,
						CreatedAt = result.CreatedAt,
						UpdateAt = result.UpdateAt
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 更新当前登录客户的基本资料。
		/// </summary>
		[HttpPost("UpdateBasicProfile")]
		public Task<ActionResult<UpdateBasicProfileResponse>> UpdateBasicProfile([FromBody] UpdateBasicProfileRequest request)
		{
			return ExecuteWithLoggingAsync<UpdateBasicProfileResponse>(
				nameof(UpdateBasicProfile),
				async () =>
				{
					if (!TryGetCurrentCustomerId(out var customerId, out var errorMessage))
					{
						return CreateErrorResult<UpdateBasicProfileResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							errorMessage);
					}

					var result = await _customerService.UpdateBasicProfile(
						new UpdateBasicProfileRequest
						{
							Id = customerId,
							NewName = request.NewName,
							Transgender = request.Transgender,
							NewBirthDate = request.NewBirthDate
						});

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<UpdateBasicProfileResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new UpdateBasicProfileResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer basic profile was updated successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 更新当前登录客户的联系方式。
		/// </summary>
		[HttpPost("UpdateContactInfo")]
		public Task<ActionResult<UpdateContactInfoResponse>> UpdateContactInfo([FromBody] UpdateContactInfoRequest request)
		{
			return ExecuteWithLoggingAsync<UpdateContactInfoResponse>(
				nameof(UpdateContactInfo),
				async () =>
				{
					if (!TryGetCurrentCustomerId(out var customerId, out var errorMessage))
					{
						return CreateErrorResult<UpdateContactInfoResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							errorMessage);
					}

					var result = await _customerService.UpdateContactInfo(
						new UpdateContactInfoRequest
						{
							CustomerId = customerId,
							NewPhoneNumber = request.NewPhoneNumber,
							NewEmail = request.NewEmail,
							NewAddress = request.NewAddress
						});

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)CustomerErrorCode.DuplicateCustomer
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<UpdateContactInfoResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new UpdateContactInfoResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer contact information was updated successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 更新当前登录客户的身份证件信息。
		/// </summary>
		[HttpPost("UpdateIdentityDocument")]
		public Task<ActionResult<UpdateIdentityDocumentResponse>> UpdateIdentityDocument([FromBody] UpdateIdentityDocumentRequest request)
		{
			return ExecuteWithLoggingAsync<UpdateIdentityDocumentResponse>(
				nameof(UpdateIdentityDocument),
				async () =>
				{
					if (!TryGetCurrentCustomerId(out var customerId, out var errorMessage))
					{
						return CreateErrorResult<UpdateIdentityDocumentResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							errorMessage);
					}

					var result = await _customerService.UpdateIdentityDocument(
						new UpdateIdentityDocumentRequest
						{
							CustomerId = customerId,
							NewIdType = request.NewIdType,
							NewIdNumber = request.NewIdNumber
						});

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<UpdateIdentityDocumentResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new UpdateIdentityDocumentResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer identity document was updated successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 注销当前登录客户。
		/// </summary>
		[HttpPost("RemoveCustomer")]
		public Task<ActionResult<RemoveCustomerResponse>> RemoveCustomer([FromBody] RemoveCustomerRequest request)
		{
			return ExecuteWithLoggingAsync<RemoveCustomerResponse>(
				nameof(RemoveCustomer),
				async () =>
				{
					if (!TryGetCurrentCustomerId(out var customerId, out var errorMessage))
					{
						return CreateErrorResult<RemoveCustomerResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							errorMessage);
					}

					var result = await _customerService.RemoveCustomer(
						new RemoveCustomerRequest
						{
							Id = customerId,
							IsDeleted = true,
							DeletedReason = request.DeletedReason
						});

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)CustomerErrorCode.CustomerHasNonClosedAccounts
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<RemoveCustomerResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new RemoveCustomerResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer account was removed successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 更新指定客户的 KYC 审核状态。
		/// </summary>
		[HttpPost("UpdateKycStatus")]
		public Task<ActionResult<UpdateKycStatusResponse>> UpdateKycStatus([FromBody] UpdateKycStatusRequest request)
		{
			return ExecuteWithLoggingAsync<UpdateKycStatusResponse>(
				nameof(UpdateKycStatus),
				async () =>
				{
					if (!_currentCustomer.IsAuthenticated)
					{
						return CreateErrorResult<UpdateKycStatusResponse>(
							StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.UnauthorizedAccess,
							"The current user is not authenticated.");
					}

					if (!User.IsInRole("Admin"))
					{
						return CreateErrorResult<UpdateKycStatusResponse>(
							StatusCodes.Status403Forbidden,
							(int)CustomerErrorCode.UnauthorizedAccess,
							"Administrator permission is required.");
					}

					var result = await _customerService.UpdateKycStatus(
						new UpdateKycStatusRequest
						{
							CustomerId = request.CustomerId,
							KycStatus = request.KycStatus
						});

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)CustomerErrorCode.KYCVerificationFailed
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<UpdateKycStatusResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new UpdateKycStatusResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The customer KYC status was updated successfully."
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 获取当前登录客户的有效 ID，并在失败时提供错误消息。
		/// </summary>
		private bool TryGetCurrentCustomerId(out int customerId, out string errorMessage)
		{
			customerId = 0;

			if (!_currentCustomer.IsAuthenticated)
			{
				errorMessage = "The current customer is not authenticated.";
				return false;
			}

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				errorMessage = "Failed to retrieve the current customer ID.";
				return false;
			}

			if (customerId <= 0)
			{
				errorMessage = "The current customer ID is invalid.";
				return false;
			}

			errorMessage = string.Empty;
			return true;
		}

		/// <summary>
		/// 构造错误响应，并按 HTTP 状态码确定日志结果。
		/// </summary>
		private (ActionResult<TResponse> Response, LogOutcome Outcome, int? ErrorCode)CreateErrorResult<TResponse>(int statusCode, int errorCode, string? message)
		{
			var outcome = statusCode == StatusCodes.Status500InternalServerError
				? LogOutcome.Failed
				: LogOutcome.Rejected;

			var response = new ErrorResponse
			{
				Success = false,
				Code = errorCode,
				Message = statusCode == StatusCodes.Status500InternalServerError
					? "An internal server error occurred."
					: message ?? string.Empty
			};

			return (StatusCode(statusCode, response), outcome, errorCode);
		}

		/// <summary>
		/// 执行 Controller 操作，记录 START、END、执行耗时和操作结果。
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