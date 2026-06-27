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
            // Падаю одразу при старті якщо ключ не налаштований —
            // краще вибухнути тут ніж отримати 500 на першому ж запиті
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
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    // ClockSkew = Zero — прибираю дефолтний допуск у 5 хвилин;
                    // токен живе рівно стільки, скільки сказано в ExpHours, не більше
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            return services;
        }

        // Приймаю довільну кількість пар (тип job, CRON) —
        // щоб додати нову задачу достатньо передати ще один елемент у Program.cs
        public static IServiceCollection AddJobs(this IServiceCollection services, params (Type type, string cronSchedule)[] jobs)
        {
            services.AddQuartz(q =>
            {
                foreach (var job in jobs)
                {
                    // Ім'я ключа беру з назви класу — зручно бачити в логах Quartz
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