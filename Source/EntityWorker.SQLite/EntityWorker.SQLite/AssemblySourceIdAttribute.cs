using System;

namespace EntityWorker.SQLite
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
	public sealed class AssemblySourceIdAttribute : Attribute
	{
		private string sourceId;

		public string SourceId
		{
			get
			{
				return this.sourceId;
			}
		}

		public AssemblySourceIdAttribute(string value)
		{
			this.sourceId = value;
		}
	}
}