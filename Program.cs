using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Security;
using Vtb.Hr.Web.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(o => {
    var read=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireAssertion(c=>Access.Read(c.User,builder.Configuration)).Build();
    o.DefaultPolicy=read; o.FallbackPolicy=read;
    o.AddPolicy("Admin",p=>p.RequireAuthenticatedUser().RequireAssertion(c=>Access.Admin(c.User,builder.Configuration)));
});
builder.Services.AddControllersWithViews(o=>{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    o.Filters.Add<WriteAccessFilter>();o.Filters.Add<DatabaseErrorFilter>();
    o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(n=>$"{n} phải là số.");
    o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((v,n)=>$"Giá trị của {n} không hợp lệ.");
    o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_=>"Vui lòng nhập giá trị.");
});
builder.Services.AddDbContext<VtbContext>(o=>{var cs=builder.Configuration.GetConnectionString("VTB2");if(!string.IsNullOrWhiteSpace(cs))o.UseSqlServer(cs);});
builder.Services.AddScoped<QualityService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<DocumentFileStore>();
if(OperatingSystem.IsWindows())builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath,".keys"))).ProtectKeysWithDpapi();
var app=builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePages(async context=>{
    var response=context.HttpContext.Response;
    response.ContentType="text/plain; charset=utf-8";
    await response.WriteAsync(response.StatusCode switch{401=>"Vui lòng đăng nhập bằng tài khoản Windows được cấp quyền. Mở website bằng Edge hoặc Chrome trên máy của bạn.",403=>"Tài khoản Windows này chưa được cấp quyền cho chức năng yêu cầu. Liên hệ quản trị hệ thống.",404=>"Không tìm thấy trang hoặc bản ghi yêu cầu.",_=>"Không thể hoàn tất yêu cầu."});
});
app.UseStaticFiles();app.UseRouting();app.UseAuthentication();app.UseAuthorization();
app.Use(async(c,next)=>{c.Response.Headers.CacheControl="no-store";c.Response.Headers["X-Content-Type-Options"]="nosniff";c.Response.Headers["Referrer-Policy"]="same-origin";c.Response.Headers["X-Frame-Options"]="SAMEORIGIN";c.Response.Headers["Content-Security-Policy"]="frame-ancestors 'self'; object-src 'none'; base-uri 'self'";await next();});
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program {}
