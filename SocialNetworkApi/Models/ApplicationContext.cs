using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Models.NToNs;

namespace SocialNetworkApi.Models
{
	public class ApplicationContext : IdentityDbContext<User, IdentityRole<int>, int>
	{
		public DbSet<Message> Messages { get; set; }
		public DbSet<Dialog> Dialogs { get; set; }

		//public DbSet<NToNs.UserToFriend> UserToFriends { get; set; }
		//public DbSet<NToNs.UserToRequest> UserToRequests { get; set; }
		//public DbSet<NToNs.UserToFollower> UserToFollowers { get; set; }
		public DbSet<NToNs.UserToDialog> UserToDialogs { get; set; }

		public ApplicationContext(DbContextOptions<ApplicationContext> options)
			: base(options)
		{
			Database.EnsureDeleted();
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(u =>
			{
				u.HasMany(x => x.Dialogs) // ✔
				.WithOne(x => x.User)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);

				u.HasMany(u => u.Messages) // ✔
				.WithOne(m => m.Author)
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.NoAction);

				//u.HasMany(u => u.Friends) // ✔
				//.WithOne(uf => uf.User)
				//.HasForeignKey(uf => uf.UserId)
				//.OnDelete(DeleteBehavior.NoAction);
			});			

			builder.Entity<Message>(m =>
			{
				m.HasOne(m => m.Author) // ✔ 
				.WithMany(u => u.Messages)
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.SetNull); // При удалении автора, что происходит с сообщением?
			});

			builder.Entity<UserToDialog>(ud => 
			{
				ud.HasOne(ud => ud.User) // ✔
				.WithMany(u => u.Dialogs)
				.HasForeignKey(ud => ud.UserId)
				.OnDelete(DeleteBehavior.Cascade);

				ud.HasOne(ud => ud.Dialog) // ✔
				.WithMany(d => d.Users)
				.HasForeignKey(ud => ud.DialogId)
				.OnDelete(DeleteBehavior.Cascade);
			});

			//builder.Entity<UserToFriend>(uf =>
			//{
			//	uf.HasOne(uf => uf.User)
			//	.WithMany(u => u.Friends)
			//	.HasForeignKey(ud => ud.UserId)
			//	.OnDelete(DeleteBehavior.Cascade);
			//});



			base.OnModelCreating(builder);
		}
	}
}
