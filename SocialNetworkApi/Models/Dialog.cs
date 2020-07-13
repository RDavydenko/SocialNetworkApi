using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.Models
{
	public class Dialog
	{
		public int Id { get; set; }
		public string Title { get; set; } // Название
		public DateTime CreatingTime { get; set; } // Время создания
		public DateTime LastMessageTime { get; set; } // Время отправки последнего сообщения

		public List<NToNs.UserToDialog> Users { get; set; } // Пользователи-участники
		public List<Message> Messages { get; set; } // Сообщения

		public Dialog()
		{
			Users = new List<NToNs.UserToDialog>();
			Messages = new List<Message>();
		}
	}
}
