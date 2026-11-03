using Microsoft.AspNetCore.Mvc;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/transactions")]
	public class TransactionController : ControllerBase
	{
		private readonly ITransactionService _transactionService;

		public TransactionController(
			ITransactionService transactionService)
		{
			_transactionService = transactionService;
		}

		/// <summary>
		/// 获取当前客户的交易记录。
		/// </summary>
		/// <param name="request">当前客户交易记录查询请求。</param>
		/// <returns>当前客户交易记录查询回应。</returns>
		[HttpPost("GetMyTransactions")]
		public async Task<ActionResult<GetMyTransactionsResponse>>GetMyTransactions([FromBody] GetMyTransactionRequest request)
		{
			// 调用 Service，由 Service 验证当前客户及查询条件。
			var result =
				await _transactionService.GetMyTransactions(request);

			// 查询失败时，返回统一的错误回应。
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode ==
						StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
			}

			// 查询成功时，将 Service DTO 映射为对外 Response。
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

			return Ok(response);
		}

		/// <summary>
		/// 向当前客户的指定账户存款。
		/// </summary>
		/// <param name="request">存款请求。</param>
		/// <returns>存款结果回应。</returns>
		[HttpPost("Deposit")]
		public async Task<ActionResult<DepositResponse>> Deposit([FromBody] DepositRequest request)
		{
			// 由 Service 执行认证、业务校验及事务操作。
			var result = await _transactionService.Deposit(request);

			// 失败时只返回公共错误字段，避免返回默认交易数据。
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode ==
						StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
			}

			// 将成功的 Service DTO 映射为对外 Response。
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

			return Ok(response);
		}

		/// <summary>
		/// 从当前客户的指定账户取款。
		/// </summary>
		/// <param name="request">取款请求。</param>
		/// <returns>取款结果回应。</returns>
		[HttpPost("Withdraw")]
		public async Task<ActionResult<WithdrawResponse>> Withdraw([FromBody] WithdrawRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode ==
						StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
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

			return Ok(response);
		}

		/// <summary>
		/// 从当前客户的账户向指定账户转账。
		/// </summary>
		/// <param name="request">转账请求。</param>
		/// <returns>转账结果回应。</returns>
		[HttpPost("Transfer")]
		public async Task<ActionResult<TransferResponse>> Transfer([FromBody] TransferRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode ==
						StatusCodes.Status500InternalServerError
							? "An internal server error occurred."
							: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
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

			return Ok(response);
		}
	}
}
