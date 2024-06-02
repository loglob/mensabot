
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using static mensabot.Discord;
using System.Text;
using System.Linq;

namespace mensabot
{
	static class Menu
	{
		private const string api = "https://www.mensa-kl.de/api.php?date=1&format=json";

		public record Essen(
			string Loc,
			[property: JsonProperty("title_with_additives")]
			string RawTitle,
			double Price,
			double Rating,
			[property: JsonProperty("rating_amt")]
			int Votes,
			string Image)
		{
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
			private string processTitle(HashSet<string> allergens)
			{
				StringBuilder res = new();

				for (int off = 0;;)
				{
					int l = RawTitle.IndexOf('(', off);

					if(l < 0)
					{
						res.Append(RawTitle, off, RawTitle.Length - off);
						break;
					}

					// trim trailing space left of '('
					{
						int n = l - off;

						while(n > 0 && char.IsWhiteSpace(RawTitle[off + n - 1]))
							--n;

						res.Append(RawTitle, off, n);
					}

					int r = RawTitle.IndexOf(')', l);

					if(r < 0)
						throw new FormatException("Menu title has unmatched parentheses");

					off = r + 1;
					var al = RawTitle.Substring(l + 1, r - l - 1).Split(',').Where(allergens.Contains).ToList();

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

			public override string ToString()
				=> $"> {RawTitle} für {Price:0.00}€ an {Ausgabe}\n> {Stars} (aus {Votes} Stimmen)\n";

			public Embed ToEmbed(HashSet<string> allergens)
			{
				EmbedField für = new("für", Price.ToString("0.00€"));

				return new (
					Ausgabe,
					processTitle(allergens),
					Votes > 0 ? [
						für,
						("rating", Stars),
						("votes", Votes)
					] : [
						für
					],
					ImageUrl is null ? null : new(ImageUrl)
				);
			}
		}

		private static string squeeze(this string str)
			=> string.Join(' ', str.Split(null as string, StringSplitOptions.RemoveEmptyEntries));

		public static async Task<IEnumerable<Essen>> GetEssenAsync()
		{
			var resp = new HttpClient().GetAsync(api).Result.Content;

			try
			{
				using(var r = await resp.ReadAsStreamAsync())
				using(var tr = new StreamReader(r))
				using(var jr = new JsonTextReader(tr))
				{
					return new JsonSerializer().Deserialize<Essen[]>(jr) ?? throw new ArgumentNullException("API returned null");
				}
			}
			catch(Exception ex) when (ex is JsonException || ex is FormatException)
			{
				throw new FormatException($"Expected valid JSON, got '{await resp.ReadAsStringAsync()}'", ex);
			}

		}
	}
}