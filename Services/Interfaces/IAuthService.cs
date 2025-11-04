using MyEventsApi.Dtos;

namespace MyEventsApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);
    }
}
