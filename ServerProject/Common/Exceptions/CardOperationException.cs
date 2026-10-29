using ShareProject.Common;

namespace ServerProject.Common
{
	public class CardOperationException : Exception
	{
		public CardErrorCode ErrorCode { get; }
		public CardOperationException(
			CardErrorCode errorCode,
			string message)
			: base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
