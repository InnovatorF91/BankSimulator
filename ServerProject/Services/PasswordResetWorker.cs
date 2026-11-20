namespace ServerProject.Services
{
	public sealed class PasswordResetWorker : BackgroundService
	{
		private readonly PasswordResetQueue _queue;
		private readonly IServiceScopeFactory _scopeFactory;

		public PasswordResetWorker(
			PasswordResetQueue queue,
			IServiceScopeFactory scopeFactory)
		{
			_queue = queue;
			_scopeFactory = scopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				await foreach (var request in _queue.ReadAllAsync(stoppingToken))
				{
					stoppingToken.ThrowIfCancellationRequested();

					try
					{
						await using var scope = _scopeFactory.CreateAsyncScope();
						var authService = scope.ServiceProvider
							.GetRequiredService<IAuthService>();

						await authService.ResetPassword(request);
					}
					catch (OperationCanceledException)
						when (stoppingToken.IsCancellationRequested)
					{
						throw;
					}
					catch (Exception)
					{
						// 当前任务结束，继续处理队列中的下一条任务。
						// 统一接入日志时补充后台任务异常记录。
					}
				}
			}
			catch (OperationCanceledException)
				when (stoppingToken.IsCancellationRequested)
			{
				// 应用停止时结束队列读取。
			}
		}
	}
}