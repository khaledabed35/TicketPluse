using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data; 
using DAL.Data.AuthModel;
using DAL.Repository.Class;
using DAL.Repository.Interface;
using Microsoft.AspNetCore.Identity;


namespace BLL.Services.Class
{
    public class ProfileService : IProfileService
    {
        private readonly IGenaricRePo<DAL.Data.AuthModel.UserProfile> _profileRepo;
        private readonly UserManager<App_user> _userManager;

        public ProfileService(IGenaricRePo<DAL.Data.AuthModel.UserProfile> profileRepo, UserManager<App_user> userManager)
        {
            _profileRepo = profileRepo;
            _userManager = userManager;
        }

        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            var profile = await _profileRepo.GetByIdAsync(userId); 

            if (profile == null) return null;

            var user = await _userManager.FindByIdAsync(userId);

            return new UserProfileDto
            {
              
                Bio = profile.Bio,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                PhoneNumber = user?.PhoneNumber ?? profile.PhoneNumber,
                FirstName = user?.f_name ?? "",
                LastName = user?.l_name ?? "",
                Email = user?.Email ?? ""
            };
        }

        public async Task<IEnumerable<UserProfile>> GetProfilesAsync()
        {
            var profiles = await _profileRepo.GetAllAsync();

            var profileDtos = new List<UserProfile>();

            foreach (var profile in profiles)
            {
                if (profile == null) continue;

                var user = await _userManager.FindByIdAsync(profile.App_userId);

                profileDtos.Add(new UserProfile
                {
                    Id = profile.Id,
                    App_userId = profile.App_userId,
                    Bio = profile.Bio,
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    PhoneNumber = user?.PhoneNumber ?? profile.PhoneNumber,
                    f_name = user?.f_name ?? "",
                    l_name = user?.l_name ?? "",
                    Email = user?.Email ?? ""
                });
            }

            // 3. إرجاع اللستة كاملة بعد انتهاء اللوب
            return profileDtos;
        }

        public async Task<bool> UpdateProfileAsync(string userId, UserProfileDto dto)
        {
            // أ. جلب البروفايل الحالي
            var profile = await _profileRepo.GetByIdAsync(userId); 
            if (profile == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            profile.Bio = dto.Bio ?? profile.Bio;
            profile.ProfilePictureUrl = dto.ProfilePictureUrl ?? profile.ProfilePictureUrl;
            profile.PhoneNumber = dto.PhoneNumber ?? profile.PhoneNumber;

            user.f_name = dto.FirstName ?? user.f_name;
            user.l_name = dto.LastName ?? user.l_name;
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            // هـ. حفظ التعديلات في جدول الـ Users
            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded) return false;

             _profileRepo.Update(profile); 

            return true;
        }
    }
}