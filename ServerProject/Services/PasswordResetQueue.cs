using System.Threading.Channels;
using ShareProject.Request;

namespace ServerProject.Services
{
	public sealed class PasswordResetQueue
	{
		private readonly Channel<ResetPasswordRequest> _channel =
			Channel.CreateBounded<ResetPasswordRequest>(
				new BoundedChannelOptions(100)
				{
					SingleReader = true,
					SingleWriter = false,
					FullMode = BoundedChannelFullMode.Wait
				});

		public bool TryEnqueue(ResetPasswordRequest request)
		{
			return _channel.Writer.TryWrite(request);
		}

		public IAsyncEnumerable<ResetPasswordRequest> ReadAllAsync(CancellationToken cancellationToken)
		{
			return _channel.Reader.ReadAllAsync(cancellationToken);
		}
	}
}