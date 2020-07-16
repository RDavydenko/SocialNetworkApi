using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.ViewModels
{
	public class ListIds<T> where T : struct
	{
		public List<T> Ids { get; set; } = new List<T>();
	}
}
