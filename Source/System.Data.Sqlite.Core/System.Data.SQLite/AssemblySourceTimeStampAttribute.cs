using System;

namespace System.Data.SQLite
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
	public sealed class AssemblySourceTimeStampAttribute : Attribute
	{
		private string sourceTimeStamp;

		public string SourceTimeStamp
		{
			get
			{
				return this.sourceTimeStamp;
			}
		}

		public AssemblySourceTimeStampAttribute(string value)
		{
			this.sourceTimeStamp = value;
		}
	}
}