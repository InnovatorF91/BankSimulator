using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 封鎖卡片結果資料傳輸物件
	/// </summary>
	public class BlockCardResultDto : DtoBase
	{
		/// <summary>
		/// 卡片ID
		/// </summary>
		public long CardId { get; set; }

		/// <summary>
		/// 卡片狀態
		/// </summary>
		public CardStatus Status { get; set; }

		public static BlockCardResultDto SuccessDto(long cardId, CardStatus status)
		{
			var dto = new BlockCardResultDto
			{
				CardId = cardId,
				Status = status
			};

			dto.MarkSuccess();
			return dto;
		}

		public static BlockCardResultDto Fail(int code,string message)
		{
			var dto = new BlockCardResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
