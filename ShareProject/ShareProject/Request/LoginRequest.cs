using ShareProject.Common;

namespace ShareProject.Request
{
	public class LoginRequest
	{
		/// <summary>
		/// 登錄ID
		/// </summary>
		public string LoginId { get; set; } = string.Empty;
		/// <summary>
		/// 密碼
		/// </summary>
		public string Password { get; set; } = string.Empty;
		/// <summary>
		/// 雙因素驗證碼
		/// </summary>
		public string? TwoFactorCode { get; set; }
		/// <summary>
		/// 認證類型
		/// </summary>
		public AuthType Type { get; set; }
		/// <summary>
		/// 設備資訊
		/// </summary>
		public string? Device { get; set; }
		/// <summary>
		/// IP地址
		/// </summary>
		public string? Ip { get; set; }
	}
}
