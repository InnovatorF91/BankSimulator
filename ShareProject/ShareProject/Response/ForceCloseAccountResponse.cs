using ShareProject.Common;

namespace ShareProject.Response
{
	public class ForceCloseAccountResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime CloseDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}