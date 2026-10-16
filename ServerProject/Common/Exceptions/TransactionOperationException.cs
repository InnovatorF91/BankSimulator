using ShareProject.Common;

namespace ServerProject.Common
{
	public class TransactionOperationException : Exception
	{
		public TransactionErrorCode ErrorCode { get; }

		public TransactionOperationException(
			TransactionErrorCode errorCode,
			string message)
			: base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
