namespace ShareProject.Request
{
	public class DisableTwoFactorRequest
	{
		/// <summary>
		/// 用户ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 当前双重认证验证码
		/// </summary>
		public string TwoFactorCode { get; set; } = string.Empty;
	}
}
