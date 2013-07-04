using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner
{
	public class Disk : Configuration, IDisk
	{
		public override async Task<bool> LoadAsync(String fileName)
		{
			return false;
		}

		protected virtual IConfiguration CreateConfiguration()
		{
			return new Configuration();
		}


		public string Value(string key)
		{
			throw new NotImplementedException();
		}
	}
}
