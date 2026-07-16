namespace ServerProject.DTOs
{
	public class LogoutCurrentDeviceResultDto : DtoBase
	{
		public static LogoutCurrentDeviceResultDto SuccessDto()
		{
			var dto = new LogoutCurrentDeviceResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static LogoutCurrentDeviceResultDto Fail(int errorCode, string message)
		{
			var dto = new LogoutCurrentDeviceResultDto();
			dto.MarkFail(errorCode, message);
			return dto;
		}
	}
}
