﻿using log4net;
using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sannel.HandBrakeRunner
{
	public class Track : ValuesBase, ITrack
	{
		private ILog log = LogManager.GetLogger(typeof(Track));

		public Arguments Args
		{
			get;
			set;
		}

		public Task<bool> LoadTrackAsync(System.Xml.Linq.XElement track, String diskFullPath)
		{
			return Task.Run<bool>(() =>
				{
					return LoadTrack(track, diskFullPath);
				});
		}

		public IDisk Disk
		{
			get;
			set;
		}

		protected virtual bool LoadTrack(XElement track, String diskFullPath)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			if (diskFullPath == null)
			{
				throw new ArgumentNullException("diskFullPath");
			}

			String diskDirectory = Path.GetDirectoryName(Path.GetFullPath(diskFullPath));

			var templateAttribute = track.Attribute("template");
			if (templateAttribute != null)
			{
				String templateFile = templateAttribute.Value;
				if (String.IsNullOrWhiteSpace(templateFile))
				{
					if (log.IsErrorEnabled)
					{
						log.Error("template attribute on Track Node was not set to a value.");
					}
				}
				else
				{
					var loaded = LoadTemplate(GetFullRelativePath(templateFile, diskFullPath));
					if (!loaded)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("Error loading template");
						}
					}
				}
			}
			else
			{
				var templateFile = this["template"];
				if (!String.IsNullOrWhiteSpace(templateFile))
				{
					var loaded = LoadTemplate(GetFullRelativePath(templateFile, diskFullPath));
					if (!loaded)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("Error loading template");
						}
					}
				}
			}

			foreach (var attribute in track.Attributes())
			{
				if (attribute.NodeType == System.Xml.XmlNodeType.Attribute)
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Found Attribute. Name: {0} Value: {1}", attribute.Name, attribute.Value);
					}

					SetValue(attribute.Name.ToString(), attribute.Value, diskDirectory);
				}
				else if (log.IsDebugEnabled)
				{
					log.DebugFormat("Node Type {0} is not an Attribute", attribute.NodeType);
				}
			}

			foreach (var element in track.Elements())
			{
				if (element.NodeType == System.Xml.XmlNodeType.Element)
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Found Node. Name: {0} Value: {1}", element.Name, element.Value);
					}

					SetValue(element.Name.ToString(), element.Value, diskDirectory);

				}
				else if (log.IsDebugEnabled)
				{
					log.DebugFormat("Node Type {0} is not an Element", element.NodeType);
				}
			}

			return true;
		}

		protected virtual bool LoadTemplate(String fullPath)
		{
			try
			{
				XDocument document = XDocument.Load(fullPath);
				if (document.Root == null || String.Compare("Template", document.Root.Name.ToString(), false, CultureInfo.InvariantCulture) != 0)
				{
					if (log.IsErrorEnabled)
					{
						log.ErrorFormat("Template file {0} has an invalid root {1}", fullPath, document.Root);
					}
					return false;
				}

				var template = document.Root;
				var directory = Path.GetDirectoryName(Path.GetFullPath(fullPath));

				foreach (var attribute in template.Attributes())
				{
					if (attribute.NodeType == System.Xml.XmlNodeType.Attribute)
					{
						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Found Attribute. Name: {0} Value: {1}", attribute.Name, attribute.Value);
						}

						SetValue(attribute.Name.ToString(), attribute.Value, directory);
					}
					else if (log.IsDebugEnabled)
					{
						log.DebugFormat("Node Type {0} is not an Attribute", attribute.NodeType);
					}
				}

				foreach (var element in template.Elements())
				{
					if (element.NodeType == System.Xml.XmlNodeType.Element)
					{
						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Found Node. Name: {0} Value: {1}", element.Name, element.Value);
						}
						
						SetValue(element.Name.ToString(), element.Value, directory);
						
					}
					else if (log.IsDebugEnabled)
					{
						log.DebugFormat("Node Type {0} is not an Element", element.NodeType);
					}
				}
			}
			catch (SecurityException se)
			{
				if (log.IsErrorEnabled)
				{
					log.ErrorFormat("Access to file {0} is denied. SecurityException: {0}", se);
				}
				return false;
			}
			catch (FormatException fe)
			{
				if (log.IsErrorEnabled)
				{
					log.ErrorFormat("Loading file {0} threw a format exception {0}", fullPath, fe);
				}
				return false;
			}

			return true;
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
				PropertyMetaData value = null;

				if (Values.ContainsKey(fixedKey))
				{
					value = Values[fixedKey];
				}

				if (value == null && Disk != null)
				{
					value = Disk[fixedKey];
				}

				value = ResolveFormatAndMethods(value);

				return value;
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
