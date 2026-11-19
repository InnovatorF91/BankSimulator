using ShareProject.Common;

namespace ShareProject.Response
{
	public class FreezeAccountResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public AccountStatus Status { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}