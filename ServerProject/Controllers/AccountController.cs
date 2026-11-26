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
	[Route("api/accounts")]
	public class AccountController : ControllerBase
	{
		private readonly IAccountService _accountService;
		private readonly ILogger<AccountController> _logger;

		public AccountController(
			IAccountService accountService,
			ILogger<AccountController> logger)
		{
			_accountService = accountService;
			_logger = logger;
		}

		/// <summary>
		/// 查询当前客户的账户列表。
		/// </summary>
		[HttpPost("GetMyAccounts")]
		public Task<ActionResult<GetMyAccountsResponse>> GetMyAccounts([FromBody] GetMyAccountsRequest request)
		{
			return ExecuteWithLoggingAsync<GetMyAccountsResponse>(
				nameof(GetMyAccounts),
				async () =>
				{
					var result = await _accountService.GetMyAccounts(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<GetMyAccountsResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetMyAccountsResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Accounts retrieved successfully.",
						Accounts = result.Accounts
							.Select(account => new AccountItemResponse
							{
								AccountId = account.AccountId,
								AccountType = account.AccountType,
								Balance = account.Balance,
								Currency = account.Currency,
								Status = account.Status,
								OpenDate = account.OpenDate,
								CloseDate = account.CloseDate,
								UpdateDate = account.UpdateDate
							})
							.ToList()
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 查询当前客户的指定账户。
		/// </summary>
		[HttpPost("GetMyAccount")]
		public Task<ActionResult<GetMyAccountResponse>> GetMyAccount([FromBody] GetMyAccountRequest request)
		{
			return ExecuteWithLoggingAsync<GetMyAccountResponse>(
				nameof(GetMyAccount),
				async () =>
				{
					var result = await _accountService.GetMyAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)AccountErrorCode.AccountNotFound
								or (int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<GetMyAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetMyAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Account retrieved successfully.",
						Account = new AccountItemResponse
						{
							AccountId = result.AccountId,
							AccountType = result.AccountType,
							Balance = result.Balance,
							Currency = result.Currency,
							Status = result.Status,
							OpenDate = result.OpenDate,
							CloseDate = result.CloseDate,
							UpdateDate = result.UpdateDate
						}
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 查询当前客户指定账户的余额。
		/// </summary>
		[HttpPost("GetMyAccountBalance")]
		public Task<ActionResult<GetMyAccountBalanceResponse>> GetMyAccountBalance([FromBody] GetMyAccountBalanceRequest request)
		{
			return ExecuteWithLoggingAsync<GetMyAccountBalanceResponse>(
				nameof(GetMyAccountBalance),
				async () =>
				{
					var result = await _accountService.GetMyAccountBalance(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)AccountErrorCode.AccountNotFound
								or (int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyClosed
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<GetMyAccountBalanceResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new GetMyAccountBalanceResponse
					{
						Success = true,
						Code = result.Code,
						Message = "Account balance retrieved successfully.",
						AccountId = result.AccountId,
						Balance = result.Balance,
						Currency = result.Currency,
						Status = result.Status,
						UpdateDate = result.UpdateDate
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}

		/// <summary>
		/// 为当前客户开立账户。
		/// </summary>
		[HttpPost("OpenAccount")]
		public Task<ActionResult<OpenAccountResponse>> OpenAccount([FromBody] OpenAccountRequest request)
		{
			return ExecuteWithLoggingAsync<OpenAccountResponse>(
				nameof(OpenAccount),
				async () =>
				{
					var result = await _accountService.OpenAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								or (int)AccountErrorCode.InvalidAccountType
								or (int)AccountErrorCode.InvalidCurrency
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyExists
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<OpenAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new OpenAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was opened successfully.",
						AccountId = result.AccountId,
						AccountType = result.AccountType,
						Balance = result.Balance,
						Currency = result.Currency,
						Status = result.Status,
						OpenDate = result.OpenDate
					};

					return (
						StatusCode(StatusCodes.Status201Created, response),
						LogOutcome.Completed,
						null);
				});
		}

		/// <summary>
		/// 为当前客户开立账户并存入初始金额。
		/// </summary>
		[HttpPost("OpenAccountWithInitialDeposit")]
		public Task<ActionResult<OpenAccountWithInitialDepositResponse>> OpenAccountWithInitialDeposit([FromBody] OpenAccountWithInitialDepositRequest request)
		{
			return ExecuteWithLoggingAsync<OpenAccountWithInitialDepositResponse>(
				nameof(OpenAccountWithInitialDeposit),
				async () =>
				{
					var result = await _accountService.OpenAccountWithInitialDeposit(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								or (int)AccountErrorCode.InvalidAccountType
								or (int)AccountErrorCode.InvalidCurrency
								or (int)AccountErrorCode.InvalidInitialDepositAmount
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyExists
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<OpenAccountWithInitialDepositResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new OpenAccountWithInitialDepositResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was opened with an initial deposit.",
						AccountId = result.AccountId,
						AccountType = result.AccountType,
						Balance = result.Balance,
						Currency = result.Currency,
						Status = result.Status,
						OpenDate = result.OpenDate,
						InitialDepositTransactionId = result.InitialDepositTransactionId,
						InitialDepositAmount = result.InitialDepositAmount,
						TransactionStatus = result.TransactionStatus,
						TransactionTime = result.TransactionTime
					};

					return (
						StatusCode(StatusCodes.Status201Created, response),
						LogOutcome.Completed,
						null);
				});
		}

		/// <summary>
		/// 关闭当前客户的账户。
		/// </summary>
		[HttpPost("CloseMyAccount")]
		public Task<ActionResult<CloseMyAccountResponse>> CloseMyAccount([FromBody] CloseMyAccountRequest request)
		{
			return ExecuteWithLoggingAsync<CloseMyAccountResponse>(
				nameof(CloseMyAccount),
				async () =>
				{
					var result = await _accountService.CloseMyAccount(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)AccountErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,
							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,
							(int)CustomerErrorCode.InvalidCustomerData
								=> StatusCodes.Status403Forbidden,
							(int)AccountErrorCode.AccountNotFound
								or (int)CustomerErrorCode.CustomerNotFound
								=> StatusCodes.Status404NotFound,
							(int)AccountErrorCode.AccountAlreadyClosed
								or (int)AccountErrorCode.AccountNotAvailable
								or (int)AccountErrorCode.AccountBalanceNotZero
								or (int)AccountErrorCode.AccountStatusInvalid
								=> StatusCodes.Status409Conflict,
							_ => StatusCodes.Status500InternalServerError
						};

						return CreateErrorResult<CloseMyAccountResponse>(
							statusCode, result.Code, result.Message);
					}

					var response = new CloseMyAccountResponse
					{
						Success = true,
						Code = result.Code,
						Message = "The account was closed successfully.",
						AccountId = result.AccountId,
						Status = result.Status,
						CloseDate = result.CloseDate,
						UpdateDate = result.UpdateDate
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
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