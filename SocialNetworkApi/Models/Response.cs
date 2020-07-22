using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetworkApi.Models
{
	public class Response // Ответ пользователю
	{
		public bool Ok { get; set; } // true, если операция/запрос успешны

		public int StatusCode { get; set; } // Статус код (200, 400 и др)

		public string Description { get; set; } // Человекочитаемое описание ошибки

		public object Result { get; set; } // Результат операции (если имеется)
	}
}
