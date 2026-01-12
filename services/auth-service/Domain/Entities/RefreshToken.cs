namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Revoked { get; private set; }

    public User? User { get; private set; }

    private RefreshToken() { } // EF Core

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        };
    }

    public void Revoke()
    {
        Revoked = true;
    }

    public bool IsValid()
    {
        return !Revoked && ExpiresAt > DateTime.UtcNow;
    }
}
