using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mensabot;
using static mensabot.Discord;

public record Essen
(
	string Loc,
	[property: JsonProperty("title_with_additives")]
	string RawTitle,
	double? Price,
	double Rating,
	[property: JsonProperty("rating_amt")]
	int Votes,
	string Image
){
	private const string api = "https://www.mensa-kl.de/api.php?date=1&format=json";
	private static readonly Dictionary<string, string> allergenEmojis = new(StringComparer.OrdinalIgnoreCase) {
		{ "la", ":milk:" },
		{ "ei", ":egg:" },
		{ "nu", ":peanuts:" },
		{ "s", ":pig:" },
		{ "r", ":cow:" },
		{ "g", ":chicken:" }
	};
	private static readonly Uri imageBaseUri = new Uri("https://www.mensa-kl.de/mimg/");

	private string Stars
	{
		get
		{
			int iscore = (int)Math.Round(Rating * 2);
			char[] s = new char[5];
			int i = 0;

			for(; iscore >= 2; iscore -= 2)
				s[i++] = '★';
			if(iscore > 0)
				s[i++] = '⋆';
			while(i < 5)
				s[i++] = '☆';

			return new string(s);
		}
	}

	private Uri? ImageUrl => string.IsNullOrEmpty(Image) ? null : new Uri(imageBaseUri, Image);

	/** Processes `RawTitle` so that each group of parenthesis is filtered for the given allergens only */
	private string processTitle(Filter allergens)
	{
		StringBuilder res = new();
		var title = WebUtility.HtmlDecode(RawTitle);

		for (int off = 0;;)
		{
			int l = title.IndexOf('(', off);

			if(l < 0)
			{
				res.Append(title, off, title.Length - off);
				break;
			}

			// trim trailing space left of '('
			{
				int n = l - off;

				while(n > 0 && char.IsWhiteSpace(title[off + n - 1]))
					--n;

				res.Append(title, off, n);
			}

			int r = title.IndexOf(')', l);

			if(r < 0)
				throw new FormatException("Menu title has unmatched parentheses");

			off = r + 1;
			var al = title.Substring(l + 1, r - l - 1).Split(',').Where(allergens.Allows).ToList();

			if(al.Count == 0)
				continue;

			res.Append(' ');

			// print emoji where applicable
			for (int i = 0; i < al.Count;)
			{
				if(allergenEmojis.TryGetValue(al[i], out var emoji))
				{
					al.RemoveAt(i);

					res.Append(emoji);
				}
				else
					++i;
			}

			if(al.Count == 0)
				continue;

			// print shorthands for the rest
			res.Append("*(");
			bool head = true;

			foreach (var a in al)
			{
				if(!head)
					res.Append(',');
				res.Append(a);
				head = false;
			}

			res.Append(")*");
		}

		return res.ToString();
	}

	public string Ausgabe => int.TryParse(Loc, out int a) ? $"Ausgabe {a}" : Loc;

	public static async Task<Essen[]> GetMenuAsync()
	{
		var resp = new HttpClient().GetAsync(api).Result.Content;

		try
		{
			using(var r = await resp.ReadAsStreamAsync())
			using(var tr = new StreamReader(r))
			using(var jr = new JsonTextReader(tr))
			{
				return new JsonSerializer().Deserialize<Essen[]>(jr)
					?? throw new ArgumentNullException("API returned null");
			}
		}
		catch(Exception ex) when (ex is JsonException || ex is FormatException)
		{
			throw new FormatException($"Expected valid JSON, got '{await resp.ReadAsStringAsync()}'", ex);
		}

	}

	private string PriceString
		=> Price.HasValue ? Price.Value.ToString("0.00€") : "???";

	public override string ToString()
		=> $"> {RawTitle} für {PriceString} an {Ausgabe}\n> {Stars} (aus {Votes} Stimmen)\n";

	public Embed ToEmbed(Filter allergens)
	{
		EmbedField für = new("für", PriceString);

		return new (
			Ausgabe,
			processTitle(allergens),
			(new EmbedField?[]{
				Price.HasValue ? new("für", PriceString) : null,
				Votes > 0 ? new("rating", Stars) : null,
				Votes > 0 ? new("votes", Votes.ToString()) : null
			}).Where(x => x is not null).ToArray()!,
			ImageUrl is null ? null : new(ImageUrl)
		);
	}
}

