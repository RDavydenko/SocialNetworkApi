using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SocialNetworkApi.Controllers
{

	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		[HttpGet]
		[Route("give")]
		public IActionResult Get()
		{
			var a = new Person[] 
			{
				new Person  { Name = "Liza", Age = 18, Sex = "Female" },
				new Person  { Name = "Liza", Age = 18, Sex = "Female" },
				new Person  { Name = "Liza", Age = 18, Sex = "Female" },
			};

			return new JsonResult(a);
		}

		public class Person
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public string Sex { get; set; }
		}
	}
}
