using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using CommandLine;

namespace ISM_CSE
{
	class Program
	{
		class Options
		{
			[Option('c', "count", Default = 10, HelpText = "Number of search results.", MetaValue = "number")]
			public int Count { get; set; }
			[Value(0, Required = true, HelpText = "Term to search for.", MetaName = "search term")]
			public IEnumerable<string> SearchTerm { get; set; }
		}

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			var parsedArgs = Parser.Default.ParseArguments<Options>(args);
			parsedArgs
				.WithParsed(options => AsyncMain(options).Wait());
#if DEBUG
			Console.ReadLine();
#endif
		}

		static async Task AsyncMain(Options options)
		{
			var config = ReadConfig();
			if (config == null) return;
			var searchService = new CustomsearchService(new BaseClientService.Initializer()
			{
				ApplicationName = "ISM-CSE",
				ApiKey = config.APIKey
			});
			try
			{
				var items = await RequestResults(options.Count);
				Console.WriteLine(
					String.Join("",
					Enumerable.Range(0, items.Count())
						.Zip(items, (idx, item) => (index: idx, item: item))
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

			async Task<IEnumerable<Result>> RequestResults(int count, int start = 1)
			{
				const int maxNum = 10;
				var request = searchService.Cse.List(String.Join(" ", options.SearchTerm));
				request.Cx = config.SearchEngineID;
				request.Num = Math.Min(count, maxNum);
				request.Start = start;
				var response = await request.ExecuteAsync();
				if (count <= maxNum || response.Items.Count < response.Items.Count)
				{
					return response.Items;
				}
				else
				{
					return response.Items.Concat(await RequestResults(count - maxNum, start + response.Items.Count));
				}
			}
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
