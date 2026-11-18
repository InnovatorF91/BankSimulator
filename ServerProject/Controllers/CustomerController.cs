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

		public CustomerController(ICustomerService customerService,ICurrentCustomer currentCustomer)
		{
			_customerService = customerService;
			_currentCustomer = currentCustomer;
		}

		/// <summary>
		/// 注册新客户。
		/// </summary>
		[HttpPost("RegisterCustomer")]
		public async Task<ActionResult<RegisterCustomerResponse>> RegisterCustomer([FromBody] RegisterCustomerRequest request)
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			var response = new RegisterCustomerResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer was registered successfully.",
				CustomerId = result.CustomerId
			};

			return StatusCode(StatusCodes.Status201Created, response);
		}

		/// <summary>
		/// 查询当前登录客户的资料。
		/// </summary>
		[HttpPost("GetMyProfile")]
		public async Task<ActionResult<GetMyProfileResponse>> GetMyProfile()
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer is not authenticated."
				});
			}

			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Failed to retrieve the current customer ID."
				});
			}

			if (customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer ID is invalid."
				});
			}

			var result = await _customerService.GetCustomer(
				new GetCustomerRequest
				{
					Id = customerId
				});

			if (!result.Success)
			{
				var statusCode = result.Code switch
				{
					(int)CustomerErrorCode.CustomerNotFound
						=> StatusCodes.Status404NotFound,

					_ => StatusCodes.Status500InternalServerError
				};

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new GetMyProfileResponse
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
			});
		}

		/// <summary>
		/// 更新当前登录客户的基本资料。
		/// </summary>
		[HttpPost("UpdateBasicProfile")]
		public async Task<ActionResult<UpdateBasicProfileResponse>> UpdateBasicProfile([FromBody] UpdateBasicProfileRequest request)
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer is not authenticated."
				});
			}

			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Failed to retrieve the current customer ID."
				});
			}

			if (customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer ID is invalid."
				});
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new UpdateBasicProfileResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer basic profile was updated successfully."
			});
		}

		/// <summary>
		/// 更新当前登录客户的联系方式。
		/// </summary>
		[HttpPost("UpdateContactInfo")]
		public async Task<ActionResult<UpdateContactInfoResponse>> UpdateContactInfo([FromBody] UpdateContactInfoRequest request)
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer is not authenticated."
				});
			}

			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Failed to retrieve the current customer ID."
				});
			}

			if (customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer ID is invalid."
				});
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new UpdateContactInfoResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer contact information was updated successfully."
			});
		}

		/// <summary>
		/// 更新当前登录客户的身份证件信息。
		/// </summary>
		[HttpPost("UpdateIdentityDocument")]
		public async Task<ActionResult<UpdateIdentityDocumentResponse>> UpdateIdentityDocument([FromBody] UpdateIdentityDocumentRequest request)
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer is not authenticated."
				});
			}

			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Failed to retrieve the current customer ID."
				});
			}

			if (customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer ID is invalid."
				});
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new UpdateIdentityDocumentResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer identity document was updated successfully."
			});
		}

		/// <summary>
		/// 注销当前登录客户。
		/// </summary>
		[HttpPost("RemoveCustomer")]
		public async Task<ActionResult<RemoveCustomerResponse>> RemoveCustomer([FromBody] RemoveCustomerRequest request)
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer is not authenticated."
				});
			}

			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Failed to retrieve the current customer ID."
				});
			}

			if (customerId <= 0)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current customer ID is invalid."
				});
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new RemoveCustomerResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer account was removed successfully."
			});
		}

		/// <summary>
		/// 更新指定客户的 KYC 审核状态。
		/// </summary>
		[HttpPost("UpdateKycStatus")]
		public async Task<ActionResult<UpdateKycStatusResponse>> UpdateKycStatus([FromBody] UpdateKycStatusRequest request)
		{
			if (!_currentCustomer.IsAuthenticated)
			{
				return Unauthorized(new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "The current user is not authenticated."
				});
			}

			if (!User.IsInRole("Admin"))
			{
				return StatusCode(
					StatusCodes.Status403Forbidden,
					new ErrorResponse
					{
						Success = false,
						Code = (int)CustomerErrorCode.UnauthorizedAccess,
						Message = "Administrator permission is required."
					});
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new UpdateKycStatusResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer KYC status was updated successfully."
			});
		}
	}
}