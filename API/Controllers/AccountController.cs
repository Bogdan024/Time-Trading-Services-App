using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var email = registerDto.Email.Trim().ToLower();
        var displayName = registerDto.DisplayName.Trim();
        var city = registerDto.City.Trim();
        var countryCode = registerDto.CountryCode.Trim().ToUpperInvariant();

        if (await EmailExists(email))
        {
            return BadRequest("Email is already in use");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            DisplayName = displayName,
            Email = email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key,
            Member = new Member
            {
                DisplayName = displayName,
                About = registerDto.About?.Trim(),
                City = city,
                CountryCode = countryCode,
                IsProfilePublic = registerDto.IsProfilePublic
            }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user is null)
        {
            return Unauthorized("Invalid email address");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid password");
            }
        }

        return user.ToDto(tokenService);
    }

    private Task<bool> EmailExists(string email)
    {
        return context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
