using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tgs.PlayerService.Constraints;

namespace Tgs.PlayerService.Swagger;

public class HttpHeaderConstraintOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var headers = context.ApiDescription.CustomAttributes().OfType<HttpHeaderConstraintAttribute>();

        foreach (var header in headers)
        {
            var parameter = new OpenApiParameter
            {
                Name = header.Header,
                In = ParameterLocation.Header,
                AllowEmptyValue = header.AllowEmpty,
                Required = false
            };
            operation.Parameters.Add(parameter);
        }
    }
}