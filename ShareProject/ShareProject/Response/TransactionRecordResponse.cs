using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 回应中的单条交易记录。
	/// </summary>
	public class TransactionRecordResponse
	{
		public long TransactionId { get; set; }

		public long AccountId { get; set; }

		public TransactionType TransactionType { get; set; }

		public decimal AmountDelta { get; set; }

		public long? RelatedAccount { get; set; }

		public DateTime CreatedAt { get; set; }

		public TransactionStatus Status { get; set; }

		public Guid? GroupId { get; set; }

		public string? Note { get; set; }

		public CurrencyCode Currency { get; set; }

		public decimal BalanceBefore { get; set; }

		public decimal BalanceAfter { get; set; }
	}
}