using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	[AttributeUsage(AttributeTargets.Method)]
	public class VariableMethodAttribute : System.Attribute
	{
		public String Name
		{
			get;
			set;
		}

		public String Description
		{
			get;
			set;
		}

		public VariableMethodAttribute(String name)
		{
			Name = name;
		}
	}
}
