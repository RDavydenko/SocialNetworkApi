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
		[Route("get/{id}")]
		public async Task<IActionResult> Get([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
				if (user == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404 });
				}
				var userViewModel = new UserViewModel(user);
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = userViewModel });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400 });
		}

		[HttpPost]
		[Authorize]
		[Route("edit")]
		public async Task<IActionResult> Edit([FromBody] UserViewModel model)
		{
			var currentUser = await _userManager.GetUserAsync(User);
			if (currentUser == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404 });
			}
			model.UpdateFields(ref currentUser);
			_context.Users.Update(currentUser);
			await _context.SaveChangesAsync();

			var userViewModel = new UserViewModel(currentUser);
			return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = userViewModel });
		}		
	}
}
