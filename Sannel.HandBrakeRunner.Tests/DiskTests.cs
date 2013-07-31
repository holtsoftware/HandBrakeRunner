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
	public class DiskTests
	{
		[TestMethod]
		[TestCategory("Disk")]
		public async Task LoadAsyncTest()
		{
			FileInfo file1 = new FileInfo("Config.xml");
			using (StreamWriter writer = new StreamWriter(file1.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"<Configuration year=""2003"">
	<Title2>Title 2</Title2>
	<Details>This is the test Details</Details>
</Configuration>");
			}

			FileInfo template = new FileInfo("Template.xml");
			using (StreamWriter writer = new StreamWriter(template.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"<Template>
	<TemplateName>This is the Template Name</TemplateName>
</Template>");
			}

			FileInfo diskFile = new FileInfo("Disk.xml");
			using (StreamWriter writer = new StreamWriter(diskFile.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"<Disk config=""Config.xml"" prop2=""Prop 2 value"">
	<Prop1>This is Prop1</Prop1>
	<Template>Template.xml</Template>
	<Track>
		<Name>Track 1</Name>
	</Track>
	<Track>
		<Name>Track 2</Name>
	</Track>
</Disk>");
			}

			DiskExposer disk = new DiskExposer();

			AssertHelpers.ThrowsException<ArgumentNullException>(() => { disk.LoadAsync(null).GetAwaiter().GetResult(); });

			var rvalue = await disk.LoadAsync("Disk.xml");

			Assert.IsTrue(rvalue, "LoadAsync did not return true.");
			var diskValues = disk.GetValues;
			Assert.IsNotNull(diskValues, "DiskValues is null");
			Assert.IsTrue(diskValues.ContainsKey("PROP1"), "Prop1 was not found");
			Assert.AreEqual<String>("This is Prop1", diskValues["PROP1"], "Prop1 value was not correct");
			Assert.IsTrue(diskValues.ContainsKey("PROP2"), "Prop2 was not found");
			Assert.AreEqual<String>("Prop 2 value", diskValues["PROP2"], "Prop2 value was not correct");
			Assert.IsTrue(diskValues.ContainsKey("TEMPLATE"), "Template was not found.");
			Assert.AreEqual<String>(Path.GetFullPath("Template.xml"), diskValues["TEMPLATE"], "Template value did not match.");

			Assert.IsInstanceOfType(disk.GetConfiguration, typeof(ConfigurationExposer));
			ConfigurationExposer config = disk.GetConfiguration as ConfigurationExposer;
			var configValues = config.GetValues;
			Assert.IsNotNull(configValues, "ConfigValues is null and should not be");
			Assert.IsTrue(configValues.ContainsKey("TITLE2"), "Title2 was not found");
			Assert.AreEqual<String>("Title 2", configValues["TITLE2"], "Title2 value was not correct");
			Assert.IsTrue(configValues.ContainsKey("DETAILS"), "Details was not found");
			Assert.AreEqual<String>("This is the test Details", configValues["DETAILS"], "Details did not match");

			var tracks = disk.Tracks;
			Assert.IsNotNull(tracks, "Tracks is null and should not be");
			Assert.AreEqual(2, tracks.Count, "The tracks count is off.");
		}

		[TestMethod]
		[TestCategory("Disk")]
		public async Task GetValueAsyncTest()
		{
			DiskExposer exposer = new DiskExposer();
			exposer.GetValues["TEST"] = "This is my Test";
			exposer.GetValues["TEST2"] = "Another Test";
			exposer.GetValues["TITLE"] = "Title value";

			var config = exposer.GetConfiguration as ConfigurationExposer;
			config.GetValues["CONFIGTITLE"] = "This is the Config Title";
			config.GetValues["DESCRIPTION"] = "This is the Description";

			Assert.AreEqual<String>("This is my Test", await exposer.GetValueAsync("test"), "Test value does not match");
			Assert.AreEqual<String>("Another Test", await exposer.GetValueAsync("test2"), "Test2 value does not match");
			Assert.AreEqual<String>("Title value", await exposer.GetValueAsync("title"), "Title value does not match");
			Assert.AreEqual<String>("This is the Config Title", await exposer.GetValueAsync("configtitle"), "ConfigTitle value does not match");
			Assert.AreEqual<String>("This is the Description", await exposer.GetValueAsync("description"), "Description value does not match");
			Assert.IsNull(await exposer.GetValueAsync("cheese"), "cheese value was suppose to be null and was not");
		}
	}
}
