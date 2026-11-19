using ShareProject.Common;

namespace ServerProject.Common
{
	public class AuthOperationException : Exception
	{
		public AuthErrorCode ErrorCode { get; }

		public AuthOperationException(AuthErrorCode errorCode, string message)
			: base(message)
		{
			ErrorCode = errorCode;
		}
	}
}