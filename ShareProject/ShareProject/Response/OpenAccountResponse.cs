using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 开户成功的回应。
	/// </summary>
	public class OpenAccountResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public AccountType AccountType { get; set; }
		public decimal Balance { get; set; }
		public CurrencyCode Currency { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime OpenDate { get; set; }
	}
}