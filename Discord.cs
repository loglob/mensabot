using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Linq;

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

		private static async Task Post(this string url, object data)
		{
			var r = await new HttpClient()
				.PostAsync(url, new StringContent(data.ToJson(), Encoding.UTF8, "application/json"));

			if(!r.IsSuccessStatusCode)
				Console.WriteLine($"Failed to POST {url}, Response:\n{r}");
		}

		private static async Task Post(this IEnumerable<string> urls, object data)
		{
			foreach (var url in urls)
				await url.Post(data);
		}

		private static IEnumerable<string> webhooks
			=> File.ReadLines("webhooks")
				.Select(ln => ln.Trim())
				.Where(ln => ln.Length > 0 && ln[0] != '#');

		public static async Task SendEmbed(string msg, IEnumerable<object> embed)
		{
			await webhooks.Post(new Dictionary<string,object>{
				{ "content", msg },
				{ "username", "Mensa Bot" },
				{ "embeds", embed }
			});
		}
		
	}
}