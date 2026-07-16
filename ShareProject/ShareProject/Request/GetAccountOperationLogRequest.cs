namespace ShareProject.Request
{
	/// <summary>
	/// 取得指定操作日志的请求
	/// </summary>
	public class GetAccountOperationLogRequest
	{
		/// <summary>
		/// 操作日志ID
		/// </summary>
		public long OperationLogId { get; set; }
	}
}
