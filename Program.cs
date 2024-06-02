using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mensabot;

/// <summary>
///  A case-insensitive filter
/// </summary>
/// <param name="bias"> Sets whether the filter will be full or empty if both lists are null </param>
public class Filter(IEnumerable<string>? whitelist, IEnumerable<string>? blacklist, bool bias)
{
	private readonly HashSet<string>? white = whitelist is null ? (bias || blacklist is not null ? null : []) : new(whitelist, StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> black = new(blacklist ?? [], StringComparer.OrdinalIgnoreCase);

	public bool Allows(string val)
		=> (white is null || white.Contains(val)) && !black.Contains(val);
}

class Program
{

	public record ServerEntry
	(
		string Webhook,
		string[]? MenuBlacklist = null,
		string[]? MenuWhitelist = null,
		string Message = "**Morgen gibt es:**",
		string[]? AllergenWhitelist = null,
		string[]? AllergenBlacklist = null
	){
		public readonly Filter MenuFilter = new(MenuWhitelist, MenuBlacklist, true);
		public readonly Filter AllergenFilter = new(AllergenWhitelist, AllergenBlacklist, false);
	}

	static async Task MainAsync()
	{
		using var f = File.OpenText("./config.json");
		var essen = await Menu.GetEssenAsync();

		if (!essen.Any())
			return;

		foreach (var server in new JsonSerializer().Deserialize<ServerEntry[]>(new JsonTextReader(f))!)
		{
			await Discord.SendEmbed(server.Webhook, server.Message, essen
				.Where(e => server.MenuFilter.Allows(e.Ausgabe))
				.Select(e => e.ToEmbed(server.AllergenFilter)));
		}
	}

	static void Main(string[] args)
	{
		MainAsync().GetAwaiter().GetResult();
	}
}
