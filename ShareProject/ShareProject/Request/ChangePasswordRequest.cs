namespace ShareProject.Request
{
	public class ChangePasswordRequest
	{
		/// <summary>
		/// 客户ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 舊密碼
		/// </summary>
		public string OldPassword { get; set; } = string.Empty;

		/// <summary>
		/// 新密碼
		/// </summary>
		public string NewPassword { get; set; } = string.Empty;
	}
}
