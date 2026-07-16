namespace ShareProject.Request
{
	/// <summary>
	/// 管理員解除凍結帳戶的請求
	/// </summary>
	public class UnfreezeAccountRequest
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 凍結原因
		/// </summary>
		public string? Reason { get; set; }
	}
}
