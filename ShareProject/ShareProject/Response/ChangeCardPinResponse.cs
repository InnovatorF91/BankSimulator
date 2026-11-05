namespace ShareProject.Response
{
	/// <summary>
	/// 修改卡片 PIN 成功的回应。
	/// </summary>
	public class ChangeCardPinResponse : BaseResponse
	{
		public long CardId { get; set; }
	}
}