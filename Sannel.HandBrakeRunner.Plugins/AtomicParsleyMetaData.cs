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
	public class AtomicParsleyMetaData : IMetaData
	{
		private static ILog log = LogManager.GetLogger(typeof(AtomicParsleyMetaData));

		public String GetFileExt(ITrack track)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			var value = track["apfileext"];
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

		protected virtual async Task<String> GetArgumentsAsync(ITrack track, String sourceFile, String destinationFile)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			if (sourceFile == null)
			{
				throw new ArgumentNullException("sourceFile");
			}

			if (destinationFile == null)
			{
				throw new ArgumentNullException("destinationFile");
			}

			StringBuilder arguments = new StringBuilder();
			arguments.AppendFormat("\"{0}\" --output \"{1}\" ", Methods.FixString(Methods.ChangeDirectorySeparator(sourceFile, '/')), Methods.FixString(Methods.ChangeDirectorySeparator(destinationFile, '/')));

			PropertyMetaData value = await track.GetValueAsync("ShowName");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--TVShowName \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("MediaKind");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--stik \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Genre");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--genre \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Image");
			if (!String.IsNullOrWhiteSpace(value))
			{
				String imagePath = value.Value;
				if (!Path.IsPathRooted(value.Value))
				{
					imagePath = Path.GetFullPath(Path.Combine(value.FullPath, value.Value));
				}
				arguments.AppendFormat("--artwork \"{0}\" ", Methods.FixString(Methods.ChangeDirectorySeparator(imagePath, '/')));
			}

			value = await track.GetValueAsync("Title");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--title \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Description");
			if (!String.IsNullOrWhiteSpace(value))
			{
				if (value.Value.Length > 255)
				{
					value = value.Value.Substring(0, 252) + "...";
				}
				arguments.AppendFormat("--description \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Album");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--album \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("TrackNumber");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--tracknum \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("DiskNumber");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--disk \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("EID");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--TVEpisode \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Episode");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--TVEpisodeNum \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Season");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--TVSeasonNum \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Year");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--year \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("Artist");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--artist \"{0}\" ", Methods.FixString(value));
			}

			value = await track.GetValueAsync("AlbumArtist");
			if (!String.IsNullOrWhiteSpace(value))
			{
				arguments.AppendFormat("--composer \"{0}\" ", Methods.FixString(value));
			}

			return arguments.ToString();
		}

		public async Task<bool> RunAsync(ITrack track, string tmpFilePath, string destinationFile)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			if (tmpFilePath == null)
			{
				throw new ArgumentNullException("tmpFilePath");
			}

			String atomicparsly = "AtomicParsley";

			String value = track["AtomicParsley"];
			if (!String.IsNullOrWhiteSpace(value))
			{
				atomicparsly = value;
			}

			String args = await GetArgumentsAsync(track, tmpFilePath, destinationFile);
			int rvalue = await Methods.ExecuteProgramAsync(atomicparsly, args, false);

			return rvalue == 0;
		}
	}
}
