namespace ShareProject.Request
{
	/// <summary>
	/// 修改卡片的请求资料传输物件
	/// </summary>
	public class ChangeCardPinRequest
	{
		/// <summary>
		/// 卡片ID
		/// </summary>
		public long CardId { get; set; }

		/// <summary>
		/// 既存的PIN
		/// </summary>
		public string CurrentPIN { get; set; } = string.Empty;

		/// <summary>
		/// 新PIN
		/// </summary>
		public string NewPIN { get; set; } = string.Empty;
	}
}
