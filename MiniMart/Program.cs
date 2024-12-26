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
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // ����ǰ�˵���Դ
              .AllowAnyHeader()                     // �����κ�����ͷ
              .AllowAnyMethod()                     // �����κη�����GET, POST �ȣ�
              .AllowCredentials();                  // ����Я��ƾ֤
    });
});

builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository>();

// ���� Cookie ѡ��
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // �������
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ���� HTTPS �´���
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

// ʹ�� CORS �м��
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// ���ÿ�����·��
app.MapControllers();


app.Run();

