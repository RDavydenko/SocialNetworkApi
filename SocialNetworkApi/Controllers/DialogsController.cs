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
	}
}
