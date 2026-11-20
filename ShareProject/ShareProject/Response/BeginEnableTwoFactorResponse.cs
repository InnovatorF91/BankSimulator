namespace ShareProject.Response
{
	public class BeginEnableTwoFactorResponse : BaseResponse
	{
		public string Secret { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public string OtpauthUri { get; set; } = string.Empty;
	}
}