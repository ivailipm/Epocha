using Epocha.Api.Contracts;
using Epocha.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Epocha.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var outcome = await authService.RegisterAsync(request.Email, request.Password, request.DisplayName, cancellationToken);

        return outcome.Error switch
        {
            AuthError.EmailAlreadyRegistered => Problem(detail: "An account with this email already exists.", statusCode: StatusCodes.Status409Conflict),
            _ => outcome.Result!
        };
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var outcome = await authService.LoginAsync(request.Email, request.Password, cancellationToken);

        return outcome.Error switch
        {
            AuthError.InvalidCredentials => Problem(detail: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized),
            _ => outcome.Result!
        };
    }
}
