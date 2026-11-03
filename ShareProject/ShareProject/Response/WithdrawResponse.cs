using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 取款结果回应。
	/// </summary>
	public class WithdrawResponse : BaseResponse
	{
		public long TransactionId { get; set; }

		public long AccountId { get; set; }

		/// <summary>
		/// 本次取款金额，当前 Service 返回正数。
		/// </summary>
		public decimal AmountDelta { get; set; }

		public decimal BalanceBefore { get; set; }

		public decimal BalanceAfter { get; set; }

		public TransactionStatus Status { get; set; }

		public CurrencyCode Currency { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}