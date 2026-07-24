using BLL.Dto;
using DAL.Data.AuthModel;
using System.Threading.Tasks;
using TicketPluse.Dto;

namespace BLL.Services.Interface
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(string userId);
        Task<IEnumerable<UserProfile?>> GetProfilesAsync();
        Task<bool> UpdateProfileAsync(string userId, UserProfileDto dto);
    }
}