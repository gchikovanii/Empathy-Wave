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
using System.Net.Mail;
using System.Net;
using EmphatyWave.ApiService.Infrastructure.Request;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


namespace EmphatyWave.Application.Services.Account
{
    public class AccountService(UserManager<User> userManager, IOptions<GoogleAppSettings> googleAppSettings,
        JwtProvider jwtProvider, TokenGenerator tokenGenerator, IValidator<RegisterDto> validator, ILogger<AccountService> logger)
        : IAccountService
    {
        private readonly GoogleAppSettings _googleAppSettings = googleAppSettings.Value;
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtProvider _jwtProvider = jwtProvider;
        private readonly TokenGenerator _tokenGenerator = tokenGenerator;
        private readonly IValidator<RegisterDto> _validator = validator;
        private readonly ILogger<AccountService> _logger = logger;

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
            var tokenToConfirm = _tokenGenerator.GenerateToken(dto.Email);
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                VerificationToken = tokenToConfirm,
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
            await SendConfirmationLink(dto.Email, tokenToConfirm, dto.UserName).ConfigureAwait(false);
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

        public async Task<ResultOrValue<string>> RequestPasswordRecovery(string email)
        {
            var user = await GetUserByEmail(email);
            if(user == null)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccNotFound", ErrorMessages.UserNotFound)));
            if (user.ResetPasswordToken != null && user.ResetTokenExp >= DateTimeOffset.UtcNow)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("TokenAlready", ErrorMessages.TokGen)));
            var token = _tokenGenerator.GenerateToken(email);
            user.ResetPasswordToken = token;
            user.ResetTokenExp = DateTimeOffset.UtcNow.AddHours(3);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ResultOrValue<string>.Failure(Result.Failure(new Error("UnexcpextedError", ErrorMessages.UnexcpectedError)));
            }
            await SendRecoveryEmail(email, "Recovery Link", token);
            return ResultOrValue<string>.Success(token);
        }
        public async Task<ResultOrValue<string>> ResetPassword(RecoveryDto dto)
        {
            var user = await GetUserByEmail(dto.Email);
            if (user.ResetPasswordToken != dto.ResetPasswordToken)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("IncorrectReset", ErrorMessages.IncorrectReset)));
            if (user.ResetTokenExp <= DateTimeOffset.UtcNow)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("ExpToken", ErrorMessages.ExpToken)));
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword).ConfigureAwait(false);
            if (!resetPasswordResult.Succeeded)
            {
                return ResultOrValue<string>.Failure(Result.Failure(new Error("ResetFailed", ErrorMessages.ResetFailed)));
            }
            user.ResetTokenExp = null;
            user.ResetPasswordToken = string.Empty;
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return ResultOrValue<string>.Success(resetToken);
        }
        public async Task<ResultOrValue<string>> ConfirmEmail(CancellationToken cancellationToken, string token)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(i => i.VerificationToken == token, cancellationToken).ConfigureAwait(false);
            if (user == null)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("AccNotFound", ErrorMessages.UserNotFound)));
            if (user.VerificationTokenExp <= DateTimeOffset.UtcNow)
                return ResultOrValue<string>.Failure(Result.Failure(new Error("ExpToken", ErrorMessages.ExpToken)));
            user.VerificationToken = string.Empty;
            user.VerifiedAt = DateTimeOffset.UtcNow;
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return ResultOrValue<string>.Success(token);
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



        #region Activate Account

        private async Task SendConfirmationLink(string email, string token, string userName)
        {
            try
            {
                string googleAppPassword = _googleAppSettings.GoogleAppPassword;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("empathywave.shop@gmail.com");
                mailMessage.To.Add(email);
                mailMessage.Subject = "Account Activation";
                string confirmationLink = $"https://localhost:7047/account-activation/?token={token}";
                string customHtml = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html dir=""ltr"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"" style=""font-family:arial, 'helvetica neue', helvetica, sans-serif"">
 <head>
  <meta charset=""UTF-8"">
  <meta content=""width=device-width, initial-scale=1"" name=""viewport"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
  <meta content=""telephone=no"" name=""format-detection"">
  <title>New Template</title><!--[if (mso 16)]>
    <style type=""text/css"">
    a {text-decoration: none;}
    </style>
    <![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>
<xml>
    <o:OfficeDocumentSettings>
    <o:AllowPNG></o:AllowPNG>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
</xml>
<![endif]--><!--[if !mso]><!-- -->
  <link href=""https://fonts.googleapis.com/css2?family=Barlow&display=swap"" rel=""stylesheet"">
  <link href=""https://fonts.googleapis.com/css2?family=Barlow+Condensed&display=swap"" rel=""stylesheet""><!--<![endif]-->
  <style type=""text/css"">
.rollover span {
	font-size:0;
}
.rollover:hover .rollover-first {
	max-height:0px!important;
	display:none!important;
}
.rollover:hover .rollover-second {
	max-height:none!important;
	display:block!important;
}
#outlook a {
	padding:0;
}
.es-button {
	mso-style-priority:100!important;
	text-decoration:none!important;
}
a[x-apple-data-detectors] {
	color:inherit!important;
	text-decoration:none!important;
	font-size:inherit!important;
	font-family:inherit!important;
	font-weight:inherit!important;
	line-height:inherit!important;
}
.es-desk-hidden {
	display:none;
	float:left;
	overflow:hidden;
	width:0;
	max-height:0;
	line-height:0;
	mso-hide:all;
}
@media only screen and (max-width:600px) {p, ul li, ol li, a { line-height:150%!important } h1, h2, h3, h1 a, h2 a, h3 a { line-height:120% } h1 { font-size:46px!important; text-align:left } h2 { font-size:28px!important; text-align:left } h3 { font-size:20px!important; text-align:center } .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:46px!important; text-align:left } .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:28px!important; text-align:left } .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important; text-align:center } .es-menu td a { font-size:12px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:14px!important } .es-content-body p, .es-content-body ul li, .es-content-body ol li, .es-content-body a { font-size:14px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:14px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class=""gmail-fix""] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:inline-block!important } a.es-button, button.es-button { font-size:18px!important; display:inline-block!important } .es-adaptive table, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0!important } .es-m-p0r { padding-right:0!important } .es-m-p0l { padding-left:0!important } .es-m-p0t { padding-top:0!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } tr.es-desk-hidden, td.es-desk-hidden, table.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } tr.es-desk-hidden { display:table-row!important } table.es-desk-hidden { display:table!important } td.es-desk-menu-hidden { display:table-cell!important } .es-menu td { width:1%!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; max-height:inherit!important } .es-m-p5 { padding:5px!important } .es-m-p5t { padding-top:5px!important } .es-m-p5b { padding-bottom:5px!important } .es-m-p5r { padding-right:5px!important } .es-m-p5l { padding-left:5px!important } .es-m-p10 { padding:10px!important } .es-m-p10t { padding-top:10px!important } .es-m-p10b { padding-bottom:10px!important } .es-m-p10r { padding-right:10px!important } .es-m-p10l { padding-left:10px!important } .es-m-p15 { padding:15px!important } .es-m-p15t { padding-top:15px!important } .es-m-p15b { padding-bottom:15px!important } .es-m-p15r { padding-right:15px!important } .es-m-p15l { padding-left:15px!important } .es-m-p20 { padding:20px!important } .es-m-p20t { padding-top:20px!important } .es-m-p20r { padding-right:20px!important } .es-m-p20l { padding-left:20px!important } .es-m-p25 { padding:25px!important } .es-m-p25t { padding-top:25px!important } .es-m-p25b { padding-bottom:25px!important } .es-m-p25r { padding-right:25px!important } .es-m-p25l { padding-left:25px!important } .es-m-p30 { padding:30px!important } .es-m-p30t { padding-top:30px!important } .es-m-p30b { padding-bottom:30px!important } .es-m-p30r { padding-right:30px!important } .es-m-p30l { padding-left:30px!important } .es-m-p35 { padding:35px!important } .es-m-p35t { padding-top:35px!important } .es-m-p35b { padding-bottom:35px!important } .es-m-p35r { padding-right:35px!important } .es-m-p35l { padding-left:35px!important } .es-m-p40 { padding:40px!important } .es-m-p40t { padding-top:40px!important } .es-m-p40b { padding-bottom:40px!important } .es-m-p40r { padding-right:40px!important } .es-m-p40l { padding-left:40px!important } }
@media screen and (max-width:384px) {.mail-message-content { width:414px!important } }
</style>
 </head>
 <body style=""width:100%;font-family:arial, 'helvetica neue', helvetica, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0"">
  <div dir=""ltr"" class=""es-wrapper-color"" lang=""en"" style=""background-color:#102B3F""><!--[if gte mso 9]>
			<v:background xmlns:v=""urn:schemas-microsoft-com:vml"" fill=""t"">
				<v:fill type=""tile"" color=""#102b3f""></v:fill>
			</v:background>
		<![endif]-->
   <table class=""es-wrapper"" width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#102B3F"">
     <tr>
      <td valign=""top"" style=""padding:0;Margin:0"">
       <table cellpadding=""0"" cellspacing=""0"" class=""es-header"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table bgcolor=""#ffffff"" class=""es-header-body"" align=""center"" cellpadding=""0"" cellspacing=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#102B3F;width:600px"">
             <tr>
              <td class=""es-m-p20r es-m-p20l"" align=""left"" style=""padding:0;Margin:0;padding-top:20px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td class=""es-m-p0r"" valign=""top"" align=""center"" style=""padding:0;Margin:0;width:600px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""center"" class=""es-m-txt-c"" style=""padding:0;Margin:0;font-size:0px""><a target=""_blank"" href=""https://viewstripo.email"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#E2CFEA;font-size:14px""><img src=""https://fhcdptr.stripocdn.email/content/guids/CABINET_63f247b0d29e062274061c80fa3a1ed0d3f6b9a0644e71dca2ecde5f937a64d5/images/mosquitoo.png"" alt=""Logo"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic"" title=""Logo"" height=""120""></a></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-content"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0;background-image:url(https://fhcdptr.stripocdn.email/content/guids/CABINET_6306d45fd9ea3b681ebe3a603101f0275312c7c136d6957f7ed43fa4b22490f7/images/frame_375_tsP.png);background-repeat:no-repeat;background-position:center top"" background=""https://fhcdptr.stripocdn.email/content/guids/CABINET_6306d45fd9ea3b681ebe3a603101f0275312c7c136d6957f7ed43fa4b22490f7/images/frame_375_tsP.png"">
           <table class=""es-content-body"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"" role=""none"">
             <tr>
              <td class=""es-m-p10b es-m-p20r es-m-p20l"" align=""left"" style=""padding:0;Margin:0;padding-top:20px"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td class=""es-m-p0r es-m-p20b"" valign=""top"" align=""center"" style=""padding:0;Margin:0;width:600px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""center"" class=""es-m-txt-c"" style=""padding:0;Margin:0""><h1 style=""Margin:0;line-height:55px;mso-line-height-rule:exactly;font-family:'Barlow Condensed', Arial, sans-serif;font-size:46px;font-style:normal;font-weight:normal;color:#E2CFEA"">Welcome to <b>Wise Buzzer</b></h1></td>
                     </tr>
                     <tr>
                      <td align=""center"" class=""es-m-txt-c es-m-p10r es-m-p10l"" style=""Margin:0;padding-bottom:10px;padding-top:15px;padding-left:40px;padding-right:40px""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Barlow, sans-serif;line-height:24px;color:#E2CFEA;font-size:16px"">«Unlocking endless possibilities in gaming»</p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
             <tr>
              <td class=""es-m-p20r es-m-p20l"" align=""left"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:600px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:10px"" role=""presentation"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-top:15px;padding-bottom:15px;font-size:0"">
                       <table border=""0"" width=""100%"" height=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;border-bottom:1px solid #ffffff;background:unset;height:1px;width:100%;margin:0px""></td>
                         </tr>
                       </table></td>
                     </tr>
                     <tr>
                      <td align=""left"" style=""padding:0;Margin:0;padding-top:20px;padding-bottom:20px""><h2 style=""Margin:0;line-height:34px;mso-line-height-rule:exactly;font-family:'Barlow Condensed', Arial, sans-serif;font-size:28px;font-style:normal;font-weight:normal;color:#ffffff"">Hi  " + userName + @"  </h2><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Barlow, sans-serif;line-height:24px;color:#E2CFEA;font-size:16px""><br></p><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Barlow, sans-serif;line-height:24px;color:#ffffff;font-size:16px"">Activate your account now to unlock a world of thrilling games and exciting challenges. Explore inspiring templates, and solve gaming challenges like never before. Get ready to immerse yourself in endless fun!</p><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:Barlow, sans-serif;line-height:24px;color:#ffffff;font-size:16px""><br></p></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-bottom:20px""><!--[if mso]><a href=""https://yle"" target=""_blank"" hidden>
	<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" esdevVmlButton href=""https://yle"" 
                style=""height:41px; v-text-anchor:middle; width:192px"" arcsize=""0%"" stroke=""f""  fillcolor=""#a06cd5"">
		<w:anchorlock></w:anchorlock>
		<center style='color:#ffffff; font-family:Barlow, sans-serif; font-size:15px; font-weight:400; line-height:15px;  mso-text-raise:1px'>Activate Account</center>
	</v:roundrect></a>
