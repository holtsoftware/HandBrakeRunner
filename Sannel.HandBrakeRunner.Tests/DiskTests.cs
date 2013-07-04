using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sannel.HandBrakeRunner.Tests.Exposers;
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
		public async Task LoadAsyncTest()
		{
			FileInfo file1 = new FileInfo("Config.xml");
			using (StreamWriter writer = new StreamWriter(file1.OpenWrite()))
			{
				await writer.WriteAsync(
@"<Configuration year=""2003"">
	<Title2>Title 2</Title2>
	<Details>This is the test Details</Details>
</Configuration>");
			}

			FileInfo template = new FileInfo("Template.xml");
			using (StreamWriter writer = new StreamWriter(template.OpenWrite()))
			{
				await writer.WriteAsync(
@"<Template>
	<TemplateName>This is the Template Name</TemplateName>
</Template>");
			}

			FileInfo diskFile = new FileInfo("Disk.xml");
			using (StreamWriter writer = new StreamWriter(diskFile.OpenWrite()))
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
			var rvalue = await disk.LoadAsync("Disk.xml");

			Assert.IsTrue(rvalue, "LoadAsync did not return true.");
			var diskValues = disk.GetValues;
			Assert.IsNotNull(diskValues, "DiskValues is null");
			Assert.IsTrue(diskValues.ContainsKey("PROP1"), "Prop1 was not found");
		}
	}
}
