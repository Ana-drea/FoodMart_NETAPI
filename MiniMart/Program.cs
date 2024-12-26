using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MiniMart.Data;
using MiniMart.Repositories;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// configure swagger for bearer token
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    // 注册自定义的 OperationFilter
    options.OperationFilter<MiniMart.Helpers.SecurityRequirementsOperationFilter>();
});



// 注册控制器服务
builder.Services.AddControllers();

builder.Services.AddAuthorization();

// 配置 MySQL 连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 使用 MySQL 配置 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// add the identity api endpoint 
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// 添加 CORS 服务以允许跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // 允许前端的来源
              .AllowAnyHeader()                     // 允许任何请求头
              .AllowAnyMethod()                     // 允许任何方法（GET, POST 等）
              .AllowCredentials();                  // 允许携带凭证
    });
});

builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository>();

// 配置 Cookie 选项
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // 允许跨域
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 仅在 HTTPS 下传输
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// add the identity middleware 
app.MapIdentityApi<IdentityUser>();
app.UseHttpsRedirection();

// 使用 CORS 中间件
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// 配置控制器路由
app.MapControllers();


app.Run();

