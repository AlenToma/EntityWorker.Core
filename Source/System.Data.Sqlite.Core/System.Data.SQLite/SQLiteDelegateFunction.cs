using System;
using System.Globalization;

namespace System.Data.SQLite
{
	public class SQLiteDelegateFunction : SQLiteFunction
	{
		private const string NoCallbackError = "No \"{0}\" callback is set.";

		private const string ResultInt32Error = "\"{0}\" result must be Int32.";

		private Delegate callback1;

		private Delegate callback2;

		public virtual Delegate Callback1
		{
			get
			{
				return this.callback1;
			}
			set
			{
				this.callback1 = value;
			}
		}

		public virtual Delegate Callback2
		{
			get
			{
				return this.callback2;
			}
			set
			{
				this.callback2 = value;
			}
		}

		public SQLiteDelegateFunction() : this(null, null)
		{
		}

		public SQLiteDelegateFunction(Delegate callback1, Delegate callback2)
		{
			this.callback1 = callback1;
			this.callback2 = callback2;
		}

		public override int Compare(string param1, string param2)
		{
			if (this.callback1 == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { "Compare" };
				throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "No \"{0}\" callback is set.", objArray));
			}
			SQLiteCompareDelegate sQLiteCompareDelegate = this.callback1 as SQLiteCompareDelegate;
			if (sQLiteCompareDelegate != null)
			{
				return sQLiteCompareDelegate("Compare", param1, param2);
			}
			object obj = this.callback1.DynamicInvoke(this.GetCompareArgs(param1, param2, false));
			if (!(obj is int))
			{
				CultureInfo cultureInfo = CultureInfo.CurrentCulture;
				object[] objArray1 = new object[] { "Compare" };
				throw new InvalidOperationException(HelperMethods.StringFormat(cultureInfo, "\"{0}\" result must be Int32.", objArray1));
			}
			return (int)obj;
		}

		public override object Final(object contextData)
		{
			if (this.callback2 == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { "Final" };
				throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "No \"{0}\" callback is set.", objArray));
			}
			SQLiteFinalDelegate sQLiteFinalDelegate = this.callback2 as SQLiteFinalDelegate;
			if (sQLiteFinalDelegate != null)
			{
				return sQLiteFinalDelegate("Final", contextData);
			}
			return this.callback1.DynamicInvoke(this.GetFinalArgs(contextData, false));
		}

		protected virtual object[] GetCompareArgs(string param1, string param2, bool earlyBound)
		{
			object[] objArray = new object[] { "Compare", param1, param2 };
			if (!earlyBound)
			{
				objArray = new object[] { objArray };
			}
			return objArray;
		}

		protected virtual object[] GetFinalArgs(object contextData, bool earlyBound)
		{
			object[] objArray = new object[] { "Final", contextData };
			if (!earlyBound)
			{
				objArray = new object[] { objArray };
			}
			return objArray;
		}

		protected virtual object[] GetInvokeArgs(object[] args, bool earlyBound)
		{
			object[] objArray = new object[] { "Invoke", args };
			if (!earlyBound)
			{
				objArray = new object[] { objArray };
			}
			return objArray;
		}

		protected virtual object[] GetStepArgs(object[] args, int stepNumber, object contextData, bool earlyBound)
		{
			object[] objArray = new object[] { "Step", args, stepNumber, contextData };
			object[] objArray1 = objArray;
			if (!earlyBound)
			{
				objArray1 = new object[] { objArray1 };
			}
			return objArray1;
		}

		public override object Invoke(object[] args)
		{
			if (this.callback1 == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { "Invoke" };
				throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "No \"{0}\" callback is set.", objArray));
			}
			SQLiteInvokeDelegate sQLiteInvokeDelegate = this.callback1 as SQLiteInvokeDelegate;
			if (sQLiteInvokeDelegate != null)
			{
				return sQLiteInvokeDelegate("Invoke", args);
			}
			return this.callback1.DynamicInvoke(this.GetInvokeArgs(args, false));
		}

		public override void Step(object[] args, int stepNumber, ref object contextData)
		{
			if (this.callback1 == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { "Step" };
				throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "No \"{0}\" callback is set.", objArray));
			}
			SQLiteStepDelegate sQLiteStepDelegate = this.callback1 as SQLiteStepDelegate;
			if (sQLiteStepDelegate != null)
			{
				sQLiteStepDelegate("Step", args, stepNumber, ref contextData);
				return;
			}
			object[] stepArgs = this.GetStepArgs(args, stepNumber, contextData, false);
			this.callback1.DynamicInvoke(stepArgs);
			this.UpdateStepArgs(stepArgs, ref contextData, false);
		}

		protected virtual void UpdateStepArgs(object[] args, ref object contextData, bool earlyBound)
		{
			object[] objArray;
			objArray = (!earlyBound ? args[0] as object[] : args);
			if (objArray == null)
			{
				return;
			}
			contextData = objArray[(int)objArray.Length - 1];
		}
	}
}