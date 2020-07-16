using System;

namespace SocialNetworkApi.Models
{
	public class Message
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public DateTime SendingTime { get; set; }

		public int DialogId { get; set; }
		public Dialog Dialog { get; set; }

		public int AuthorId { get; set; }
		public User Author { get; set; }
	}
}
