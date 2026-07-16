namespace ShareProject.Request
{
	public class ResetPasswordRequest
	{
		/// <summary>
		/// 登入ID，必填，用於識別需要重置密碼的用戶帳戶，可以是用戶名、電子郵件地址或其他唯一標識符
		/// </summary>
		public string LoginId { get; set; } = string.Empty;

		/// <summary>
		/// Ip地址，選填，用於記錄請求來源的IP地址，增強安全審計和風險評估
		/// </summary>
		public string? Ip { get; set; }

		/// <summary>
		/// 裝置資訊，選填，用於記錄請求來源的裝置資訊，例如瀏覽器類型、操作系統等，有助於安全審計和風險評估
		/// </summary>
		public string? Device { get; set; }
	}
}
