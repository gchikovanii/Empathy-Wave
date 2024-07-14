using EmphatyWave.Application.Helpers;
using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.DataSeeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace EmphatyWave.Application.Services.Account
{
    public class AccountService(UserManager<User> userManager, JwtProvider jwtProvider, TokenGenerator tokenGenerator)
        : IAccountService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtProvider _jwtProvider = jwtProvider;
        private readonly TokenGenerator _tokenGenerator = tokenGenerator;

        public async Task<string> Register(RegisterDto dto)
        {
            var userName = await _userManager.FindByNameAsync(dto.UserName).ConfigureAwait(false);
            var userEmail = await _userManager.FindByEmailAsync(dto.Email).ConfigureAwait(false);
            if (userEmail != null)
                throw new Exception("User already exists");
            if (userName != null)
                throw new Exception("Username already exists");
            if (userEmail != null)
                throw new Exception("Email already exists");
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                VerificationToken = _tokenGenerator.GenerateToken(dto.Email),
                VerificationTokenExp = DateTime.UtcNow.Add(TimeSpan.FromHours(24))
            };
            var result = await _userManager.CreateAsync(user, dto.Password).ConfigureAwait(false);
            var role = RoleType.User.ToString();
            var userRole = await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
            if (!userRole.Succeeded || !result.Succeeded)
            {
                return String.Empty;
            }
            return _jwtProvider.CreateToken(user, role);
        }
        public async Task<string> Login(LoginDto dto)
        {
            var user = await GetUserByEmail(dto.Email);
            var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (user == null)
                throw new Exception("Incorrect data");
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password).ConfigureAwait(false);
            if (!isPasswordValid)
            {
                return String.Empty;
            }
            if (user.EmailConfirmed == false)
                throw new Exception("Account is not activated!");
            //Restrictied to user or admin! Can be both!
            return _jwtProvider.CreateToken(user, userRoles.First());
        }

        public async Task<bool> ConfirmEmail(CancellationToken cancellationToken, string token)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(i => i.VerificationToken == token, cancellationToken).ConfigureAwait(false);
            if (user == null)
                throw new Exception("Bad Token");
            if(user.VerificationTokenExp <= DateTimeOffset.UtcNow)
                throw new Exception("Verification Time is up");
            user.VerificationToken = string.Empty;
            user.VerifiedAt = DateTimeOffset.UtcNow;
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return result.Succeeded;
        }

        public async Task<bool> RequestPasswordRecovery(string email)
        {
            var user = await GetUserByEmail(email);
            if (user.ResetPasswordToken != null && user.ResetTokenExp >= DateTimeOffset.UtcNow)
                throw new Exception("Token is already generated!");
            var token = _tokenGenerator.GenerateToken(email);
            user.ResetPasswordToken = token;
            user.ResetTokenExp = DateTimeOffset.UtcNow.AddHours(3);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        public async Task<bool> ResetPassword(RecoveryDto dto)
        {
            var user = await GetUserByEmail(dto.Email);
            if (user.ResetPasswordToken != dto.ResetPasswordToken)
                return false;
            if (user.ResetTokenExp <= DateTimeOffset.UtcNow)
                throw new Exception("Token Time Expired");
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user,resetToken,dto.NewPassword).ConfigureAwait(false);
            if (!resetPasswordResult.Succeeded)
            {
                throw new Exception("Failed to reset password");
            }
            user.ResetTokenExp = null;
            user.ResetPasswordToken = string.Empty;
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return result.Succeeded;
        }
        private async Task<User> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null)
                throw new Exception("User already exists");
            return user;
        }
    }
}
