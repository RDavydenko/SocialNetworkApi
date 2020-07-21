using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SocialNetworkApi.Models
{
	public class User : IdentityUser<int>
	{
		public DateTime? BirthdayDate { get; set; } // Дата рождения
		public Sex? Sex { get; set; } // Пол
		public string Status { get; set; } // Статус

		public List<NToNs.UserToDialog> Dialogs { get; set; } // Диалоги
		public List<Message> Messages { get; set; } // Сообщения

		public List<NToNs.UserToFriend> Friends { get; set; } // Друзья
		public List<NToNs.UserToRequest> Requests { get; set; } // Запросы
		public List<NToNs.UserToFollower> Followers { get; set; } // Подписчики


		public User()
		{
			Dialogs = new List<NToNs.UserToDialog>();
			Messages = new List<Message>();
			Friends = new List<NToNs.UserToFriend>();
			Requests = new List<NToNs.UserToRequest>();
			Followers = new List<NToNs.UserToFollower>();
		}
	}
}
