using System;

namespace System.Data.SQLite
{
	public delegate bool SessionTableFilterCallback(object clientData, string name);
}