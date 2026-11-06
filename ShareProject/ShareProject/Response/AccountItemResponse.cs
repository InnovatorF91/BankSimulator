using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 銀行帳戶項目回應模型
	/// </summary>
	public class AccountItemResponse
	{
		public long AccountId { get; set; }
		public AccountType AccountType { get; set; }
		public decimal Balance { get; set; }
		public CurrencyCode Currency { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime OpenDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}