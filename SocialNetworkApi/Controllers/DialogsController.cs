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
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class DialogsController : ControllerBase
	{
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;

		public DialogsController([FromServices] ApplicationContext context, [FromServices] UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> GetDialogs()
		{
			var userId = (await _userManager.GetUserAsync(User)).Id;
			var user = await _context.Users
					.Include(u => u.Dialogs)
					.ThenInclude(d => d.Dialog)
					.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404 });
			}

			var dialogs = user.Dialogs.Select(d => new DialogSimpleViewModel(d.Dialog)).ToList();
			return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = dialogs });
		}

		[HttpGet]
		[Route("{id}/messages")]
		public async Task<IActionResult> GetMessages([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				int userId = (await _userManager.GetUserAsync(User)).Id;
				var userToDialog = await _context.UserToDialogs
					.Include(d => d.Dialog)
					.ThenInclude(d => d.Messages)
					.FirstOrDefaultAsync(d => d.DialogId == id && d.UserId == userId);

				if (userToDialog == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var messages = userToDialog.Dialog.Messages.Select(m => new MessageViewModel(m)).ToList();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = messages });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpGet]
		[Route("{id}/members")]
		public async Task<IActionResult> GetMembers([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				int userId = (await _userManager.GetUserAsync(User)).Id;
				var userToDialog = await _context.UserToDialogs
					.Include(d => d.Dialog)
					.ThenInclude(d => d.Users)
					.FirstOrDefaultAsync(d => d.DialogId == id && d.UserId == userId);

				if (userToDialog == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var users = userToDialog.Dialog.Users.Select(u => u.UserId).ToList();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = users });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpPost]
		[Route("{id}/message")]
		public async Task<IActionResult> CreateNewMessage([FromRoute] int id, [FromBody] MessageTextViewModel model)
		{
			if (ModelState.IsValid)
			{
				var dialog = await _context.Dialogs
				.Include(d => d.Users)
				.FirstOrDefaultAsync(d => d.Id == id);
				if (dialog == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}

				var currentUser = await _userManager.GetUserAsync(User);
				if (!dialog.Users.Exists(d => d.UserId == currentUser.Id)) // Пользователь не состоит в диалоге
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403 });
				}

				if (string.IsNullOrWhiteSpace(model.Text) || model.Text.Length > 10000)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 400 });
				}

				var newMessage = new Message { AuthorId = currentUser.Id, DialogId = dialog.Id, Text = model.Text, SendingTime = DateTime.Now };
				_context.Messages.Add(newMessage);
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new MessageViewModel(newMessage) });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpPost]
		[Route("create")]
		public async Task<IActionResult> CreateDialog([FromBody] DialogSimpleViewModel model)
		{
			if (string.IsNullOrWhiteSpace(model.Title) || model.Title.Length < 1 || model.Title.Length > 32)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 400 });
			}

			var user = await _userManager.GetUserAsync(User);

			var newDialog = new Dialog { Title = model.Title, CreatingTime = DateTime.Now, LastMessageTime = DateTime.Now };
			_context.Dialogs.Add(newDialog);
			await _context.SaveChangesAsync();

			_context.UserToDialogs.Add(new Models.NToNs.UserToDialog { DialogId = newDialog.Id, UserId = user.Id });
			await _context.SaveChangesAsync();

			var dialogViewModel = new DialogSimpleViewModel(newDialog);
			return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = dialogViewModel });
		}

		[HttpPost]
		[Route("{id}/add")]
		public async Task<IActionResult> AddUsers([FromRoute] int id, [FromBody] ListIds<int> userIds)
		{
			var dialog = await _context.Dialogs
				.Include(d => d.Users)
				.FirstOrDefaultAsync(d => d.Id == id);
			if (dialog == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404 });
			}

			var currentUser = await _userManager.GetUserAsync(User);
			if (!dialog.Users.Exists(d => d.UserId == currentUser.Id)) // Пользователь не состоит в диалоге
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 403 });
			}

			var userToDialogs = new List<Models.NToNs.UserToDialog>();
			foreach (int userId in userIds.Ids)
			{
				if (!dialog.Users.Exists(d => d.UserId == userId) && _context.Users.Any(u => u.Id == userId)) // Пользователь не состоит в диалоге
				{
					userToDialogs.Add(new Models.NToNs.UserToDialog { UserId = userId, DialogId = dialog.Id });
				}
			}
			await _context.UserToDialogs.AddRangeAsync(userToDialogs);
			await _context.SaveChangesAsync();
			return new JsonResult(new Response { Ok = true, StatusCode = 200 });
		}

		[HttpPost]
		[Route("{id}/leave")]
		public async Task<IActionResult> LeaveFromDialog([FromRoute] int id)
		{
			var dialog = await _context.Dialogs
				.Include(d => d.Users)
				.FirstOrDefaultAsync(d => d.Id == id);
			if (dialog == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404 });
			}

			var currentUser = await _userManager.GetUserAsync(User);

			var userToDialog = dialog.Users.FirstOrDefault(d => d.UserId == currentUser.Id);
			if (userToDialog == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404 });
			}
			_context.UserToDialogs.Remove(userToDialog);
			await _context.SaveChangesAsync();
			return new JsonResult(new Response { Ok = true, StatusCode = 200 });
		}
	}
}
