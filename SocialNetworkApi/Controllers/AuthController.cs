using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Models;
using SocialNetworkApi.ViewModels;

namespace SocialNetworkApi.Controllers
{
	[AllowAnonymous]
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly SignInManager<User> _signInManager;

		public AuthController([FromServices] SignInManager<User> signInManager, [FromServices] UserManager<User> userManager)
		{
			_signInManager = signInManager;
		}

		[HttpPost]
		[Route("signin")]
		public async Task<IActionResult> SignIn([FromBody] LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
				if (result.Succeeded)
				{
					return new JsonResult(new Response { Ok = true, StatusCode = 200 });
				}
				else
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}
			}
			else
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 400 });
			}
		}

		[HttpPost]
		[Route("signout")]
		public async Task<IActionResult> SignOut()
		{
			await _signInManager.SignOutAsync();
			return new JsonResult(new Response { Ok = true, StatusCode = 200 });
		}

		[HttpPost]
		[Route("create")]
		public async Task<IActionResult> CreateNewAccount([FromBody] LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (User.Identity.IsAuthenticated)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				if (model.UserName == null || string.IsNullOrWhiteSpace(model.UserName) || model.UserName.Length < 3 || model.UserName.Length > 32)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 400 });
				}

				if (model.Password == null || string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6 || model.Password.Length > 16)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 400 });
				}

				var checkUser = await _signInManager.UserManager.FindByNameAsync(model.UserName);
				if (checkUser != null) // Такой уже есть пользователь
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				var newUser = new User { UserName = model.UserName, NormalizedUserName = model.UserName.ToUpper() };
				var result = await _signInManager.UserManager.CreateAsync(newUser, model.Password);
				if (!result.Succeeded)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 500 });
				}
				else
				{
					await SignIn(model);
					return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new { Message = "Created and signined" } });
				}
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpGet]
		[Route("NeedAuthorization")]
		public IActionResult NeedAuthorization()
		{
			return new JsonResult(new Response { Ok = false, StatusCode = 401 });
		}

		[HttpGet]
		[Route("AccessDenied")]
		public IActionResult AccessDenied()
		{
			return new JsonResult(new Response { Ok = false, StatusCode = 403 });
		}
	}
}
