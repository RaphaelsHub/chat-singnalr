using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatSinglar.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ChatSinglar.Services;

public class JwtService : IJwtService
{
    private readonly int _tokenExpirationMinutes;
    private readonly string _secretKey;
    private readonly byte[] _secretKeyToByteArray;
    private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

    public JwtService(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not set in configuration.");
        _secretKeyToByteArray = Encoding.ASCII.GetBytes(_secretKey);
        _tokenExpirationMinutes = int.TryParse(configuration["Jwt:TokenExpirationMinutes"], out var minutes) ? minutes : 30;
    }

    public string GenerateToken(IdentityUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_secretKeyToByteArray), SecurityAlgorithms.HmacSha256Signature)
        };

        return _tokenHandler.WriteToken(_tokenHandler.CreateToken(tokenDescriptor));
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            _tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_secretKeyToByteArray),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
