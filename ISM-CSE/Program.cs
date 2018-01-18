using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

namespace ISM_CSE
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			var config = ReadConfig();
			if (config == null) return;
			if (args.Length == 0)
			{
				Console.WriteLine($"Usage: {Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0])} <search term>");
				return;
			}
			Task.Run(async () =>
			{
				var searchService = new CustomsearchService(new BaseClientService.Initializer()
				{
					ApplicationName = "ISM-CSE",
					ApiKey = config.APIKey
				});
				var request = searchService.Cse.List(String.Join(" ", args));
				request.Cx = config.SearchEngineID;
				request.Num = 10;
				try
				{
					var response = await request.ExecuteAsync();
					Console.WriteLine(
						String.Join("",
						Enumerable.Range(0, response.Items.Count)
							.Select(i => (index: i, item: response.Items[i]))
							.Aggregate("",
								(text, t) => text + $"Search result {t.index + 1}: {t.item.Title} <{t.item.Link}>\n{t.item.Snippet}\n\n")
							.Reverse()
							.SkipWhile(c => c == '\n')
							.Reverse()));
				}
				catch (Google.GoogleApiException ex)
				{
					Console.WriteLine($"API call failed. Reason: {ex.Message}");
				}
			}).Wait();
#if DEBUG
			Console.ReadLine();
#endif
		}

		static Config ReadConfig()
		{
			try
			{
				return new ConfigurationBuilder()
					.AddJsonFile("config.json")
					.Build()
					.Get<Config>();
			}
			catch (FileNotFoundException ex) when (ex.FileName == "config.json")
			{
				Console.WriteLine("No configuration available. Please make sure that config.json is available, see config.example.json for the structure.");
			}
			return null;
		}
	}
}
