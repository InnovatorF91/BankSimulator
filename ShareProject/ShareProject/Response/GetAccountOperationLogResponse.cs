namespace ShareProject.Response
{
	/// <summary>
	/// 查询单条账户操作日志的响应。
	/// </summary>
	public class GetAccountOperationLogResponse : BaseResponse
	{
		public AccountOperationLogItemResponse Log { get; set; } = new();
	}
}