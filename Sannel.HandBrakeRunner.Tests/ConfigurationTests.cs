using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.HandBrakeRunner.Tests.Exposers;
using Sannel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Tests
{
	[TestClass]
	public class ConfigurationTests
	{
		[TestMethod]
		[TestCategory("Configuration")]
		public async Task LoadAsyncTest()
		{
			Directory.CreateDirectory("Dir1");
			FileInfo file1 = new FileInfo("Dir1\\Config.xml");
			using (StreamWriter writer = new StreamWriter(file1.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"<Configuration config=""../Config.xml"">
	<Title1>Title</Title1>
	<Title2>number six</Title2>
</Configuration>");
			}

			FileInfo file2 = new FileInfo("Config.xml");
			using (StreamWriter writer = new StreamWriter(file2.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"<Configuration year=""2003"">
	<Title2>Title 2</Title2>
	<Details>This is the test Details</Details>
</Configuration>");
			}

			ConfigurationExposer config = new ConfigurationExposer();

			AssertHelpers.ThrowsException<ArgumentNullException>(() => { config.LoadAsync(null).GetAwaiter().GetResult(); });

			var rvalue = await config.LoadAsync("Dir1\\Config.xml");

			Assert.IsTrue(rvalue, "LoadAsync did not return true");

			var values = config.GetValues;
			Assert.IsTrue(values.ContainsKey("TITLE1"), "Title1 was not found");
			Assert.AreEqual<String>("Title", values["TITLE1"], "Title1 value does not match");
			Assert.IsTrue(values.ContainsKey("TITLE2"), "Title2 was not found");
			Assert.AreEqual<String>("number six", values["TITLE2"], "Title2 value does not match");
			Assert.IsTrue(values.ContainsKey("DETAILS"), "Details was not found");
			Assert.AreEqual<String>("This is the test Details", values["DETAILS"], "Details value does not match");
			Assert.IsTrue(values.ContainsKey("YEAR"), "Year was not found");
			Assert.AreEqual<String>("2003", values["YEAR"], "Year value does not match");

		}

		[TestMethod]
		[TestCategory("Configuration")]
		public async Task GetValueAsyncTest()
		{
			ConfigurationExposer exposer = new ConfigurationExposer();
			exposer.GetValues["TEST"] = "This is my Test";
			exposer.GetValues["TEST2"] = "Another Test";
			exposer.GetValues["TITLE"] = "Title value";

			Assert.AreEqual<String>("This is my Test", await exposer.GetValueAsync("test"), "Test value does not match");
			Assert.AreEqual<String>("Another Test", await exposer.GetValueAsync("test2"), "Test2 value does not match");
			Assert.AreEqual<String>("Title value", await exposer.GetValueAsync("title"), "Title value does not match");
			Assert.IsNull(await exposer.GetValueAsync("cheese"), "cheese value was suppose to be null and was not");
		}
	}
}
