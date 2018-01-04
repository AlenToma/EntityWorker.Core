using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Data.SQLite
{
	public abstract class SQLiteFunction : IDisposable
	{
		internal SQLiteBase _base;

		private Dictionary<IntPtr, SQLiteFunction.AggregateData> _contextDataList;

		private SQLiteConnectionFlags _flags;

		private SQLiteCallback _InvokeFunc;

		private SQLiteCallback _StepFunc;

		private SQLiteFinalCallback _FinalFunc;

		private SQLiteCollation _CompareFunc;

		private SQLiteCollation _CompareFunc16;

		internal IntPtr _context;

		private static IDictionary<SQLiteFunctionAttribute, object> _registeredFunctions;

		private bool disposed;

		public System.Data.SQLite.SQLiteConvert SQLiteConvert
		{
			get
			{
				this.CheckDisposed();
				return this._base;
			}
		}

		//[FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
		static SQLiteFunction()
		{
			Type[] types;
			SQLiteFunction._registeredFunctions = new Dictionary<SQLiteFunctionAttribute, object>();
			try
			{
				if (UnsafeNativeMethods.GetSettingValue("No_SQLiteFunctions", null) == null)
				{
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					int length = (int)assemblies.Length;
					AssemblyName name = Assembly.GetExecutingAssembly().GetName();
					for (int i = 0; i < length; i++)
					{
						bool flag = false;
						try
						{
							AssemblyName[] referencedAssemblies = assemblies[i].GetReferencedAssemblies();
							int num = (int)referencedAssemblies.Length;
							int num1 = 0;
							while (num1 < num)
							{
								if (referencedAssemblies[num1].Name != name.Name)
								{
									num1++;
								}
								else
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								types = assemblies[i].GetTypes();
							}
							else
							{
								goto Label0;
							}
						}
						catch (ReflectionTypeLoadException reflectionTypeLoadException)
						{
							types = reflectionTypeLoadException.Types;
						}
						int length1 = (int)types.Length;
						for (int j = 0; j < length1; j++)
						{
							if (types[j] != null)
							{
								object[] customAttributes = types[j].GetCustomAttributes(typeof(SQLiteFunctionAttribute), false);
								int length2 = (int)customAttributes.Length;
								for (int k = 0; k < length2; k++)
								{
									SQLiteFunctionAttribute sQLiteFunctionAttribute = customAttributes[k] as SQLiteFunctionAttribute;
									if (sQLiteFunctionAttribute != null)
									{
										sQLiteFunctionAttribute.InstanceType = types[j];
										SQLiteFunction.ReplaceFunction(sQLiteFunctionAttribute, null);
									}
								}
							}
						}
                    Label0:
                        break;
					}
				}
			}
			catch
			{
			}
		}

		protected SQLiteFunction()
		{
			this._contextDataList = new Dictionary<IntPtr, SQLiteFunction.AggregateData>();
		}

		protected SQLiteFunction(SQLiteDateFormats format, DateTimeKind kind, string formatString, bool utf16) : this()
		{
			if (utf16)
			{
				this._base = new SQLite3_UTF16(format, kind, formatString, IntPtr.Zero, null, false);
				return;
			}
			this._base = new SQLite3(format, kind, formatString, IntPtr.Zero, null, false);
		}

		internal static void BindFunction(SQLiteBase sqliteBase, SQLiteFunctionAttribute functionAttribute, SQLiteFunction function, SQLiteConnectionFlags flags)
		{
			SQLiteCallback sQLiteCallback;
			SQLiteCallback sQLiteCallback1;
			SQLiteFinalCallback sQLiteFinalCallback;
			SQLiteCollation sQLiteCollation;
			SQLiteCollation sQLiteCollation1;
			if (sqliteBase == null)
			{
				throw new ArgumentNullException("sqliteBase");
			}
			if (functionAttribute == null)
			{
				throw new ArgumentNullException("functionAttribute");
			}
			if (function == null)
			{
				throw new ArgumentNullException("function");
			}
			FunctionType funcType = functionAttribute.FuncType;
			function._base = sqliteBase;
			function._flags = flags;
			SQLiteFunction sQLiteFunction = function;
			if (funcType == FunctionType.Scalar)
			{
				sQLiteCallback = new SQLiteCallback(function.ScalarCallback);
			}
			else
			{
				sQLiteCallback = null;
			}
			sQLiteFunction._InvokeFunc = sQLiteCallback;
			SQLiteFunction sQLiteFunction1 = function;
			if (funcType == FunctionType.Aggregate)
			{
				sQLiteCallback1 = new SQLiteCallback(function.StepCallback);
			}
			else
			{
				sQLiteCallback1 = null;
			}
			sQLiteFunction1._StepFunc = sQLiteCallback1;
			SQLiteFunction sQLiteFunction2 = function;
			if (funcType == FunctionType.Aggregate)
			{
				sQLiteFinalCallback = new SQLiteFinalCallback(function.FinalCallback);
			}
			else
			{
				sQLiteFinalCallback = null;
			}
			sQLiteFunction2._FinalFunc = sQLiteFinalCallback;
			SQLiteFunction sQLiteFunction3 = function;
			if (funcType == FunctionType.Collation)
			{
				sQLiteCollation = new SQLiteCollation(function.CompareCallback);
			}
			else
			{
				sQLiteCollation = null;
			}
			sQLiteFunction3._CompareFunc = sQLiteCollation;
			SQLiteFunction sQLiteFunction4 = function;
			if (funcType == FunctionType.Collation)
			{
				sQLiteCollation1 = new SQLiteCollation(function.CompareCallback16);
			}
			else
			{
				sQLiteCollation1 = null;
			}
			sQLiteFunction4._CompareFunc16 = sQLiteCollation1;
			string name = functionAttribute.Name;
			if (funcType == FunctionType.Collation)
			{
				sqliteBase.CreateCollation(name, function._CompareFunc, function._CompareFunc16, true);
				return;
			}
			bool flag = function is SQLiteFunctionEx;
			sqliteBase.CreateFunction(name, functionAttribute.Arguments, flag, function._InvokeFunc, function._StepFunc, function._FinalFunc, true);
		}

		internal static IDictionary<SQLiteFunctionAttribute, SQLiteFunction> BindFunctions(SQLiteBase sqlbase, SQLiteConnectionFlags flags)
		{
			SQLiteFunction sQLiteFunction;
			IDictionary<SQLiteFunctionAttribute, SQLiteFunction> sQLiteFunctionAttributes = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();
			foreach (KeyValuePair<SQLiteFunctionAttribute, object> _registeredFunction in SQLiteFunction._registeredFunctions)
			{
				SQLiteFunctionAttribute key = _registeredFunction.Key;
				if (key == null)
				{
					continue;
				}
				if (!SQLiteFunction.CreateFunction(key, out sQLiteFunction))
				{
					sQLiteFunctionAttributes[key] = null;
				}
				else
				{
					SQLiteFunction.BindFunction(sqlbase, key, sQLiteFunction, flags);
					sQLiteFunctionAttributes[key] = sQLiteFunction;
				}
			}
			return sQLiteFunctionAttributes;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteFunction).Name);
			}
		}

		public virtual int Compare(string param1, string param2)
		{
			this.CheckDisposed();
			return 0;
		}

		internal int CompareCallback(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
		{
			int num;
			try
			{
				num = this.Compare(System.Data.SQLite.SQLiteConvert.UTF8ToString(ptr1, len1), System.Data.SQLite.SQLiteConvert.UTF8ToString(ptr2, len2));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this._flags))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "Compare", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
				if (this._base != null && this._base.IsOpen())
				{
					this._base.Cancel();
				}
				return 0;
			}
			return num;
		}

		internal int CompareCallback16(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
		{
			int num;
			try
			{
				num = this.Compare(SQLite3_UTF16.UTF16ToString(ptr1, len1), SQLite3_UTF16.UTF16ToString(ptr2, len2));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this._flags))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "Compare (UTF16)", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
				if (this._base != null && this._base.IsOpen())
				{
					this._base.Cancel();
				}
				return 0;
			}
			return num;
		}

		internal object[] ConvertParams(int nArgs, IntPtr argsptr)
		{
			object[] paramValueInt64 = new object[nArgs];
			IntPtr[] intPtrArray = new IntPtr[nArgs];
			Marshal.Copy(argsptr, intPtrArray, 0, nArgs);
			int num = 0;
			while (num < nArgs)
			{
                var x = (int)this._base.GetParamValueType(intPtrArray[num]);

                switch (x)
				{
					case (int)TypeAffinity.Int64:
					{
						paramValueInt64[num] = this._base.GetParamValueInt64(intPtrArray[num]);
						goto case 9;
					}
					case (int)TypeAffinity.Double:
					{
						paramValueInt64[num] = this._base.GetParamValueDouble(intPtrArray[num]);
						goto case 9;
					}
					case (int)TypeAffinity.Text:
					{
						paramValueInt64[num] = this._base.GetParamValueText(intPtrArray[num]);
						goto case 9;
					}
					case (int)TypeAffinity.Blob:
					{
						int paramValueBytes = (int)this._base.GetParamValueBytes(intPtrArray[num], 0, null, 0, 0);
						byte[] numArray = new byte[paramValueBytes];
						this._base.GetParamValueBytes(intPtrArray[num], 0, numArray, 0, paramValueBytes);
						paramValueInt64[num] = numArray;
						goto case 9;
					}
					case (int)TypeAffinity.Null:
					{
						paramValueInt64[num] = DBNull.Value;
						goto case 9;
					}
					case (int)TypeAffinity.Double | (int)TypeAffinity.Blob:
					case (int)TypeAffinity.Int64 | (int)TypeAffinity.Double | (int)TypeAffinity.Text | (int)TypeAffinity.Blob | (int)TypeAffinity.Null:
					case 8:
					case 9:
					{
						num++;
						continue;
					}
					case (int)TypeAffinity.DateTime:
					{
						paramValueInt64[num] = this._base.ToDateTime(this._base.GetParamValueText(intPtrArray[num]));
						goto case 9;
					}
					default:
					{
						goto case 9;
					}
				}
			}
			return paramValueInt64;
		}

		private static bool CreateFunction(SQLiteFunctionAttribute functionAttribute, out SQLiteFunction function)
		{
			if (functionAttribute == null)
			{
				function = null;
				return false;
			}
			if (functionAttribute.Callback1 != null || functionAttribute.Callback2 != null)
			{
				function = new SQLiteDelegateFunction(functionAttribute.Callback1, functionAttribute.Callback2);
				return true;
			}
			if (functionAttribute.InstanceType == null)
			{
				function = null;
				return false;
			}
			function = (SQLiteFunction)Activator.CreateInstance(functionAttribute.InstanceType);
			return true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					foreach (KeyValuePair<IntPtr, SQLiteFunction.AggregateData> keyValuePair in this._contextDataList)
					{
						IDisposable value = keyValuePair.Value._data as IDisposable;
						if (value == null)
						{
							continue;
						}
						value.Dispose();
					}
					this._contextDataList.Clear();
					this._contextDataList = null;
					this._flags = SQLiteConnectionFlags.None;
					this._InvokeFunc = null;
					this._StepFunc = null;
					this._FinalFunc = null;
					this._CompareFunc = null;
					this._base = null;
				}
				this.disposed = true;
			}
		}

		public virtual object Final(object contextData)
		{
			this.CheckDisposed();
			return null;
		}

		internal void FinalCallback(IntPtr context)
		{
			SQLiteFunction.AggregateData aggregateDatum;
			try
			{
				object obj = null;
				if (this._base != null)
				{
					IntPtr intPtr = this._base.AggregateContext(context);
					if (this._contextDataList != null && this._contextDataList.TryGetValue(intPtr, out aggregateDatum))
					{
						obj = aggregateDatum._data;
						this._contextDataList.Remove(intPtr);
					}
				}
				try
				{
					this._context = context;
					this.SetReturnValue(context, this.Final(obj));
				}
				finally
				{
					IDisposable disposable = obj as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this._flags))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "Final", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
			}
		}

		~SQLiteFunction()
		{
			this.Dispose(false);
		}

		public virtual object Invoke(object[] args)
		{
			this.CheckDisposed();
			return null;
		}

		public static void RegisterFunction(Type typ)
		{
			object[] customAttributes = typ.GetCustomAttributes(typeof(SQLiteFunctionAttribute), false);
			for (int i = 0; i < (int)customAttributes.Length; i++)
			{
				SQLiteFunctionAttribute sQLiteFunctionAttribute = customAttributes[i] as SQLiteFunctionAttribute;
				if (sQLiteFunctionAttribute != null)
				{
					SQLiteFunction.RegisterFunction(sQLiteFunctionAttribute.Name, sQLiteFunctionAttribute.Arguments, sQLiteFunctionAttribute.FuncType, typ, sQLiteFunctionAttribute.Callback1, sQLiteFunctionAttribute.Callback2);
				}
			}
		}

		public static void RegisterFunction(string name, int argumentCount, FunctionType functionType, Type instanceType, Delegate callback1, Delegate callback2)
		{
			SQLiteFunctionAttribute sQLiteFunctionAttribute = new SQLiteFunctionAttribute(name, argumentCount, functionType)
			{
				InstanceType = instanceType,
				Callback1 = callback1,
				Callback2 = callback2
			};
			SQLiteFunction.ReplaceFunction(sQLiteFunctionAttribute, null);
		}

		private static bool ReplaceFunction(SQLiteFunctionAttribute at, object newValue)
		{
			object obj;
			if (!SQLiteFunction._registeredFunctions.TryGetValue(at, out obj))
			{
				SQLiteFunction._registeredFunctions.Add(at, newValue);
				return false;
			}
			IDisposable disposable = obj as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
				disposable = null;
			}
			SQLiteFunction._registeredFunctions[at] = newValue;
			return true;
		}

		internal void ScalarCallback(IntPtr context, int nArgs, IntPtr argsptr)
		{
			try
			{
				this._context = context;
				this.SetReturnValue(context, this.Invoke(this.ConvertParams(nArgs, argsptr)));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this._flags))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "Invoke", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
			}
		}

		private void SetReturnValue(IntPtr context, object returnValue)
		{
			if (returnValue == null || returnValue == DBNull.Value)
			{
				this._base.ReturnNull(context);
				return;
			}
			Type type = returnValue.GetType();
			if (type == typeof(DateTime))
			{
				this._base.ReturnText(context, this._base.ToString((DateTime)returnValue));
				return;
			}
			Exception exception = returnValue as Exception;
			if (exception != null)
			{
				this._base.ReturnError(context, exception.Message);
				return;
			}
			switch (System.Data.SQLite.SQLiteConvert.TypeToAffinity(type, this._flags))
			{
				case TypeAffinity.Int64:
				{
					this._base.ReturnInt64(context, Convert.ToInt64(returnValue, CultureInfo.CurrentCulture));
					return;
				}
				case TypeAffinity.Double:
				{
					this._base.ReturnDouble(context, Convert.ToDouble(returnValue, CultureInfo.CurrentCulture));
					return;
				}
				case TypeAffinity.Text:
				{
					this._base.ReturnText(context, returnValue.ToString());
					return;
				}
				case TypeAffinity.Blob:
				{
					this._base.ReturnBlob(context, (byte[])returnValue);
					return;
				}
				case TypeAffinity.Null:
				{
					this._base.ReturnNull(context);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public virtual void Step(object[] args, int stepNumber, ref object contextData)
		{
			this.CheckDisposed();
		}

		internal void StepCallback(IntPtr context, int nArgs, IntPtr argsptr)
		{
			try
			{
				SQLiteFunction.AggregateData aggregateDatum = null;
				if (this._base != null)
				{
					IntPtr intPtr = this._base.AggregateContext(context);
					if (this._contextDataList != null && !this._contextDataList.TryGetValue(intPtr, out aggregateDatum))
					{
						aggregateDatum = new SQLiteFunction.AggregateData();
						this._contextDataList[intPtr] = aggregateDatum;
					}
				}
				if (aggregateDatum == null)
				{
					aggregateDatum = new SQLiteFunction.AggregateData();
				}
				try
				{
					this._context = context;
					this.Step(this.ConvertParams(nArgs, argsptr), aggregateDatum._count, ref aggregateDatum._data);
				}
				finally
				{
					aggregateDatum._count++;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this._flags))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "Step", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
			}
		}

		internal static bool UnbindAllFunctions(SQLiteBase sqlbase, SQLiteConnectionFlags flags, bool registered)
		{
			SQLiteFunction sQLiteFunction;
			if (sqlbase == null)
			{
				return false;
			}
			IDictionary<SQLiteFunctionAttribute, SQLiteFunction> functions = sqlbase.Functions;
			if (functions == null)
			{
				return false;
			}
			bool flag = true;
			if (!registered)
			{
				functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>(functions);
				foreach (KeyValuePair<SQLiteFunctionAttribute, SQLiteFunction> function in functions)
				{
					SQLiteFunctionAttribute key = function.Key;
					if (key == null)
					{
						continue;
					}
					SQLiteFunction value = function.Value;
					if (value == null || !SQLiteFunction.UnbindFunction(sqlbase, key, value, flags))
					{
						flag = false;
					}
					else
					{
						sqlbase.Functions.Remove(key);
					}
				}
			}
			else
			{
				foreach (KeyValuePair<SQLiteFunctionAttribute, object> _registeredFunction in SQLiteFunction._registeredFunctions)
				{
					SQLiteFunctionAttribute sQLiteFunctionAttribute = _registeredFunction.Key;
					if (sQLiteFunctionAttribute == null || functions.TryGetValue(sQLiteFunctionAttribute, out sQLiteFunction) && sQLiteFunction != null && SQLiteFunction.UnbindFunction(sqlbase, sQLiteFunctionAttribute, sQLiteFunction, flags))
					{
						continue;
					}
					flag = false;
				}
			}
			return flag;
		}

		internal static bool UnbindFunction(SQLiteBase sqliteBase, SQLiteFunctionAttribute functionAttribute, SQLiteFunction function, SQLiteConnectionFlags flags)
		{
			if (sqliteBase == null)
			{
				throw new ArgumentNullException("sqliteBase");
			}
			if (functionAttribute == null)
			{
				throw new ArgumentNullException("functionAttribute");
			}
			if (function == null)
			{
				throw new ArgumentNullException("function");
			}
			FunctionType funcType = functionAttribute.FuncType;
			string name = functionAttribute.Name;
			if (funcType == FunctionType.Collation)
			{
				return sqliteBase.CreateCollation(name, null, null, false) == SQLiteErrorCode.Ok;
			}
			bool flag = function is SQLiteFunctionEx;
			return sqliteBase.CreateFunction(name, functionAttribute.Arguments, flag, null, null, null, false) == SQLiteErrorCode.Ok;
		}

		private class AggregateData
		{
			internal int _count;

			internal object _data;

			public AggregateData()
			{
			}
		}
	}
}