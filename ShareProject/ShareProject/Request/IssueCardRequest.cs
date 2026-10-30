using ShareProject.Common;

namespace ShareProject.Request
{
	/// <summary>
	/// 發行卡片請求
	/// </summary>
	public class IssueCardRequest
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 卡片類型
		/// </summary>
		public CardType CardType { get; set; }

		/// <summary>
		/// PIN碼
		/// </summary>
		public string PIN { get; set; } = string.Empty;
	}
}
