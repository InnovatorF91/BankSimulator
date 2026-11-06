namespace ShareProject.Response
{
	/// <summary>
	/// 取得我的銀行帳戶回應模型
	/// </summary>
	public class GetMyAccountsResponse : BaseResponse
	{
		/// <summary>
		/// 銀行帳戶列表
		/// </summary>
		public List<AccountItemResponse> Accounts { get; set; } = new();
	}
}