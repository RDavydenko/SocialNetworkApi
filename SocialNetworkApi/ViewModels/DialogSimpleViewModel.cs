using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.ViewModels
{
	public class DialogSimpleViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; } // Название
		public DateTime CreatingTime { get; set; } // Время создания
		public DateTime LastMessageTime { get; set; } // Время отправки последнего сообщения

		public DialogSimpleViewModel()
		{

		}

		public DialogSimpleViewModel(Models.Dialog dialog)
		{
			Id = dialog.Id;
			Title = dialog.Title;
			CreatingTime = dialog.CreatingTime;
			LastMessageTime = dialog.LastMessageTime;
		}
	}
}
