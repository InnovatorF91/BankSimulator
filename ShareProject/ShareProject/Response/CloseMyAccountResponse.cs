using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 关闭账户成功的回应。
	/// </summary>
	public class CloseMyAccountResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime CloseDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}