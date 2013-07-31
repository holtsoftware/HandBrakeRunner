using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Tests.Exposers
{
	public class RunnerExposer : Runner
	{
		public int[] ParseTracksExposed(String args)
		{
			return ParseTracks(args);
		}
	}
}
