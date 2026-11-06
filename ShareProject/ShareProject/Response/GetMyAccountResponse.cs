namespace ShareProject.Response
{
	/// <summary>
	/// 查询单个账户成功的回应。
	/// </summary>
	public class GetMyAccountResponse : BaseResponse
	{
		public AccountItemResponse Account { get; set; } = new();
	}
}