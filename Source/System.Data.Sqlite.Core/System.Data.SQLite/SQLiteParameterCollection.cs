using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace System.Data.SQLite
{
	[Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ListBindable(false)]
	public sealed class SQLiteParameterCollection : DbParameterCollection
	{
		private SQLiteCommand _command;

		private List<SQLiteParameter> _parameterList;

		private bool _unboundFlag;

		public override int Count
		{
			get
			{
				return this._parameterList.Count;
			}
		}

		public override bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public override bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public new SQLiteParameter this[string parameterName]
		{
			get
			{
				return (SQLiteParameter)this.GetParameter(parameterName);
			}
			set
			{
				this.SetParameter(parameterName, value);
			}
		}

		public new SQLiteParameter this[int index]
		{
			get
			{
				return (SQLiteParameter)this.GetParameter(index);
			}
			set
			{
				this.SetParameter(index, value);
			}
		}

		public override object SyncRoot
		{
			get
			{
				return null;
			}
		}

		internal SQLiteParameterCollection(SQLiteCommand cmd)
		{
			this._command = cmd;
			this._parameterList = new List<SQLiteParameter>();
			this._unboundFlag = true;
		}

		public SQLiteParameter Add(string parameterName, DbType parameterType, int parameterSize, string sourceColumn)
		{
			SQLiteParameter sQLiteParameter = new SQLiteParameter(parameterName, parameterType, parameterSize, sourceColumn);
			this.Add(sQLiteParameter);
			return sQLiteParameter;
		}

		public SQLiteParameter Add(string parameterName, DbType parameterType, int parameterSize)
		{
			SQLiteParameter sQLiteParameter = new SQLiteParameter(parameterName, parameterType, parameterSize);
			this.Add(sQLiteParameter);
			return sQLiteParameter;
		}

		public SQLiteParameter Add(string parameterName, DbType parameterType)
		{
			SQLiteParameter sQLiteParameter = new SQLiteParameter(parameterName, parameterType);
			this.Add(sQLiteParameter);
			return sQLiteParameter;
		}

		public int Add(SQLiteParameter parameter)
		{
			int count = -1;
			if (!string.IsNullOrEmpty(parameter.ParameterName))
			{
				count = this.IndexOf(parameter.ParameterName);
			}
			if (count == -1)
			{
				count = this._parameterList.Count;
				this._parameterList.Add(parameter);
			}
			this.SetParameter(count, parameter);
			return count;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int Add(object value)
		{
			return this.Add((SQLiteParameter)value);
		}

		public void AddRange(SQLiteParameter[] values)
		{
			int length = (int)values.Length;
			for (int i = 0; i < length; i++)
			{
				this.Add(values[i]);
			}
		}

		public override void AddRange(Array values)
		{
			int length = values.Length;
			for (int i = 0; i < length; i++)
			{
				this.Add((SQLiteParameter)values.GetValue(i));
			}
		}

		public SQLiteParameter AddWithValue(string parameterName, object value)
		{
			SQLiteParameter sQLiteParameter = new SQLiteParameter(parameterName, value);
			this.Add(sQLiteParameter);
			return sQLiteParameter;
		}

		public override void Clear()
		{
			this._unboundFlag = true;
			this._parameterList.Clear();
		}

		public override bool Contains(string parameterName)
		{
			return this.IndexOf(parameterName) != -1;
		}

		public override bool Contains(object value)
		{
			return this._parameterList.Contains((SQLiteParameter)value);
		}

		public override void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator GetEnumerator()
		{
			return this._parameterList.GetEnumerator();
		}

		protected override DbParameter GetParameter(string parameterName)
		{
			return this.GetParameter(this.IndexOf(parameterName));
		}

		protected override DbParameter GetParameter(int index)
		{
			return this._parameterList[index];
		}

		public override int IndexOf(string parameterName)
		{
			int count = this._parameterList.Count;
			for (int i = 0; i < count; i++)
			{
				if (string.Compare(parameterName, this._parameterList[i].ParameterName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public override int IndexOf(object value)
		{
			return this._parameterList.IndexOf((SQLiteParameter)value);
		}

		public override void Insert(int index, object value)
		{
			this._unboundFlag = true;
			this._parameterList.Insert(index, (SQLiteParameter)value);
		}

		internal void MapParameters(SQLiteStatement activeStatement)
		{
			int i;
			int num;
			if (!this._unboundFlag || this._parameterList.Count == 0 || this._command._statementList == null)
			{
				return;
			}
			int num1 = 0;
			int num2 = -1;
			foreach (SQLiteParameter sQLiteParameter in this._parameterList)
			{
				num2++;
				string parameterName = sQLiteParameter.ParameterName;
				if (parameterName == null)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { num1 };
					parameterName = HelperMethods.StringFormat(invariantCulture, ";{0}", objArray);
					num1++;
				}
				bool flag = false;
				num = (activeStatement != null ? 1 : this._command._statementList.Count);
				SQLiteStatement item = activeStatement;
				for (i = 0; i < num; i++)
				{
					flag = false;
					if (item == null)
					{
						item = this._command._statementList[i];
					}
					if (item._paramNames != null && item.MapParameter(parameterName, sQLiteParameter))
					{
						flag = true;
					}
					item = null;
				}
				if (flag)
				{
					continue;
				}
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] objArray1 = new object[] { num2 };
				parameterName = HelperMethods.StringFormat(cultureInfo, ";{0}", objArray1);
				item = activeStatement;
				for (i = 0; i < num; i++)
				{
					if (item == null)
					{
						item = this._command._statementList[i];
					}
					if (item._paramNames != null && item.MapParameter(parameterName, sQLiteParameter))
					{
						flag = true;
					}
					item = null;
				}
			}
			if (activeStatement == null)
			{
				this._unboundFlag = false;
			}
		}

		public override void Remove(object value)
		{
			this._unboundFlag = true;
			this._parameterList.Remove((SQLiteParameter)value);
		}

		public override void RemoveAt(string parameterName)
		{
			this.RemoveAt(this.IndexOf(parameterName));
		}

		public override void RemoveAt(int index)
		{
			this._unboundFlag = true;
			this._parameterList.RemoveAt(index);
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			this.SetParameter(this.IndexOf(parameterName), value);
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			this._unboundFlag = true;
			this._parameterList[index] = (SQLiteParameter)value;
		}

		internal void Unbind()
		{
			this._unboundFlag = true;
		}
	}
}