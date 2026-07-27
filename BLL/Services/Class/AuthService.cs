using BLL.Dto;
using BLL.Services.Interface;
using DAL.Data.AuthModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography; 
using System.Text;
using System.Threading.Tasks;
using TicketPluse.Helper;

namespace TicketPluse.Services.Classes
{
    public class AuthServices : IAuthService
    {
        private readonly UserManager<App_user> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailAsync;
        private readonly JWT _jwt;

        public AuthServices(
            UserManager<App_user> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IEmailService emailAsync,
            IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailAsync = emailAsync;
            _jwt = jwt.Value;
        }

        public async Task<string> AddRoleAsync(AddRoleModel addRoleModel)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == addRoleModel.email);

            if (user == null) return "user not found";
            if (!await _roleManager.RoleExistsAsync(addRoleModel.role)) return "role is not exist";
            if (await _userManager.IsInRoleAsync(user, addRoleModel.role)) return "user already has this role";

            await _userManager.AddToRoleAsync(user, addRoleModel.role);
            return "Role added successfully";
        }

        public async Task<string> ConfirmEmail(Guid userid, string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userid);
            if (user == null) return "user not found";

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return "email is confirmed";

            return "email confirmed failed ";
        }

        public async Task<string> ConfirmPassword(forgetpasswordconfirm forgetPasswordConfirm)
        {
            if (forgetPasswordConfirm.userid is null || forgetPasswordConfirm.token is null)
                return "token is expired";

            if (forgetPasswordConfirm.newpassword != forgetPasswordConfirm.confirmpassword)
                return "password not match";

            if (!Guid.TryParse(forgetPasswordConfirm.userid, out var userGuid))
                return "invalid user id format";

            if (string.IsNullOrEmpty(forgetPasswordConfirm.token))
                return "token is required";

            var user = await _userManager.FindByIdAsync(forgetPasswordConfirm.userid);
            if (user == null)
                return "user not found";

            var decodedToken = System.Web.HttpUtility.UrlDecode(forgetPasswordConfirm.token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, forgetPasswordConfirm.newpassword);

            if (result.Succeeded)
                return string.Empty;

            return result.Errors.FirstOrDefault()?.Description ?? "password reset failed";
        }

        public async Task<IReadOnlyCollection<UserDto>> GetAllUserAsync(Guid adminId)
        {
            var admin = await _userManager.FindByIdAsync(adminId.ToString());

            if (admin == null || !await _userManager.IsInRoleAsync(admin, "Admin"))
                throw new UnauthorizedAccessException("Only admin can access this resource");

            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.f_name,
                    LastName = user.l_name,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    PhoneNumber = user.PhoneNumber
                });
            }

            return userDtos;
        }

        public async Task<string> setAdminRoleAsync(Guid adminId, string userEmail)
        {
            var admin = await _userManager.FindByIdAsync(adminId.ToString());
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (admin == null || !await _userManager.IsInRoleAsync(admin, "Admin"))
                return "Access denied";

            if (user == null) return "User not found";

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded) return "Failed to assign role";

            return "Admin role assigned successfully";
        }

        public async Task<string> RemoveAdminRoleAsync(Guid adminId, string userEmail)
        {
            var admin = await _userManager.FindByIdAsync(adminId.ToString());
            if (admin == null || !await _userManager.IsInRoleAsync(admin, "Admin"))
                return "Access denied";

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return "User not found";

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return "User is not an Admin";

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded) return "Admin role removed successfully";

            return "Something went wrong";
        }

        public async Task<bool> IsAdmin(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, "Admin");
        }

        public async Task<bool> IsUser(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null;
        }

        public async Task<AuthModel> LoginAsync(loginModel loginModel)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == loginModel.email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.password))
            {
                return new AuthModel { Message = "Email or password is invalid" };
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new AuthModel { Message = "Email is not confirmed" };
            }

            // 🌟 توليد وتحديث الـ Refresh Token ليعيش 7 أيام
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            var jwtsecuritytoken = await CreateJwt(user);
            var role = await _userManager.GetRolesAsync(user);

            return new AuthModel
            {
                Message = "Login Success",
                IsAuthenticated = true,
                username = user.UserName,
                email = user.Email,
                expireon = jwtsecuritytoken.ValidTo,
                token = new JwtSecurityTokenHandler().WriteToken(jwtsecuritytoken),
                RefreshToken = refreshToken, // 👈 محتاج تضيف البروبرتي دي في الـ AuthModel لو مش موجودة
                role = role.ToList()
            };
        }

        public async Task<AuthModel> RegisterAsync(registerModel registerModel)
        {
            if (await _userManager.FindByEmailAsync(registerModel.Email) is not null)
            {
                return new AuthModel { Message = "Email Already Exists" };
            }

            if (await _userManager.FindByNameAsync(registerModel.username) is not null)
            {
                return new AuthModel { Message = "Username Already Exists" };
            }

            var user = new App_user
            {
                UserName = registerModel.username,
                Email = registerModel.Email,
                f_name = registerModel.firstname,
                l_name = registerModel.lastname
            };

            var result = await _userManager.CreateAsync(user, registerModel.password);

            if (!result.Succeeded)
            {
                return new AuthModel
                {
                    Message = string.Join("\n", result.Errors.Select(e => e.Description)),
                    IsAuthenticated = false
                };
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var sendemail = await _emailAsync.SendEmailAsync(
                registerModel.Email,
                token,
                "auth",
                "https://localhost:7200/api",
                "Confirm your email"
            );

            if (!string.IsNullOrEmpty(sendemail))
            {
                await _userManager.DeleteAsync(user);
                return new AuthModel { Message = $"Email failed: {sendemail}", IsAuthenticated = false };
            }

            var rolename = "User";
            if (!await _roleManager.RoleExistsAsync(rolename))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(rolename));
            }

            await _userManager.AddToRoleAsync(user, rolename);

            // 🌟 توليد وتحديث الـ Refresh Token عند التسجيل أيضاً
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var jwtsecuritytoken = await CreateJwt(user);

            return new AuthModel
            {
                Message = "Register Success",
                IsAuthenticated = true,
                username = user.UserName,
                email = user.Email,
                expireon = jwtsecuritytoken.ValidTo,
                token = new JwtSecurityTokenHandler().WriteToken(jwtsecuritytoken),
                RefreshToken = refreshToken,
                role = new List<string> { rolename }
            };
        }

        // 🌟 تعديل توليد الـ Access Token ليموت بعد ربع ساعة
        private async Task<JwtSecurityToken> CreateJwt(App_user user)
        {
            var claimsUser = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("username", $"{user.f_name} {user.l_name}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", user.Id.ToString())
            }
            .Union(claimsUser)
            .Union(roleClaims);

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var Token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // ⏱️ تموت كل ربع ساعة بالظبط
                signingCredentials: signingCredentials
            );
            return Token;
        }

        // 🌟 ميثود توليد سترينج عشوائي مشفر للـ Refresh Token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // 🌟 ميثود التجديد التلقائي (تأكد من إضافتها لـ IAuthService أولاً)
        public async Task<AuthModel> RefreshTokenAsync(TokenRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.AccessToken) || string.IsNullOrEmpty(dto.RefreshToken))
                return new AuthModel { Message = "Invalid client request" };

            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return new AuthModel { Message = "Invalid access token" };

            // استخراج الـ uid من التوكن الميت
            var userIdStr = principal.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return new AuthModel { Message = "Invalid user token claims" };

            var user = await _userManager.FindByIdAsync(userId.ToString());

            // التشيك: هل التوكن مات؟ وهل الـ Refresh Token متطابق وصالح؟
            if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpireTime <= DateTime.UtcNow)
                return new AuthModel { Message = "Invalid or expired refresh token" };

            // توليد طقم جديد تماماً
            var newJwtToken = await CreateJwt(user);
            var newRefreshToken = GenerateRefreshToken();

            // حفظ التحديثات في الداتابيز وتمديد الـ 7 أيام
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return new AuthModel
            {
                Message = "Token Refreshed Successfully",
                IsAuthenticated = true,
                username = user.UserName,
                email = user.Email,
                expireon = newJwtToken.ValidTo,
                token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
                RefreshToken = newRefreshToken,
                role = roles.ToList()
            };
        }

        // 🌟 فك التوكن الميت لاستخراج الـ Claims بدون ضرب Exception بسبب الـ Validation
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                ValidateLifetime = false // 👈 بنلغي تشيك الوقت هنا عشان نعرف نقرأ التوكن الميت
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }

        public async Task<App_user> GetuserData(Guid userid)
        {
            return await _userManager.FindByIdAsync(userid.ToString());
        }

        public async Task<string> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "User not found";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var emailResult = await _emailAsync.SendResetPasswordEmailAsync(
                user.Email,
                token,
                "auth",
                "https://localhost:7200/api",
                "Reset Your Password"
            );

            if (emailResult != null) return emailResult;

            return "Password reset email sent successfully";
        }
    }
}