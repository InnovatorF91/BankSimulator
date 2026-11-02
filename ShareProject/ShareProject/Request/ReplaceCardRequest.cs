using ShareProject.Common;

namespace ShareProject.Request
{
	/// <summary>
	/// 卡片替換請求
	/// </summary>
	public class ReplaceCardRequest
	{
		/// <summary>
		/// 卡片ID
		/// </summary>
		public long CardId { get; set; }

		/// <summary>
		/// 替換原因
		/// </summary>
		public CardReplaceReason Reason { get; set; }

		/// <summary>
		/// PIN碼
		/// </summary>
		public string PIN { get; set; } = string.Empty;
	}
}
