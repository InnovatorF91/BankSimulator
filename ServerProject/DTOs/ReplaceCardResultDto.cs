namespace ServerProject.DTOs
{
	/// <summary>
	/// 卡片替換結果 DTO
	/// </summary>
	public class ReplaceCardResultDto : DtoBase
	{
		public long OldCardId { get; set; }

		public CardDto Card { get; set; } = new();

		public static ReplaceCardResultDto SuccessDto(
			long oldCardId,
			CardDto card)
		{
			var dto = new ReplaceCardResultDto
			{
				OldCardId = oldCardId,
				Card = card
			};

			dto.MarkSuccess();
			return dto;
		}

		public static ReplaceCardResultDto Failure(
			int code,
			string message)
		{
			var dto = new ReplaceCardResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
