using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 操作日志DTO
	/// </summary>
	public class AccountOperationLogDto
	{
		/// <summary>
		/// 操作日志ID
		/// </summary>
		public long OperationLogId { get; set; }

		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 客户ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 操作类型
		/// </summary>
		public AccountOperationType OperationType { get; set; }

		/// <summary>
		/// 旧的状态
		/// </summary>
		public AccountStatus? OldStatus { get; set; }

		/// <summary>
		/// 新的状态
		/// </summary>
		public AccountStatus? NewStatus { get; set; }

		/// <summary>
		/// 操作理由
		/// </summary>
		public string? Reason { get; set; }

		/// <summary>
		/// 操作员编号
		/// </summary>
		public int? OperatedBy { get; set; }

		/// <summary>
		/// 操作时间
		/// </summary>
		public DateTime OperatedAt { get; set; }
	}
}
