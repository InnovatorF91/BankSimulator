using Microsoft.AspNetCore.Mvc;
using ServerProject.Common;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;
using System.Diagnostics;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/transactions")]
	public class TransactionController : ControllerBase
	{
		private readonly ITransactionService _transactionService;

		private readonly ILogger<TransactionController> _logger;

		public TransactionController(
			ITransactionService transactionService, ILogger<TransactionController> logger)
		{
			_transactionService = transactionService;
			_logger = logger;
		}

		/// 获取当前客户的交易记录。
		/// </summary>
		/// <param name="request">当前客户交易记录查询请求。</param>
		/// <returns>当前客户交易记录查询回应。</returns>
		[HttpPost("GetMyTransactions")]
		public Task<ActionResult<GetMyTransactionsResponse>> GetMyTransactions([FromBody] GetMyTransactionRequest request)
		{
			return ExecuteWithLoggingAsync<GetMyTransactionsResponse>(
				nameof(GetMyTransactions),
				async () =>
				{
					var result = await _transactionService.GetMyTransactions(request);

					if (!result.Success)
					{
						var statusCode = result.Code switch
						{
							(int)TransactionErrorCode.ValidationFailed
								=> StatusCodes.Status400BadRequest,

							(int)CustomerErrorCode.UnauthorizedAccess
								=> StatusCodes.Status401Unauthorized,

							(int)TransactionErrorCode.SourceAccountAccessDenied
								=> StatusCodes.Status403Forbidden,

							(int)TransactionErrorCode.AccountNotAvailable
								=> StatusCodes.Status409Conflict,

							_ => StatusCodes.Status500InternalServerError
						};

						var outcome = statusCode == StatusCodes.Status500InternalServerError
							? LogOutcome.Failed
							: LogOutcome.Rejected;

						var errorResponse = new ErrorResponse
						{
							Success = false,
							Code = result.Code,
							Message = statusCode == StatusCodes.Status500InternalServerError
								? "An internal server error occurred."
								: result.Message ?? string.Empty
						};

						return (StatusCode(statusCode, errorResponse), outcome, result.Code);
					}

					var response = new GetMyTransactionsResponse
					{
						Success = result.Success,
						Code = result.Code,
						Message = "Transaction records retrieved successfully.",
						Transactions = result.Transactions
							.Select(transaction => new TransactionRecordResponse
							{
								TransactionId = transaction.TransactionId,
								AccountId = transaction.AccountId,
								TransactionType = transaction.TransactionType,
								AmountDelta = transaction.AmountDelta,
								RelatedAccount = transaction.RelatedAccount,
								CreatedAt = transaction.CreatedAt,
								Status = transaction.Status,
								GroupId = transaction.GroupId,
								Note = transaction.Note,
								Currency = transaction.Currency,
								BalanceBefore = transaction.BalanceBefore,
								BalanceAfter = transaction.BalanceAfter
							})
							.ToList()
					};

					return (Ok(response), LogOutcome.Completed, null);
				});
		}
		/// <summary>
		/// 向当前客户的指定账户存款。
		/// </summary>
		/// <param name="request">存款请求。</param>
		/// <returns>存款结果回应。</returns>
		[HttpPost("Deposit")]
		public Task<ActionResult<DepositResponse>> Deposit([FromBody] DepositRequest request)
		{
			return ExecuteWithLoggingAsync<DepositResponse>(nameof(Deposit), async () =>
			{
				var result = await _transactionService.Deposit(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)TransactionErrorCode.ValidationFailed
							=> StatusCodes.Status400BadRequest,

						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,

						(int)TransactionErrorCode.SourceAccountNotFound
							=> StatusCodes.Status404NotFound,

						(int)TransactionErrorCode.SourceAccountAccessDenied
							=> StatusCodes.Status403Forbidden,

						(int)TransactionErrorCode.AccountNotAvailable
							=> StatusCodes.Status409Conflict,

						_ => StatusCodes.Status500InternalServerError
					};

					var outcome = statusCode == StatusCodes.Status500InternalServerError
						? LogOutcome.Failed
						: LogOutcome.Rejected;

					var errorResponse = new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = statusCode == StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
					};

					return (StatusCode(statusCode, errorResponse), outcome, result.Code);
				}

				var response = new DepositResponse
				{
					Success = result.Success,
					Code = result.Code,
					Message = "Deposit completed successfully.",
					TransactionId = result.TransactionId,
					AccountId = result.AccountId,
					AmountDelta = result.AmountDelta,
					BalanceBefore = result.BalanceBefore,
					BalanceAfter = result.BalanceAfter,
					Status = result.Status,
					Currency = result.Currency,
					CreatedAt = result.CreatedAt
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 从当前客户的指定账户取款。
		/// </summary>
		/// <param name="request">取款请求。</param>
		/// <returns>取款结果回应。</returns>
		[HttpPost("Withdraw")]
		public Task<ActionResult<WithdrawResponse>> Withdraw([FromBody] WithdrawRequest request)
		{
			return ExecuteWithLoggingAsync<WithdrawResponse>(nameof(Withdraw), async () =>
			{
				var result = await _transactionService.Withdraw(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)TransactionErrorCode.ValidationFailed
							=> StatusCodes.Status400BadRequest,

						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,

						(int)TransactionErrorCode.SourceAccountNotFound
							=> StatusCodes.Status404NotFound,

						(int)TransactionErrorCode.SourceAccountAccessDenied
							=> StatusCodes.Status403Forbidden,

						(int)TransactionErrorCode.AccountNotAvailable
							=> StatusCodes.Status409Conflict,

						(int)TransactionErrorCode.InsufficientBalance
							=> StatusCodes.Status409Conflict,

						_ => StatusCodes.Status500InternalServerError
					};

					var outcome = statusCode == StatusCodes.Status500InternalServerError
						? LogOutcome.Failed
						: LogOutcome.Rejected;

					var errorResponse = new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = statusCode == StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
					};

					return (StatusCode(statusCode, errorResponse), outcome, result.Code);
				}

				var response = new WithdrawResponse
				{
					Success = result.Success,
					Code = result.Code,
					Message = "Withdrawal completed successfully.",
					TransactionId = result.TransactionId,
					AccountId = result.AccountId,
					AmountDelta = result.AmountDelta,
					BalanceBefore = result.BalanceBefore,
					BalanceAfter = result.BalanceAfter,
					Status = result.Status,
					Currency = result.Currency,
					CreatedAt = result.CreatedAt
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 从当前客户的账户向指定账户转账。
		/// </summary>
		/// <param name="request">转账请求。</param>
		/// <returns>转账结果回应。</returns>
		[HttpPost("Transfer")]
		public async Task<ActionResult<TransferResponse>> Transfer([FromBody] TransferRequest request)
		{
			return await ExecuteWithLoggingAsync<TransferResponse>(nameof(Transfer), async () =>
			{
				var result = await _transactionService.Transfer(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)TransactionErrorCode.ValidationFailed
							=> StatusCodes.Status400BadRequest,

						(int)TransactionErrorCode.SameAccountTransfer
							=> StatusCodes.Status400BadRequest,

						(int)TransactionErrorCode.CurrencyMismatch
							=> StatusCodes.Status400BadRequest,

						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,

						(int)TransactionErrorCode.SourceAccountNotFound
							=> StatusCodes.Status404NotFound,

						(int)TransactionErrorCode.TargetAccountNotFound
							=> StatusCodes.Status404NotFound,

						(int)TransactionErrorCode.SourceAccountAccessDenied
							=> StatusCodes.Status403Forbidden,

						(int)TransactionErrorCode.AccountNotAvailable
							=> StatusCodes.Status409Conflict,

						(int)TransactionErrorCode.InsufficientBalance
							=> StatusCodes.Status409Conflict,

						_ => StatusCodes.Status500InternalServerError
					};

					var outcome = statusCode == StatusCodes.Status500InternalServerError
						? LogOutcome.Failed
						: LogOutcome.Rejected;

					var errorResponse = new ErrorResponse
					{
						Success = false,
						Code = result.Code,
						Message = statusCode == StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
					};

					return (StatusCode(statusCode, errorResponse), outcome, result.Code);
				}

				var response = new TransferResponse
				{
					Success = result.Success,
					Code = result.Code,
					Message = "Transfer completed successfully.",
					GroupId = result.GroupId,
					SourceTransactionId = result.SourceTransactionId,
					DestinationTransactionId = result.DestinationTransactionId,
					SourceAccountId = result.SourceAccountId,
					DestinationAccountId = result.DestinationAccountId,
					AmountDelta = result.AmountDelta,
					SourceBalanceBefore = result.SourceBalanceBefore,
					SourceBalanceAfter = result.SourceBalanceAfter,
					Status = result.Status,
					Currency = result.Currency,
					CreatedAt = result.CreatedAt
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 执行 Controller 操作，记录 START、END、执行耗时和操作结果。
		/// </summary>
		/// <typeparam name="TResponse">操作成功时的响应类型。</typeparam>
		/// <param name="operation">写入日志的操作名称，通常通过 nameof 指定 Action 名称。</param>
		/// <param name="execute">执行操作的异步委托，返回 Action 响应、日志结果和可选的错误码。</param>
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

			// 未处理异常向上传播时，finally 会记录 Failed。
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
				_logger.LogEnd(
					outcome,
					stopwatch.ElapsedMilliseconds,
					errorCode,
					operation: operation);
			}
		}
	}
}
