namespace AuthService.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string Role
);

public record ValidateTokenResponse(
    bool IsValid,
    UserInfo? User
);
