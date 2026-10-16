using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 单条交易记录Dto
	/// </summary>
	public class TransactionRecordDto
	{
		/// <summary>
		/// 交易ID
		/// </summary>
		public long TransactionId { get; set; }

		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 交易类型
		/// </summary>
		public TransactionType TransactionType { get; set; }

		/// <summary>
		/// 交易金额，始终为正数
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 关联账户ID
		/// </summary>
		public long? RelatedAccount { get; set; }

		/// <summary>
		/// 交易创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 交易状态
		/// </summary>
		public TransactionStatus Status { get; set; }

		/// <summary>
		/// 交易组ID
		/// </summary>
		public Guid? GroupId { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string? Note { get; set; }

		/// <summary>
		/// 货币种类
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 交易前余额
		/// </summary>
		public decimal BalanceBefore { get; set; }

		/// <summary>
		/// 交易后余额
		/// </summary>
		public decimal BalanceAfter { get; set; }
	}
}
