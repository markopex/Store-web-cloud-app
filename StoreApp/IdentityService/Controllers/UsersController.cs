using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IdentityService.Interfaces;
using IdentityService.Dto;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _config = configuration;
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetUser()
        {
            var userEmail = GetUserEmail();
            try
            {
                return Ok(_userService.GetUserByEmail(userEmail));
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }
        [HttpPost("login")]
        public ActionResult Login(LoginDto dto)
        {
            var retVal = _userService.LoginUser(dto);
            if (retVal == null) return BadRequest(new { message = "Wrong password/email" });
            return Ok(retVal);
        }

        [HttpPost]
        public ActionResult AddUser([FromBody] RegisterDto dto)
        {
            try
            {
                
                _userService.AddUser(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }

            return Ok();
        }
        [HttpPut]
        [Authorize]
        public ActionResult UpdateUser([FromBody] UpdateUserDto dto)
        {
            var userEmail = GetUserEmail();
            try
            {
                _userService.UpdateUser(userEmail, dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }

            return Ok();
        }

        [HttpPost("image")]
        [Authorize]
        public async Task<IActionResult> OnPostUploadAsync()
        {
            var email = GetUserEmail();

            var files = Request.Form.Files;

            if (files.Count != 1)
                return BadRequest();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Combine(_config["StoredFilesPath"], email);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            return Ok();
        }
        [HttpGet("image/me")]
        [Authorize]
        public async Task<IActionResult> DownloadImage(string filename)
        {
            var email = GetUserEmail();

            var path = Path.Combine(_config["StoredFilesPath"], email);
            MemoryStream memory = new MemoryStream();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "image/png", Path.GetFileName(path));
        }

        [NonAction]
        private string GetUserEmail()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            return claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
