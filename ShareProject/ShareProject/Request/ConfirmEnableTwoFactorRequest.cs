namespace ShareProject.Request
{
	public class ConfirmEnableTwoFactorRequest
	{
		public int CustomerId { get; set; }// 客戶ID
		public string TwoFactorCode { get; set; } = string.Empty; // 二步驗證碼
	}
}
