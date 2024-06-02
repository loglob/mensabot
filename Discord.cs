using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mensabot
{
	static class Discord
	{
		public record Embed(string Title, string? Description, EmbedField[] Fields, EmbedImage? Image);
		public record EmbedField(string Name, string Value, bool Inline = true);
		public record EmbedImage(Uri url);

		private static string ToJson(this object data)
		{
			DefaultContractResolver contractResolver = new DefaultContractResolver {
				NamingStrategy = new CamelCaseNamingStrategy()
			};


			using(var writer = new StringWriter())
			{
				new JsonSerializer() {
					ContractResolver = contractResolver,
					NullValueHandling = NullValueHandling.Ignore
				}.Serialize(writer, data);
				writer.Flush();

				return writer.ToString();
			}
		}

		private static async Task Post(string webhook, object data)
		{
			var json = data.ToJson();
			var r = await new HttpClient()
				.PostAsync(webhook, new StringContent(json, Encoding.UTF8, "application/json"));

			if(!r.IsSuccessStatusCode)
			{
				Console.WriteLine($"Failed to POST {webhook}, Response:\n{r}");
				Console.WriteLine($"Tried to send content: {json}");
			}
		}

		public static async Task SendEmbed(string webhook, string msg, IEnumerable<Embed> embeds)
		{
			bool head = true;

			foreach(var ch in embeds.Chunk(10))
			{
				var post = new Dictionary<string,object>{
					{ "username", "Mensa Bot" },
					{ "embeds", ch }
				};

				if(head)
					post["content"] = msg;

				await Post(webhook, post);

				head = false;
			}
		}
	}
}