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

    // Register custom OperationFilter
    options.OperationFilter<MiniMart.Helpers.SecurityRequirementsOperationFilter>();
});



builder.Services.AddControllers();

builder.Services.AddAuthorization();

// configure MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// add the identity api endpoint 
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// Add CORS services to allow cross-origin requests
builder.Services.AddCors(options =>
{
    // Add CORS policy for specific origin
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // Allow specific frontend origin
              .AllowAnyHeader()                    // Allow any headers
              .AllowAnyMethod()                    // Allow any methods (GET, POST, etc.)
              .AllowCredentials();                 // Allow credentials (cookies, authentication headers, etc.)
    });

});

builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository>();

// Configure Cookie options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin requests
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Transmit cookies only over HTTPS
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

// use CORS middleware
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Configure controller routes
app.MapControllers();


app.Run();

