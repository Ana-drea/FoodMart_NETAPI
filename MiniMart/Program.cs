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

    // ע���Զ���� OperationFilter
    options.OperationFilter<MiniMart.Helpers.SecurityRequirementsOperationFilter>();
});



// ע�����������
builder.Services.AddControllers();

builder.Services.AddAuthorization();

// ���� MySQL �����ַ���
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ʹ�� MySQL ���� DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// add the identity api endpoint 
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

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

// add the identity middleware 
app.MapIdentityApi<IdentityUser>();
app.UseHttpsRedirection();

// ʹ�� CORS �м��
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ���ÿ�����·��
app.MapControllers();


app.Run();

