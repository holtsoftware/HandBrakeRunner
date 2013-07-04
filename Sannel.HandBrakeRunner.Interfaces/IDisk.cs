using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public interface IDisk
	{
		Task<bool> LoadAsync(String fileName);

		String Value(String key);
	}
}
