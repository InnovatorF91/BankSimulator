namespace ShareProject.Response
{
	/// <summary>
	/// 当前客户交易记录查询回应。
	/// </summary>
	public class GetMyTransactionsResponse : BaseResponse
	{
		public List<TransactionRecordResponse> Transactions { get; set; }
			= new();
	}
}