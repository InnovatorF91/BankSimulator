namespace ServerProject.DTOs
{
	public class UpdateIdentityDocumentResultDto : DtoBase
	{
		public static UpdateIdentityDocumentResultDto SuccessDto()
		{
			var dto = new UpdateIdentityDocumentResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static UpdateIdentityDocumentResultDto Fail(int code, string message)
		{
			var dto = new UpdateIdentityDocumentResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
