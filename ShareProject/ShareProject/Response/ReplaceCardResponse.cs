namespace ShareProject.Response
{
	/// <summary>
	/// 换卡成功的回应。
	/// </summary>
	public class ReplaceCardResponse : BaseResponse
	{
		public long OldCardId { get; set; }

		/// <summary>
		/// 新卡片信息
		/// </summary>
		public CardItemResponse Card { get; set; } = new();
	}
}