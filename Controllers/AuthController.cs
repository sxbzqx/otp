namespace ftn.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ftn.Dtos;
using ftn.Models;
using ftn.Services.Interfaces;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<User> userManager,
        IOtpService otpService,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _otpService = otpService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null)
            return BadRequest("Пользователь уже существует.");

        var user = new User
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Регистрация прошла успешно.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Неверный логин или пароль.");

        var token = _jwtService.GenerateAccessToken(user);
        return Ok(new { AccessToken = token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Ok("Если Email существует, код отправлен.");

        var otp = await _otpService.GenerateAndSaveOtpAsync(user.Email!, TimeSpan.FromMinutes(5));

        // Вызов отправки письма/СМС
        return Ok("Код отправлен.");
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var isValid = await _otpService.ValidateOtpAsync(dto.Email, dto.Otp);
        if (!isValid)
            return BadRequest("Неверный или истекший код.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        var resetToken = _jwtService.GeneratePasswordResetToken(user!);

        return Ok(new { ResetToken = resetToken });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var userId = GetUserIdFromResetToken(dto.ResetToken);
        if (userId == null)
            return Unauthorized("Недействительный токен сброса.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден.");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors);

        var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors);

        return Ok("Пароль успешно изменен.");
    }

    private string? GetUserIdFromResetToken(string token)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            
            var purpose = jwt.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
            if (purpose != "password_reset") return null;

            return jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }
}