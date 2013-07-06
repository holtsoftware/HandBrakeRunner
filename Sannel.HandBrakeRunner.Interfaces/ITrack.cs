using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public interface ITrack
	{
		Task<bool> LoadTrackAsync(XElement track, String diskFullPath);

		IDisk Disk
		{
			get;
			set;
		}

		String this[String key]
		{
			get;
		}

		Task<String> GetValueAsync(String key);
	}
}
