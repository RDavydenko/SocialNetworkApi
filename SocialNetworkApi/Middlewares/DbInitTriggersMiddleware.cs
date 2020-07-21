using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace SocialNetworkApi.Middlewares
{	
	public class DbInitTriggersMiddleware
	{
		private readonly RequestDelegate _next;
		private Models.ApplicationContext _db;		

		public DbInitTriggersMiddleware(RequestDelegate next)
		{
			this._next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			_db = context.RequestServices.GetService(typeof(Models.ApplicationContext)) as Models.ApplicationContext;
			await _db.Database.ExecuteSqlInterpolatedAsync($"CREATE OR ALTER TRIGGER set_null_in_UserToFriends_after_deleting_from_AspNetUsers ON [dbo].[AspNetUsers] AFTER DELETE AS BEGIN UPDATE [dbo].[UserToFriends] SET FriendId = NULL WHERE FriendId = (SELECT Id FROM deleted) END");

			await _next.Invoke(context);
		}
	}
}
