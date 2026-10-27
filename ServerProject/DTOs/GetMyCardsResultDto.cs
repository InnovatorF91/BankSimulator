namespace ServerProject.DTOs
{
	/// <summary>
	/// 獲取我的卡片結果資料傳輸物件
	/// </summary>
	public class GetMyCardsResultDto : DtoBase
	{
		public List<CardDto> Cards { get; set; } = new();

		public static GetMyCardsResultDto SuccessDto(List<CardDto> cards)
		{
			var dto = new GetMyCardsResultDto
			{
				Cards = cards
			};
			dto.MarkSuccess();
			return dto;
		}

		public static GetMyCardsResultDto Failure(int code, string message)
		{
			var dto = new GetMyCardsResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
