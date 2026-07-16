using ShareProject.Common;

namespace ShareProject.Request
{
	public class UpdateKycStatusRequest
	{
		public int CustomerId { get; set; }

		public KYCStatus KycStatus { get; set; }
	}
}
