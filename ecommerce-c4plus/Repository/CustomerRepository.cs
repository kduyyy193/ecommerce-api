using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dapper;
using Databases;
using ecommerce_c4plus.IRepository;
using ecommerce_c4plus.Models;
using ecommerce_c4plus.Models.DTOs.Request;
using ecommerce_c4plus.Models.DTOs.Response;

namespace ecommerce_c4plus.Repository
{
    public class CustomerRepository: ICustomerRepository
	{

        private readonly DBContext _context;
        private readonly IConfiguration _configuration;

        public CustomerRepository(DBContext context, IConfiguration configuration)
		{
            _context = context;
            _configuration = configuration;
        }

        public async Task<APIResponse<TokenResponse>> Register(SignUpRequest userRegister)
           {
                try
                {
                    var existingUser = await GetUserByUsername(userRegister.Username);

                    if (existingUser != null)
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Username already exists."
                        };
                    }

                    var _user = new UserDTO
                    {
                        Username = userRegister.Username,
                        Email = userRegister.Email,
                        Phone = userRegister.Phone,
                    }; ;

                    var (hashedPassword, salt) = HashPassword(userRegister.Password);
                    _user.PasswordHash = hashedPassword;
                    _user.Salt = salt;

                    var newUser = await AddUser(_user);
                    if (newUser == null)
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Failed!",
                        };
                    }

                    var accessToken = GenerateAccessToken(newUser.UserId);
                    var refreshToken = GenerateRefreshToken();

                    var tokenInfo = new TokenInfoDTO
                    {
                        userId = newUser.UserId,
                        AccessToken = accessToken,
                        refreshToken = refreshToken,
                        accessTokenExpiration = DateTime.UtcNow.AddHours(24),
                        refreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                    };

                    var isTokenSaved = await SaveTokenInfo(tokenInfo);

                    if (isTokenSaved)
                    {
                        var result = new TokenResponse
                        {
                            accessToken = accessToken,
                            refreshToken = refreshToken
                        };

                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 200,
                            Message = "Success!",
                            Result = result
                        };
                    }

                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Failed!",
                    };
                }
                catch (Exception ex)
                {
                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 500,
                        Message = ex.Message
                    };
                }
            }

        public async Task<APIResponse<TokenResponse>> Login(LoginRequest userLogin)
        {
            try
            {
                if (string.IsNullOrEmpty(userLogin.Username) || string.IsNullOrEmpty(userLogin.Password))
                {
                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Username and password are required."
                    };
                }

                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "SELECT * FROM Users WHERE Username = @Username";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Username", userLogin.Username, DbType.String);

                    var user = await connection.QueryFirstOrDefaultAsync<UserDTO>(query, parameters);

                    if (user == null)
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }

                    if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.Salt))
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Password not set up properly."
                        };
                    }

                    if (!VerifyPassword(userLogin.Password, user.PasswordHash, user.Salt))
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Incorrect password."
                        };
                    }

                    string queryToken = "SELECT * FROM token_info WHERE userId = @UserId";
                    var token = await connection.QueryFirstOrDefaultAsync<TokenInfoDTO>(queryToken, new { UserId = user.UserId });

                    if (token != null)
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var oldToken = handler.ReadJwtToken(token.AccessToken);

                        if (oldToken.ValidTo >= DateTime.UtcNow)
                        {
                            var result = new TokenResponse
                            {
                                accessToken = token.AccessToken,
                                refreshToken = token.refreshToken
                            };

                            return new APIResponse<TokenResponse>
                            {
                                ResponseCode = 200,
                                Message = "Success!",
                                Result = result
                            };
                        }
                    }

                    var accessToken = GenerateAccessToken(user.UserId);
                    var refreshToken = GenerateRefreshToken();

                    var tokenInfo = new TokenInfoDTO
                    {
                        userId = user.UserId,
                        AccessToken = accessToken,
                        refreshToken = refreshToken,
                        accessTokenExpiration = DateTime.UtcNow.AddHours(24),
                        refreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                    };

                    var isTokenSaved = await SaveTokenInfo(tokenInfo);

                    if (isTokenSaved)
                    {
                        var result = new TokenResponse
                        {
                            accessToken = accessToken,
                            refreshToken = refreshToken
                        };

                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 200,
                            Message = "Success!",
                            Result = result
                        };
                    }

                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Failed!",
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse<TokenResponse>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<UserDTO?> GetUserByUsername(string username)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = "SELECT * FROM Users WHERE Username = @Username";
                var user = await connection.QueryFirstOrDefaultAsync<UserDTO>(query, new { Username = username });
                return user;
            }
        }

        public static (string hashedPassword, string salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            string saltedPassword = password + salt;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string hashedPassword = Convert.ToBase64String(hashBytes);

                return (hashedPassword, salt);
            }
        }

        public async Task<UserDTO> AddUser(UserDTO user)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = @"
                    INSERT INTO users (username, password_hash, salt, email, phone)
                    VALUES (@Username, @HashedPassword, @Salt, @Email, @Phone);
                    SELECT LAST_INSERT_ID();
                ";
                var parameters = new
                {
                    Username = user.Username,
                    HashedPassword = user.PasswordHash,
                    Salt = user.Salt,
                    Email = user.Email,
                    Phone = user.Phone
                };

                int userId = await connection.ExecuteScalarAsync<int>(query, parameters);

                user.UserId = userId;
                return user;
            }
        }

        public async Task<Boolean> SaveTokenInfo(TokenInfoDTO tokenInfo)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = @"
                    INSERT INTO token_info (userId, accessToken, refreshToken, accessTokenExpiration, refreshTokenExpiration)
                    VALUES (@UserId, @AccessToken, @RefreshToken, @AccessTokenExpiration, @RefreshTokenExpiration)
                ";

                int affectedRows = await connection.ExecuteAsync(query, tokenInfo);
                return affectedRows > 0;
            }
        }

        private string GenerateAccessToken(int userId)
        {
            var secretKey = _configuration["AppSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("SecretKey is missing or empty in appsettings.json.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var accessTokenExpiration = DateTime.UtcNow.AddHours(23);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userId.ToString())
            };

            // generate token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessTokenExpiration,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            // Tạo chuỗi mã băm từ mật khẩu và salt lưu trữ
            string saltedPassword = password + salt;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string inputHashedPassword = Convert.ToBase64String(hashBytes);

                // So sánh chuỗi mã băm từ mật khẩu nhập vào với chuỗi mã băm lưu trữ
                return string.Equals(inputHashedPassword, hashedPassword);
            }
        }

    }
}

