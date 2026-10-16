namespace ShareProject.Request
{
	/// <summary>
	/// 转账请求
	/// </summary>
	public class TransferRequest
	{
		/// <summary>
		/// 源账户ID
		/// </summary>
		public long SourceAccountId { get; set; }

		/// <summary>
		/// 目标账户ID
		/// </summary>
		public long DestinationAccountId { get; set; }

		/// <summary>
		/// 余额变化，必须为正数
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 交易備註
		/// </summary>
		public string? Note { get; set; }
	}
}
