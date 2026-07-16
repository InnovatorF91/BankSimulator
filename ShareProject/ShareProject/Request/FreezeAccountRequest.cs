namespace ShareProject.Request
{
	/// <summary>
	/// 管理員凍結帳戶的請求
	/// </summary>
	public class FreezeAccountRequest
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 凍結原因
		/// </summary>
		public string? Reason { get; set; }
	}
}
