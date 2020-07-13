using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.ViewModels
{
	public class IdViewModel<T> where T : struct
	{
		[Required]
		public T Id { get; set; }
	}
}
