namespace ShareProject.Response
{
	/// <summary>
	/// 管理员查询指定客户账户列表的响应。
	/// </summary>
	public class GetCustomerAccountsResponse : BaseResponse
	{
		public int CustomerId { get; set; }
		public List<AccountItemResponse> Accounts { get; set; } = new();
	}
}