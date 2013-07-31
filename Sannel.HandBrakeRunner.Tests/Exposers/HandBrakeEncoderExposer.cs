using Sannel.HandBrakeRunner.Interfaces;
using Sannel.HandBrakeRunner.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Tests.Exposers
{
	public class HandBrakeEncoderExposer : HandBrakeEncoder
	{
		public String GenerateArgumentsExposed(String tmpFilePath, ITrack track)
		{
			return base.GenerateArguments(tmpFilePath, track);
		}
	}
}
