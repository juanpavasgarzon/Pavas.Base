using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.IntegrationEvents;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Infrastructure.IntegrationEvents;
using Infrastructure.Interceptors;
using Infrastructure.Jobs;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Quartz;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);

        AddServices(services);
        
        AddIntegrationEvents(services);

        AddJobs(services, configuration);
        
        AddInterceptorsInternal(services);

        AddHealths(services, configuration);

        AddAuthenticationInternal(services, configuration);

        AddAuthorizationInternal(services);
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    }

    private static void AddIntegrationEvents(IServiceCollection services)
    {
        services.AddScoped<IEventPublisher>(sp => sp.GetRequiredService<EventPublisher>());
    }
    
    private static void AddJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(configurator =>
        {
            var outboxJobKey = new JobKey(nameof(PublishOutboxMessagesJob));

            configurator.AddJob<PublishOutboxMessagesJob>(outboxJobKey);

            configurator.AddTrigger(trigger => trigger.ForJob(outboxJobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(Convert.ToInt32(configuration["Outbox:IntervalInSeconds"]))
                        .RepeatForever()));
        });

        services.AddQuartzHostedService();
    }

    private static void AddInterceptorsInternal(IServiceCollection services)
    {
        services.AddSingleton<OutboxMessageInterceptor>();
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var outboxInterceptor = serviceProvider.GetRequiredService<OutboxMessageInterceptor>();

            var connectionString = configuration.GetConnectionString("Database");

            options.UseNpgsql(connectionString, ConfigureMigrations);

            options.AddInterceptors(outboxInterceptor);

            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }

    private static void ConfigureMigrations(NpgsqlDbContextOptionsBuilder npgsqlOptions)
    {
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default);
    }

    private static void AddHealths(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddHealthChecks().AddNpgSql(connectionString!);
    }

    private static void AddAuthenticationInternal(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
    }

    private static void AddAuthorizationInternal(IServiceCollection services)
    {
        services.AddAuthorization();
    }
}
