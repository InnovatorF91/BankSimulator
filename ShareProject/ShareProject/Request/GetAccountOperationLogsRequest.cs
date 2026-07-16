namespace ShareProject.Request
{
	/// <summary>
	/// 取得指定账户下所有操作日志的请求
	/// </summary>
	public class GetAccountOperationLogsRequest
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }
	}
}
