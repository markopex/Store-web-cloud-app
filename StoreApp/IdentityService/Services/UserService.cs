using AutoMapper;
using Azure;
using Azure.Data.Tables;
using IdentityService.Dto;
using IdentityService.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly TableClient _tableClient;

        private readonly IConfigurationSection _secretKey;
        private readonly IConfigurationSection _imagePath;

        public UserService(IConfiguration config, IMapper mapper)
        {
            _secretKey = config.GetSection("SecretKey");
            _imagePath = config.GetSection("StoredFilesPath");
            _mapper = mapper;
            string connectionString = "UseDevelopmentStorage=true;"; // For Azure Storage Emulator
            string tableName = "Users";
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();

        }
        public UserDto AddUser(RegisterDto registerDto)
        {
            // Query for the user by username (as PartitionKey) or email (as RowKey)
            string filterByUsername = TableClient.CreateQueryFilter($"PartitionKey eq {registerDto.Username.ToLower()}");
            string filterByEmail = TableClient.CreateQueryFilter($"RowKey eq {registerDto.Email}");
            Pageable<TableEntity> users = _tableClient.Query<TableEntity>(filter: $"{filterByUsername} or {filterByEmail}");

            if (users.Any())
            {
                throw new Exception("User with same username-email already exists.");
            }

            // Create a new user entity
            var userEntity = new TableEntity(registerDto.Username.ToLower(), registerDto.Email)
            {
                { "Password", BCrypt.Net.BCrypt.HashPassword(registerDto.Password) },
                { "FirstName", registerDto.FirstName },
                { "LastName", registerDto.LastName },
                { "Birthday", registerDto.Birthday.ToString() }, // Storing as string for simplicity
                { "Address", registerDto.Address },
            };

            _tableClient.AddEntity(userEntity);

            // For simplicity, mapping directly back to UserDto from registerDto. Adjust mapping as necessary.
            var retVal = _mapper.Map<UserDto>(userEntity);

            return retVal;
        }


        public UserDto GetUserByEmail(string email)
        {
            // Assuming email is used as RowKey
            var userEntity = _tableClient.Query<TableEntity>(filter: $"RowKey eq '{email}'").FirstOrDefault();

            if (userEntity == null)
            {
                return null;
            }

            // Use AutoMapper to map the TableEntity to UserDto
            var userDto = _mapper.Map<UserDto>(userEntity);

            return userDto;
        }

        public UserDto GetUserByUsername(string username)
        {
            // Assuming username is used as PartitionKey
            var userEntity = _tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{username.ToLower()}'").FirstOrDefault();

            if (userEntity == null)
            {
                return null;
            }

            // Use AutoMapper to map the TableEntity to UserDto
            return _mapper.Map<UserDto>(userEntity);
        }


        public SuccessLoginDto LoginUser(LoginDto loginDto)
        {
            // Query for the user by username. Assuming username is used as the PartitionKey.
            Pageable<TableEntity> users = _tableClient.Query<TableEntity>(
                filter: $"PartitionKey eq '{loginDto.Username.ToLower()}'");

            var userEntity = users.FirstOrDefault();
            if (userEntity == null)
            {
                return null; // User not found
            }

            // Assuming password is stored in the "Password" column
            var hashedPassword = userEntity["Password"].ToString();
            if (BCrypt.Net.BCrypt.Verify(loginDto.Password, hashedPassword))
            {
                // Map the TableEntity to your User or UserDto object. Simplified here for brevity.
                var user = _mapper.Map<UserDto>(userEntity);

                return new SuccessLoginDto()
                {
                    Token = GenerateToken(user)
                };
            }
            else
            {
                return null; // Password verification failed
            }
        }


        private string GenerateToken(UserDto user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            //Kreiramo kredencijale za potpisivanje tokena. Token mora biti potpisan privatnim kljucem
            //kako bi se sprecile njegove neovlascene izmene
            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey.Value));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: "http://localhost:5001", //url servera koji je izdao token
                claims: claims, //claimovi
                expires: DateTime.Now.AddMinutes(60), //vazenje tokena u minutama
                signingCredentials: signinCredentials //kredencijali za potpis
            );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }

        public UserDto UpdateUser(string email, UpdateUserDto dto)
        {
            // Assuming email is used as RowKey
            var userEntity = _tableClient.Query<TableEntity>(filter: $"RowKey eq '{email}'").FirstOrDefault();
            if (userEntity == null)
            {
                throw new Exception("User not found");
            }

            // Update properties
            userEntity["FirstName"] = dto.FirstName;
            userEntity["LastName"] = dto.LastName;
            userEntity["Birthday"] = dto.Birthday.ToString();
            userEntity["Address"] = dto.Address;

            _tableClient.UpdateEntity(userEntity, ETag.All, TableUpdateMode.Replace);

            // Assuming UserDto and TableEntity structure alignment for AutoMapper
            return _mapper.Map<UserDto>(userEntity);
        }

    }
}
