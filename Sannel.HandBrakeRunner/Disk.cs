using log4net;
using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Sannel.HandBrakeRunner
{
	public class Disk : ValuesBase, IDisk
	{
		private ILog log = LogManager.GetLogger(typeof(Disk));
		private List<ITrack> tracks = new List<ITrack>();

		public IReadOnlyList<ITrack> Tracks
		{
			get
			{
				return tracks;
			}
		}

		public virtual async Task<bool> LoadAsync(String fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			if (!File.Exists(fileName))
			{
				if (log.IsErrorEnabled)
				{
					log.ErrorFormat("The file {0} was not found.", Path.GetFullPath(fileName));
				}

				return false;
			}

			var diskDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName));

			XDocument document = null;

			var loaded = await Task.Run<bool>(() =>
			{
				var fullPath = Path.GetFullPath(fileName);
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("Attempting to load file {0}", fullPath);
				}

				try
				{
					document = XDocument.Load(Path.GetFullPath(fileName));
				}
				catch (SecurityException se)
				{
					if (log.IsErrorEnabled)
					{
						log.ErrorFormat("Access to file {0} is denied. SecurityException: {1}", fullPath, se);
					}
					return false;
				}
				catch (XmlException xe)
				{
					if (log.IsErrorEnabled)
					{
						log.ErrorFormat("Loading file {0} through an XmlException exception {1}", fullPath, xe);
					}
					return false;
				}
				catch (FormatException fe)
				{
					if (log.IsErrorEnabled)
					{
						log.ErrorFormat("Loading file {0} through a format exception {1}", fullPath, fe);
					}
					return false;
				}
				return true;
			});

			if (!loaded || document == null)
			{
				return false;
			}

			if (document.Root == null || String.Compare(document.Root.Name.ToString(), "Disk", false, CultureInfo.InvariantCulture) != 0)
			{
				if (log.IsErrorEnabled)
				{
					log.ErrorFormat("Invalid Root detected in Disk File {0}.", fileName);
				}

				return false;
			}

			var root = document.Root;

			var config = root.Attribute("config");
			if (config != null)
			{
				String configValue = config.Value;

				if (String.IsNullOrWhiteSpace(configValue))
				{
					if (log.IsErrorEnabled)
					{
						log.Error("config attribute does not have any value.");
					}
				}
				else
				{
					String dir = Path.GetDirectoryName(Path.GetFullPath(fileName));
					String includeFile = Path.GetFullPath(Path.Combine(dir, configValue));
					if (log.IsInfoEnabled)
					{
						log.InfoFormat("Loading included file {0}", includeFile);
					}

					IConfiguration iConfig = CreateConfiguration();
					var results = await iConfig.LoadAsync(includeFile);

					if (!results)
					{
						if (log.IsWarnEnabled)
						{
							log.WarnFormat("Error loading included config file {0}", includeFile);
						}
					}
					else
					{
						Configuration = iConfig;
					}
				}
			}

			foreach (var attribute in root.Attributes())
			{
				if (attribute.NodeType == System.Xml.XmlNodeType.Attribute)
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Found Attribute. Name: {0} Value: {1}", attribute.Name, attribute.Value);
					}

					if (String.Compare(attribute.Name.ToString(), "template", true, CultureInfo.InvariantCulture) == 0)
					{
						String template = GetFullRelativePath(attribute.Value, fileName);
						if (log.IsInfoEnabled)
						{
							log.InfoFormat("Template included full path {0}", template);
						}
						SetValue(attribute.Name.ToString(), template, diskDirectory);
					}
					else
					{
						SetValue(attribute.Name.ToString(), attribute.Value, diskDirectory);
					}
				}
				else if (log.IsDebugEnabled)
				{
					log.DebugFormat("Node Type {0} is not an Attribute", attribute.NodeType);
				}
			}

			String fullpath = Path.GetFullPath(fileName);

			foreach (var element in document.Root.Elements())
			{
				if (element.NodeType == System.Xml.XmlNodeType.Element)
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Found Node. Name: {0} Value: {1}", element.Name, element.Value);
					}
					if (String.Compare(element.Name.ToString(), "track", true, CultureInfo.InvariantCulture) == 0)
					{
						ITrack track = CreateTrack();
						track.Disk = this;
						bool trackLoadValue = await track.LoadTrackAsync(element, fullpath);
						if (!trackLoadValue)
						{
							if (log.IsErrorEnabled)
							{
								log.Error("There was an error loading the track or template.");
							}
						}
						tracks.Add(track);
					}
					else if (String.Compare(element.Name.ToString(), "template", true, CultureInfo.InvariantCulture) == 0)
					{
						String template = GetFullRelativePath(element.Value, fileName);
						if (log.IsInfoEnabled)
						{
							log.InfoFormat("Template included full path {0}", template);
						}
						SetValue(element.Name.ToString(), template, diskDirectory);
					}
					else
					{
						SetValue(element.Name.ToString(), element.Value, diskDirectory);
					}
				}
				else if (log.IsDebugEnabled)
				{
					log.DebugFormat("Node Type {0} is not an Element", element.NodeType);
				}
			}

			return true;
		}

		protected virtual IConfiguration CreateConfiguration()
		{
			return new Configuration();
		}

		protected virtual ITrack CreateTrack()
		{
			return new Track();
		}

		protected IConfiguration Configuration
		{
			get;
			set;
		}

		/// <summary>
		/// Returns the value associated with <paramref name="key"/> or null if its not found.
		/// </summary>
		/// <param name="key">The key associated with the desired value.</param>
		/// <returns></returns>
		public override PropertyMetaData this[string key]
		{
			get 
			{
				var fixedKey = NormalizeKey(key);

				if (Values.ContainsKey(fixedKey))
				{
					return Values[fixedKey];
				}

				if (Configuration != null)
				{
					return Configuration[fixedKey];
				}

				return null;
			}
		}

		/// <summary>
		/// Returns the value associated with <paramref name="key"/> or null if its not found.
		/// </summary>
		/// <param name="key">The key associated with the desired value.</param>
		/// <returns></returns>
		public virtual Task<PropertyMetaData> GetValueAsync(string key)
		{
			return Task.Run<PropertyMetaData>(() =>
				{
					return this[key];
				});
		}
	}
}