<![endif]--><!--[if !mso]><!-- --><span class=""msohide es-button-border"" style=""border-style:solid;border-color:#2CB543;background:#A06CD5;border-width:0px;display:inline-block;border-radius:0px;width:auto;mso-hide:all""><a href=" + confirmationLink + @" "" class=""es-button"" target=""_blank"" style=""mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;color:#FFFFFF;font-size:18px;padding:10px 20px 10px 20px;display:inline-block;background:#A06CD5;border-radius:0px;font-family:Barlow, sans-serif;font-weight:normal;font-style:normal;line-height:22px;width:auto;text-align:center;mso-padding-alt:0;mso-border-alt:10px solid #A06CD5"">Activate Account</a></span><!--<![endif]--></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
             <tr>
              <td align=""left"" style=""padding:0;Margin:0;padding-left:20px;padding-right:20px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""center"" height=""40"" style=""padding:0;Margin:0""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table cellpadding=""0"" cellspacing=""0"" class=""es-content"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table bgcolor=""#102b3f"" class=""es-content-body"" align=""center"" cellpadding=""0"" cellspacing=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#102B3F;width:600px"">
             <tr>
              <td class=""es-m-p20r es-m-p20l"" align=""left"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:600px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:10px"" role=""presentation"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-top:15px;padding-bottom:15px;font-size:0"">
                       <table border=""0"" width=""100%"" height=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;border-bottom:1px solid #ffffff;background:unset;height:1px;width:100%;margin:0px""></td>
                         </tr>
                       </table></td>
                     </tr>
                     <tr>
                      <td align=""center"" class=""es-m-txt-c"" style=""padding:0;Margin:0;padding-top:20px;padding-bottom:20px""><h2 style=""Margin:0;line-height:34px;mso-line-height-rule:exactly;font-family:'Barlow Condensed', Arial, sans-serif;font-size:28px;font-style:normal;font-weight:normal;color:#ffffff"">Follow us!</h2></td>
                     </tr>
                     <tr>
                      <td align=""center"" class=""es-m-txt-c"" style=""padding:0;Margin:0;padding-top:10px;padding-bottom:20px;font-size:0"">
                       <table cellpadding=""0"" cellspacing=""0"" class=""es-table-not-adapt es-social"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td align=""center"" valign=""top"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://www.facebook.com/profile.php?id=61557941683615"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#E2CFEA;font-size:16px""><img title=""Facebook"" src=""https://fhcdptr.stripocdn.email/content/assets/img/social-icons/logo-colored/facebook-logo-colored.png"" alt=""Fb"" height=""24"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td align=""center"" valign=""top"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://www.instagram.com/wisebuzzer/"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#E2CFEA;font-size:16px""><img title=""Instagram"" src=""https://fhcdptr.stripocdn.email/content/assets/img/social-icons/logo-colored/instagram-logo-colored.png"" alt=""Inst"" height=""24"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td align=""center"" valign=""top"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://twitter.com/wisebuzzer24"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#E2CFEA;font-size:16px""><img title=""X"" src=""https://fhcdptr.stripocdn.email/content/assets/img/social-icons/logo-colored/x-logo-colored.png"" alt=""In"" height=""24"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td align=""center"" valign=""top"" style=""padding:0;Margin:0""><a target=""_blank"" href=""https://www.tiktok.com/@wisebuzzer"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#E2CFEA;font-size:16px""><img title=""Youtube"" src=""https://fhcdptr.stripocdn.email/content/assets/img/social-icons/logo-colored/tiktok-logo-colored.png"" alt=""Yt"" height=""24"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                         </tr>
                       </table></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table></td>
     </tr>
   </table>
  </div>
 </body>
