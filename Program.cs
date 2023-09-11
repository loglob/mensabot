using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace mensabot
{

	class Program
	{
		public record ServerEntry(string Webhook, string[] MenuBlacklist = null, string[] MenuWhitelist = null, string Message = null);

		const string DefaultMessage = "**Morgen gibt es:**";

		static async Task MainAsync()
		{
			using(var f = File.OpenText("./config.json"))
			{
				var essen = await Menu.GetEssenAsync();

				foreach(var server in new JsonSerializer().Deserialize<ServerEntry[]>(new JsonTextReader(f)))
				{
					await Discord.SendEmbed(server.Webhook, server.Message ?? DefaultMessage, essen
						.Where(e => server.MenuWhitelist is null
							|| server.MenuWhitelist.Any(m => e.Ausgabe.ToLower() == m.ToLower()))
						.Where(e => server.MenuBlacklist is null
							|| server.MenuBlacklist.All(m => e.Ausgabe.ToLower() != m.ToLower())));
				}
			}

		}

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}
	}
}
