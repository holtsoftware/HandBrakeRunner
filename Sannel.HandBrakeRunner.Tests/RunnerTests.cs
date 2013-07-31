using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.HandBrakeRunner.Tests.Exposers;

namespace Sannel.HandBrakeRunner.Tests
{
	[TestClass]
	public class RunnerTests
	{
		[TestMethod]
		public void ParseTracksTest()
		{
			RunnerExposer runner = new RunnerExposer();
			var trackNumbers = runner.ParseTracksExposed("1,5,3,4-7");
			//1,3,4,5,6,7
			Assert.IsNotNull(trackNumbers);
			Assert.AreEqual(6, trackNumbers.Length, "Incorrect number of tracks");
			Assert.AreEqual(1, trackNumbers[0], "trackNumbers[0]");
			Assert.AreEqual(3, trackNumbers[1], "trackNumbers[1]");
			Assert.AreEqual(4, trackNumbers[2], "trackNumbers[2]");
			Assert.AreEqual(5, trackNumbers[3], "trackNumbers[3]");
			Assert.AreEqual(6, trackNumbers[4], "trackNumbers[4]");
			Assert.AreEqual(7, trackNumbers[5], "trackNumbers[5]");

			trackNumbers = runner.ParseTracksExposed("50");
			Assert.IsNotNull(trackNumbers);
			Assert.AreEqual(1, trackNumbers.Length, "trackNumbers Length does not match");

			trackNumbers = runner.ParseTracksExposed("a");
			Assert.IsNull(trackNumbers, "trackNumbers should be null");

			trackNumbers = runner.ParseTracksExposed("3,8-1");
			Assert.IsNull(trackNumbers, "trackNumbers should be null");

			trackNumbers = runner.ParseTracksExposed("3-a");
			Assert.IsNull(trackNumbers, "trackNumbers should be null");

			trackNumbers = runner.ParseTracksExposed("1-5-8");
			Assert.IsNull(trackNumbers, "trackNumbers should be null");

			trackNumbers = runner.ParseTracksExposed("a-5");
			Assert.IsNull(trackNumbers, "trackNumbers should be null");

		}
	}
}
