namespace ShareProject.Request
{
	/// <summary>
	/// 取得我的所有帳戶請求模型
	/// </summary>
	public class GetMyAccountsRequest
	{
		/// <summary>
		/// 是否包含已關閉的帳戶
		/// </summary>
		public bool IncludeClosedAccounts { get; set; } = false;
	}
}
