using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 转账结果回应。
	/// </summary>
	public class TransferResponse : BaseResponse
	{
		public Guid GroupId { get; set; }

		public long SourceTransactionId { get; set; }

		public long DestinationTransactionId { get; set; }

		public long SourceAccountId { get; set; }

		public long DestinationAccountId { get; set; }

		/// <summary>
		/// 转账金额，为正数。
		/// </summary>
		public decimal AmountDelta { get; set; }

		public decimal SourceBalanceBefore { get; set; }

		public decimal SourceBalanceAfter { get; set; }

		public TransactionStatus Status { get; set; }

		public CurrencyCode Currency { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}