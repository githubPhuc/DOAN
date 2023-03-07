
using DOAN.Data;
using DOAN.Models;

using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace DOAN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly DOANContext _context;


        public AuthenticateController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, DOANContext context)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;

        }

        [HttpGet]
        [Route("getAccount")]

        public async Task<IActionResult> GetAllAccount()
        {

            var result = (from a in _context.User
                          select new
                          {
                              Id = a.Id,

                              Username = a.UserName,
                              Email = a.Email,
                              
                              
                              AccoutType = a.AccoutType,
                              IsLocked = a.IsLooked,
                          }).ToList();
            var acc = await _context.User.ToListAsync();

            return Ok(new
            {
                acc = result,
                count = acc.Count()
            });

        }
        [HttpGet]
        [Route("filAccount")]
        public async Task<IActionResult> FilterAccount(int id)
        {

            var result = (from a in _context.User
                          where a.AccoutType == (id == 1 ? "Admin" : "User")
                          select new
                          {
                              Id = a.Id,

                              Username = a.UserName,
                              Email = a.Email,
                              
                              
                              AccoutType = a.AccoutType,
                              IsLocked = a.IsLooked,
                          }).ToList();
            var acc = await _context.User.ToListAsync();

            return Ok(new
            {
                acc = result,
                count = acc.Count()
            });

        }
        [HttpGet]
        [Route("searchAccount")]
        public async Task<IActionResult> searchAccount(string txt)
        {

            var result = (from a in _context.User
                          where a.UserName.Contains(txt) || a.Email.Contains(txt) || a.PhoneNumber.Contains(txt)
                          select new
                          {
                              Id = a.Id,
                              Username = a.UserName,
                              Email = a.Email,
                              
                              
                              AccoutType = a.AccoutType,
                              IsLocked = a.IsLooked,
                          }).ToList();
            var acc = await _context.User.ToListAsync();

            return Ok(new
            {
                acc = result,
                count = acc.Count()
            });

        }
        [HttpGet]
        [Route("getAccountById")]

        public async Task<IActionResult> GetAccount(string id)
        {

            var result = (from a in _context.User
                          where a.Id == id
                          select new
                          {
                              Id = a.Id,
                              Username = a.UserName,
                              Email = a.Email,
                              
                              
                              AccoutType = a.AccoutType,
                              IsLocked = a.IsLooked,
                              Password = a.PasswordHash,
                          }).FirstOrDefault();
            return Ok(result);
        }

        [HttpPost]
        [Route("lockAccount")]

        public async Task<IActionResult> LockAccount(string id)
        {

            var acc = await _context.User.FindAsync(id);

            if (acc != null)
            {
                if (acc.IsLooked == true)
                {
                    acc.IsLooked = false;
                    _context.User.Update(acc);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        status = 200,
                        msg = "Đã mở khoá tài khoản"
                    });
                }
                acc.IsLooked = true;
                _context.User.Update(acc);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    status = 200,
                    msg = "Đã khoá tài khoản"
                });
            }
            return BadRequest();
        }

        //[HttpPost]
        //[Route("editAcount")]

        //public async Task<IActionResult> EditAccount(EditAccountModel model)
        //{
        //    var user= await  userManager.FindByIdAsync(model.Id);
        //    if(user!=null)
        //    {


        //        if(model.Password!="")
        //        {
        //            var token = await userManager.GeneratePasswordResetTokenAsync(user);
        //            var result = await userManager.ResetPasswordAsync(user, token, model.Password);
        //            if (!result.Succeeded)
        //            {
        //                return Ok(new
        //                {
        //                    status = 500,
        //                    msg = "Cập nhật tài khoản thất bại"
        //                });
        //            }
        //        }

        //        user.Email=model.Email;
        //        user.PhoneNumber = model.Phone;
        //        user.ShippingAddress = model.Address;
        //        _context.Update(user);
        //        _context.SaveChanges();

        //        return Ok(new
        //        {
        //            status = 200,
        //            msg = "Đã cập nhật thông tin tài khoản"
        //        });

        //    }
        //    return Ok(new
        //    {
        //        status = 500,
        //        msg = "Cập nhật tài khoản thất bại"
        //    });


        //}

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password) && user.IsLooked == false)
            {
                //var userRoles = await userManager.GetRolesAsync(user);
                // var userRoles = await this.userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                //if (userRoles.Count > 0)
                //{
                //    foreach (var userRole in userRoles)
                //    {
                //        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                //    }

                //}
                //else
                //{
                //}
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,



                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    id = user.Id,
                    role = user.AccoutType,
                    username = user.UserName,

                });
            }
            return Ok(new
            {
                status = 400
            });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            var us = await _context.User.Where(u => u.Email == model.Email).ToListAsync();
            if (us.Count > 0)
            {
                return Ok(new
                {
                    status = 500,
                    msg = "Email already exits"
                });
            }
            User user = new User()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                AccoutType = "User",
               
                IsLooked = false

            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }
        //https://localhost:7249/api/Authenticate/register

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            var us = await _context.User.Where(u => u.Email == model.Email).ToListAsync();
            if (us.Count > 0)
            {
                return Ok(new
                {
                    status = 500,
                    msg = "Email already exits"
                });
            }
            User user = new User()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                AccoutType = "Admin",
                IsLooked = false
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }



        [HttpPost]
        [Route("TaoMaXacThuc")]
        public async Task<IActionResult> TaoMaXacThuc(string mail)
        {
            var email = await _context.User.Where(t => t.Email == mail).ToListAsync();
            if (email.Count > 0)
            {
                string UpperCase = "QWERTYUIOPASDFGHJKLZXCVBNM";
                string LowerCase = "qwertyuiopasdfghjklzxcvbnm";
                string Digits = "1234567890";
                string allCharacters = UpperCase + Digits;
                Random r = new Random();
                String password = "";
                //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(password);
                //var pass64= System.Convert.ToBase64String(plainTextBytes);
                for (int i = 0; i < 6; i++)
                {
                    double rand = r.NextDouble();
                    if (i == 0)
                    {
                        password += UpperCase.ToCharArray()[(int)Math.Floor(rand * UpperCase.Length)];
                    }
                    else
                    {
                        password += allCharacters.ToCharArray()[(int)Math.Floor(rand * allCharacters.Length)];
                    }
                }
                string content = "Đây là mã xác thực tài khoản 2P Shop của bạn ! <br>";
                string token = content + "<h1>" + password + "</h1>";
                string _from = "0306191061@caothang.edu.vn";
                string _subject = "Xác thực tài khoản game";
                string _body = token;
                string _gmail = "0306191061@caothang.edu.vn";
                string _password = "285728207";
                MailMessage message = new MailMessage(_from, mail, _subject, _body);
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.ReplyToList.Add(new MailAddress(_from));
                message.Sender = new MailAddress(_from);

                using var smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_gmail, _password);

                try
                {
                    await smtpClient.SendMailAsync(message);
                    //HttpContext.Session.SetString("OTP", _body);
                    //HttpContext.Session.SetString("Gmail", _to);
                    //HttpContext.Session.SetString("Sdt", sdt);
                    return Ok(new
                    {
                        status = 200,
                        msg = "Mã xác thực đã gửi đến mail ",
                        otp = password
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return Ok(new
                    {
                        status = 500,
                        msg = "Gửi thất bại, kiểm tra lại địa chỉ email"
                    });
                }

            }
            return Ok(new
            {
                status = 500,
                msg = "Email không đúng"
            });

        }

        [HttpPost]
        [Route("XacThuc")]

        public async Task<IActionResult> XacThuc(string otp)
        {
            if (HttpContext.Session.GetString("OTP") == otp)
            {
                return Ok(new
                {
                    status = 200,
                    msg = "Xác minh thành công"
                });
            }
            return Ok(new
            {
                status = 500,
                msg = "Xác mminh không thành công"
            });
        }

        [HttpPost]
        [Route("ChangePassword")]


        public async Task<IActionResult> ChangePassword(string mail, string newPass)
        {
            var user = await userManager.FindByEmailAsync(mail);

            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                var result = await userManager.ResetPasswordAsync(user, token, newPass);

                return Ok(new
                {
                    status = 200,
                    msg = "Đã cập nhật password"
                });
            }
            return BadRequest();
        }
    }
}
