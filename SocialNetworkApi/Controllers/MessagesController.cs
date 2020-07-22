using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
	/* ЗАМЕТКИ:
	 * Контроллер обязательно должен разрешать тип json с помощью атрибута:
	 * [Produces("application/json")]
	 * Такой тип данных должен быть указан в запросе (contentType)
	 * Пример: contentType: 'application/json; charset=utf-8'
	 * Поэтому данные запроса должны заворачиваться в анонимный тип и отправляться с помощью
	 * JSON.stringify() - в javaScript
	 * Данные ответа представляют собой json формат, поэтому в ajax-запросе:
	 * dataType: "json"
	 * 
	 * !!! ВАЖНО !!!
	 * Данные в параметрах метода идут с атрибутом [FromBody] и обернуты в ViewModel
	 * Поля ViewModel должны соответствовать с названиями отправленных полей из ajax-запроса
	 * Если не оборачивать во ViewModel, то будет ошибка в валидации
	 * 
	 * О РЕГИСТРОЗАВИСИМОСТИ: 
	 * (При отправке данных можно не соблюдать регистр) Id == id
	 * (ПРИ ПОЛУЧЕНИИ И ОБРАБОТКЕ ВАЖЕН РЕГИСТР) Id != id
	 * 
	*/

	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class MessagesController : ControllerBase
	{
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;

		public MessagesController([FromServices] ApplicationContext context, [FromServices] UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetMessage([FromRoute] int id)
		{
			var currentUser = await _userManager.GetUserAsync(User);
			if (currentUser == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Запрашивающий пользователь не найден" });
			}

			var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
			if (message == null)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Сообщение не найдено" });
			}

			if (message.AuthorId != currentUser.Id)
			{
				return new JsonResult(new Response { Ok = false, StatusCode = 403, Description = "Пользователь не является автором этого сообщения" });
			}

			var messageViewModel = new MessageViewModel(message);
			return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = messageViewModel });
		}

		
		[HttpPost]
		[Route("{id}/edit")]
		public async Task<IActionResult> EditMessage([FromRoute] int id, [FromBody] MessageTextViewModel model)
		{
			if (ModelState.IsValid)
			{
				var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
				if (message == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Сообщение не найдено" });
				}

				var currentUser = await _userManager.GetUserAsync(User);
				if (message.AuthorId != currentUser.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403, Description = "Пользователь не является автором этого сообщения" });
				}

				if (string.IsNullOrWhiteSpace(model.Text) || model.Text.Length < 1 || model.Text.Length > 10000)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Текст сообщения не соответствует требованиям (Длина до 10000)" });
				}

				message.Text = model.Text;
				_context.Messages.Update(message);
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200, Result = new MessageViewModel(message) }); 
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}
		
		[HttpPost]
		[Route("{id}/delete")]
		public async Task<IActionResult> DeleteMessage([FromRoute] int id)
		{
			if (ModelState.IsValid)
			{
				var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
				if (message == null)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 404, Description = "Сообщение не найдено" });
				}

				var currentUser = await _userManager.GetUserAsync(User);
				if (message.AuthorId != currentUser.Id)
				{
					return new JsonResult(new Response { Ok = false, StatusCode = 403, Description = "Пользователь не является автором этого сообщения" });
				}
								
				_context.Messages.Remove(message);
				await _context.SaveChangesAsync();
				return new JsonResult(new Response { Ok = true, StatusCode = 200 });
			}
			return new JsonResult(new Response { Ok = false, StatusCode = 400, Description = "Заполнены не все (либо неверно) поля запроса" });
		}
	}
}
