using ShareProject.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerProject.Entities
{
	/// <summary>
	/// 账户操作日志实体
	/// </summary>
	[Table("AccountOperationLogs")]
	public class AccountOperationLogEntity
	{
		/// <summary>
		/// 操作日志ID
		/// </summary>
		[Key]
		[Column("operation_log_id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long OperationLogId { get; set; }

		/// <summary>
		/// 账户ID
		/// </summary>
		[Column("account_id")]
		public long AccountId { get; set; }

		/// <summary>
		/// 客户ID
		/// </summary>
		[Column("customer_id")]
		public int CustomerId { get; set; }

		/// <summary>
		/// 操作类型
		/// </summary>
		[Column("operation_type")]
		public AccountOperationType OperationType { get; set; }

		/// <summary>
		/// 旧的账户状态
		/// </summary>
		[Column("old_status")]
		public AccountStatus? OldStatus { get; set; }

		/// <summary>
		/// 新的账户状态
		/// </summary>
		[Column("new_status")]
		public AccountStatus? NewStatus { get; set; }

		/// <summary>
		/// 操作理由
		/// </summary>
		[Column("reason")]
		public string? Reason { get; set; }

		/// <summary>
		/// 操作员编号
		/// </summary>
		[Column("operated_by")]
		public int? OperatedBy { get; set; }

		/// <summary>
		/// 操作时间
		/// </summary>
		[Column("operated_at")]
		public DateTime OperatedAt { get; set; }
	}
}
