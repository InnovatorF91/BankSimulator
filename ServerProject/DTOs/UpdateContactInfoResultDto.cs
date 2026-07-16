namespace ServerProject.DTOs
{
	public class UpdateContactInfoResultDto : DtoBase
	{
		public static UpdateContactInfoResultDto SuccessDto()
		{
			var dto = new UpdateContactInfoResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static UpdateContactInfoResultDto Fail(int code, string message)
		{
			var dto = new UpdateContactInfoResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
