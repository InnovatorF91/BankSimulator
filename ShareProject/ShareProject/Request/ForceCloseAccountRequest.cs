namespace ShareProject.Request
{
	public class ForceCloseAccountRequest
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 强制关闭理由
		/// </summary>
		public string? Reason { get; set; }
	}
}
