namespace ftn.Services.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndSaveOtpAsync(string identity, TimeSpan ttl);
    Task<bool> ValidateOtpAsync(string identity, string inputOtp);
}