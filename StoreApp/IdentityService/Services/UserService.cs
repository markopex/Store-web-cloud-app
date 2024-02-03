using AutoMapper;
using Common.Models.Identity;
using IdentityService.Dto;
using IdentityService.Infrastructure;
using IdentityService.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _dbContext;

        private readonly IConfigurationSection _secretKey;
        private readonly IConfigurationSection _imagePath;

        public UserService(IConfiguration config, IMapper mapper, IdentityDbContext dbContext)
        {
            _secretKey = config.GetSection("SecretKey");
            _imagePath = config.GetSection("StoredFilesPath");
            _mapper = mapper;
            _dbContext = dbContext;
        }
        public UserDto AddUser(RegisterDto registerDto)
        {
            User user = _dbContext.Users.ToList().Find(match: x =>
            {
                return x.Username.ToLower() == registerDto.Username.ToLower() || x.Email == registerDto.Email;
            });
            if (user != null) throw new Exception("User with same username-email already exists.");

            user = _mapper.Map<User>(registerDto);
            user.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            user.Username = user.Username;

            _dbContext.Add(user);
            _dbContext.SaveChanges();

            var retVal = _mapper.Map<UserDto>(_dbContext.Users.ToList().Find(x => x.Username.ToLower() == registerDto.Username.ToLower()));

            return retVal;
        }

        public UserDto GetUserByEmail(string email)
        {
            return _mapper.Map<UserDto>(_dbContext.Users.Find(email));
        }

        public UserDto GetUserByUsername(string username)
        {
            return _mapper.Map<UserDto>(_dbContext.Users.ToList().Find(i => i.Username.ToLower() == username.ToLower()));
        }

        public SuccessLoginDto LoginUser(LoginDto loginDto)
        {
            User user = _dbContext.Users.ToList().Find(x => x.Username == loginDto.Username);

            if (user == null)
                return null;

            if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))//Uporedjujemo hes pasvorda iz baze i unetog pasvorda
            {
                return new SuccessLoginDto()
                {
                    Token = GenerateToken(user)
                };
            }
            else
            {
                return null;
            }
        }

        private string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            //Kreiramo kredencijale za potpisivanje tokena. Token mora biti potpisan privatnim kljucem
            //kako bi se sprecile njegove neovlascene izmene
            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey.Value));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                //issuer: "http://localhost:5001", //url servera koji je izdao token
                claims: claims, //claimovi
                expires: DateTime.Now.AddMinutes(60), //vazenje tokena u minutama
                signingCredentials: signinCredentials //kredencijali za potpis
            );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }

        public UserDto UpdateUser(string email, UpdateUserDto dto)
        {
            var user = _dbContext.Users.Find(email);
            if (user == null)
                throw new Exception("User not found");
            _mapper.Map<UpdateUserDto, User>(dto, user);
            _dbContext.SaveChanges();
            return _mapper.Map<UserDto>(user);
        }
    }
}
