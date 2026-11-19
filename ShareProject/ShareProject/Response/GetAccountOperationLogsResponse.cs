namespace ShareProject.Response
{
	/// <summary>
	/// 查询指定账户操作日志列表的响应。
	/// </summary>
	public class GetAccountOperationLogsResponse : BaseResponse
	{
		public long AccountId { get; set; }
		public List<AccountOperationLogItemResponse> Logs { get; set; } = new();
	}
}