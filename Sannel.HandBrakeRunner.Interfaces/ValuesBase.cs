using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	/// <summary>
	/// The base class for all Value classes like Configuration, Disk and Track
	/// </summary>
	public abstract class ValuesBase
	{
		private ILog log = LogManager.GetLogger(typeof(ValuesBase));

		private Dictionary<String, String> values = new Dictionary<string, string>();

		/// <summary>
		/// The Collection of Values associated with this object.
		/// </summary>
		protected Dictionary<String, string> Values
		{
			get
			{
				return values;
			}
		}

		/// <summary>
		/// Gets the full path of <paramref name="destFile"/> based on the full path of <paramref name="relativeFile"/>.
		/// i.e. if <paramref name="relativeFile"/> is at the path C:\Temp\includes\Config.xml and the path of <paramref name="destFile"/> is ..\Global.xml
		/// this method would return c:\Temp\Global.xml
		/// </summary>
		/// <param name="destFile"></param>
		/// <param name="relativeFile"></param>
		/// <returns></returns>
		protected String GetFullRelativePath(String destFile, String relativeFile)
		{
			String dir = Path.GetDirectoryName(Path.GetFullPath(relativeFile));
			return Path.GetFullPath(Path.Combine(dir, destFile));
		}

		/// <summary>
		/// Converts the given <paramref name="key"/> to uppercase and trims it.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected virtual String NormalizeKey(String key)
		{
			return key.ToUpper(CultureInfo.InvariantCulture).Trim();
		}

		/// <summary>
		/// Adds <paramref name="name"/> into values but first makes it all uppercase and trims it.
		/// </summary>
		/// <param name="name">The name of the value</param>
		/// <param name="value">The value to set it to</param>
		protected void SetValue(String name, String value)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("An empty name was received");
				}
				return;
			}

			Values[NormalizeKey(name)] = value;
		}
	}
}
