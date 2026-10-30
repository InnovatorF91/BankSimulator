namespace ServerProject.DTOs
{
	/// <summary>
	/// 發行卡片結果資料傳輸物件
	/// </summary>
	public class IssueCardResultDto : DtoBase
	{
		/// <summary>
		/// 卡片資料
		/// </summary>
		public CardDto Card { get; set; } = new();

		public static IssueCardResultDto SuccessDto(CardDto card)
		{
			var dto = new IssueCardResultDto();
			dto.Card = card;
			dto.MarkSuccess();
			return dto;
		}

		public static IssueCardResultDto Failure(int code, string message)
		{
			var dto = new IssueCardResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
