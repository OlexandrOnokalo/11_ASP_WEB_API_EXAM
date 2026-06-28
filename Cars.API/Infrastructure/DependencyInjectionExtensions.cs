using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;

namespace Cars.API.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Перевіряю ключ при старті — краще впасти одразу, ніж ловити загадкові 401 у продакшені
            string? secretKey = configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey), "Jwt secret key is null");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true, // перевіряю всі три — без будь-якого токен можна підробити або прийняти з чужого сервера
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // за замовчуванням JWT дає 5 хв. буфер — скидаю в нуль, токен вмирає рівно вчасно
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // HMAC-SHA256, симетричний — один ключ і для підпису, і для перевірки
                };
            });

            return services;
        }

        // params — реєструю будь-яку кількість задач без зміни методу; тип як аргумент, бо Quartz потребує Type, а не generic
        public static IServiceCollection AddJobs(this IServiceCollection services, params (Type type, string cronSchedule)[] jobs)
        {
            services.AddQuartz(q =>
            {
                foreach (var job in jobs)
                {
                    // Ім'я типу як ключ — унікальне і зручно читати в логах Quartz
                    var jobKey = new JobKey(job.type.Name);

                    q.AddJob(job.type, jobKey);

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity($"{job.type.Name}-trigger")
                        .WithCronSchedule(job.cronSchedule));
                }
            });

            return services;
        }
    }
}