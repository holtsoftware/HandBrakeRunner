using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public interface IMetaData
	{
		String GetFileExt(ITrack track);

		Task<bool> RunAsync(ITrack track, String tmpFilePath, String destinationFile);
	}
}
