using API.Helpers;
using API.IRepository;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Repository
{
    public class UserService : IUserService
    {
        private DataContext _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserService> _logger;
        public UserService(DataContext context,
            IOptions<AppSettings> appSettings, ILogger<UserService> _logger)
        {
            this._logger = _logger;
            this._context = context;
            _appSettings = appSettings.Value;
        }
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken(user.Id);

            // save refresh token
            _context.RefreshTokens.Update(refreshToken);
            // _context.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token, "success", 200);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public AuthenticateResponse RefreshToken(string token)
        {
            try
            {
                var user = _context.Users.Join(
                    _context.RefreshTokens,
                    users => users.Id,
                    refreshTokens => refreshTokens.UserId,
                    (users, refresh) => new
                    {
                        userID = users.Id,
                        token = refresh.Token,
                        isActive = refresh.IsActive
                    }).FirstOrDefault();

                var UserOnly = _context.Users.Where(x => x.Id == user.userID).FirstOrDefault();

                // return null if no user found with token
                if (UserOnly == null) return null;

                var refreshToken = _context.RefreshTokens.Where(x => x.Token == token).FirstOrDefault();
                // return null if token is no longer active
                if (!refreshToken.IsActive) return null;

                // replace old refresh token with a new one and save
                var newRefreshToken = generateRefreshToken(UserOnly.Id);
                refreshToken.Revoked = DateTime.UtcNow;
                // refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;
                _context.RefreshTokens.Add(newRefreshToken);
                _context.Update(refreshToken);
                _context.SaveChanges();

                // generate new jwt
                var jwtToken = generateJwtToken(UserOnly);

                return new AuthenticateResponse(UserOnly, jwtToken, newRefreshToken.Token, "success", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception");

                return new AuthenticateResponse();
            }

        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(user);
            _context.SaveChanges();

            return true;
        }

        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(int usrID)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,

                    UserId = usrID
                };
            }
        }
    }
}
