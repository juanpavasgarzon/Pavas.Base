namespace Web.Api.Endpoints;

internal interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
