
/*
Copyright 2013 Sannel Software, L.L.C.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/
using System;
using System.Linq;
using log4net;
using Sannel.Helpers;
using System.Reflection;

namespace Sannel.HandBrakeRunner
{
	class Program
	{
		static ILog log = LogManager.GetLogger(typeof(Program));

		static void changeLogLevel(String level)
		{
			log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
			log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
			rootLogger.Level = h.LevelMap[level];
			log.WarnFormat("Changing log level to {0}", level);
		}

		static void printVersion()
		{
			Console.WriteLine("hbrunner version {0}", Assembly.GetCallingAssembly().GetAssemblyVersion());
		}

		static void printHelp()
		{
			Console.WriteLine("usage: hbrunner [options] file");
			Console.WriteLine("  --version\t\tPrints version information");
			Console.WriteLine("  --v\t\tUps the log level to Warn");
			Console.WriteLine("  --vv\t\tUps the log level to Info");
			Console.WriteLine("  --vvv\t\tUps the log level to Debug");
			/*Console.WriteLine("  --print-tracks\t\tPrints track information from file");
			Console.WriteLine("  --tracks=(Number)\tRiponly the tracks specified this can be a , separated list");
			Console.WriteLine("  --no-metadata\t\tDo not set metadata");
			Console.WriteLine("  --metadata-only\tDon't encode just write metadata");
			Console.WriteLine("  --config=(configfile)\tA custom config file to override values in all other configs");*/
		}

		static int Main(String[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			Arguments arguments = new Arguments();
			arguments.Parse(args);

			if (arguments.HasArgument("vvv"))
			{
				changeLogLevel("DEBUG");
#if DEBUG
				log.Fatal("Fatal Color");
				log.Error("Error Color");
				log.Warn("Warn Color");
				log.Info("Info Color");
				log.Debug("Debug Color");
#endif
			}
			else if (arguments.HasArgument("vv"))
			{
				changeLogLevel("INFO");
			}
			else if (arguments.HasArgument("v"))
			{
				changeLogLevel("WARN");
			}

			if (log.IsInfoEnabled)
			{
				log.InfoFormat("Version {0}", Assembly.GetCallingAssembly().GetAssemblyVersion());
			}

			if (log.IsDebugEnabled)
			{
				foreach (var key in arguments.ArgumentValues.Keys)
				{
					if (arguments.ArgumentValues[key] == null)
					{
						log.DebugFormat("Argument --{0} as passed", key);
					}
					else
					{
						log.DebugFormat("Argument --{0}={1} as passed", key, arguments.ArgumentValues[key]);
					}
				}

				foreach (var item in arguments.NonArgumentValues)
				{
					log.DebugFormat("Argument {0} passed", item);
				}
			}

			if (arguments.HasArgument("version"))
			{
				printVersion();
				return 0;
			}

			if (arguments.HasArgument("help"))
			{
				printHelp();
				return 0;
			}

			return 0;
		}
	}
}
