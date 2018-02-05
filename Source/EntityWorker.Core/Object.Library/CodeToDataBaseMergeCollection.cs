using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityWorker.Core.Object.Library
{
    public sealed class CodeToDataBaseMergeCollection : List<CodeToDataBaseMerge>
    {
        private readonly Transaction.Transaction _provider;

        internal static Custom_ValueType<string, CodeToDataBaseMerge> ExecutedData { get; private set; } = new Custom_ValueType<string, CodeToDataBaseMerge>();
        public CodeToDataBaseMergeCollection(Transaction.Transaction provider)
        {
            _provider = provider;
        }

        public new void Add(CodeToDataBaseMerge item)
        {

            if (item != null && item.Sql.Length >= 2 && !this.Any(x => x.Object_Type == item.Object_Type))
                base.Add(item);
            item.DataBaseTypes = _provider.DataBaseTypes;
        }

        public Dictionary<string, Tuple<string, ForeignKey>> Keys { get; internal set; } = new Dictionary<string, Tuple<string, ForeignKey>>();

        /// <summary>
        /// Execute changes
        /// </summary>
        public void Execute(bool allowDataLoss = false)
        {
            lock (ExecutedData)
            {
                try
                {
                    _provider.CreateTransaction();
                    Exception exp = null;
                    var data = this.Where(x => !x.DataLoss || allowDataLoss).ToList();

                    while (data.Any(x => !x.Executed))
                    {
                        var items = data.Where(x => !x.Executed).ToList();

                        for (var i = items.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                var key = items[i];
                                var sSql = key.Sql.ToString();
                                if (sSql.EndsWith(",)"))
                                    sSql = sSql.TrimEnd(",)") + ")";
                                var cmd = _provider.GetSqlCommand(sSql);
                                _provider.ExecuteNonQuery(cmd);
                                key.Executed = true;
                                key.Exception = null;
                                ExecutedData.TryAdd(key.Object_Type.FullName + _provider.DataBaseTypes.ToString(), key);
                            }
                            catch (NpgsqlException ex)
                            {
                                if (ex.ToString().Contains("already exists"))
                                    items[i].Executed = true;
                                items[i].Exception = ex;
                                _provider.Renew();
                                exp = ex;
                            }
                            catch (Exception ex)
                            {
                                data[i].Exception = ex;
                                if (ex.ToString().Contains("already exists"))
                                    items[i].Executed = true;
                                exp = ex;
                            }
                        }
                    }
                    var sql = new StringBuilder();
                    if (Keys.Any() && (_provider.DataBaseTypes == DataBaseTypes.Mssql || _provider.DataBaseTypes == DataBaseTypes.PostgreSql))
                    {
                        foreach (var key in Keys)
                        {
                            var type = key.Value.Item2.Type.GetActualType();
                            var keyPrimary = type.GetPrimaryKey().GetPropertyName();
                            var tb = type.GetCustomAttribute<Table>()?.Name ?? type.Name;
                            if (_provider.DataBaseTypes == DataBaseTypes.Mssql)
                                sql.Append("ALTER TABLE [" + key.Value.Item1 + "] ADD FOREIGN KEY (" + key.Key.Split('-')[0] + ") REFERENCES [" + tb + "](" + keyPrimary + ");");
                            else sql.Append("ALTER TABLE " + key.Value.Item1 + " ADD CONSTRAINT fk_" + (tb + "_" + key.Key.Split('-')[0]) + " FOREIGN KEY (" + key.Key.Split('-')[0] + ") REFERENCES " + tb + "(" + keyPrimary + ");");

                        }
                        var s = sql.ToString();
                        if (s.EndsWith(",)"))
                            s = s.TrimEnd(",)") + ")";
                        var cmd = _provider.GetSqlCommand(s);
                        _provider.ExecuteNonQuery(cmd);

                    }
                    DbSchema.CachedObjectColumn.Clear();
                    DbSchema.CachedSql.Clear();
                    Extension.CachedDataRecord.Clear();
                    _provider.SaveChanges();
                }
                catch
                {
                    _provider.Rollback();
                    throw;
                }
            }
        }
    }
}
