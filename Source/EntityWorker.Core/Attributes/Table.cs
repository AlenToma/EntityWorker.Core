using EntityWorker.Core.Helper;
using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Define diffrent name for the table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Table : Attribute
    {
        public string Name { get; private set; }

        public string Schema { get; private set; }

        public string FullName { get; private set; }
        /// <summary>
        /// schema works only for MSSQl and postGreSql
        /// Database should allow Create Schema for this to work or the Schema should already be created
        /// </summary>
        /// <param name="name"></param>
        /// <param name="schema"></param>
        public Table(string name, string schema = null)
        {
            Name = name.Replace("[", string.Empty).Replace("]", string.Empty);
            Schema = schema?.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!string.IsNullOrEmpty(Schema))
                FullName = $"[{Schema}].[{Name}]";
            else FullName = $"[{Name}]";
        }

        public string GetName(DataBaseTypes databaseTypes)
        {
            if (databaseTypes == DataBaseTypes.Sqllight || string.IsNullOrEmpty(Schema))
                return $"[{Name}]";
            else return FullName;
        }
    }
}
