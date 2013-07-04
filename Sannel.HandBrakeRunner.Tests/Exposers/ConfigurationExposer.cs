using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Tests.Exposers
{
	public class ConfigurationExposer : Configuration
	{

		public Dictionary<String, String> GetValues
		{
			get
			{
				return base.Values;
			}
		}
	}
}
