using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 存款结果回应。
	/// </summary>
	public class DepositResponse : BaseResponse
	{
		public long TransactionId { get; set; }

		public long AccountId { get; set; }

		public decimal AmountDelta { get; set; }

		public decimal BalanceBefore { get; set; }

		public decimal BalanceAfter { get; set; }

		public TransactionStatus Status { get; set; }

		public CurrencyCode Currency { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}