using ShareProject.Common;

namespace ServerProject.Common
{
	public class CustomerOperationException : Exception
	{
		public CustomerErrorCode ErrorCode { get; }

		public CustomerOperationException(
			CustomerErrorCode errorCode,
			string message) : base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
