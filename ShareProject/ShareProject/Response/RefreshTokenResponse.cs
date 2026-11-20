namespace ShareProject.Response
{
	public class RefreshTokenResponse : BaseResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
	}
}