using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 卡片資料傳輸物件
	/// </summary>
	public class CardDto
	{
		/// <summary>
		/// 卡片ID
		/// </summary>
		public long CardId { get; set; }

		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 卡片號碼（已遮蔽）
		/// </summary>
		public string MaskedCardNumber { get; set; } = string.Empty;

		/// <summary>
		/// 卡片到期年份
		/// </summary>
		public short ExpiryYear { get; set; }

		/// <summary>
		/// 卡片到期月份
		/// </summary>
		public short ExpiryMonth { get; set; }

		/// <summary>
		/// 卡片類型
		/// </summary>
		public CardType CardType { get; set; }

		/// <summary>
		/// 卡片狀態
		/// </summary>
		public CardStatus Status { get; set; }
	}
}
