using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Data;
using System.Text.Json.Serialization;
using MyEventsApi.Models;
using MyEventsApi.Services.Interfaces;
using MyEventsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var JwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtSettings["Issuer"],
            ValidAudience = JwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings["Key"]))
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyEventsApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token. \nExample: Bearer [token]",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
               Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
               Id = "Bearer"
            }
        },
        new string[] { }
        }
    });
});
builder.Services.AddCors(options =>
{ 
    options.AddPolicy("AllowFrontend",
        policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});   

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();




var app = builder.Build();

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "Healthy", database = "Connected" });
    }
    catch
    {
        return Results.Problem("Database connection failed");
    }


});

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ErrorHandlingMiddleware>();   

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCors("AllowFrontend");
app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    
    if(!db.Users.Any())
    {
        var u1 = new User { Email = "Beer@example.com", DisplayName = "Budejovice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") };
        var u2 = new User { Email = "Vodka@example.com", DisplayName = "Rada", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") };

        db.Users.AddRange(u1, u2);
        db.SaveChanges();

        db.Events.AddRange(
            new Event
            {
                Title = "Beer Festival",
                Description = "Join us for a day of beer tasting and fun!",
                Date = DateTime.UtcNow.AddDays(30),
                Location = "City Park",
                Capacity = 100,
                IsPublic = true,
                OrganizerId = u1.Id
            },
            new Event
            {
                Title = "Vodka Tasting",
                Description = "Experience the finest vodkas from around the world.",
                Date = DateTime.UtcNow.AddDays(45),
                Location = "Downtown Bar",
                Capacity = 50,
                IsPublic = true,
                OrganizerId = u2.Id
            },
            new Event
            {
                Title = "Private Whiskey Night",
                Description = "An exclusive event for whiskey enthusiasts.",
                Date = DateTime.UtcNow.AddDays(60),
                Location = "Uptown Lounge",
                Capacity = 30,
                IsPublic = false,
                OrganizerId = u1.Id
            }
            
        );
        db.SaveChanges();
    }
}

app.Run();