</html>";

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(customHtml, null, "text/html");
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("empathywave.shop@gmail.com", googleAppPassword);
                mailMessage.AlternateViews.Add(htmlView);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send notification to {email}: {ex.Message}");
            }
        }
        #endregion

        #region Get Recovery 
        private async Task SendRecoveryEmail(string userEmail, string subject, string token)
        {
            try
            {
                string googleAppPassword = _googleAppSettings.GoogleAppPassword;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("empathywave.shop@gmail.com");
                mailMessage.To.Add(userEmail);
                mailMessage.Subject = subject;
                string recoveryLink = $"https://localhost:7047/recovery-activation/?token={token}";
                string customHtml = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html dir=""ltr"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"" style=""padding:0;Margin:0"">
 <head>
  <meta charset=""UTF-8"">
  <meta content=""width=device-width, initial-scale=1"" name=""viewport"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
  <meta content=""telephone=no"" name=""format-detection"">
  <title>New Template 3</title>
  <style type=""text/css"">
#outlook a {
	padding:0;
}
.ExternalClass {
	width:100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
	line-height:100%;
}
.es-button {
	mso-style-priority:100!important;
	text-decoration:none!important;
}
a[x-apple-data-detectors] {
	color:inherit!important;
	text-decoration:none!important;
	font-size:inherit!important;
	font-family:inherit!important;
	font-weight:inherit!important;
	line-height:inherit!important;
}
.es-desk-hidden {
	display:none;
	float:left;
	overflow:hidden;
	width:0;
	max-height:0;
	line-height:0;
	mso-hide:all;
}
.es-button-border:hover a.es-button, .es-button-border:hover button.es-button {
	background:#ffffff!important;
}
.es-button-border:hover {
	background:#ffffff!important;
	border-style:solid solid solid solid!important;
	border-color:#3d5ca3 #3d5ca3 #3d5ca3 #3d5ca3!important;
}
@media only screen and (max-width:600px) {p, ul li, ol li, a { line-height:150%!important } h1, h2, h3, h1 a, h2 a, h3 a { line-height:120%!important } h1 { font-size:20px!important; text-align:center } h2 { font-size:16px!important; text-align:left } h3 { font-size:20px!important; text-align:center } .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:20px!important } h2 a { text-align:left } .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:16px!important } .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important } .es-menu td a { font-size:14px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:10px!important } .es-content-body p, .es-content-body ul li, .es-content-body ol li, .es-content-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:12px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class=""gmail-fix""] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button, button.es-button { font-size:14px!important; display:block!important; border-left-width:0px!important; border-right-width:0px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } tr.es-desk-hidden, td.es-desk-hidden, table.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } tr.es-desk-hidden { display:table-row!important } table.es-desk-hidden { display:table!important } td.es-desk-menu-hidden { display:table-cell!important } .es-menu td { width:1%!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; max-height:inherit!important } }
@media screen and (max-width:384px) {.mail-message-content { width:414px!important } }
</style>
 </head>
 <body style=""width:100%;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0"">
  <div dir=""ltr"" class=""es-wrapper-color"" lang=""en"" style=""background-color:#FAFAFA""><!--[if gte mso 9]>
			<v:background xmlns:v=""urn:schemas-microsoft-com:vml"" fill=""t"">
				<v:fill type=""tile"" color=""#fafafa""></v:fill>
			</v:background>
		<![endif]-->
   <table class=""es-wrapper"" width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#FAFAFA"">
     <tr style=""border-collapse:collapse"">
      <td valign=""top"" style=""padding:0;Margin:0"">
       <table cellpadding=""0"" cellspacing=""0"" class=""es-content"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr style=""border-collapse:collapse"">
          <td class=""es-adaptive"" align=""center"" style=""padding:0;Margin:0"">
           <table class=""es-content-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#ffffff"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td align=""left"" style=""padding:10px;Margin:0"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:580px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" class=""es-infoblock"" style=""padding:0;Margin:0;line-height:24px;font-size:12px;color:#CCCCCC""><h1 style=""Margin:0;line-height:24px;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-size:20px;font-style:normal;font-weight:normal;color:#333333"">Wise Buzzer&nbsp;</h1></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table cellpadding=""0"" cellspacing=""0"" class=""es-header"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr style=""border-collapse:collapse"">
          <td class=""es-adaptive"" align=""center"" style=""padding:0;Margin:0"">
           <table class=""es-header-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#3d5ca3;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#3d5ca3"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td style=""Margin:0;padding-top:20px;padding-bottom:20px;padding-left:20px;padding-right:20px;background-color:#3d5ca3"" bgcolor=""#3d5ca3"" align=""left""><!--[if mso]><table style=""width:560px"" cellpadding=""0"" 
                        cellspacing=""0""><tr><td style=""width:270px"" valign=""top""><![endif]-->
               <table class=""es-left"" cellspacing=""0"" cellpadding=""0"" align=""left"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left"">
                 <tr style=""border-collapse:collapse"">
                  <td class=""es-m-p20b"" align=""left"" style=""padding:0;Margin:0;width:270px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td class=""es-m-p0l es-m-txt-c"" align=""left"" style=""padding:0;Margin:0;font-size:0px""><a href=""https://viewstripo.email"" target=""_blank"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#1376C8;font-size:14px""><img src=""https://ibb.co/yPJPVvd"" alt style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic"" width=""183""></a></td>
                     </tr>
                   </table></td>
                 </tr>
               </table><!--[if mso]></td><td style=""width:20px""></td><td style=""width:270px"" valign=""top""><![endif]-->
               <table class=""es-right"" cellspacing=""0"" cellpadding=""0"" align=""right"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:right"">
                 <tr style=""border-collapse:collapse"">
                  <td align=""left"" style=""padding:0;Margin:0;width:270px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;display:none""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table><!--[if mso]></td></tr></table><![endif]--></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-content"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr style=""border-collapse:collapse"">
          <td style=""padding:0;Margin:0;background-color:#fafafa"" bgcolor=""#fafafa"" align=""center"">
           <table class=""es-content-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#ffffff;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#ffffff"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td style=""padding:0;Margin:0;padding-left:20px;padding-right:20px;padding-top:40px;background-color:transparent;background-position:left top"" bgcolor=""transparent"" align=""left"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:560px"">
                   <table style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-position:left top"" width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;padding-top:5px;padding-bottom:5px;font-size:0""><img src=""https://fecyrch.stripocdn.email/content/guids/CABINET_dd354a98a803b60e2f0411e893c82f56/images/23891556799905703.png"" alt style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic"" width=""175""></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;padding-top:15px;padding-bottom:15px""><h1 style=""Margin:0;line-height:24px;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-size:20px;font-style:normal;font-weight:normal;color:#333333""><strong>FORGOT YOUR </strong></h1><h1 style=""Margin:0;line-height:24px;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-size:20px;font-style:normal;font-weight:normal;color:#333333""><strong>&nbsp;PASSWORD?</strong></h1></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
