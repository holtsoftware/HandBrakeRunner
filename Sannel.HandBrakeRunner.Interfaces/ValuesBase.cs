using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	/// <summary>
	/// The base class for all Value classes like Configuration, Disk and Track
	/// </summary>
	public abstract class ValuesBase
	{
		private ILog log = LogManager.GetLogger(typeof(ValuesBase));
		private Dictionary<String, PropertyMetaData> values = new Dictionary<string, PropertyMetaData>();

		public abstract PropertyMetaData this[String key]
		{
			get;
		}

		/// <summary>
		/// The Collection of Values associated with this object.
		/// </summary>
		protected Dictionary<String, PropertyMetaData> Values
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
		public static String NormalizeKey(String key)
		{
			return key.ToUpper(CultureInfo.InvariantCulture).Trim();
		}

		/// <summary>
		/// Adds <paramref name="name"/> into values but first makes it all uppercase and trims it.
		/// </summary>
		/// <param name="name">The name of the value</param>
		/// <param name="value">The value to set it to</param>
		protected void SetValue(String name, String value, String fileDirectory)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("An empty name was received");
				}
				return;
			}

			if(String.IsNullOrWhiteSpace(fileDirectory))
			{
				fileDirectory = Environment.CurrentDirectory;
			}

			Values[NormalizeKey(name)] = new PropertyMetaData
			{
				Value = value,
				FullPath = fileDirectory
			};
		}

		protected PropertyMetaData ResolveFormatAndMethods(PropertyMetaData value)
		{
			if (value == null)
			{
				return null;
			}
			String fixedValue = VariableReferenceRegex.Replace(value, VariableMatch);
			fixedValue = MethodReferenceRegex.Replace(fixedValue, MethodMatch);
			return new PropertyMetaData
				{
					Value = fixedValue,
					FullPath = value.FullPath
				};
		}

		protected virtual String MethodMatch(Match m)
		{
			String method = m.Groups["Method"].Value;
			String param = m.Groups["Params"].Value;

			if (!String.IsNullOrWhiteSpace(method))
			{
				return Methods.FindAndExecuteMethod(method, this, param);
			}

			return m.Value;
		}

		protected virtual String VariableMatch(Match m)
		{
			var key = m.Groups["Key"].Value;
			var format = m.Groups["Format"].Value;

			if (!String.IsNullOrWhiteSpace(key))
			{
				String value = this[key];
				if (!String.IsNullOrWhiteSpace(format))
				{
					if (format.IndexOf('.') > -1)
					{
						double dvalue;
						if (double.TryParse(value, out dvalue))
						{
							return dvalue.ToString(format, CultureInfo.InvariantCulture);
						}
					}
					else
					{
						long lvalue;
						if (long.TryParse(value, out lvalue))
						{
							return lvalue.ToString(format, CultureInfo.InvariantCulture);
						}
					}

					return value;
				}
				else
				{
					return value;
				}
			}

			return m.Value;
		}

		public readonly static Regex VariableReferenceRegex = new Regex("\\${(?<Key>[a-zA-Z0-9-_]*)(:(?<Format>[0]+[.]?[0]+))?}",
								RegexOptions.IgnoreCase
								| RegexOptions.CultureInvariant
								| RegexOptions.IgnorePatternWhitespace
								| RegexOptions.Compiled
								);
		public readonly static Regex MethodReferenceRegex = new Regex("\\%{(?<Method>[a-zA-Z0-9-_]+)\\((?<Params>.+)\\)}",
								RegexOptions.IgnoreCase
								| RegexOptions.CultureInvariant
								| RegexOptions.IgnorePatternWhitespace
								| RegexOptions.Compiled
								);
	}
}
