using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public interface IConfiguration
	{
		Task<bool> LoadAsync(String fileName);

		PropertyMetaData this[String key]
		{
			get;
		}

		Task<PropertyMetaData> GetValueAsync(String key);
	}
}
