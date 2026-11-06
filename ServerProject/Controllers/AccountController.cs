using Microsoft.AspNetCore.Mvc;
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

		public AccountController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		/// <summary>
		/// 查询当前客户的账户列表。
		/// </summary>
		[HttpPost("GetMyAccounts")]
		public async Task<ActionResult<GetMyAccountsResponse>> GetMyAccounts([FromBody] GetMyAccountsRequest request)
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

			return Ok(response);
		}

		/// <summary>
		/// 查询当前客户的指定账户。
		/// </summary>
		[HttpPost("GetMyAccount")]
		public async Task<ActionResult<GetMyAccountResponse>> GetMyAccount([FromBody] GetMyAccountRequest request)
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

			return Ok(new GetMyAccountResponse
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
			});
		}

		/// <summary>
		/// 查询当前客户指定账户的余额。
		/// </summary>
		[HttpPost("GetMyAccountBalance")]
		public async Task<ActionResult<GetMyAccountBalanceResponse>> GetMyAccountBalance([FromBody] GetMyAccountBalanceRequest request)
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

			return Ok(new GetMyAccountBalanceResponse
			{
				Success = true,
				Code = result.Code,
				Message = "Account balance retrieved successfully.",
				AccountId = result.AccountId,
				Balance = result.Balance,
				Currency = result.Currency,
				Status = result.Status,
				UpdateDate = result.UpdateDate
			});
		}

		/// <summary>
		/// 为当前客户开立账户。
		/// </summary>
		[HttpPost("OpenAccount")]
		public async Task<ActionResult<OpenAccountResponse>> OpenAccount([FromBody] OpenAccountRequest request)
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

			return StatusCode(StatusCodes.Status201Created, response);
		}

		/// <summary>
		/// 为当前客户开立账户并存入初始金额。
		/// </summary>
		[HttpPost("OpenAccountWithInitialDeposit")]
		public async Task<ActionResult<OpenAccountWithInitialDepositResponse>>OpenAccountWithInitialDeposit([FromBody] OpenAccountWithInitialDepositRequest request)
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
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

			return StatusCode(StatusCodes.Status201Created, response);
		}

		/// <summary>
		/// 关闭当前客户的账户。
		/// </summary>
		[HttpPost("CloseMyAccount")]
		public async Task<ActionResult<CloseMyAccountResponse>> CloseMyAccount([FromBody] CloseMyAccountRequest request)
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

				return StatusCode(statusCode, new ErrorResponse
				{
					Success = false,
					Code = result.Code,
					Message = statusCode == StatusCodes.Status500InternalServerError
						? "An internal server error occurred."
						: result.Message ?? string.Empty
				});
			}

			return Ok(new CloseMyAccountResponse
			{
				Success = true,
				Code = result.Code,
				Message = "The account was closed successfully.",
				AccountId = result.AccountId,
				Status = result.Status,
				CloseDate = result.CloseDate,
				UpdateDate = result.UpdateDate
			});
		}
	}
}