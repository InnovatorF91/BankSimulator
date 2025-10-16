using Microsoft.AspNetCore.Mvc;
using ServerProject.Logics;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		/// <summary>
		/// 帳戶邏輯接口，用於處理帳戶相關的業務邏輯
		/// </summary>
		private readonly IAccountLogic _accountLogic;

		/// <summary>
		/// 帳戶控制器構造函數，注入帳戶邏輯接口
		/// </summary>
		/// <param name="accountLogic">帳戶邏輯</param>
		public AccountController(IAccountLogic accountLogic)
		{
			_accountLogic = accountLogic;
		}

		/// <summary>
		/// 獲取帳戶信息的API端點
		/// </summary>
		/// <param name="accountRequest">賬戶請求</param>
		/// <returns>賬戶回應</returns>
		[HttpPost("GetAccount")]
		public async Task<AccountResponse> GetAccount(AccountRequest accountRequest)
		{
			// 執行帳戶邏輯以獲取帳戶信息
			var response = new AccountResponse
			{
				Account = await _accountLogic.GetAccount(accountRequest.AccountId)
			};

			// 檢查是否成功獲取帳戶信息
			if (response.Account == null)
			{
				response.Success = false;
				response.Message = "Account not found.";
			}
			else
			{
				response.Success = true;
				response.Message = "Account retrieved successfully.";
			}

			return response;
		}

		/// <summary>
		/// 開立新帳戶的API端點
		/// </summary>
		/// <param name="accountRequest">需要開啓的新的賬戶請求</param>
		/// <returns>需要開啓的新的賬戶回應</returns>
		[HttpPost("OpenAccount")]
		public async Task<OpenAccountResponse> OpenAccount(OpenAccountRequest accountRequest)
		{
			// 執行帳戶邏輯以開立新帳戶
			var response = new OpenAccountResponse
			{
				Account = new AccountDto 
				{
					AccountId = await _accountLogic.OpenAccount(accountRequest)
				},
			};

			// 根據操作結果設置回應訊息和帳戶資料
			if (response.Account.AccountId != -1)
			{
				response.Message = "Account opened successfully.";
				response.Account = await _accountLogic.GetAccount(response.Account.AccountId);
			}
			else
			{
				response.Message = "Failed to open account.";
			}

			// 返回回應
			return response;
		}

		/// <summary>
		/// 關閉帳戶的API端點
		/// </summary>
		/// <param name="accountRequest">需要關閉的賬戶請求</param>
		/// <returns>需要關閉的賬戶回應</returns>
		[HttpPost("CloseAccount")]
		public async Task<CloseAccountResponse> CloseAccount(CloseAccountRequest accountRequest)
		{
			// 執行帳戶邏輯以關閉帳戶
			var response = new CloseAccountResponse
			{
				Success = await _accountLogic.CloseAccount(accountRequest)
			};
			// 根據操作結果設置回應訊息和帳戶資料
			if (response.Success)
			{
				response.Message = "Account closed successfully.";
			}
			else
			{
				response.Message = "Failed to close account.";
			}
			// 返回回應
			return response;
		}

		/// <summary>
		/// 更新帳戶信息的API端點
		/// </summary>
		/// <param name="accountRequest">需要更新的賬戶請求</param>
		/// <returns>需要更新的賬戶回應</returns>
		[HttpPost("UpdateAccount")]
		public async Task<ModifyAccountResponse> UpdateAccount(ModifyAccountRequest accountRequest)
		{
			// 執行帳戶邏輯以更新帳戶信息
			var response = new ModifyAccountResponse
			{
				Success = await _accountLogic.ModifyAccount(accountRequest)
			};
			// 根據操作結果設置回應訊息和帳戶資料
			if (response.Success)
			{
				response.Message = "Account updated successfully.";
			}
			else
			{
				response.Message = "Failed to update account.";
			}
			// 返回回應
			return response;
		}

		/// <summary>
		/// 獲取賬戶餘額的API端點
		/// </summary>
		/// <param name="accountRequest">賬戶請求</param>
		/// <returns>賬戶回應</returns>
		[HttpPost("GetAmount")]
		public async Task<AccountResponse> GetAmount(AccountRequest accountRequest)
		{
			// 執行賬戶邏輯以得到賬戶餘額
			var response = new AccountResponse()
			{
				Amount = await _accountLogic.GetBalance(accountRequest.AccountId)
			};

			// 如果賬戶餘額為空，則返回失敗的布爾值，否則返回成功的布爾值
			if (response.Amount == null)
			{
				response.Success = false;
				response.Message = "No account found.";
			}
			else
			{
				response.Success = true;
				response.Message = "Amount retrieved successfully.";
			}

			// 返回回應
			return response;
		}

		/// <summary>
		/// 獲取客戶所有賬戶的API端點
		/// </summary>
		/// <param name="accountRequest">賬戶請求</param>
		/// <returns>賬戶回應</returns>
		[HttpPost("GetAccounts")]
		public async Task<AccountResponse> GetAccounts(AccountRequest accountRequest)
		{
			// 執行賬戶邏輯以得到客戶所有賬戶
			var response = new AccountResponse()
			{
				Accounts = await _accountLogic.GetAccounts(accountRequest.CustomerId)
			};

			// 如果賬戶列表為空，則返回失敗的布爾值，否則返回成功的布爾值
			if (response.Accounts == null || response.Accounts.Count == 0)
			{
				response.Success = false;
				response.Message = "No accounts found.";
			}
			else
			{
				response.Success = true;
				response.Message = "Accounts retrieved successfully.";
			}

			// 返回回應
			return response;
		}

		/// <summary>
		/// 設定賬戶幣別的API端點
		/// </summary>
		/// <param name="accountRequest">賬戶請求</param>
		/// <returns>賬戶回應</returns>
		[HttpPost("SetCurrency")]
		public async Task<AccountResponse> SetCurrency(AccountRequest accountRequest)
		{
			// 執行賬戶邏輯以設定帳戶幣別
			var response = new AccountResponse()
			{
				Success = await _accountLogic.SetCurrency(accountRequest.AccountId, accountRequest.Currency)
			};

			// 根據操作結果設置回應訊息
			if (response.Success)
			{
				response.Message = "Currency set successfully.";
			}
			else
			{
				response.Message = "Failed to set currency.";
			}

			// 返回回應
			return response;
		}
	}
}
