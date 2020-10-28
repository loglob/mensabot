using System;
using System.Threading.Tasks;
using System.Linq;

namespace mensabot
{
    class Program
    {
		static async Task MainAsync()
		{
			await Discord.SendEmbed("**Morgen gibt es:**", await Menu.GetEssenAsync());
		}

        static void Main(string[] args)
        {
			MainAsync().GetAwaiter().GetResult();
        }
    }
}
