using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.Models.NToNs
{
	public class UserToRequest
	{
		[Key]
		public int Id { get; set; }

		public int ReceiverId { get; set; } // Получатель
		[ForeignKey("ReceiverId")]
		public User Receiver { get; set; } // Получатель

		public int RequesterId { get; set; } // Запрашивающий
		[ForeignKey("RequesterId")]
		public User Requester { get; set; } // Запрашивающий
	}
}
