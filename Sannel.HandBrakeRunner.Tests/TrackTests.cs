using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.HandBrakeRunner.Interfaces;
using Sannel.HandBrakeRunner.Tests.Exposers;
using Sannel.HandBrakeRunner.Plugins;
using Sannel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sannel.HandBrakeRunner.Tests
{
	[TestClass]
	public class TrackTests
	{
		[TestMethod]
		[TestCategory("Track")]
		public async Task LoadTrackAsyncTest()
		{
			XElement trackXML = new XElement("Track",
				new XAttribute("template", "Template.xml"),
				new XAttribute("song", "Itsy Bitsy Spider"),
				new XElement("Name", "This is the name"),
				new XElement("Description", "Description of the show"),
				new XElement("Cheese", "Cheddar"));
			
			FileInfo template = new FileInfo("Template.xml");
			using (StreamWriter writer = new StreamWriter(template.Open(FileMode.Create)))
			{
				await writer.WriteAsync(
@"
<Template templateId=""2"">
	<Name>This name is going to be overridden</Name>
	<Year>2008</Year>
	<Color>Green</Color>
</Template>
");
			}

			TrackExposer track = new TrackExposer();

			AssertHelpers.ThrowsException<ArgumentNullException>(() =>
				{
					track.LoadTrackAsync(null, Path.GetFullPath("Disk.xml")).GetAwaiter().GetResult();
				});

			AssertHelpers.ThrowsException<ArgumentNullException>(() =>
				{
					track.LoadTrackAsync(trackXML, null).GetAwaiter().GetResult();
				});

			var status = await track.LoadTrackAsync(trackXML, Path.GetFullPath("Disk.xml"));

			Assert.IsTrue(status, "Status was not true and should have been.");
			var values = track.GetValues;
			Assert.IsNotNull(values, "Values was null and should not have been.");
			Assert.IsTrue(values.ContainsKey("SONG"), "Song was not found.");
			Assert.AreEqual<String>("Itsy Bitsy Spider", values["SONG"], "Song value did not match.");
			Assert.IsTrue(values.ContainsKey("NAME"), "Name was not found.");
			Assert.AreEqual<String>("This is the name", values["NAME"], "Name value did not match.");
			Assert.IsTrue(values.ContainsKey("DESCRIPTION"), "Description was not found");
			Assert.AreEqual<String>("Description of the show", values["DESCRIPTION"], "Description value did not match");
			Assert.IsTrue(values.ContainsKey("CHEESE"), "Cheese was not found.");
			Assert.AreEqual<String>("Cheddar", values["CHEESE"], "Cheese value did not match.");
			Assert.IsTrue(values.ContainsKey("TEMPLATEID"), "TemplateId was not found");
			Assert.AreEqual<String>("2", values["TEMPLATEID"], "TemplateId did not match");
			Assert.IsTrue(values.ContainsKey("YEAR"), "Year was not found.");
			Assert.AreEqual<String>("2008", values["YEAR"], "Year value did not match");
			Assert.IsTrue(values.ContainsKey("COLOR"), "Color was not found.");
			Assert.AreEqual<String>("Green", values["COLOR"], "Color value did not match");
		}

		[TestMethod]
		[TestCategory("Track")]
		public async Task GetValueAsyncTest()
		{
			TrackExposer exposer = new TrackExposer();
			exposer.GetValues["TEST"] = "This is my Test";
			exposer.GetValues["TEST2"] = "Another Test";
			exposer.GetValues["TITLE"] = "Title value";

			var disk = new DiskExposer();
			exposer.Disk = disk;
			disk.GetValues["DISKTITLE"] = "This is the disk title";
			disk.GetValues["TITLE"] = "This is not the title we want.";

			var config = disk.GetConfiguration as ConfigurationExposer;
			config.GetValues["CONFIGTITLE"] = "This is the Config Title";
			config.GetValues["DESCRIPTION"] = "This is the Description";
			config.GetValues["TEST"] = "This is not the test we want.";

			Assert.AreEqual<String>("This is my Test", await exposer.GetValueAsync("test"), "Test value does not match");
			Assert.AreEqual<String>("Another Test", await exposer.GetValueAsync("test2"), "Test2 value does not match");
			Assert.AreEqual<String>("Title value", await exposer.GetValueAsync("title"), "Title value does not match");
			Assert.AreEqual<String>("This is the disk title", await exposer.GetValueAsync("disktitle"), "DiskTitle value does not match");
			Assert.AreEqual<String>("This is the Config Title", await exposer.GetValueAsync("configtitle"), "ConfigTitle value does not match");
			Assert.AreEqual<String>("This is the Description", await exposer.GetValueAsync("description"), "Description value does not match");
			Assert.IsNull(await exposer.GetValueAsync("cheese"), "cheese value was suppose to be null and was not");
		}

		[TestMethod]
		[TestCategory("Track")]
		public async Task ResolveFormatAndMethods()
		{
			await Methods.LoadVariableMethods();
			TrackExposer exposer = new TrackExposer();
			exposer.GetValues["TEST"] = "23.4";
			exposer.GetValues["TEST2"] = "6";
			exposer.GetValues["CHEESE"] = "Cheddar";
			exposer.GetValues["SHOWNAME"] = "Future";
			exposer.GetValues["FILENAME"] = "Into, the \"Wild\", Green Yonder.mp4";

			var results = exposer.ResolveFormatAndMethodsPublic("${Test:000.000}");
			Assert.AreEqual("023.400", results, "double test");
			results = exposer.ResolveFormatAndMethodsPublic("${Test2:000}");
			Assert.AreEqual("006", results, "long test");
			results = exposer.ResolveFormatAndMethodsPublic("${SHOWNAME} ${Cheese}");
			Assert.AreEqual("Future Cheddar", results, "String Combine");
			results = exposer.ResolveFormatAndMethodsPublic("${Test2} %{FixFileName(FileName)}");
			Assert.AreEqual("6 Into, the _Wild_, Green Yonder.mp4", results, "Method call");
		}
	}
}
