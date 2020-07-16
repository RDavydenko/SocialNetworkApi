using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetworkApi.Models;

namespace SocialNetworkApi.ViewModels
{
	public class UserViewModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string BirthdayDate { get; set; } // Дата рождения
		public Sex? Sex { get; set; } // Пол
		public string Status { get; set; }

		public UserViewModel()
		{

		}

		public UserViewModel(User user)
		{
			Id = user.Id;
			UserName = user.UserName;
			Email = user.Email;
			BirthdayDate = user.BirthdayDate?.ToString() ?? null;
			Sex = user.Sex ?? null;
			Status = user.Status;
		}

		public void UpdateFields(ref User user)
		{
			if (UserName != null && !string.IsNullOrWhiteSpace(UserName) && UserName.Length > 3 && UserName.Length < 32)
			{
				user.UserName = UserName;
				user.NormalizedUserName = UserName.ToUpper();
			}

			if (Email != null && !string.IsNullOrWhiteSpace(Email) && Email.Length > 3 && Email.Length < 32 && Email.Contains("@"))
			{
				user.Email = Email;
				user.NormalizedUserName = Email.ToUpper();
			}

			bool dateIsParsed = DateTime.TryParse(this?.BirthdayDate, out DateTime newDate);
			user.BirthdayDate = dateIsParsed == true ? newDate : user.BirthdayDate;

			if (Sex != null && Enum.IsDefined(typeof(Sex), Sex))
			{
				user.Sex = this?.Sex ?? user.Sex;
			}

			if (Status != null && Status.Length < 300)
			{
				user.Status = Status;
			}
		}
	}
}
