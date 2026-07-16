namespace ShareProject.Request
{
	/// <summary>
	/// 管理員取得帳戶的請求。
	/// </summary>
	public class GetAccountForAdminRequest
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }
	}
}
