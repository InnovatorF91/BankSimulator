namespace ServerProject.DTOs
{
	public class UpdateKycStatusResultDto : DtoBase
	{
		public static UpdateKycStatusResultDto SuccessDto()
		{
			var dto = new UpdateKycStatusResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static UpdateKycStatusResultDto Fail(int code, string message)
		{
			var dto = new UpdateKycStatusResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
