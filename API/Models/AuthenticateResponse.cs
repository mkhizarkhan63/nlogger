using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace API.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        public string Msg { get; set; }
        public int Status { get; set; }
        /*[JsonIgnore] */// refresh token is returned in http only cookie
        public string RefreshToken { get; set; }


        public AuthenticateResponse(User user, string jwtToken, string refreshToken, string msg, int statusCode)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
            Msg = msg;
            Status = statusCode;

        }
        public AuthenticateResponse()
        {
            Status = 400;
            Msg = "Server Error";
        }
    }
}
