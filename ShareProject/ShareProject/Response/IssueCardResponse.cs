namespace ShareProject.Response
{
	/// <summary>
	/// 发卡成功的回应。
	/// </summary>
	public class IssueCardResponse : BaseResponse
	{
		/// <summary>
		/// 卡片信息
		/// </summary>
		public CardItemResponse Card { get; set; } = new();
	}
}