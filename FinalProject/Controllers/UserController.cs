﻿using FinalProject.DAL;
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
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            var userNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (userExists != null) return BadRequest("User Exists!");
            if (userNameExists != null) return BadRequest("User Name Exists!");

            ApiUser newUser = new ApiUser()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.UserName,
                IsActive = true
            };
            IdentityResult identityResult = await _userManager.CreateAsync(newUser, dto.Password);
            await _userManager.AddToRoleAsync(newUser, "Member");

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
        public async Task<IActionResult> Confirm(string email)
        {
            ApiUser user = await _userManager.FindByEmailAsync(email);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return Ok("Confirmed");
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
            return Ok("User logged out");
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
            return Ok(user);
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateDTO dto)
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
            user.RelationshipStatus = dto.RelationshipStatus;
            user.Occupation = dto.Occupation;
            user.Education = dto.Education;
            user.Status = dto.Status;
            user.Country = dto.Country;
            if (dto.ImageFile != null && Extensions.IsImage(dto.ImageFile) && Extensions.IsvalidSize(dto.ImageFile, 500))
            {
                user.ImageUrl = await Extensions.Upload(dto.ImageFile, @"images");
            }
            if (dto.CoverPicFile != null && Extensions.IsImage(dto.CoverPicFile) && Extensions.IsvalidSize(dto.CoverPicFile, 500))
            {
                user.CoverPicUrl = await Extensions.Upload(dto.CoverPicFile, @"images");
            }
            await _userManager.UpdateAsync(user);
            return Ok(user);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            var EmailToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(nameof(ResetToken), "User", new { email = user.Email, EmailToken }, Request.Scheme);
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("socialnetworkproj1@gmail.com", "ftdmmjgojxqtyapp");
            client.EnableSsl = true;

            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", user.Email, link, "Reset Password", "Reset Password");

            client.Send(message);
            message.Dispose();

            return Ok(EmailToken);
        }
        [HttpGet("ResetToken")]
        public async Task<IActionResult> ResetToken(string email, string emailToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            ResetPasswordDTO dto = new ResetPasswordDTO()
            {
                Email = user.Email,
                Token = emailToken,
            };
            return Ok(dto);
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
        public IActionResult GetAllUsers([FromBody] int? skip, int? take)
        {
            int currentSkip = skip ?? 1;
            int currentTake = take ?? 5;
            List <ApiUser> users = _userManager.Users.ToList();
            return Ok(users.Skip(currentSkip).Take(currentTake));
        }
        [HttpGet("seacrhUser")]
        public IActionResult Search([FromBody]string query)
        {
            if (query == null) return BadRequest("NotFound");
            List<ApiUser> users = _userManager.Users.ToList();
            var searchedUser = users.Where(x => x.UserName.Contains(query) || x.FullName.Contains(query));
            if (searchedUser.Count() == 0) return NotFound("User not found");
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


        //[HttpPost("roles")]
        //public async Task<IActionResult> InitRoles()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));
        //    return Ok("okay");
        //}
    }
}
