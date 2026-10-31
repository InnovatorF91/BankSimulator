namespace ServerProject.DTOs
{
	/// <summary>
	/// 修改卡片PIN的结果Dto
	/// </summary>
	public class ChangeCardPinResultDto : DtoBase
	{
		/// <summary>
		/// 卡片ID
		/// </summary>
		public long CardId { get; set; }

		public static ChangeCardPinResultDto SuccessDto(long cardId)
		{
			var dto = new ChangeCardPinResultDto
			{
				CardId = cardId
			};

			dto.MarkSuccess();
			return dto;
		}

		public static ChangeCardPinResultDto Failure(
			int code,
			string message)
		{
			var dto = new ChangeCardPinResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
