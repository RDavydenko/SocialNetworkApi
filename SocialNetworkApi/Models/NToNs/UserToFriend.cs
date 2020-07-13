using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.Models.NToNs
{
	public class UserToFriend
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public User User { get; set; }

		public int? FriendId { get; set; }
		public User Friend { get; set; }
	}
}
