using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public class PropertyMetaData
	{
		public String Value 
		{ 
			get; 
			set; 
		}

		public String FullPath
		{
			get;
			set;
		}

		public static implicit operator String(PropertyMetaData prop)
		{
			if (prop == null)
			{
				return null;
			}
			return prop.Value;
		}

		public static implicit operator PropertyMetaData(String value)
		{
			return new PropertyMetaData
			{
				Value = value
			};
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
