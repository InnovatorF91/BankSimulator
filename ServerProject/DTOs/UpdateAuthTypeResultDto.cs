namespace ServerProject.DTOs
{
	public class UpdateAuthTypeResultDto : DtoBase
	{
		public static UpdateAuthTypeResultDto SuccessDto()
		{
			var dto = new UpdateAuthTypeResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static UpdateAuthTypeResultDto Fail(int code, string message)
		{
			var dto = new UpdateAuthTypeResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
