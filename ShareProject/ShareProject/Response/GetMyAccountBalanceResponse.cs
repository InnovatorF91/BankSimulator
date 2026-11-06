using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 查询账户余额成功的回应。
	/// </summary>
	public class GetMyAccountBalanceResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public decimal Balance { get; set; }
		public CurrencyCode Currency { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}