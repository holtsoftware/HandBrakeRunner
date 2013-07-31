using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Tests.Exposers
{
	public class DiskExposer : Disk
	{
		public Dictionary<String, PropertyMetaData> GetValues
		{
			get
			{
				return base.Values;
			}
		}

		protected override IConfiguration CreateConfiguration()
		{
			return new ConfigurationExposer();
		}

		public IConfiguration GetConfiguration
		{
			get
			{
				return this.Configuration ?? (this.Configuration = CreateConfiguration());
			}
		}
	}
}
