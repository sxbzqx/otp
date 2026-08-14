namespace ftn.Services.Interfaces;

using ftn.Models;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GeneratePasswordResetToken(User user);
}