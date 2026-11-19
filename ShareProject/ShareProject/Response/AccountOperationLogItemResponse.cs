using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 账户操作日志条目。
	/// </summary>
	public class AccountOperationLogItemResponse
	{
		public long OperationLogId { get; set; }
		public long AccountId { get; set; }
		public int CustomerId { get; set; }
		public AccountOperationType OperationType { get; set; }
		public AccountStatus? OldStatus { get; set; }
		public AccountStatus? NewStatus { get; set; }
		public string? Reason { get; set; }
		public int? OperatedBy { get; set; }
		public DateTime OperatedAt { get; set; }
	}
}