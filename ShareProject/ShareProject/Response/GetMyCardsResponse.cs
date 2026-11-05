namespace ShareProject.Response
{
	/// <summary>
	/// 获取我的卡片列表响应类
	/// </summary>
	public class GetMyCardsResponse : BaseResponse
	{
		/// <summary>
		/// 卡片列表
		/// </summary>
		public List<CardItemResponse> Cards { get; set; } = new();
	}
}