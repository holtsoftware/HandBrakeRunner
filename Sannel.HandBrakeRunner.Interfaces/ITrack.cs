using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public interface ITrack : IDisk
	{
		IDisk Disk
		{
			get;
		}
	}
}
