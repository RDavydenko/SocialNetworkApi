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
using SocialNetworkApi.Models.NToNs;

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

		private async Task<IActionResult> TrySendFriendRequestAsync(int userId, int friendId)
		{
			var userToFriend = await _context.UserToFriends.FirstOrDefaultAsync(x => x.UserId == userId && x.FriendId == friendId);
			if (userToFriend != null) // Уже друзья
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 403 });
			}

			var userToRequest = await _context.UserToRequests.FirstOrDefaultAsync(x => x.UserId == userId && x.RequestId == friendId);
			if (userToRequest != null) // Уже имеет запрос пользователю с Id = friendId. Аналогично уже имеется зеркальная запись в подписчиках юзера с Id = friendId
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 403 });
			}

			var mirrorUserToRequest = await _context.UserToRequests.FirstOrDefaultAsync(x => x.UserId == friendId && x.RequestId == userId);
			if (mirrorUserToRequest != null) // Тот пользователь уже подписан, т.е. взаимная дружба => добавляем в друзья
			{
				var newUserToFriend = new UserToFriend { UserId = userId, FriendId = friendId };
				var newMirrorUserToFriend = new UserToFriend { UserId = friendId, FriendId = userId };
				await _context.UserToFriends.AddRangeAsync(newUserToFriend, newMirrorUserToFriend);
				// И удаляю запрос
				_context.UserToRequests.Remove(mirrorUserToRequest);
				// И удаляю подписчика				
				var userToFollower = await _context.UserToFollowers.FirstAsync(x => x.UserId == userId && x.FollowerId == friendId);
				_context.UserToFollowers.Remove(userToFollower);

				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new { Message = "Added", Id = newUserToFriend.Id } });
			}
			else // Тот пользователь не подписан, т.е. отправляется запрос на дружбу => добавляем запрос и подписчика
			{
				var newUserToRequest = new UserToRequest { UserId = userId, RequestId = friendId }; // У userId добавляем запрос (кому?) friendId
				_context.UserToRequests.Add(newUserToRequest);
				var newUserToFollower = new UserToFollower { UserId = friendId, FollowerId = userId }; // У friendId добавляем подписчика (кого?) userId
				_context.UserToFollowers.Add(newUserToFollower);
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new { Message = "Requested", Id = newUserToRequest.Id } });
			}
		}

		[HttpPost]
		[Route("add/{userId}")]
		public async Task<IActionResult> AddFriend([FromRoute] int userId)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
				if (friend == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				if (friend.Id == user.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				return await TrySendFriendRequestAsync(user.Id, friend.Id);
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
				if (userToFriend == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				if (userToFriend.UserId != user.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				if (userToFriend.FriendId == null)
				{
					// Если друга уже нет (friendId == null), то и зеркальной записи нет
					// А значит ее не нужно удалять, подписчика и запрос не нужно добавлять
					_context.UserToFriends.Remove(userToFriend);
					await _context.SaveChangesAsync();
					return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new { Message = "Deleted user removed" } });
				}
				else // Если friendId не null, т.е. существует
				{
					int followerId = userToFriend.FriendId.Value; // Id друга, который станет подписчиком
					var mirrorUserToFriend = await _context.UserToFriends.FirstAsync(u => u.UserId == userToFriend.FriendId && u.FriendId == userToFriend.UserId);
					_context.UserToFriends.RemoveRange(userToFriend, mirrorUserToFriend); // Удаляем записи о дружбе

					// Добавляем подписчика и запрос
					var newUserToFolower = new UserToFollower { UserId = user.Id, FollowerId = followerId }; // Подписчик
					_context.UserToFollowers.Add(newUserToFolower);
					var newUserToRequest = new UserToRequest { UserId = followerId, RequestId = user.Id }; // Запрос
					_context.UserToRequests.Add(newUserToRequest);
					await _context.SaveChangesAsync();
					return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new { Message = "Existing user removed", FollowerId = followerId } });
				}
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpPost]
		[Route("unfollow/{userId}")]
		public async Task<IActionResult> Unfollow([FromRoute] int userId)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var userToRequest = await _context.UserToRequests.FirstOrDefaultAsync(x => x.UserId == user.Id && x.RequestId == userId);
				if (userToRequest == null) // Если нет такого запроса
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}
				else // Если запрос есть, то удаляем запрос и подписчика
				{
					var userToFollower = await _context.UserToFollowers.FirstAsync(x => x.UserId == userId && x.FollowerId == user.Id);
					_context.UserToRequests.Remove(userToRequest);
					_context.UserToFollowers.Remove(userToFollower);
					await _context.SaveChangesAsync();
					return new JsonResult(new Response { Ok = false, StatusCode = 200, Result = new { Message = "Unfollowed", OldRequestedId = userId } });
				}
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}
	}
}
