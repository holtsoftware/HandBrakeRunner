using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBreakRunner.Plugins
{
	public static class EnvironmentHelpers
	{
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
