namespace SwiftUI_backend_demo.sources.Dtos;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);

public record AuthResponseDto(
    int Id,
    string Username,
    string Token = ""
);