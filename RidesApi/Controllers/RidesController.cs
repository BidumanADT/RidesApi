using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RidesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]    // require a valid JWT
public class RidesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
    Ok(new[] {
      new { Id = 1, Driver = "Alice", From = "Downtown", To = "Airport" },
      new { Id = 2, Driver = "Bob",   From = "Mall",     To = "Home" }
    });
}
