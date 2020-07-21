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
		public int Id { get; set; }

		public int UserId { get; set; } // Кто запросил
		public User User { get; set; } // Кто запросил

		public int RequestId { get; set; } // Кому запрашивает
	}
}
