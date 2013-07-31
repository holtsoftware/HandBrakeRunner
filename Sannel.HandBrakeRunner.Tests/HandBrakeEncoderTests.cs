using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.HandBrakeRunner.Plugins;


using Sannel.HandBrakeRunner.Tests.Exposers;
using Sannel.Helpers;
namespace Sannel.HandBrakeRunner.Tests
{
	[TestClass]
	public class HandBrakeEncoderTests
	{
		[TestMethod]
		[TestCategory("HandBrakeEncoder")]
		public void GetFileExtTest()
		{
			HandBrakeEncoder encoder = new HandBrakeEncoder();
			TrackExposer track = new TrackExposer();

			AssertHelpers.ThrowsException<ArgumentNullException>(() => { encoder.GetFileExt(null); });

			var actual = encoder.GetFileExt(track);
			Assert.AreEqual(".mp4", actual, "HandBrakeEncoder did not return the correct default fileExtension");
			track.GetValues["FILENAME"] = "Test.m4v";
			actual = encoder.GetFileExt(track);
			Assert.AreEqual(".m4v", actual, "HandBrakeEncoder did not return the fileExtension of property FILENAME");
			track.GetValues["FILEEXT"] = ".mp3";
			actual = encoder.GetFileExt(track);
			Assert.AreEqual(".mp3", actual, "HandBrakeEncoder did not return the fileExtension of property FILEEXT");
			track.GetValues["HBFILEEXT"] = ".mkv";
			actual = encoder.GetFileExt(track);
			Assert.AreEqual(".mkv", actual, "HandBrakeEncoder did not return the fileExtension of property HBFILEEXT");

		}

		[TestMethod]
		[TestCategory("HandBrakeEncoder")]
		public void GenerateArguments()
		{
			HandBrakeEncoderExposer encoder = new HandBrakeEncoderExposer();
			TrackExposer track = new TrackExposer();
			String tmpFilePath = "/tmp/File01.mp4";

			AssertHelpers.ThrowsException<ArgumentNullException>(() => { encoder.GenerateArgumentsExposed(null, track); });
			AssertHelpers.ThrowsException<ArgumentNullException>(() => { encoder.GenerateArgumentsExposed(tmpFilePath, null); });

			var actual = encoder.GenerateArgumentsExposed(tmpFilePath, track);
			Assert.AreEqual("-o \"/tmp/File01.mp4\" ", actual);
			track.GetValues["HANDBRAKEOPTIONS"] = "-e x264 -q 20 -X 720 -a 1 -B 160 -R 48 -6 dpl2 -D 1.0 -2 -T -E faac --loose-anamorphic  -f mp4 --decomb -m -x b-adapt=2:rc-lookahead=50";
			
			actual = encoder.GenerateArgumentsExposed(tmpFilePath, track);
			Assert.AreEqual("-o \"/tmp/File01.mp4\" -e x264 -q 20 -X 720 -a 1 -B 160 -R 48 -6 dpl2 -D 1.0 -2 -T -E faac --loose-anamorphic  -f mp4 --decomb -m -x b-adapt=2:rc-lookahead=50", actual, "Unexpected arguments returned.");
			track.GetValues["TITLECHAPTER"] = "-t 1 -c 5-8";
			
			actual = encoder.GenerateArgumentsExposed(tmpFilePath, track);
			Assert.AreEqual("-t 1 -c 5-8 -o \"/tmp/File01.mp4\" -e x264 -q 20 -X 720 -a 1 -B 160 -R 48 -6 dpl2 -D 1.0 -2 -T -E faac --loose-anamorphic  -f mp4 --decomb -m -x b-adapt=2:rc-lookahead=50", actual);
			track.GetValues["INPUTPATH"] = "/dev/dvd";

			actual = encoder.GenerateArgumentsExposed(tmpFilePath, track);
			Assert.AreEqual("-i \"/dev/dvd\" -t 1 -c 5-8 -o \"/tmp/File01.mp4\" -e x264 -q 20 -X 720 -a 1 -B 160 -R 48 -6 dpl2 -D 1.0 -2 -T -E faac --loose-anamorphic  -f mp4 --decomb -m -x b-adapt=2:rc-lookahead=50", actual);
		}
	}
}
