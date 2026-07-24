using BLL.Dto;
using DAL.Data.AuthModel;
using DAL.Specification;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IUser
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(UserQueryParameters pram);
        Task<UserDto?> GetUserById(Guid id);
        Task<bool> DeleteUser(Guid id);
        Task<UserDto?> UpdateUser(UserDto userDto); 
        Task<UserDto> AddUser(UserDto userDto);

    }
}
