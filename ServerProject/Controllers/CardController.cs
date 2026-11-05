using Microsoft.AspNetCore.Mvc;
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

		public CardController(ICardService cardService)
		{
			_cardService = cardService;
		}

		/// <summary>
		/// 查询当前客户的卡片。
		/// </summary>
		[HttpPost("GetMyCards")]
		public async Task<ActionResult<GetMyCardsResponse>> GetMyCards()
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
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

			return Ok(response);
		}

		/// <summary>
		/// 冻结当前客户的指定卡片。
		/// </summary>
		[HttpPost("BlockCard")]
		public async Task<ActionResult<BlockCardResponse>> BlockCard([FromBody] BlockCardRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode switch
					{
						StatusCodes.Status500InternalServerError
							=> "An internal server error occurred.",

						_ when result.Code ==
							(int)CardErrorCode.CardNotFoundOrAccessDenied
							=> "The card does not exist or access is denied.",

						_ => result.Message ?? string.Empty
					}
				};

				return StatusCode(statusCode, errorResponse);
			}

			return Ok(new BlockCardResponse
			{
				Success = result.Success,
				Code = result.Code,
				Message = "The card is blocked.",
				CardId = result.CardId,
				Status = result.Status
			});
		}

		/// <summary>
		/// 为当前客户的指定账户发卡。
		/// </summary>
		[HttpPost("IssueCard")]
		public async Task<ActionResult<IssueCardResponse>> IssueCard([FromBody] IssueCardRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				};

				return StatusCode(statusCode, errorResponse);
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

			return StatusCode(StatusCodes.Status201Created, response);
		}

		/// <summary>
		/// 修改当前客户指定卡片的 PIN。
		/// </summary>
		[HttpPost("ChangeCardPin")]
		public async Task<ActionResult<ChangeCardPinResponse>> ChangeCardPin([FromBody] ChangeCardPinRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode switch
					{
						StatusCodes.Status500InternalServerError
							=> "An internal server error occurred.",

						_ when result.Code ==
							(int)CardErrorCode.CardNotFoundOrAccessDenied
							=> "The card does not exist or access is denied.",

						_ => result.Message ?? string.Empty
					}
				};

				return StatusCode(statusCode, errorResponse);
			}

			return Ok(new ChangeCardPinResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The card PIN was changed successfully.",
				CardId = result.CardId
			});
		}

		/// <summary>
		/// 替换当前客户的指定卡片。
		/// </summary>
		[HttpPost("ReplaceCard")]
		public async Task<ActionResult<ReplaceCardResponse>> ReplaceCard([FromBody] ReplaceCardRequest request)
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

				var errorResponse = new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode switch
					{
						StatusCodes.Status500InternalServerError
							=> "An internal server error occurred.",

						_ when result.Code ==
							(int)CardErrorCode.CardNotFoundOrAccessDenied
							=> "The card does not exist or access is denied.",

						_ => result.Message ?? string.Empty
					}
				};

				return StatusCode(statusCode, errorResponse);
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

			return StatusCode(StatusCodes.Status201Created, response);
		}
	}
}