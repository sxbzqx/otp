namespace ftn.Dtos;

public record RegisterDto(string Email, string Password, string FirstName, string LastName);
public record LoginDto(string Email, string Password);
public record ForgotPasswordDto(string Email);
public record VerifyOtpDto(string Email, string Otp);
public record ResetPasswordDto(string ResetToken, string NewPassword);
