namespace ftn.Services;

using System.Security.Cryptography;
using ftn.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;

    public OtpService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateAndSaveOtpAsync(string identity, TimeSpan ttl)
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl};

        await _cache.SetStringAsync($"otp:{identity}", otp, options);
        return otp;
    }

    public async Task<bool> ValidateOtpAsync(string identity, string inputOtp)
    {
        var key = $"otp:{identity}";
        var cachedOtp = await _cache.GetStringAsync(key);

        if (cachedOtp == null || cachedOtp != inputOtp)
            return false;
        
        await _cache.RemoveAsync(key);
        return true;
    }
}