namespace ServerProject.Common
{
	public class CardNumberConflictException : Exception
	{
		public CardNumberConflictException(string message,Exception innerException)	: base(message, innerException)
		{
		}
	}
}
