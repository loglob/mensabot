using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace mensabot
{
	using static System.StringComparison;

	class Program
	{
		public record ServerEntry(
			string Webhook,
			string[]? MenuBlacklist = null,
			string[]? MenuWhitelist = null,
			string? Message = null,
			string[]? Allergens = null);

		const string DefaultMessage = "**Morgen gibt es:**";

		static async Task MainAsync()
		{
			using(var f = File.OpenText("./config.json"))
			{
				var essen = await Menu.GetEssenAsync();

				if(! essen.Any())
					return;

				foreach(var server in new JsonSerializer().Deserialize<ServerEntry[]>(new JsonTextReader(f))!)
				{
					var allergens = new HashSet<string>(server.Allergens ?? [], StringComparer.OrdinalIgnoreCase);

					await Discord.SendEmbed(server.Webhook, server.Message ?? DefaultMessage, essen
						.Where(e => server.MenuWhitelist is null
							|| server.MenuWhitelist.Any(m => e.Ausgabe.Equals(m, OrdinalIgnoreCase)))
						.Where(e => server.MenuBlacklist is null
							|| server.MenuBlacklist.All(m => !e.Ausgabe.Equals(m, OrdinalIgnoreCase)))
						.Select(e => e.ToEmbed(allergens)) );
				}
			}

		}

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}
	}
}
