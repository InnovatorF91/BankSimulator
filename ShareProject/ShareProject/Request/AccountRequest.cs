using ShareProject.Common;

namespace ShareProject.Request
{
	/// <summary>
	/// 賬戶請求類別，包含賬戶相關的請求屬性
	/// </summary>
	public class AccountRequest : BaseRequest
	{
		public long AccountId { get; set; } // 賬戶ID

		public int CustomerId { get; set; } // 客戶ID

		public CurrencyCode Currency { get; set; } // 賬戶貨幣
	}
}
