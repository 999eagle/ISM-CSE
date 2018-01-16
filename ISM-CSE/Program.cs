using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ISM_CSE
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = ReadConfig();
			if (config == null) return;
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
