using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Messenger.DataLayer.SqlServer;
using Messenger.WebApi.Models;
using Messenger.Logger;
using Messenger.Model;
using NLog;
using Messenger.WebApi.Principals;
using System.Security.Principal;
using System.Web.Http.Cors;
using Newtonsoft.Json.Linq;

namespace Messenger.WebApi.App_Start
{
    public class AccountController : ApiController
    {
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public AuthToken Login(UserCredentials user)
        {
            NLogger.Logger.Debug("Authenticating user by login and pass. Login: \"{0}\"", user.Login);
            var result = RepositoryBuilder.UsersRepository.GetUserByLogin(user.Login);
            result.Password = RepositoryBuilder.UsersRepository.GetPassword(result.Id);
            var token = RepositoryBuilder.TokensRepository.GenerateToken(result.Id);

            if (result.Password.Equals(user.Password))
                return new AuthToken(token,result.Id, result.Login);
            else return new AuthToken();
        }

        [AllowAnonymous]
        [Route("register")]
        public IPrincipal Register(UserCredentials user)
        {
            NLogger.Logger.Debug("Resgister user with login: \"{0}\"", user.Login);

            try
            {
                var result = RepositoryBuilder.UsersRepository.GetUserByLogin(user.Login);
                return result.Password.Equals(user.Password) ? null : new UserPrincipal(result.Id, result.Login, Guid.Empty);
            }
            catch (ArgumentException)
            {
                NLogger.Logger.Error("Registration failed");
                return null;
            }
        }

    }
}
