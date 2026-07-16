namespace ShareProject.Request
{
	public class UpdateIdentityDocumentRequest
	{
		public int CustomerId { get; set; }
		public short NewIdType { get; set; }
		public string NewIdNumber { get; set; } = string.Empty;

		public string NewEmail { get; set; } = string.Empty;

		public string NewPhoneNumber { get; set; } = string.Empty;
	}
}
