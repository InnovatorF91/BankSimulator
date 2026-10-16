namespace ShareProject.Request
{
	/// <summary>
	/// 存款请求
	/// </summary>
	public class DepositRequest
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 余额变化
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 交易備註
		/// </summary>
		public string? Note { get; set; }
	}
}
