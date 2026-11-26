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
	[Route("api/cards")]
	public class CardController : ControllerBase
	{
		private readonly ICardService _cardService;
		private readonly ILogger<CardController> _logger;

		public CardController(ICardService cardService, ILogger<CardController> logger)
		{
			_cardService = cardService;
			_logger = logger;
		}

		/// <summary>
		/// 查询当前客户的卡片。
		/// </summary>
		[HttpPost("GetMyCards")]
		public Task<ActionResult<GetMyCardsResponse>> GetMyCards()
		{
			return ExecuteWithLoggingAsync<GetMyCardsResponse>(nameof(GetMyCards), async () =>
			{
				var result = await _cardService.GetMyCards();

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,
						(int)CustomerErrorCode.CustomerNotFound
							=> StatusCodes.Status404NotFound,
						(int)CustomerErrorCode.InvalidCustomerData
							=> StatusCodes.Status403Forbidden,
						_ => StatusCodes.Status500InternalServerError
					};

					return CreateErrorResult<GetMyCardsResponse>(
						statusCode, result.Code, result.Message);
				}

				var response = new GetMyCardsResponse
				{
					Success = result.Success,
					Code = result.Code,
					Message = "Cards retrieved successfully.",
					Cards = result.Cards
						.Select(card => new CardItemResponse
						{
							CardId = card.CardId,
							AccountId = card.AccountId,
							MaskedCardNumber = card.MaskedCardNumber,
							ExpiryYear = card.ExpiryYear,
							ExpiryMonth = card.ExpiryMonth,
							CardType = card.CardType,
							Status = card.Status
						})
						.ToList()
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 冻结当前客户的指定卡片。
		/// </summary>
		[HttpPost("BlockCard")]
		public Task<ActionResult<BlockCardResponse>> BlockCard([FromBody] BlockCardRequest request)
		{
			return ExecuteWithLoggingAsync<BlockCardResponse>(nameof(BlockCard), async () =>
			{
				var result = await _cardService.BlockCard(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)CardErrorCode.ValidationFailed
							=> StatusCodes.Status400BadRequest,
						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,
						(int)CustomerErrorCode.InvalidCustomerData
							=> StatusCodes.Status403Forbidden,
						(int)CustomerErrorCode.CustomerNotFound
							or (int)CardErrorCode.CardNotFoundOrAccessDenied
							=> StatusCodes.Status404NotFound,
						(int)AccountErrorCode.AccountAlreadyClosed
							or (int)CardErrorCode.CardExpired
							or (int)CardErrorCode.CardStatusInvalid
							or (int)CardErrorCode.CardStateConflict
							=> StatusCodes.Status409Conflict,
						_ => StatusCodes.Status500InternalServerError
					};

					return CreateErrorResult<BlockCardResponse>(
						statusCode, result.Code, result.Message);
				}

				var response = new BlockCardResponse
				{
					Success = result.Success,
					Code = result.Code,
					Message = "The card is blocked.",
					CardId = result.CardId,
					Status = result.Status
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 为当前客户的指定账户发卡。
		/// </summary>
		[HttpPost("IssueCard")]
		public Task<ActionResult<IssueCardResponse>> IssueCard([FromBody] IssueCardRequest request)
		{
			return ExecuteWithLoggingAsync<IssueCardResponse>(nameof(IssueCard), async () =>
			{
				var result = await _cardService.IssueCard(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)CardErrorCode.ValidationFailed
							or (int)CardErrorCode.InvalidCardType
							or (int)CardErrorCode.InvalidPIN
							=> StatusCodes.Status400BadRequest,
						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,

						// InvalidCustomerData 和 AccountAccessDenied 都是 1002。
						(int)CustomerErrorCode.InvalidCustomerData
							=> StatusCodes.Status403Forbidden,
						(int)CustomerErrorCode.CustomerNotFound
							=> StatusCodes.Status404NotFound,
						(int)AccountErrorCode.AccountNotAvailable
							or (int)CardErrorCode.CardAlreadyExists
							=> StatusCodes.Status409Conflict,
						_ => StatusCodes.Status500InternalServerError
					};

					return CreateErrorResult<IssueCardResponse>(
						statusCode, result.Code, result.Message);
				}

				var card = result.Card;
				var response = new IssueCardResponse
				{
					Success = true,
					Code = result.Code,
					Message = "The card was issued successfully.",
					Card = new CardItemResponse
					{
						CardId = card.CardId,
						AccountId = card.AccountId,
						MaskedCardNumber = card.MaskedCardNumber,
						ExpiryYear = card.ExpiryYear,
						ExpiryMonth = card.ExpiryMonth,
						CardType = card.CardType,
						Status = card.Status
					}
				};

				return (
					StatusCode(StatusCodes.Status201Created, response),
					LogOutcome.Completed,
					null);
			});
		}

		/// <summary>
		/// 修改当前客户指定卡片的 PIN。
		/// </summary>
		[HttpPost("ChangeCardPin")]
		public Task<ActionResult<ChangeCardPinResponse>> ChangeCardPin([FromBody] ChangeCardPinRequest request)
		{
			return ExecuteWithLoggingAsync<ChangeCardPinResponse>(nameof(ChangeCardPin), async () =>
			{
				var result = await _cardService.ChangeCardPin(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)CardErrorCode.ValidationFailed
							or (int)CardErrorCode.InvalidPIN
							or (int)CardErrorCode.NewPINUnchanged
							or (int)CardErrorCode.CurrentPINIncorrect
							=> StatusCodes.Status400BadRequest,
						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,
						(int)CustomerErrorCode.InvalidCustomerData
							=> StatusCodes.Status403Forbidden,
						(int)CustomerErrorCode.CustomerNotFound
							or (int)CardErrorCode.CardNotFoundOrAccessDenied
							=> StatusCodes.Status404NotFound,
						(int)AccountErrorCode.AccountNotAvailable
							or (int)CardErrorCode.CardExpired
							or (int)CardErrorCode.CardStatusInvalid
							or (int)CardErrorCode.PINLocked
							=> StatusCodes.Status409Conflict,
						_ => StatusCodes.Status500InternalServerError
					};

					return CreateErrorResult<ChangeCardPinResponse>(
						statusCode, result.Code, result.Message);
				}

				var response = new ChangeCardPinResponse
				{
					Success = true,
					Code = result.Code,
					Message = "The card PIN was changed successfully.",
					CardId = result.CardId
				};

				return (Ok(response), LogOutcome.Completed, null);
			});
		}

		/// <summary>
		/// 替换当前客户的指定卡片。
		/// </summary>
		[HttpPost("ReplaceCard")]
		public Task<ActionResult<ReplaceCardResponse>> ReplaceCard([FromBody] ReplaceCardRequest request)
		{
			return ExecuteWithLoggingAsync<ReplaceCardResponse>(nameof(ReplaceCard), async () =>
			{
				var result = await _cardService.ReplaceCard(request);

				if (!result.Success)
				{
					var statusCode = result.Code switch
					{
						(int)CardErrorCode.ValidationFailed
							or (int)CardErrorCode.InvalidReplaceReason
							or (int)CardErrorCode.InvalidPIN
							=> StatusCodes.Status400BadRequest,
						(int)CustomerErrorCode.UnauthorizedAccess
							=> StatusCodes.Status401Unauthorized,
						(int)CustomerErrorCode.InvalidCustomerData
							=> StatusCodes.Status403Forbidden,
						(int)CustomerErrorCode.CustomerNotFound
							or (int)CardErrorCode.CardNotFoundOrAccessDenied
							=> StatusCodes.Status404NotFound,
						(int)AccountErrorCode.AccountNotAvailable
							or (int)CardErrorCode.CardAlreadyReplaced
							or (int)CardErrorCode.CardAlreadyDeactivated
							or (int)CardErrorCode.CardStatusInvalid
							or (int)CardErrorCode.CardStateConflict
							or (int)CardErrorCode.CardAlreadyExists
							=> StatusCodes.Status409Conflict,
						_ => StatusCodes.Status500InternalServerError
					};

					return CreateErrorResult<ReplaceCardResponse>(
						statusCode, result.Code, result.Message);
				}

				var card = result.Card;
				var response = new ReplaceCardResponse
				{
					Success = true,
					Code = result.Code,
					Message = "The card was replaced successfully.",
					OldCardId = result.OldCardId,
					Card = new CardItemResponse
					{
						CardId = card.CardId,
						AccountId = card.AccountId,
						MaskedCardNumber = card.MaskedCardNumber,
						ExpiryYear = card.ExpiryYear,
						ExpiryMonth = card.ExpiryMonth,
						CardType = card.CardType,
						Status = card.Status
					}
				};

				return (
					StatusCode(StatusCodes.Status201Created, response),
					LogOutcome.Completed,
					null);
			});
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
				Message = statusCode switch
				{
					StatusCodes.Status500InternalServerError
						=> "An internal server error occurred.",
					StatusCodes.Status404NotFound
						when errorCode == (int)CardErrorCode.CardNotFoundOrAccessDenied
						=> "The card does not exist or access is denied.",
					_ => message ?? string.Empty
				}
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