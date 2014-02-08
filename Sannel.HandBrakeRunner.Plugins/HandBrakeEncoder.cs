using log4net;
using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Plugins
{
	public class HandBrakeEncoder : IEncoder
	{
		private static ILog log = LogManager.GetLogger(typeof(HandBrakeEncoder));

		public String GetFileExt(ITrack track)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			var value = track["hbfileext"];
			if (value != null)
			{
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("HandBrake File Extension {0}", value);
				}
				return value;
			}

			value = track["fileext"];
			if (value != null)
			{
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("HandBrake File Extension {0}", value);
				}
				return value;
			}

			value = track["filename"];
			if (value != null)
			{
				FileInfo fi = new FileInfo(value);
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("HandBrake File Extension {0}", fi.Extension);
				}
				return fi.Extension;
			}

			if (log.IsDebugEnabled)
			{
				log.Debug("HandBrake File Extension .mp4");
			}
			return ".mp4";
		}

		protected String GenerateArguments(String tmpFilePath, ITrack track)
		{
			if (tmpFilePath == null)
			{
				throw new ArgumentNullException("tmpFilePath");
			}

			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			StringBuilder builder = new StringBuilder();

			PropertyMetaData prop;

			if (track.Args.HasArgument("inputpath"))
			{
				builder.AppendFormat("-i \"{0}\" ", track.Args.ArgumentValue("inputpath"));
			}
			else
			{
				prop = track["INPUTPATH"];
				if (prop != null)
				{
					builder.AppendFormat("-i \"{0}\" ", prop);
				}
			}

			prop = track["TitleChapter"];
			if (prop != null)
			{
				builder.AppendFormat("{0} ", prop);
			}

			builder.AppendFormat("-o \"{0}\" ", tmpFilePath);

			prop = track["HANDBRAKEOPTIONS"];
			if (prop != null)
			{
				builder.Append(prop);
			}

			if (log.IsDebugEnabled)
			{
				log.DebugFormat("HandBrake Options {0}", builder);
			}

			return builder.ToString();
		}

		public async Task<bool> RunAsync(ITrack track, string tmpFilePath)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			if (tmpFilePath == null)
			{
				throw new ArgumentNullException("tmpFilePath");
			}

			String handbrakeclr = "HandBrakeCLI";

			String value = track["HandBrakeCLI"];
			if(!String.IsNullOrWhiteSpace(value))
			{
				handbrakeclr = value;
			}

			String args = GenerateArguments(tmpFilePath, track);
			int rvalue = await Methods.ExecuteProgramAsync(handbrakeclr, args, false);

			return rvalue == 0;
		}
	}
}
