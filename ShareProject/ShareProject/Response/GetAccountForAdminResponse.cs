using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 管理员查询指定账户的响应。
	/// </summary>
	public class GetAccountForAdminResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public int CustomerId { get; set; }
		public AccountType AccountType { get; set; }
		public decimal Balance { get; set; }
		public CurrencyCode Currency { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime OpenDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}