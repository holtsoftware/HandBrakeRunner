
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
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sannel.Helpers;
using Sannel.HandBrakeRunner.Interfaces;
using System.Runtime.Remoting;
using System.IO;

namespace Sannel.HandBrakeRunner
{
	public class Runner
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Runner));
		internal static Arguments Args;

		public Runner()
		{

		}

		protected int[] ParseTracks(String trackValue)
		{
			List<int> tracks = new List<int>();
			var splits = trackValue.Split(',');
			int tmp = 0;
			for (int i = 0; i < splits.Length; i++)
			{
				String segment = splits[i];
				if (segment.IndexOf('-') > -1)
				{
					var ss = segment.Split('-');
					if (ss.Length != 2)
					{
						if (log.IsErrorEnabled)
						{
							log.ErrorFormat("The argument passed to --tracks is invalid. Invalid segment '{0}'.", segment);
						}
						return null;
					}

					int start = 0, end = 0;
					if (!int.TryParse(ss[0], out start))
					{
						if (log.IsErrorEnabled)
						{
							log.ErrorFormat("The argument passed to --tracks is invalid. Invalid segment '{0}'.", segment);
						}
						return null;
					}

					if (!int.TryParse(ss[1], out end))
					{
						if (log.IsErrorEnabled)
						{
							log.ErrorFormat("The argument passed to --tracks is invalid. Invalid segment '{0}'.", segment);
						}
						return null;
					}

					if (end < start)
					{
						if (log.IsErrorEnabled)
						{
							log.ErrorFormat("The argument passed to --tracks is invalid. Start is greater then end in range '{0}'.", segment);
						}

						return null;
					}
					for (int j = start; j <= end; j++)
					{
						tracks.Add(j);
					}

				}
				else
				{
					if (!int.TryParse(segment, out tmp))
					{
						if (log.IsErrorEnabled)
						{
							log.ErrorFormat("The argument passed to --tracks is invalid. Invalid segment '{0}'", segment);
						}
						return null;
					}

					tracks.Add(tmp);
				}
			}

			return tracks.OrderBy(i => i).Distinct().ToArray();
		}

		public async Task<int> RunAsync(Arguments args)
		{
			Runner.Args = args;
			if (args.HasArgument("vvv"))
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
			else if (args.HasArgument("vv"))
			{
				changeLogLevel("INFO");
			}
			else if (args.HasArgument("v"))
			{
				changeLogLevel("WARN");
			}

			if (log.IsInfoEnabled)
			{
				log.InfoFormat("Version {0}", Assembly.GetCallingAssembly().GetAssemblyVersion());
			}

			if (log.IsDebugEnabled)
			{
				foreach (var key in args.ArgumentValues.Keys)
				{
					if (args.ArgumentValues[key] == null)
					{
						log.DebugFormat("Argument --{0} as passed", key);
					}
					else
					{
						log.DebugFormat("Argument --{0}={1} as passed", key, args.ArgumentValues[key]);
					}
				}

				foreach (var item in args.NonArgumentValues)
				{
					log.DebugFormat("Argument {0} passed", item);
				}
			}

			if (args.HasArgument("version"))
			{
				printVersion();
				return 0;
			}

			if (args.HasArgument("help"))
			{
				printHelp();
				return 0;
			}

			if (args.NonArgumentValues.Count < 1)
			{
				printHelp();
				return 1;
			}

			int[] tracks = new int[0];
			if (args.HasArgument("tracks") && !String.IsNullOrWhiteSpace(args.ArgumentValues["tracks"]))
			{
				tracks = ParseTracks(args.ArgumentValues["tracks"]);
				if (tracks == null)
				{
					return 1;
				}
			}

			return (await beginAsync(args.NonArgumentValues[0], tracks)) ? 1 : 0;
		}

		private async Task<IEncoder> getEncoderAsync(ITrack track)
		{
			var encoder = await track.GetValueAsync("Encoder");
			if (String.IsNullOrWhiteSpace(encoder))
			{
				if (log.IsFatalEnabled)
				{
					log.Fatal("No valid Encoder property found quitting");
				}

				return null;
			}

			var encoderSplit = encoder.Value.Split(',');
			if (encoderSplit.Length < 2)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("Encoder is in the incorrect formation it should be '<FullTypeName>, <FullAssemblyName>' instead '{0}' was received.", encoder);
				}

				return null;
			}
			ObjectHandle encoderBox = null;
			try
			{
				encoderBox = Activator.CreateInstance(encoderSplit[1], encoderSplit[0]);
			}
			catch (Exception ex)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("An exception was thrown while trying to create an encoder of type '{0}'. Exception: {1}", encoder, ex);
				}
				return null;
			}

			if (encoderBox == null)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("Unable to create an encoder of type '{0}'.", encoder);
				}

				return null;
			}

			IEncoder e = encoderBox.Unwrap() as IEncoder;
			if (e == null)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("Type of {0} does not implement IEncoder.", encoderBox.Unwrap().GetType());
				}
				return null;
			}

			return e;
		}

		private async Task<IMetaData> getMetaDataAsync(ITrack track)
		{
			var metaDataType = await track.GetValueAsync("MetaData");
			if (String.IsNullOrWhiteSpace(metaDataType))
			{
				if (log.IsFatalEnabled)
				{
					log.Fatal("No valid MetaData property found quitting");
				}

				return null;
			}

			var metaDataSplit = metaDataType.Value.Split(',');
			if (metaDataSplit.Length < 2)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("MetaData is in the incorrect formation it should be '<FullTypeName>, <FullAssemblyName>' instead '{0}' was received.", metaDataType);
				}

				return null;
			}
			ObjectHandle metaDataBox = null;
			try
			{
				metaDataBox = Activator.CreateInstance(metaDataSplit[1], metaDataSplit[0]);
			}
			catch (Exception ex)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("An exception was thrown while trying to create an IMetaData of type '{0}'. Exception: {1}", metaDataType, ex);
				}
				return null;
			}

			if (metaDataBox == null)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("Unable to create an IMetaData of type '{0}'.", metaDataType);
				}

				return null;
			}

			IMetaData md = metaDataBox.Unwrap() as IMetaData;
			if (md == null)
			{
				if (log.IsFatalEnabled)
				{
					log.FatalFormat("Type of {0} does not implement IMetaData.", metaDataBox.Unwrap().GetType());
				}
				return null;
			}

			return md;
		}

		private async Task<bool> beginAsync(String xmlFile, int[] tracks)
		{
			await Methods.LoadVariableMethods();
			Disk disk = new Disk();
			var results = await disk.LoadAsync(xmlFile);

			if (!results)
			{
				return false;
			}

			for (int i = 0; i < disk.Tracks.Count; i++)
			{
				if (tracks.Length == 0 || tracks.Contains(i + 1))
				{
					var track = disk.Tracks[i];

					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Using track {0}", i + 1);
					}

					IEncoder encoder = await getEncoderAsync(track);
					if (encoder == null)
					{
						return false;
					}
					IMetaData metaData = await getMetaDataAsync(track);
					if (metaData == null)
					{
						return false;
					}
					var orgTempFile = Path.GetTempFileName();

					if (File.Exists(orgTempFile))
					{
						File.Delete(orgTempFile);
					}

					var tmpFile = Path.ChangeExtension(orgTempFile, encoder.GetFileExt(track));
					var destDirProp = await track.GetValueAsync("DestinationDirectory");
					if (destDirProp == null)
					{
						if (log.IsFatalEnabled)
						{
							log.Fatal("DestinationDirectory Property is not set.");
						}
						return false;
					}
					String destinationDirectory;
					if (Path.IsPathRooted(destDirProp))
					{
						destinationDirectory = destDirProp;
					}
					else
					{
						destinationDirectory = Path.GetFullPath(Path.Combine(destDirProp.FullPath, destDirProp.Value));
					}

					var destFileProp = await track.GetValueAsync("DestinationFile");
					if (destFileProp == null)
					{
						if (log.IsFatalEnabled)
						{
							log.Fatal("DestinationFile Property is not set.");
						}

						return false;
					}

					String fullDestinationFilePath = Path.GetFullPath(Path.Combine(destinationDirectory, String.Format("{0}{1}", destFileProp.Value, metaData.GetFileExt(track))));

					if (!Directory.Exists(fullDestinationFilePath))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullDestinationFilePath));
					}

					bool rvalue;

					try
					{
						rvalue = await encoder.RunAsync(track, tmpFile);
						if (rvalue == false)
						{
							if (log.IsFatalEnabled)
							{
								log.Fatal("An error accrued while encoding exiting.");
							}
							return false;
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("Exception was through while executing encoder.RunAsync", e);
						}
					}

					try
					{
						rvalue = await metaData.RunAsync(track, tmpFile, fullDestinationFilePath);
						if (rvalue == false)
						{
							if(Args.HasArgument("continue-meta-error") || Args.HasArgument("cme"))
							{
								File.Move(tmpFile, fullDestinationFilePath); 
							}
							else if (log.IsFatalEnabled)
							{
								log.Fatal("An error accrued while applying metadata exiting.");
							}
							return false;
						}
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("Exception was through while executing metaData.RunAsync", e);
						}
					}

					if (File.Exists(tmpFile))
					{
						File.Delete(tmpFile);
					}
				}
				else
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Not running track {0}", i + 1);
					}
				}
			}

			return true;
		}

		private void changeLogLevel(String level)
		{
			log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
			log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
			rootLogger.Level = h.LevelMap[level];
			log.WarnFormat("Changing log level to {0}", level);
		}

		private void printVersion()
		{
			Console.WriteLine("hbrunner version {0}", Assembly.GetCallingAssembly().GetAssemblyVersion());
		}

		private void printHelp()
		{
			Console.WriteLine("usage: hbrunner [options] file");
			Console.WriteLine("  --version\t\tPrints version information");
			Console.WriteLine("  --v\t\tUps the log level to Warn");
			Console.WriteLine("  --vv\t\tUps the log level to Info");
			Console.WriteLine("  --vvv\t\tUps the log level to Debug");
			Console.WriteLine("  --tracks=(Range)\t\tThe Track number(s) comma separated and/or Track Range. i.e. 1,3,5-9");
			Console.WriteLine("  --continue-meta-error(-cme)\t\tContinue on metadata error.");
			/*Console.WriteLine("  --print-tracks\t\tPrints track information from file");
			Console.WriteLine("  --no-metadata\t\tDo not set metadata");
			Console.WriteLine("  --metadata-only\tDon't encode just write metadata");
			Console.WriteLine("  --config=(configfile)\tA custom config file to override values in all other configs");*/
		}
	}
}
