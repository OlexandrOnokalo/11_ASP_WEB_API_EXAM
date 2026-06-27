using Cars.API.Infrastructure;
using Cars.API.Jobs;
using Cars.API.Middlewares;
using Cars.API.Models;
using Cars.API.Settings;
using Cars.BLL.MapperProfiles;
using Cars.BLL.Services;
using Cars.DAL;
using Cars.DAL.Entities.Identity;
using Cars.DAL.Repositories;
using Cars.DAL.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Quartz;
using Serilog;
using JwtSettings = Cars.BLL.Settings.JwtSettings;

var builder = WebApplication.CreateBuilder(args);

// Serilog підключаю першим — щоб навіть помилки старту програми потрапили в лог
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string? connectionString = builder.Configuration.GetConnectionString("LocalDb");
    options.UseNpgsql(connectionString);
});

// Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add identity
// Свідомо послабив вимоги до пароля — це навчальний проєкт, не продакшн;
// у реальному додатку RequireUppercase/RequireDigit варто вмикати
builder.Services.AddIdentity<AppUserEntity, AppRoleEntity>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Quartz (щотижня у неділю о 00:00)
// WaitForJobsToComplete = true — щоб при зупинці сервера Quartz дочекався завершення джобу,
// а не обрізав DELETE посередині
builder.Services.AddJobs(
    (typeof(RefreshTokensCleanupJob), "0 0 0 ? * SUN")
);
builder.Services.AddQuartzHostedService(cfg => cfg.WaitForJobsToComplete = true);

// CORS
const string corsPolicyName = "allowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Validation response format
// Перевизначаю стандартну відповідь 400 від ASP.NET — хочу отримувати
// свій ErrorResponseDto з полем errors замість дефолтного ProblemDetails
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    .ToArray()
            );

        var response = new ErrorResponseDto
        {
            Message = "Validation failed",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

// OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cars API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT токен у форматі: Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

// Repositories
builder.Services.AddScoped<RefreshTokenRepository>();

// Services
builder.Services.AddScoped<ManufactureService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddAutoMapper(cfg => { }, typeof(CarMapperProfile).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Static files for cars images
// Маплю фізичну папку Storage/Cars/ на URL-префікс /images/cars —
// так фронт може брати зображення напряму без контролера
string storagePath = Path.Combine(app.Environment.ContentRootPath, StaticFilesSettings.StorageDir, StaticFilesSettings.CarsDir);
if (!Directory.Exists(storagePath))
{
    // Папка може не існувати при першому запуску — створюю щоб UseStaticFiles не впав
    Directory.CreateDirectory(storagePath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = StaticFilesSettings.CarsUrl
});

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

// ExceptionMiddleware іде після auth — щоб помилки авторизації теж
// перехоплювались і поверталися у форматі ErrorResponseDto
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

// Сідер запускаю після app.Build() але до app.Run() —
// міграції та дефолтні дані мають бути в БД до першого реального запиту
await Seeder.SeedAsync(app.Services);

app.Run();

// Явний flush — гарантую що останні логи запишуться при завершенні процесу
Log.CloseAndFlush();
