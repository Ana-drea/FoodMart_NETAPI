using Microsoft.EntityFrameworkCore;
using MiniMart.Data;
using MiniMart.Repositories;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ע�����������
builder.Services.AddControllers();

// ���� MySQL �����ַ���
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ʹ�� MySQL ���� DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// ��� CORS �����������������
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // ����������Դ
              .AllowAnyMethod()  // �������� HTTP ����
              .AllowAnyHeader(); // ��������ͷ
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

// ���ÿ�����·��
app.MapControllers();

// ʹ�� CORS �м��
app.UseCors("AllowAll");

app.Run();

