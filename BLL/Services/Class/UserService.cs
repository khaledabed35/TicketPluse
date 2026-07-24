using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data.AuthModel;
using DAL.Repository.Class;
using DAL.Repository.Interface;
using DAL.Specification;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Class
{
    public class UserService : IUser
    {
        private readonly IGenaricRePo<App_user> _userRepo;
        private readonly UserManager<App_user> _userManager;

        public UserService(IGenaricRePo<App_user> user, UserManager<App_user> userManager)
        {
            _userRepo = user; 
            _userManager = userManager; 
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(UserQueryParameters pram)
        {
            var spec = new UserSpecification(pram);
            var data = await _userRepo.GetWithSpecAsync(spec);

            var userDtos = new List<UserDto>();

            foreach (var u in data)
            {
                // 👇 بنجيب الأدوار لكل مستخدم من الـ UserManager مباشرة
                var roles = await _userManager.GetRolesAsync(u);

                userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.f_name,
                    LastName = u.l_name,
                    Email = u.Email,
                    Roles = roles.ToList() // الأدوار هتنزل هنا مظبوطة مش null 👍
                });
            }

            return userDtos;
        }
        // 2. جلب مستخدم بالـ ID (يرجع UserDto)
        public async Task<UserDto?> GetUserById(Guid id)
        {
            var u = await _userRepo.GetByIdAsync(id);
            if (u == null) return null;

            // 👇 بنجيب الأدوار لليوزر ده بالـ UserManager
            var roles = await _userManager.GetRolesAsync(u);

            return new UserDto
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                FirstName = u.f_name,
                LastName = u.l_name,
                Email = u.Email,
                Roles = roles.ToList()
            };
        }
        // 3. حذف مستخدم
        public async Task<bool> DeleteUser(Guid id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            _userRepo.Delete(user);
            return true;
        }

        // 4. تعديل بيانات مستخدم (يرجع الـ DTO المعدل أو null)
        public async Task<UserDto?> UpdateUser(UserDto dto)
        {
            var existingUser = await _userRepo.GetByIdAsync(dto.Id);
            if (existingUser == null) return null;

            existingUser.f_name = dto.FirstName;
            existingUser.l_name = dto.LastName;
            existingUser.PhoneNumber = dto.PhoneNumber;
            existingUser.Email = dto.Email;

            _userRepo.Update(existingUser);

            return dto;
        }

        // 5. إضافة مستخدم جديد (يرجع الـ DTO بالـ ID الجديد)
        public async Task<UserDto> AddUser(UserDto dto)
        {
            var newUser = new App_user
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                f_name = dto.FirstName,
                l_name = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                UserName = dto.Email // العادة بنساوي الـ UserName بالـ Email
            };

            await _userRepo.AddAsync(newUser);

            dto.Id = newUser.Id;
            return dto;
        }
    }
}