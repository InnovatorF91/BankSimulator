using Microsoft.AspNetCore.Mvc;
using ServerProject.Common;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/admin/accounts")]
	public class AccountAdminController : ControllerBase
	{
		private readonly IAccountAdminService _accountAdminService;
		private readonly ICurrentCustomer _currentCustomer;

		public AccountAdminController(
			IAccountAdminService accountAdminService,
			ICurrentCustomer currentCustomer)
		{
			_accountAdminService = accountAdminService;
			_currentCustomer = currentCustomer;
		}

		/// <summary>
		/// 查询指定账户的管理详情。
		/// </summary>
		[HttpPost("GetAccount")]
		public async Task<ActionResult<GetAccountForAdminResponse>> GetAccount([FromBody] GetAccountForAdminRequest request)
		{
			var accessError = GetAdminAccessError();
			if (accessError != null)
			{
				return accessError;
			}

			var result = await _accountAdminService.GetAccount(request);

			if (!result.Success)
			{
				var statusCode = result.Code switch
				{
					(int)AccountErrorCode.ValidationFailed
						=> StatusCodes.Status400BadRequest,

					(int)AccountErrorCode.AccountNotFound
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

			return Ok(new GetAccountForAdminResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The account was retrieved successfully.",
				AccountId = result.AccountId,
				CustomerId = result.CustomerId,
				AccountType = result.AccountType,
				Balance = result.Balance,
				Currency = result.Currency,
				Status = result.Status,
				OpenDate = result.OpenDate,
				CloseDate = result.CloseDate,
				UpdateDate = result.UpdateDate
			});
		}

		/// <summary>
		/// 查询指定客户的账户列表。
		/// </summary>
		[HttpPost("GetCustomerAccounts")]
		public async Task<ActionResult<GetCustomerAccountsResponse>> GetCustomerAccounts([FromBody] GetCustomerAccountsRequest request)
		{
			if (GetAdminAccessError() is { } accessError)
			{
				return accessError;
			}

			var result = await _accountAdminService.GetCustomerAccounts(request);

			if (!result.Success)
			{
				var statusCode = result.Code switch
				{
					(int)AccountErrorCode.ValidationFailed
						or (int)CustomerErrorCode.InvalidCustomerData
						=> StatusCodes.Status400BadRequest,

					(int)CustomerErrorCode.CustomerNotFound
						=> StatusCodes.Status404NotFound,

					(int)AccountErrorCode.InvalidCurrency
						or (int)AccountErrorCode.UnableToOperateDb
						=> StatusCodes.Status500InternalServerError,

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

			return Ok(new GetCustomerAccountsResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The customer accounts were retrieved successfully.",
				CustomerId = result.CustomerId,
				Accounts = result.Accounts.Select(account => new AccountItemResponse
				{
					AccountId = account.AccountId,
					AccountType = account.AccountType,
					Balance = account.Balance,
					Currency = account.Currency,
					Status = account.Status,
					OpenDate = account.OpenDate,
					CloseDate = account.CloseDate,
					UpdateDate = account.UpdateDate
				}).ToList()
			});
		}

		/// <summary>
		/// 查询指定的账户操作日志。
		/// </summary>
		[HttpPost("GetAccountOperationLog")]
		public async Task<ActionResult<GetAccountOperationLogResponse>> GetAccountOperationLog([FromBody] GetAccountOperationLogRequest request)
		{
			if (GetAdminAccessError() is { } accessError)
			{
				return accessError;
			}

			var result = await _accountAdminService.GetAccountOperationLog(request);

			if (!result.Success)
			{
				var statusCode = result.Code switch
				{
					(int)AccountErrorCode.ValidationFailed
						=> StatusCodes.Status400BadRequest,

					(int)AccountErrorCode.AccountOperationLogNotFound
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

			// Service 的成功分支会填充 Log。
			var log = result.Log!;

			return Ok(new GetAccountOperationLogResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The account operation log was retrieved successfully.",
				Log = new AccountOperationLogItemResponse
				{
					OperationLogId = log.OperationLogId,
					AccountId = log.AccountId,
					CustomerId = log.CustomerId,
					OperationType = log.OperationType,
					OldStatus = log.OldStatus,
					NewStatus = log.NewStatus,
					Reason = log.Reason,
					OperatedBy = log.OperatedBy,
					OperatedAt = log.OperatedAt
				}
			});
		}

		/// <summary>
		/// 查询指定账户的操作日志列表。
		/// </summary>
		[HttpPost("GetAccountOperationLogs")]
		public async Task<ActionResult<GetAccountOperationLogsResponse>> GetAccountOperationLogs([FromBody] GetAccountOperationLogsRequest request)
		{
			if (GetAdminAccessError() is { } accessError)
			{
				return accessError;
			}

			var result = await _accountAdminService.GetAccountOperationLogs(request);

			if (!result.Success)
			{
				var statusCode = result.Code switch
				{
					(int)AccountErrorCode.ValidationFailed
						=> StatusCodes.Status400BadRequest,

					(int)AccountErrorCode.AccountNotFound
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

			return Ok(new GetAccountOperationLogsResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The account operation logs were retrieved successfully.",
				AccountId = result.AccountId,
				Logs = result.Logs.Select(log => new AccountOperationLogItemResponse
				{
					OperationLogId = log.OperationLogId,
					AccountId = log.AccountId,
					CustomerId = log.CustomerId,
					OperationType = log.OperationType,
					OldStatus = log.OldStatus,
					NewStatus = log.NewStatus,
					Reason = log.Reason,
					OperatedBy = log.OperatedBy,
					OperatedAt = log.OperatedAt
				}).ToList()
			});
		}

		/// <summary>
		/// 检查当前用户是否具有管理员权限，如果没有，则返回相应的错误响应。
		/// </summary>
		/// <returns>返回一个 ActionResult 对象，如果当前用户没有管理员权限，则返回错误响应；否则返回 null。</returns>
		private ActionResult? GetAdminAccessError()
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
				return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
				{
					Success = false,
					Code = (int)CustomerErrorCode.UnauthorizedAccess,
					Message = "Administrator permission is required."
				});
			}

			return null;
		}
	}
}