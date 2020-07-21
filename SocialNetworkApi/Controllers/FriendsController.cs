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

namespace SocialNetworkApi.Controllers
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class FriendsController : ControllerBase
	{
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;

		public FriendsController([FromServices] ApplicationContext context, [FromServices] UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		[Route("add/{id}")]
		public async Task<IActionResult> AddNewFriend([FromRoute] int id)
		{
			if(ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if(user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
				if (friend == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				if (friend.Id == user.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}			

				var userToFriend = await _context.UserToFriends.FirstOrDefaultAsync(x => x.UserId == user.Id && x.FriendId == friend.Id);
				if (userToFriend != null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				userToFriend = new Models.NToNs.UserToFriend { UserId = user.Id, FriendId = friend.Id };
				var friendToUser = new Models.NToNs.UserToFriend { UserId = friend.Id, FriendId = user.Id }; // Зеркальная запись
				await _context.AddRangeAsync(userToFriend, friendToUser);
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200 });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		} 

		[HttpPost]
		[Route("{userToFriendId}/remove")]
		public async Task<IActionResult> RemoveFriend([FromRoute] int userToFriendId)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var userToFriend = await _context.UserToFriends.FirstOrDefaultAsync(x => x.Id == userToFriendId);
				if(userToFriend == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				if (userToFriend.UserId != user.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				if (userToFriend.FriendId == null)
				{
					_context.UserToFriends.Remove(userToFriend);					
				}
				else
				{
					var mirrorUserToFriend = await _context.UserToFriends.FirstAsync(u => u.UserId == userToFriend.FriendId && u.FriendId == userToFriend.UserId);
					_context.UserToFriends.RemoveRange(userToFriend, mirrorUserToFriend);
				}
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200 });

			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}
	}
}
