using Microsoft.AspNetCore.Authorization;

namespace MCA.Context.Identity.Infrastructure.Authorization;

public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string ClaimType = "permission";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, PermissionRequirement req)
    {
        if (ctx.User.HasClaim(ClaimType, req.Value))
            ctx.Succeed(req);

        return Task.CompletedTask;
    }
}