using System.Threading.Tasks;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Interfaces;

public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
