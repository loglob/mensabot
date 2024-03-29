
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace mensabot
{
	static class Menu
	{
		private const string api = "https://www.mensa-kl.de/api.php?date=1&format=json";

		[JsonObject(MemberSerialization.OptIn)]
		public class Essen
		{
			public readonly int Votes;

			[JsonProperty("title")]
			public readonly string Ausgabe;

			[JsonProperty("description")]
			public readonly string Name;
			public readonly string Image;
			public readonly double Preis;
			public readonly double Rating;

			public string Stars
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

			private EmbedField für => ("für", Preis.ToString("0.00€"));

			[JsonProperty("fields")]
			public EmbedField[] EmbedFields
				=> Votes > 0 ? new EmbedField[] {
					für,
					("rating", Stars),
					("votes", Votes)
				} : new EmbedField[] {
					für
				};

			[JsonProperty("image", NullValueHandling=NullValueHandling.Ignore)]
			public EmbedImage EmbedImage
				=> string.IsNullOrEmpty(Image) ? null : new EmbedImage{url = Image};

			[JsonConstructor]
			public Essen(string title, string price, string rating, string rating_amt, string image, string loc)
			{
				this.Name = title.squeeze();
				double.TryParse(price, out this.Preis);
				double.TryParse(rating, out this.Rating);
				int.TryParse(rating_amt, out this.Votes);
				this.Image = string.IsNullOrEmpty(image) ? null : $"https://www.mensa-kl.de/mimg/{image}";
				this.Ausgabe = int.TryParse(loc, out int a) ? $"Ausgabe {a}" : loc;
			}

			public override string ToString()
				=> $"> {Name} für {Preis.ToString("0.00")}€ an {Ausgabe}\n> {Stars} (aus {Votes} Stimmen)\n";
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
					return new JsonSerializer().Deserialize<Essen[]>(jr);
				}
			}
			catch(Exception ex) when (ex is JsonException || ex is FormatException)
			{
				throw new FormatException($"Expected valid JSON, got '{await resp.ReadAsStringAsync()}'", ex);
			}

		}
	}
}