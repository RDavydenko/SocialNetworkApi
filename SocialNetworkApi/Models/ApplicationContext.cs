using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SocialNetworkApi.Models.NToNs;

namespace SocialNetworkApi.Models
{
	public class ApplicationContext : IdentityDbContext<User, IdentityRole<int>, int>
	{
		public DbSet<Message> Messages { get; set; } // Сообщения
		public DbSet<Dialog> Dialogs { get; set; } // Диалоги

		// *NtoN - связь многие ко многим в БД
		public DbSet<NToNs.UserToDialog> UserToDialogs { get; set; } // NtoN таблица для связи диалогов и пользователей
		public DbSet<NToNs.UserToFriend> UserToFriends { get; set; } // NtoN таблица для связи пользователей и друзей

		public DbSet<NToNs.UserToFollower> UserToFollowers { get; set; } // NtoN таблица для связи пользователей и подписчиков (входящие запросы)
		public DbSet<NToNs.UserToRequest> UserToRequests { get; set; } // NtoN таблица для связи пользователей и запросов (исходящих (подписок))

		public ApplicationContext(DbContextOptions<ApplicationContext> options)
			: base(options)
		{			
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(u =>
			{
				u.HasMany(x => x.Dialogs) // ✔
				.WithOne(x => x.User)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction); // При удалении диалога с пользователем ничего не происходит

				u.HasMany(u => u.Messages) // ✔
				.WithOne(m => m.Author)
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.NoAction); // При удалении сообщения с пользователем ничего не происходит
			});

			builder.Entity<Message>(m =>
			{
				m.HasOne(m => m.Author) // ✔ 
				.WithMany(u => u.Messages)
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.SetNull); // При удалении автора, в сообщении автор сообщения ставится в NULL
			});

			builder.Entity<UserToDialog>(ud =>
			{
				ud.HasOne(ud => ud.User) // ✔
				.WithMany(u => u.Dialogs)
				.HasForeignKey(ud => ud.UserId)
				.OnDelete(DeleteBehavior.Cascade); // При удалении пользователя удаляется запись в вспомогательной таблице

				ud.HasOne(ud => ud.Dialog) // ✔
				.WithMany(d => d.Users)
				.HasForeignKey(ud => ud.DialogId)
				.OnDelete(DeleteBehavior.Cascade); // При удалении диалога удаляется запись в вспомогательной таблице
			});

			builder.Entity<UserToFriend>(uf =>
			{
				uf.HasOne(uf => uf.User) // ✔
				.WithMany(u => u.Friends)
				.HasForeignKey(ud => ud.UserId)
				.OnDelete(DeleteBehavior.Cascade); // При удалении главного User'а (по UserId) удаляется запись каскадно
				// Нужен еще и триггер, который будет ставить null при удалении FriendId User'а - добавляется автоматически // ✔
			});

			builder.Entity<UserToFollower>(uf =>
			{
				uf.HasOne(uf => uf.User) // ✔
				.WithMany(u => u.Followers)
				.HasForeignKey(ud => ud.UserId)
				.OnDelete(DeleteBehavior.Cascade); // При удалении главного User'а (по UserId) удаляется запись каскадно 
				// Нужен еще и триггер, который будет удалять строку из UserToFollowers при удалении FollowerId User'а - добавляется автоматически // ✔
			});

			builder.Entity<UserToRequest>(ur =>
			{
				ur.HasOne(ur => ur.User) // ✔
				.WithMany(u => u.Requests)
				.HasForeignKey(ud => ud.UserId)
				.OnDelete(DeleteBehavior.Cascade); // При удалении главного User'а (по UserId) удаляется запись каскадно
				// Нужен еще и триггер, который будет удалять строку из UserToRequests при удалении RequesterId User'а - добавляется автоматически // ✔
			});





			base.OnModelCreating(builder);
		}		
	}
}
