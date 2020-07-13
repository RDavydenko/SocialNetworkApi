using System;

namespace SocialNetworkApi.Models
{
	public class Message
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public DateTime? SendingTime { get; set; }
	}
}
