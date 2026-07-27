using BLL.Dto;
using DAL.Data.AuthModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(registerModel registerModel);
        Task<AuthModel> LoginAsync(loginModel loginModel);
        Task<string> AddRoleAsync(AddRoleModel addRoleModel);
        Task<string> setAdminRoleAsync(Guid adminId, string userEmail);
        Task<string> RemoveAdminRoleAsync(Guid adminId, string userEmail);
        Task<string> ConfirmEmail(Guid userid, string token);
        Task<string> ConfirmPassword(forgetpasswordconfirm forgetPasswordConfirm);
        Task<string> ForgotPassword(string email);
        Task<App_user> GetuserData(Guid userid);
        Task<IReadOnlyCollection<UserDto>> GetAllUserAsync(Guid adminId);
        Task<bool> IsAdmin(Guid userId);
        Task<bool> IsUser(Guid userId);
        Task<AuthModel> RefreshTokenAsync(TokenRequestDto dto);
    }
}