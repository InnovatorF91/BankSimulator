using System.Runtime.CompilerServices;

namespace ServerProject.Common
{
	public enum LogOutcome
	{
		Completed,
		Rejected,
		Failed
	}

	public static class LoggerExtensions
	{
		public static void LogStart(
			this ILogger logger,
			LogLevel level = LogLevel.Information,
			[CallerMemberName] string operation = "")
		{
			logger.Log(level, "START {Operation}", operation);
		}

		public static void LogEnd(
			this ILogger logger,
			LogOutcome outcome,
			long elapsedMilliseconds,
			int? errorCode = null,
			LogLevel level = LogLevel.Information,
			[CallerMemberName] string operation = "")
		{
			logger.Log(
				level,
				"END {Operation} {Outcome} ElapsedMilliseconds={ElapsedMilliseconds} ErrorCode={ErrorCode}",
				operation,
				outcome,
				elapsedMilliseconds,
				errorCode);
		}
	}
}