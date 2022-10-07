using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.IRepository
{
    public interface IUserService
    {
        public AuthenticateResponse Authenticate(AuthenticateRequest model);
        public AuthenticateResponse RefreshToken(string token);
        public bool RevokeToken(string token, string ipAddress);
        public IEnumerable<User> GetAll();
        public User GetById(int id);
    }
}
