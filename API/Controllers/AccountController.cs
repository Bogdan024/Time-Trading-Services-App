using System.Security.Cryptography;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, AppDbContext context) : BaseApiController
{
    private const string RefreshTokenCookieName = "refreshToken";

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var email = registerDto.Email.Trim().ToLowerInvariant();
        var displayName = registerDto.DisplayName.Trim();
        var city = registerDto.City.Trim();
        var countryCode = registerDto.CountryCode.Trim().ToUpperInvariant();

        if (await EmailExists(email))
        {
            return BadRequest("Email is already in use");
        }

        var user = new AppUser
        {
            DisplayName = displayName,
            Email = email,
            UserName = email,
            Member = new Member
            {
                DisplayName = displayName,
                About = registerDto.About?.Trim(),
                City = city,
                CountryCode = countryCode,
                IsProfilePublic = registerDto.IsProfilePublic
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Member");

        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        await AddRefreshToken(user);

        return await user.ToDto(tokenService, userManager);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var email = loginDto.Email.Trim().ToLowerInvariant();
        var user = await context.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            return Unauthorized("Invalid email address");
        }

        var isValidPassword = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!isValidPassword)
        {
            return Unauthorized("Invalid password");
        }

        await AddRefreshToken(user);

        return await user.ToDto(tokenService, userManager);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized("No refresh token found");
        }

        var user = await context.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

        if (user is null)
        {
            return Unauthorized("Invalid refresh token");
        }

        var oldRefreshToken = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (!oldRefreshToken.IsActive)
        {
            return Unauthorized("Inactive refresh token");
        }

        var newRefreshToken = GenerateRefreshToken();
        oldRefreshToken.RevokedAtUtc = DateTime.UtcNow;
        oldRefreshToken.RevokedByIp = GetIpAddress();
        oldRefreshToken.ReasonRevoked = "Replaced by new token";
        oldRefreshToken.ReplacedByToken = newRefreshToken.Token;

        user.RefreshTokens.Add(newRefreshToken);
        RemoveOldRefreshTokens(user);
        await context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshToken);

        return await user.ToDto(tokenService, userManager);
    }

    [HttpPost("revoke-token")]
    public async Task<ActionResult> RevokeToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Ok();
        }

        var user = await context.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

        if (user is null)
        {
            DeleteRefreshTokenCookie();
            return Ok();
        }

        var token = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (token.IsActive)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            token.ReasonRevoked = "Revoked by user";
            await context.SaveChangesAsync();
        }

        DeleteRefreshTokenCookie();
        return Ok();
    }

    private async Task AddRefreshToken(AppUser user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);
        RemoveOldRefreshTokens(user);
        await context.SaveChangesAsync();
        SetRefreshTokenCookie(refreshToken);
    }

    private RefreshToken GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedByIp = GetIpAddress()
        };
    }

    private void SetRefreshTokenCookie(RefreshToken refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.ExpiresAtUtc,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken.Token, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }

    private static void RemoveOldRefreshTokens(AppUser user)
    {
        user.RefreshTokens.RemoveAll(x => !x.IsActive && x.CreatedAtUtc.AddDays(2) <= DateTime.UtcNow);
    }

    private string? GetIpAddress()
    {
        return Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor)
            ? forwardedFor.ToString()
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private async Task<bool> EmailExists(string email)
    {
        return await userManager.FindByEmailAsync(email) is not null;
    }
}
