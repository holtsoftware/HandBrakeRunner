using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.HandBrakeRunner.Tests.Exposers;
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
			using (StreamWriter writer = new StreamWriter(template.OpenWrite()))
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
			Assert.AreEqual("Itsy Bitsy Spider", values["SONG"], "Song value did not match.");
			Assert.IsTrue(values.ContainsKey("NAME"), "Name was not found.");
			Assert.AreEqual("This is the name", values["NAME"], "Name value did not match.");
			Assert.IsTrue(values.ContainsKey("DESCRIPTION"), "Description was not found");
			Assert.AreEqual("Description of the show", values["DESCRIPTION"], "Description value did not match");
			Assert.IsTrue(values.ContainsKey("CHEESE"), "Cheese was not found.");
			Assert.AreEqual("Cheddar", values["CHEESE"], "Cheese value did not match.");
			Assert.IsTrue(values.ContainsKey("TEMPLATEID"), "TemplateId was not found");
			Assert.AreEqual("2", values["TEMPLATEID"], "TemplateId did not match");
			Assert.IsTrue(values.ContainsKey("YEAR"), "Year was not found.");
			Assert.AreEqual("2008", values["YEAR"], "Year value did not match");
			Assert.IsTrue(values.ContainsKey("COLOR"), "Color was not found.");
			Assert.AreEqual("Green", values["COLOR"], "Color value did not match");
		}
	}
}
