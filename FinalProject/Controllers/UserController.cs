using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
using Google.Apis.Auth.OAuth2;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtHandler _jwtHandler;

        public UserController(ApiDbContext db,
            UserManager<ApiUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            SignInManager<ApiUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterDTO dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            var userNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (userExists != null) return BadRequest("User Exists!");
            if (userNameExists != null) return BadRequest("User Name Exists!");
            DateTime today = DateTime.Today;
            int age = today.Year - dto.BirthDate.Year;
            if (age <= 12) return BadRequest("User cannot be younger than 12 years old!");
            ApiUser newUser = new ApiUser()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.UserName,
                IsActive = true
            };
            IdentityResult identityResult = await _userManager.CreateAsync(newUser, dto.Password);
            await _db.SaveChangesAsync();

            if (!identityResult.Succeeded)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (IdentityError error in identityResult.Errors)
                {
                    stringBuilder.Append(error.Description);
                    stringBuilder.Append("\r\n");
                }
                return BadRequest("User could not be created" + stringBuilder);
            }

            await _userManager.AddToRoleAsync(newUser, "Member");
            await ConfirmEmail(newUser.Email);

            return Ok("User was registered successfully!");
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmEmail(string email)
        {
            var newUser = await _userManager.FindByEmailAsync(email);
            var EmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var link = Url.Action(nameof(Confirm), "User", new { email = newUser.Email, EmailToken }, Request.Scheme);
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("socialnetworkproj1@gmail.com", "ftdmmjgojxqtyapp");
            client.EnableSsl = true;

            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", newUser.Email, link, "Confirm Email", "Confirm");
            client.Send(message);
            message.Dispose();
            return Ok();
        }
        [HttpGet("confirmed")]
        public async Task<IActionResult> Confirm(string email, string EmailToken)
        {
            ApiUser user = await _userManager.FindByEmailAsync(email);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            Response.Cookies.Append("jwt", EmailToken.ToString(), new CookieOptions
            {
                HttpOnly = true
            });
            return Redirect("http://localhost:3000/login");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn(LoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password) && await _signInManager.CanSignInAsync(user))
            {
                if (user.EmailConfirmed == false) Unauthorized("user email is not confirmed");

                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                Response.Cookies.Append("jwt", token.ToString(), new CookieOptions
                {
                    HttpOnly = true
                });

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult LogOut()
        {
            Response.Cookies.Delete("jwt");
            return Redirect("http://localhost:3000/login");
        }

        [Authorize]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (!result.Succeeded)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (IdentityError error in result.Errors)
                    {
                        stringBuilder.Append(error.Description);
                        stringBuilder.Append("\r\n");
                    }
                    return BadRequest("User could not be created" + stringBuilder);
                }
                return Ok("Password changed successfully!");
            }
            return BadRequest("user not found");
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            user.ImageUrl = @"Resources\Images\" + user.ImageUrl;
            if (user.CoverPicUrl != null && !user.CoverPicUrl.Contains(@"Resources\Images\"))
            {
                user.CoverPicUrl = @"Resources\Images\" + user.CoverPicUrl;
            }
            return Ok(user);
        }
        [Authorize]
        [HttpGet("userById")]
        public async Task<IActionResult> GetUserById([FromQuery] string userId)
        {
            if (userId == null) return BadRequest("Id is null");
            var user = await _userManager.FindByIdAsync(userId);
            user.ImageUrl = @"Resources\Images\" + user.ImageUrl;
            if (user.CoverPicUrl != null && !user.CoverPicUrl.Contains(@"Resources\Images\"))
            {
                user.CoverPicUrl = @"Resources\Images\" + user.CoverPicUrl;
            }
            return Ok(user);
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userManager.FindByEmailAsync(this.User.FindFirstValue(ClaimTypes.Email));
            if (dto.SocialMediaLinks != null)
            {
                foreach (string item in dto.SocialMediaLinks)
                {
                    SocialMediaLink newLink = new SocialMediaLink()
                    {
                        Link = item,
                        UserId = user.Id
                    };
                    _db.SocialMediaLinks.Add(newLink);
                }
                _db.SaveChanges();
            }
            user.FullName = dto.FullName;
            user.RelationshipStatus = dto.RelationshipStatus;
            user.Occupation = dto.Occupation;
            user.Education = dto.Education;
            user.Status = dto.Status;
            user.Country = dto.Country;
            user.PhoneNumber = dto.PhoneNumber;
            
            await _userManager.UpdateAsync(user);
            return Ok(user);
        }
        [HttpPost("profilePic")]
        public async Task<IActionResult> UpdateProfilePic([FromForm] UpdateUserProfilePicDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userManager.FindByEmailAsync(this.User.FindFirstValue(ClaimTypes.Email));
            if (dto.ImageFile != null && Files.IsImage(dto.ImageFile) && Files.IsvalidSize(dto.ImageFile, 500))
            {
                if(user.ImageUrl != null) Files.Delete(@"Resources", @"Images", user.ImageUrl);
                user.ImageUrl = Files.Upload(dto.ImageFile, "Images");
            }
            await _userManager.UpdateAsync(user);
            return Ok("Profile picture updated!");
        }
        [HttpPost("coverPic")]
        public async Task<IActionResult> UpdateCoverPic([FromForm] UpdateUserCoverPicDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userManager.FindByEmailAsync(this.User.FindFirstValue(ClaimTypes.Email));
            if (dto.CoverPicFile != null && Files.IsImage(dto.CoverPicFile) && Files.IsvalidSize(dto.CoverPicFile, 500))
            {
                if (user.CoverPicUrl != null) Files.Delete(@"Resources", @"Images", user.CoverPicUrl);
                user.CoverPicUrl = Files.Upload(dto.CoverPicFile, "Images");
            }
            await _userManager.UpdateAsync(user);
            return Ok("Cover picture updated!");
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (email == null) return BadRequest("Email cannot be empty!");
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            var EmailToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = "http://localhost:3000/reset?token=" + EmailToken+"&email="+user.Email;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("nargizramazanova28@gmail.com", "");
            client.EnableSsl = true;

            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", user.Email, link, "Reset Password", "Reset Password");

            client.Send(message);
            message.Dispose();

            return Ok(EmailToken);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            if (string.Compare(dto.NewPassword, dto.PasswordConfirm) != 0)
            {
                return BadRequest("passwords not matching");
            }
            if (string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest("not authenticated");
            }
            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (IdentityError error in result.Errors)
                {
                    stringBuilder.Append(error.Description);
                    stringBuilder.Append("\r\n");
                }
                return BadRequest("User could not be created" + stringBuilder);
            }
            return Ok(dto);
        }
        [HttpGet("users")]
        public IActionResult GetAllUsers([FromQuery]PaginationDTO dto)
        {
            int currentSkip = dto.Skip ?? 0;
            int currentTake = dto.Take ?? 5;
            List <ApiUser> users = _userManager.Users.ToList();
            foreach (ApiUser user in users)
            {
                user.ImageUrl = (@"Resources\Images\" + user.ImageUrl);
            }
            return Ok(users.Skip(currentSkip).Take(currentTake));
        }
        [HttpGet("seacrhUser")]
        public IActionResult Search([FromQuery]string query)
        {
            if (query == null) return BadRequest("NotFound");
            List<ApiUser> users = _userManager.Users.ToList();
            var searchedUser = users.Where(x => x.UserName.Contains(query) || x.FullName.Contains(query));
            if (searchedUser.Count() == 0) return NotFound("User not found");
            foreach (ApiUser user in users)
            {
                user.ImageUrl = (@"Resources\Images\" + user.ImageUrl);
            }
            return Ok(searchedUser);
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        [HttpPost("disableUnableUser")]
        [Authorize(Policy ="Admin")]
        public async Task<IActionResult> DisableUnableUser([FromBody] string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound("user not found");
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return Ok(user);
        }

        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
        {
            var payload = await _jwtHandler.VerifyGoogleToken(externalAuth);
            if (payload == null)
                return BadRequest("Invalid External Authentication.");
            var info = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new ApiUser { Email = payload.Email, UserName = payload.Email };
                    await _userManager.CreateAsync(user);
                    //prepare and send an email for the email confirmation
                    await _userManager.AddToRoleAsync(user, "Member");
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }
            }
            if (user == null)
                return BadRequest("Invalid External Authentication.");
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var token = GetToken(authClaims);
            return Ok(new { Token = token, IsAuthSuccessful = true });
        }


        //[HttpPost("roles")]
        //public async Task<IActionResult> InitRoles()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));
        //    return Ok("okay");
        //}
    }
}
