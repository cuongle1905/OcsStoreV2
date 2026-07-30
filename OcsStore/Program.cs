using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OcsStore.Controllers;
using OcsStore.Models;
using System.Data.Entity;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

string mySqlConnectionStr = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr));
    options.EnableSensitiveDataLogging();
});

builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddTransient<ItemController>();
builder.Services.AddTransient<ProcessingController>();
builder.Services.AddTransient<SalesController>();
builder.Services.AddTransient<BillingController>();
builder.Services.AddTransient<StockController>();
builder.Services.AddTransient<ReportController>();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ocsstore.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(3600 * 24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token))
    {
        //context.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        context.Request.Headers.Append("Authorization", "Bearer " + token);
    }
    await next();
});
app.UseStatusCodePages(context => {
    var response = context.HttpContext.Response;
    if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
        response.StatusCode == (int)HttpStatusCode.Forbidden)
        response.Redirect("/Login");
    return Task.CompletedTask;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();

// Scaffold-DbContext name=MySqlConnection Pomelo.EntityFrameworkCore.MySql -OutputDir Models -Context MyDbContext -f

// dotnet publish -c Release -o ./publish

/* ssh cuong@103.15.222.91
 * cd ~
 * ./unicoffee-stop
 * ./unicoffee-reload
 * 
 * database: ocsstore   App folder: /home/cuong/unicoffee
 * 
 * cd /apps/unicoffee/mysql
 * mysql -u cuong -pcuong@food -h localhost ocsstore -e "source ocsstore_update_20260618.sql"
 * mysql -u cuong -pcuong@food -h localhost ocsstore -e "select * from item;"
 * 
 * mysqldump --skip-lock-tables --routines --add-drop-table --disable-keys --extended-insert -u cuong -pcuong@food --host=localhost ocsstore > ocsstore_dump.sql
 * 
 * ./canteen-stop
 * ./canteen-reload
 * 
 * ./ghg-stop
 * ./ghg-reload
 * mysql -u cuong -pcuong@food -h localhost ghg -e "select * from company_info;"
 * mysql -u cuong -pcuong@food -h localhost ghg -e "source ocsstore_update_20260618.sql"
 * 
 * ./stop-food-app
 * ./reload-food-app
 * mysql -u cuong -pcuong@food -h localhost food -e "source food_20251016.sql"
 * mysql -u cuong -pcuong@food -h localhost food -e "select * from param;"
 * mysqldump --skip-lock-tables --routines --add-drop-table --disable-keys --extended-insert -u cuong -pcuong@food --host=localhost food > food_dump.sql
 * 
 */