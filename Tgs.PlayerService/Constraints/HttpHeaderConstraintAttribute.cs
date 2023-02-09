using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Tgs.PlayerService.Constraints;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class HttpHeaderConstraintAttribute : Attribute, IActionConstraint
{
    public string Header { get; set; } = default!;

    public int Order => 0;

    public string? Value { get; set; }

    internal bool AllowEmpty => string.IsNullOrEmpty(Value);

    public HttpHeaderConstraintAttribute(string headerName)
    {
        Header = headerName;
    }

    public bool Accept(ActionConstraintContext context)
    {
        return context.RouteContext.HttpContext.Request.Headers.TryGetValue(Header, out var value)
               && (string.IsNullOrEmpty(Value) || value[0] == Value);
    }
}