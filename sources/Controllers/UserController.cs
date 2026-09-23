using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SwiftUI_backend_demo.sources.Data;
using SwiftUI_backend_demo.sources.Dtos;
using System.Security.Claims;

namespace SwiftUI_backend_demo.sources.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This attribute ensures that all actions in this controller require authentication
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/users/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // 兼容多种常用的 User ID Claim 提取方式
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? User.FindFirst("sub")?.Value 
                   ?? User.FindFirst("id")?.Value;
                           if (userIdClaim == null)
        {
            return Unauthorized("User ID claim not found.");
        }

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User ID claim not found." });
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var userProfileDto = new UserProfileDto(user.Id, user.Username, user.CreatedAt);
        return Ok(userProfileDto);
    }
}


