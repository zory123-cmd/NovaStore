using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NovaStore.WebApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");
        return userId;
    }
}
