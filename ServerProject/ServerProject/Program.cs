using ServerProject.Common;     // IConnectionFactory, IDataAccess
using ServerProject.Logics;     // ICustomerLogic, CustomerLogic
using ServerProject.Services;   // ICustomerService, CustomerService, ICustomerAuthService, CustomerAuthService

var builder = WebApplication.CreateBuilder(args);

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

// 2) Domain Services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();

// 3) Business Logic
builder.Services.AddScoped<ICustomerLogic, CustomerLogic>();

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

// 若未來啟用驗證，記得開這行並在 DI 註冊方案
// app.UseAuthentication();
app.UseAuthorization();

// 傳統 MVC 路由（預設 Home/Index）
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 支援 attribute routing 的 API Controller
app.MapControllers();

app.Run();
