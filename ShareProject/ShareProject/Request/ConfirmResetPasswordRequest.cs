namespace ShareProject.Request
{
	public class ConfirmResetPasswordRequest
	{
		/// <summary>
		/// 重置密碼令牌，從重置密碼電子郵件中獲取，用於驗證重置密碼請求的合法性和安全性
		/// </summary>
		public string ResetToken { get; set; } = string.Empty;

		/// <summary>
		/// 新密碼，使用者輸入的新密碼，應該符合安全性要求，例如最小長度、包含特殊字符等，以確保帳戶安全
		/// </summary>
		public string NewPassword { get; set; } = string.Empty;
	}
}
