namespace ShareProject.Request
{
	/// <summary>
	/// 当前用户交易记录查询请求
	/// </summary>
	public class GetMyTransactionRequest
	{
		/// <summary>
		/// 指定账户ID，null表示全部账户
		/// </summary>
		public long? AccountId { get; set; }

		/// <summary>
		/// 查询开始时间
		/// </summary>
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// 查询结束时间
		/// </summary>
		public DateTime? EndDate { get; set; }
	}
}
