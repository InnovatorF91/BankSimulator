namespace ServerProject.DTOs
{
	public class UpdateBasicProfileResultDto : DtoBase
	{
		public static UpdateBasicProfileResultDto SuccessDto()
		{
			var dto = new UpdateBasicProfileResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static UpdateBasicProfileResultDto Fail(int code, string message)
		{
			var dto = new UpdateBasicProfileResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
