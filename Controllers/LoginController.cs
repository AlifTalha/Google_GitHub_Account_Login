using GoogleLogin.Data;
using GoogleLogin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
//using Microsoft.AspNetCore.Authentication.GitHub;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoogleLogin.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LoginController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string name, string email)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email))
            {
                var user = new UserInfo
                {
                    Name = name,
                    Email = email,
                    GoogleId = null,
                    GitHubId = null
                };

                _db.UserInfos.Add(user);
                await _db.SaveChangesAsync();

                ViewBag.Message = "User information saved successfully!";
            }

            return View();
        }

        public IActionResult LoginWithGoogle()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalLoginResponse")
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        public IActionResult LoginWithGitHub()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalLoginResponse")
            };
            return Challenge(props, "GitHub");
        }

        public async Task<IActionResult> ExternalLoginResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Index");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Handle Google
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier &&
                                                     c.Issuer == "https://accounts.google.com")?.Value;

            // Handle GitHub
            var githubId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier &&
                                                    c.Issuer == "https://github.com/")?.Value;

            if (!string.IsNullOrEmpty(googleId) && !_db.UserInfos.Any(u => u.GoogleId == googleId))
            {
                var user = new UserInfo
                {
                    GoogleId = googleId,
                    Name = name,
                    Email = email
                };
                _db.UserInfos.Add(user);
                await _db.SaveChangesAsync();
            }
            else if (!string.IsNullOrEmpty(githubId) && !_db.UserInfos.Any(u => u.GitHubId == githubId))
            {
                var user = new UserInfo
                {
                    GitHubId = githubId,
                    Name = name,
                    Email = email ?? $"{githubId}@users.noreply.github.com" // GitHub might not provide email
                };
                _db.UserInfos.Add(user);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}