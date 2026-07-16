using Dapper;
using ServerProject.Common;
using ServerProject.Middlewares;
using ServerProject.Repositories;
using ServerProject.Services;

namespace ServerProject
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Dapper 的預設映射規則是將資料庫欄位名稱與物件屬性名稱進行匹配。
			DefaultTypeMap.MatchNamesWithUnderscores = true;

			// MVC（含 Views）與 API
			builder.Services.AddControllersWithViews();

			// 1) Infrastructure
			builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>();
			builder.Services.AddScoped<IDataAccess>(sp =>
			{
				var cf = sp.GetRequiredService<IConnectionFactory>();
				var conn = cf.GetConnection(transaction: false);     // 非交易連線；交易於 UseCase 內部控制
				return new DataAccess(conn, useTransaction: false);
			});
			builder.Services.AddSingleton<ITimeProvider, SystemClock>();
			builder.Services.AddSingleton<ICryptoRepository, CryptoRepository>();
			builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
			builder.Services.AddSingleton<ISessionOption, SessionOption>();
			builder.Services.AddSingleton<IPasswordValidator, PasswordValidator>();

			builder.Services.Configure<SmtpOption>(
				builder.Configuration.GetSection("SmtpOption"));
			builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

			// 2) Domain Repositories
			builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

			builder.Services.AddScoped<CustomerAuthRepository>();
			builder.Services.AddScoped<ICustomerAuthRepository>(sp => sp.GetRequiredService<CustomerAuthRepository>());
			builder.Services.AddScoped<ICustomerSessionRepository>(sp => sp.GetRequiredService<CustomerAuthRepository>());
			builder.Services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<CustomerAuthRepository>());
			builder.Services.AddScoped<IPasswordResetTokenRepository>(sp => sp.GetRequiredService<CustomerAuthRepository>());

			builder.Services.AddScoped<IAccountRepository, AccountRepository>();
			builder.Services.AddScoped<ICardRepository, CardRepository>();
			builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

			// 3) Business Service
			builder.Services.AddScoped<ICustomerService, CustomerService>();
			builder.Services.AddScoped<IAccountService, AccountService>();
			builder.Services.AddScoped<IAuthService, AuthService>();

			// 4) Session Authentication Service
			builder.Services.AddScoped<ISessionAuthenticationService, SessionAuthenticationService>();

			// 5) Current Customer Service
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<ICurrentCustomer, CurrentCustomer>();

			var app = builder.Build();

			// 生產環境錯誤頁與 HSTS
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			// 启用身份验证中间件
			app.UseAuthentication();

			// 启用自定义的会话身份验证中间件
			app.UseMiddleware<SessionAuthenticationMiddleware>();

			// 启用授权中间件
			app.UseAuthorization();

			// 傳統 MVC 路由（預設 Home/Index）
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			// 支援 attribute routing 的 API Controller
			app.MapControllers();

			app.Run();
		}
	}
}
