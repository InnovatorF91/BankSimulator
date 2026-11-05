using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 阻止卡片响应类
	/// </summary>
	public class BlockCardResponse : BaseResponse
	{
		public long CardId { get; set; }

		public CardStatus Status { get; set; }
	}
}