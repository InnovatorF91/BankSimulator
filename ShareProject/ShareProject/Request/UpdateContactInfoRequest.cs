namespace ShareProject.Request
{
	public class UpdateContactInfoRequest
	{
		public int CustomerId { get; set; }

		public string NewAddress { get; set; } = string.Empty;

		public string NewEmail { get; set; } = string.Empty;

		public string NewPhoneNumber { get; set; } = string.Empty;
	}
}
