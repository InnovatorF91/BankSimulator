using ShareProject.Common;

namespace ServerProject.Common
{
	public class AccountOperationException : Exception
	{
		public AccountErrorCode ErrorCode { get; }

		public AccountOperationException(
			AccountErrorCode errorCode,
			string message) : base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
