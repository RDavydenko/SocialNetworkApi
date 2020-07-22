using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Models;
using SocialNetworkApi.ViewModels;

namespace SocialNetworkApi.Controllers
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;

		public UsersController([FromServices] ApplicationContext context, [FromServices] UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> Get([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашиваемый пользователь не найден" });
				}
				var userViewModel = new UserViewModel(user);
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = userViewModel });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}

		[HttpGet]
		[Authorize]
		[Route("me")]
		public async Task<IActionResult> GetMe()
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашивающий пользователь не найден" });
				}

				var userViewModel = new UserViewModel(user);
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = userViewModel });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}

		[HttpGet]
		[Route("{id}/friends")]
		public async Task<IActionResult> GetFriends([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				var user = await _context.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.Id == id);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашиваемый пользователь не найден" });
				}

				var friendIds = user.Friends.Select(f => new FriendUserViewModel { Id = f.Id, FriendId = f.FriendId });
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = friendIds });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}

		[HttpGet]
		[Route("{id}/followers")]
		public async Task<IActionResult> GetFollowers([FromRoute] int id) // Подписчики
		{
			if (ModelState.IsValid)
			{
				var user = await _context.Users.Include(u => u.Followers).FirstOrDefaultAsync(u => u.Id == id);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашиваемый пользователь не найден" });
				}

				var followerIds = user.Followers.Select(f => new { Id = f.Id, FollowerId = f.FollowerId });
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = followerIds });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}

		[HttpGet]
		[Route("{id}/requests")]
		public async Task<IActionResult> GetRequests([FromRoute] int id) // Подписки (запросы на добавление в друзья)
		{
			if (ModelState.IsValid)
			{
				var user = await _context.Users.Include(u => u.Requests).FirstOrDefaultAsync(u => u.Id == id);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашиваемый пользователь не найден" });
				}

				var requestsIds = user.Requests.Select(f => new { Id = f.Id, RequestId = f.RequestId });
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = requestsIds });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}

		[HttpPost]
		[Authorize]
		[Route("edit")]
		public async Task<IActionResult> Edit([FromBody] UserViewModel model)
		{
			var currentUser = await _userManager.GetUserAsync(User);
			if (currentUser == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашивающий пользователь не найден" });
			}
			model.UpdateFields(ref currentUser);
			_context.Users.Update(currentUser);
			await _context.SaveChangesAsync();

			var userViewModel = new UserViewModel(currentUser);
			return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = userViewModel });
		}
	}
}
