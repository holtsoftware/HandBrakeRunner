using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.Helpers;
using Sannel.HandBrakeRunner.Tests.Exposers;
using System.Threading.Tasks;
using Sannel.HandBrakeRunner.Interfaces;

namespace Sannel.HandBrakeRunner.Tests
{
	[TestClass]
	public class MethodsTests
	{
		[TestMethod]
		[TestCategory("Methods")]
		public void FixFileNameTest()
		{
			AssertHelpers.ThrowsException<ArgumentNullException>(() => { Methods.FixFileName(null, "test"); });
			
			TrackExposer track = new TrackExposer();
			track.GetValues["TEST"] = "Hi";
			track.GetValues["NAME"] = "Into, the \"Wild\", Green Yonder.mp4";

			Assert.AreEqual("", Methods.FixFileName(track), "Return value was not correct for no arguments");
			Assert.AreEqual("Into, the _Wild_, Green Yonder.mp4", Methods.FixFileName(track, "name"), "Name value was not transformed correctly.");
			Assert.AreEqual("Into, the _Wild_, Green Yonder.mp4", Methods.FixFileName(track, "name", "Description"), "Name value was not transformed correctly.");
		}

		[TestMethod]
		[TestCategory("Methods")]
		public async Task LoadVariableMethods()
		{
			await Methods.LoadVariableMethods();
			Assert.IsTrue(Methods.RegisteredMethods.ContainsKey("FIXFILENAME"), "The FixFileName method was not registered.");
		}
	}
}
