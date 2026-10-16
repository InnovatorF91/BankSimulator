namespace ShareProject.Request
{
	/// <summary>
	/// 取款请求
	/// </summary>
	public class WithdrawRequest
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 取款金额，必须为正数
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 交易備註
		/// </summary>
		public string? Note { get; set; }
	}
}
