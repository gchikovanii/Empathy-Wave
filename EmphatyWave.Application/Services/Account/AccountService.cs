using EmphatyWave.Application.Helpers;
using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.Account.Helper;
using EmphatyWave.Domain;
using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.DataSeeding;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace EmphatyWave.Application.Services.Account
{
    public class AccountService(UserManager<User> userManager, JwtProvider jwtProvider, TokenGenerator tokenGenerator, IValidator<RegisterDto> validator)
        : IAccountService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtProvider _jwtProvider = jwtProvider;
        private readonly TokenGenerator _tokenGenerator = tokenGenerator;
        private readonly IValidator<RegisterDto> _validator = validator;

        public async Task<ResultOrValue<string>> Register(RegisterDto dto)
        {
            ValidationResult result = await _validator.ValidateAsync(dto, default).ConfigureAwait(false);
            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                return ResultOrValue<string>.Failure(Result.Failure(new Error("ValidationError", errorMessage)));
            }
            var userName = await _userManager.FindByNameAsync(dto.UserName).ConfigureAwait(false);
            var userEmail = await _userManager.FindByEmailAsync(dto.Email).ConfigureAwait(false);
            if (userEmail != null)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("EmailAlreadyExists", ErrorMessages.EmailAlreadyExists)));
            if (userName != null)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("UserNameAlreadyExists", ErrorMessages.UserNameAlreadyExists)));
          
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                VerificationToken = _tokenGenerator.GenerateToken(dto.Email),
                VerificationTokenExp = DateTime.UtcNow.Add(TimeSpan.FromHours(24))
            };
            var res = await _userManager.CreateAsync(user, dto.Password).ConfigureAwait(false);
            var role = RoleType.User.ToString();
            var userRole = await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
            if (!userRole.Succeeded || !res.Succeeded || !res.Succeeded)
            {
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccountError", ErrorMessages.AccountError)));
            }
            var token = _jwtProvider.CreateToken(user, role);
            return ResultOrValue<string>.Success(token);
        }
        public async Task<ResultOrValue<string>> Login(LoginDto dto)
        {
            var user = await GetUserByEmail(dto.Email);
            if (user == null)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccountError", ErrorMessages.AccountError)));
            var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password).ConfigureAwait(false);
            if (!isPasswordValid)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccountError", ErrorMessages.AccountError)));
            if (user.EmailConfirmed == false)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccountIsNotActivated", ErrorMessages.AccountAct)));
            
           
            //Restrictied to user or admin! Can be both!
            var token = _jwtProvider.CreateToken(user, userRoles.First());
            return ResultOrValue<string>.Success(token);
        }

        public async Task<bool> ConfirmEmail(CancellationToken cancellationToken, string token)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(i => i.VerificationToken == token, cancellationToken).ConfigureAwait(false);
            if (user == null)
                throw new Exception("Bad Token");
            if (user.VerificationTokenExp <= DateTimeOffset.UtcNow)
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
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword).ConfigureAwait(false);
            if (!resetPasswordResult.Succeeded)
            {
                throw new Exception("Failed to reset password");
            }
            user.ResetTokenExp = null;
            user.ResetPasswordToken = string.Empty;
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return result.Succeeded;
        }
        public async Task RemoveExpiredTokensAsync(CancellationToken token, string option)
        {
            if (option == "Verification")
            {
                var usersTable = await _userManager.Users.Where(i => i.VerificationTokenExp <= DateTimeOffset.UtcNow).ToListAsync(token).ConfigureAwait(false);
                if (usersTable.Any())
                {
                    foreach (var user in usersTable)
                    {
                        user.VerificationTokenExp = null;
                        await _userManager.UpdateAsync(user).ConfigureAwait(false);
                    }
                }
            }
            else if (option == "ResetPassword")
            {
                var usersTable = await _userManager.Users.Where(i => i.ResetTokenExp <= DateTimeOffset.UtcNow).ToListAsync(token).ConfigureAwait(false);
                if (usersTable.Any())
                {
                    foreach (var user in usersTable)
                    {
                        user.ResetTokenExp = null;
                        await _userManager.UpdateAsync(user).ConfigureAwait(false);
                    }
                }
            }
        }
        private async Task<User> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            return user;
        }
    }
}
