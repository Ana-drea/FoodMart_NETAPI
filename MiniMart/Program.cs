using Microsoft.EntityFrameworkCore;
using MiniMart.Data;
using MiniMart.Repositories;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册控制器服务
builder.Services.AddControllers();

// 配置 MySQL 连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 使用 MySQL 配置 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

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

app.UseHttpsRedirection();

// 配置控制器路由
app.MapControllers();

// 使用 CORS 中间件
app.UseCors("AllowAll");

app.Run();

