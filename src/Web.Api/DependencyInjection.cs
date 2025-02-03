using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    public static void AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGenWithAuth();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();
    }
}
