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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // 允许所有来源
              .AllowAnyMethod()  // 允许所有 HTTP 方法
              .AllowAnyHeader(); // 允许所有头
    });
});

builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository>();

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
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// 配置控制器路由
app.MapControllers();


app.Run();

