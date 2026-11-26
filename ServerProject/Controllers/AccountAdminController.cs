using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<AccountAdminController> _logger;

		public AccountAdminController(
			IAccountAdminService accountAdminService,
			ICurrentCustomer currentCustomer,
			ILogger<AccountAdminController> logger)
		{
			_accountAdminService = accountAdminService;
			_currentCustomer = currentCustomer;
			_logger = logger;
		}

		/// <summary>
		/// 查询指定账户的管理详情。
		/// </summary>
		[HttpPost("GetAccount")]
		public Task<ActionResult<GetAccountForAdminResponse>> GetAccount([FromBody] GetAccountForAdminRequest request)
		{
			return ExecuteWithLoggingAsync<GetAccountForAdminResponse>(
				nameof(GetAccount),
				async () =>
				{
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

						return CreateErrorResult<GetAccountForAdminResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetAccountForAdminResponse
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
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 查询指定客户的账户列表。
		/// </summary>
		[HttpPost("GetCustomerAccounts")]
		public Task<ActionResult<GetCustomerAccountsResponse>> GetCustomerAccounts([FromBody] GetCustomerAccountsRequest request)
		{
			return ExecuteWithLoggingAsync<GetCustomerAccountsResponse>(
				nameof(GetCustomerAccounts),
				async () =>
				{
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

						return CreateErrorResult<GetCustomerAccountsResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetCustomerAccountsResponse
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
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 查询指定的账户操作日志。
		/// </summary>
		[HttpPost("GetAccountOperationLog")]
		public Task<ActionResult<GetAccountOperationLogResponse>> GetAccountOperationLog([FromBody] GetAccountOperationLogRequest request)
		{
			return ExecuteWithLoggingAsync<GetAccountOperationLogResponse>(
				nameof(GetAccountOperationLog),
				async () =>
				{
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

						return CreateErrorResult<GetAccountOperationLogResponse>(
							statusCode, result.Code, result.Message);
					}

					// Service 的成功分支会填充 Log。
					var log = result.Log!;

					var response = new GetAccountOperationLogResponse
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
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 查询指定账户的操作日志列表。
		/// </summary>
		[HttpPost("GetAccountOperationLogs")]
		public Task<ActionResult<GetAccountOperationLogsResponse>> GetAccountOperationLogs([FromBody] GetAccountOperationLogsRequest request)
		{
			return ExecuteWithLoggingAsync<GetAccountOperationLogsResponse>(
				nameof(GetAccountOperationLogs),
				async () =>
				{
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

						return CreateErrorResult<GetAccountOperationLogsResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetAccountOperationLogsResponse
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
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 冻结指定账户。
		/// </summary>
		/// <param name="request">冻结账户请求对象，包含账户ID和冻结原因等信息。</param>
		/// <returns>返回冻结账户的响应对象，包含操作结果和相关信息。</returns>
		[HttpPost("FreezeAccount")]
		public Task<ActionResult<FreezeAccountResponse>> FreezeAccount([FromBody] FreezeAccountRequest request)
		{
			return ExecuteWithLoggingAsync<FreezeAccountResponse>(
				nameof(FreezeAccount),
				async () =>
				{
					var result = await _accountAdminService.FreezeAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)AccountErrorCode.AccountNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyClosed
								or (int)AccountErrorCode.AccountStatusInvalid
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<FreezeAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new FreezeAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was frozen successfully.",
						AccountId = result.AccountId,
						Status = result.Status,
						UpdateDate = result.UpdateDate
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 解冻指定账户。
		/// </summary>
		/// <param name="request">解冻账户请求对象，包含账户ID和解冻原因等信息。</param>
		/// <returns>返回解冻账户的响应对象，包含操作结果和相关信息。</returns>
		[HttpPost("UnfreezeAccount")]
		public Task<ActionResult<UnfreezeAccountResponse>> UnfreezeAccount([FromBody] UnfreezeAccountRequest request)
		{
			return ExecuteWithLoggingAsync<UnfreezeAccountResponse>(
				nameof(UnfreezeAccount),
				async () =>
				{
					var result = await _accountAdminService.UnfreezeAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)AccountErrorCode.AccountNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyClosed
								or (int)AccountErrorCode.AccountStatusInvalid
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<UnfreezeAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new UnfreezeAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was unfrozen successfully.",
						AccountId = result.AccountId,
						Status = result.Status,
						UpdateDate = result.UpdateDate
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 管理员强制关闭指定账户。
		/// </summary>
		/// <param name="request">包含账户ID和强制关闭理由的请求对象。</param>
		/// <returns>返回强制关闭指定账户的响应对象，包含操作结果和相关信息。</returns>
		[HttpPost("ForceCloseAccount")]
		public Task<ActionResult<ForceCloseAccountResponse>> ForceCloseAccount([FromBody] ForceCloseAccountRequest request)
		{
			return ExecuteWithLoggingAsync<ForceCloseAccountResponse>(
				nameof(ForceCloseAccount),
				async () =>
				{
					var result = await _accountAdminService.ForceCloseAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)AccountErrorCode.AccountNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyClosed
								or (int)AccountErrorCode.AccountBalanceNotZero
								or (int)AccountErrorCode.AccountStatusInvalid
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<ForceCloseAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new ForceCloseAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was force closed successfully.",
						AccountId = result.AccountId,
						Status = result.Status,
						CloseDate = result.CloseDate,
						UpdateDate = result.UpdateDate
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 检查当前用户是否具有管理员权限。
		/// </summary>
		/// <returns>权限不足时的错误响应，或验证通过时的 null。</returns>
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

		/// <summary>
		/// 构造错误响应，并按 HTTP 状态码确定日志结果。
		/// </summary>
		private (ActionResult<TResponse> Response, LogOutcome Outcome, int? ErrorCode)CreateErrorResult<TResponse>(int statusCode, int errorCode, string? message)
		{
			var outcome = statusCode >= StatusCodes.Status500InternalServerError
				? LogOutcome.Failed
				: LogOutcome.Rejected;

			var response = new ErrorResponse
			{
				Success = false,
				Code = errorCode,
				// 数据库或操作日志写入失败时，对外返回通用消息。
				Message = statusCode == StatusCodes.Status500InternalServerError
					? "An internal server error occurred."
					: message ?? string.Empty
			};

			return (StatusCode(statusCode, response), outcome, errorCode);
		}

		/// <summary>
		/// 检查管理员权限并执行 Action，记录 START、END、耗时和结果。
		/// </summary>
		/// <typeparam name="TResponse">操作成功时的响应类型。</typeparam>
		/// <param name="operation">写入日志的 Action 名称。</param>
		/// <param name="execute">权限验证通过后执行的异步委托。</param>
		/// <returns>权限检查的错误响应，或委托返回的 Action 响应。</returns>
		/// <remarks>
		/// 如果权限检查或委托抛出异常，结束日志会记录
		/// <see cref="LogOutcome.Failed"/>，异常继续向调用方传播。
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
				if (GetAdminAccessError() is { } accessError)
				{
					outcome = LogOutcome.Rejected;
					errorCode = (int)CustomerErrorCode.UnauthorizedAccess;
					return accessError;
				}

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