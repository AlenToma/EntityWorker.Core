using System;

namespace System.Data.SQLite
{
	[AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
	public sealed class SQLiteFunctionAttribute : Attribute
	{
		private string _name;

		private int _argumentCount;

		private FunctionType _functionType;

		private Type _instanceType;

		private Delegate _callback1;

		private Delegate _callback2;

		public int Arguments
		{
			get
			{
				return this._argumentCount;
			}
			set
			{
				this._argumentCount = value;
			}
		}

		internal Delegate Callback1
		{
			get
			{
				return this._callback1;
			}
			set
			{
				this._callback1 = value;
			}
		}

		internal Delegate Callback2
		{
			get
			{
				return this._callback2;
			}
			set
			{
				this._callback2 = value;
			}
		}

		public FunctionType FuncType
		{
			get
			{
				return this._functionType;
			}
			set
			{
				this._functionType = value;
			}
		}

		internal Type InstanceType
		{
			get
			{
				return this._instanceType;
			}
			set
			{
				this._instanceType = value;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public SQLiteFunctionAttribute() : this(null, -1, FunctionType.Scalar)
		{
		}

		public SQLiteFunctionAttribute(string name, int argumentCount, FunctionType functionType)
		{
			this._name = name;
			this._argumentCount = argumentCount;
			this._functionType = functionType;
			this._instanceType = null;
			this._callback1 = null;
			this._callback2 = null;
		}
	}
}