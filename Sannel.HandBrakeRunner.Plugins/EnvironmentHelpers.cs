using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Plugins
{
	public static class EnvironmentHelpers
	{
		public static String GetTempFile(String extention)
		{
			var tmpFile = Path.GetTempFileName();
			var dir = Path.GetDirectoryName(tmpFile);
			var file = Path.Combine(dir, Path.GetFileNameWithoutExtension(tmpFile) + extention);

			return file;
		}

		/// <summary>
		/// Right now this returns Path.GetTempPath but if i have issues with that in the future i will be adding more logic.
		/// </summary>
		/// <returns></returns>
		public static String GetTempDir()
		{
			return Path.GetTempPath();
		}

		public static String GetHomeDir()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				return Environment.GetEnvironmentVariable("HOME");
			}
			else
			{
				return Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
			}
		}

		public static String GetBaseConfigFilePath()
		{
			return Path.Combine(GetHomeDir(), ".hbrunner", "Config.xml");
		}
	}
}
