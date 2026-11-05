using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 卡片信息响应类
	/// </summary>
	public class CardItemResponse
	{
		public long CardId { get; set; }
		public long AccountId { get; set; }
		public string MaskedCardNumber { get; set; } = string.Empty;
		public short ExpiryYear { get; set; }
		public short ExpiryMonth { get; set; }
		public CardType CardType { get; set; }
		public CardStatus Status { get; set; }
	}
}