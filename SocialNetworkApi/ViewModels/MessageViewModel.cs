using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetworkApi.Models;

namespace SocialNetworkApi.ViewModels
{
	public class MessageViewModel
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public DateTime SendingTime { get; set; }
		public int AuthorId { get; set; }

		public MessageViewModel()
		{

		}

		public MessageViewModel(Message message)
		{
			Id = message.Id;
			Text = message.Text;
			SendingTime = message.SendingTime;
			AuthorId = message.AuthorId;
		}
	}
}
