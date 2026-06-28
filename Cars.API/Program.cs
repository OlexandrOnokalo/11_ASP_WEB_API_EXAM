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

// Ініціалізую Serilog до build'у host'а — щоб будь-яке падіння при старті теж потрапило в лог
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // sink'и і рівні логування беру з appsettings.json
    .CreateLogger();

// Підмінюю Microsoft.Extensions.Logging на Serilog для всього host'а
builder.Host.UseSerilog();

// AppDbContext — Scoped за замовчуванням; всі сервіси що його використовують теж мусять бути Scoped
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string? connectionString = builder.Configuration.GetConnectionString("LocalDb");
    options.UseNpgsql(connectionString);
});

// Прив'язую POCO JwtSettings до секції конфігу — щоб IOptions<JwtSettings> працював у JwtService
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// RequireUniqueEmail — логін іде через email, дублікати зламають FindByEmailAsync
// Правила пароля навмисно послаблені — навчальний проект, зручніше тестувати
// AddDefaultTokenProviders — без нього UserManager не згенерує токен для підтвердження email
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

// Деталі JWT-валідації (Issuer, Audience, ClockSkew=0) — у DependencyInjectionExtensions.cs
builder.Services.AddJwtAuthentication(builder.Configuration);

// Щотижня у неділю о 00:00 UTC — чищу протерміновані refresh-токени старші за 7 днів
builder.Services.AddJobs(
    (typeof(RefreshTokensCleanupJob), "0 0 0 ? * SUN")
);
// WaitForJobsToComplete — щоб при shutdown сервер дочекався завершення DELETE перед зупинкою
builder.Services.AddQuartzHostedService(cfg => cfg.WaitForJobsToComplete = true);

// Дозволяю тільки Vite dev server на 5173 — в продакшені потрібно замінити на реальний домен
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

builder.Services.AddControllers();

// Перезаписую дефолтний формат помилок валідації — хочу свій ErrorResponseDto, а не стандартний ProblemDetails
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

// AddSecurityRequirement — щоб кнопка "Authorize" у Swagger UI діяла глобально для всіх ендпоінтів
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

// Всі Scoped — AppDbContext (EF Core) теж Scoped; Singleton + DbContext = exception у runtime
builder.Services.AddScoped<RefreshTokenRepository>();

builder.Services.AddScoped<ManufactureService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ImageService>();
// Передаю assembly де CarMapperProfile — AutoMapper сам знайде і ManufactureMapperProfile у тому ж assembly
builder.Services.AddAutoMapper(cfg => { }, typeof(CarMapperProfile).Assembly);

var app = builder.Build();

// Swagger тільки в Development — не хочу відкривати схему API назовні в продакшені
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Маплю фізичний Storage/Cars/ на URL /images/cars/ — звідти віддаються фото авто
// Папку створюю сам, бо git не трекає порожні директорії — на свіжому clone її не буде
string storagePath = Path.Combine(app.Environment.ContentRootPath, StaticFilesSettings.StorageDir, StaticFilesSettings.CarsDir);
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = StaticFilesSettings.CarsUrl
});

// CORS перед Authentication — браузер відправляє preflight OPTIONS до автентифікації
app.UseCors(corsPolicyName);

// Authentication → Authorization — порядок критичний, інакше [Authorize] не бачить user'а
app.UseAuthentication();
app.UseAuthorization();

// ExceptionMiddleware після Auth — ловить бізнес-виключення з сервісів і перетворює на ErrorResponseDto
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

// Запускаю після Build() але до Run() — міграції і seed виконуються до першого HTTP-запиту
await Seeder.SeedAsync(app.Services);

app.Run();

// Флашу буфер Serilog після зупинки — щоб останні логи не загубились у пам'яті
Log.CloseAndFlush();
