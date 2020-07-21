using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.Models.NToNs
{
	public class UserToFollower
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public User User { get; set; }

		public int FollowerId { get; set; }		
	}
}