<td align=""center"" style=""padding:0;Margin:0;padding-left:40px;padding-right:40px""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:24px;color:#666666;font-size:16px""> HELLO " + userEmail + @" FROM Wise Buzzer!&nbsp;
 </p></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;padding-right:35px;padding-left:40px""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:24px;color:#666666;font-size:16px"">There was a request to reset your password!</p></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;padding-top:25px;padding-left:40px;padding-right:40px""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:24px;color:#666666;font-size:16px"">If did not make this request, just ignore this email or contact the administrator. Otherwise, please click the button below to change your password:</p></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""Margin:0;padding-left:10px;padding-right:10px;padding-top:40px;padding-bottom:40px""><span class=""es-button-border"" style=""border-style:solid;border-color:#3D5CA3;background:#FFFFFF;border-width:2px;display:inline-block;border-radius:10px;width:auto""><a href=" + recoveryLink + @" class=""es-button"" target=""_blank"" style=""mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;color:#3D5CA3;font-size:14px;display:inline-block;background:#FFFFFF;border-radius:10px;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-weight:bold;font-style:normal;line-height:17px;width:auto;text-align:center;padding:15px 20px 15px 20px;mso-padding-alt:0;mso-border-alt:10px solid #FFFFFF"">RESET PASSWORD</a></span></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
             <tr style=""border-collapse:collapse"">
              <td style=""Margin:0;padding-top:5px;padding-bottom:20px;padding-left:20px;padding-right:20px;background-position:left top"" align=""left"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:560px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:21px;color:#666666;font-size:14px"">Contact us: <a target=""_blank"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#666666;font-size:14px"" href=""tel:123456789"">+</a>995 574 83 62 59&nbsp;</p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-content"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr style=""border-collapse:collapse"">
          <td style=""padding:0;Margin:0;background-color:#fafafa"" bgcolor=""#fafafa"" align=""center"">
           <table class=""es-content-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""transparent"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td style=""padding:0;Margin:0;padding-top:15px;background-position:left top"" align=""left"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:600px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;display:none""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-footer"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr style=""border-collapse:collapse"">
          <td style=""padding:0;Margin:0;background-color:#fafafa"" bgcolor=""#fafafa"" align=""center"">
           <table class=""es-footer-body"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#ffffff"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"">
             <tr style=""border-collapse:collapse"">
              <td style=""Margin:0;padding-top:10px;padding-left:20px;padding-right:20px;padding-bottom:30px;background-color:#0b5394"" bgcolor=""#0b5394"" align=""left"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:560px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""left"" style=""padding:0;Margin:0;padding-top:5px;padding-bottom:5px""><h2 style=""Margin:0;line-height:19px;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-size:16px;font-style:normal;font-weight:normal;color:#ffffff""><strong>Have you got any questions?</strong></h2></td>
                     </tr>
                     <tr style=""border-collapse:collapse"">
                      <td align=""left"" style=""padding:0;Margin:0;padding-bottom:5px""><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:21px;color:#ffffff;font-size:14px"">We are here to help,</p><p style=""Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;line-height:21px;color:#ffffff;font-size:14px""><a target=""_blank"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#ffffff;font-size:14px"" href="""">contact us anytime!</a></p></td>
                     </tr>
                   </table></td>
                 </tr>
                 <tr style=""border-collapse:collapse"">
                  <td align=""left"" style=""padding:0;Margin:0;width:560px"">
                   <table style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-position:center center"" width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                     <tr style=""border-collapse:collapse"">
                      <td class=""es-m-txt-c"" align=""left"" style=""padding:0;Margin:0;padding-bottom:5px;padding-top:10px;font-size:0"">
                       <table class=""es-table-not-adapt es-social"" cellspacing=""0"" cellpadding=""0"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr style=""border-collapse:collapse"">
                          <td valign=""top"" align=""center"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://www.facebook.com/profile.php?id=61557941683615"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px""><img src=""https://fecyrch.stripocdn.email/content/assets/img/social-icons/rounded-gray/facebook-rounded-gray.png"" alt=""Fb"" title=""Facebook"" width=""32"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td valign=""top"" align=""center"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://www.instagram.com/wisebuzzer/"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px""><img src=""https://fecyrch.stripocdn.email/content/assets/img/social-icons/rounded-gray/instagram-rounded-gray.png"" alt=""Ig"" title=""Instagram"" width=""32"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td valign=""top"" align=""center"" style=""padding:0;Margin:0;padding-right:10px"" ><a target=""_blank"" href=""https://www.tiktok.com/@wisebuzzer"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px""><img src=""https://fecyrch.stripocdn.email/content/assets/img/social-icons/rounded-gray/tiktok-rounded-gray.png"" alt=""Yt"" title=""TikTok"" width=""32"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td valign=""top"" align=""center"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://www.linkedin.com/company/wise-buzzer"" style=""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px""><img src=""https://fecyrch.stripocdn.email/content/assets/img/social-icons/rounded-gray/linkedin-rounded-gray.png"" alt=""linkedIn"" title=""LinkedIn"" width=""32"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                          <td valign=""top"" align=""center"" style=""padding:0;Margin:0;padding-right:10px""><a target=""_blank"" href=""https://twitter.com/wisebuzzer24"" style =""-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px""><img src=""https://fecyrch.stripocdn.email/content/assets/img/social-icons/rounded-gray/x-rounded-gray.png"" alt=""x"" title=""X"" width=""32"" style=""display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic""></a></td>
                         </tr>
                       </table></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-footer"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr style=""border-collapse:collapse"">
          <td style=""padding:0;Margin:0;background-color:#fafafa"" bgcolor=""#fafafa"" align=""center"">
           <table class=""es-footer-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""transparent"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td align=""left"" style=""Margin:0;padding-bottom:5px;padding-top:15px;padding-left:20px;padding-right:20px"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:560px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;display:none""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
       <table class=""es-content"" cellspacing=""0"" cellpadding=""0"" align=""center"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%"">
         <tr style=""border-collapse:collapse"">
          <td align=""center"" style=""padding:0;Margin:0"">
           <table class=""es-content-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#ffffff"" align=""center"" role=""none"">
             <tr style=""border-collapse:collapse"">
              <td align=""left"" style=""Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px"">
               <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr style=""border-collapse:collapse"">
                  <td valign=""top"" align=""center"" style=""padding:0;Margin:0;width:560px"">
                   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr style=""border-collapse:collapse"">
                      <td align=""center"" style=""padding:0;Margin:0;display:none""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table></td>
     </tr>
   </table>
  </div>
 </body>
</html>";

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(customHtml, null, "text/html");
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("empathywave.shop@gmail.com", googleAppPassword);

                mailMessage.AlternateViews.Add(htmlView);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send notification to {userEmail}: {ex.Message}");
            }
        }
        #endregion

    }
}
