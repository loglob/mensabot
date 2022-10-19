using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System;

namespace mensabot
{
	static class Discord
	{
		private static string ToJson(this object data)
		{
			using(var writer = new StringWriter())
			{
				new JsonSerializer().Serialize(writer, data);
				writer.Flush();

				return writer.ToString();
			}
		}

		private static async Task Post(string webhook, object data)
		{
			var r = await new HttpClient()
				.PostAsync(webhook, new StringContent(data.ToJson(), Encoding.UTF8, "application/json"));

			if(!r.IsSuccessStatusCode)
				Console.WriteLine($"Failed to POST {webhook}, Response:\n{r}");
		}

		public static async Task SendEmbed(string webhook, string msg, IEnumerable<object> embed)
		{
			await Post(webhook, new Dictionary<string,object>{
				{ "content", msg },
				{ "username", "Mensa Bot" },
				{ "embeds", embed }
			});
		}
	}
}