using System;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace EntityWorker.SQLite
{
	public sealed class SQLiteFactory : DbProviderFactory, IDisposable, IServiceProvider
	{
		private bool disposed;

		public readonly static SQLiteFactory Instance;

		private readonly static string DefaultTypeName;

		private readonly static BindingFlags DefaultBindingFlags;

		private static Type _dbProviderServicesType;

		private static object _sqliteServices;

		static SQLiteFactory()
		{
			SQLiteFactory.Instance = new SQLiteFactory();
			SQLiteFactory.DefaultTypeName = "EntityWorker.SQLite.Linq.SQLiteProviderServices, EntityWorker.SQLite.Linq, Version={0}, Culture=neutral, PublicKeyToken=db937bc2d44ff139";
			SQLiteFactory.DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
			EntityWorker.SQLite.UnsafeNativeMethods.Initialize();
			SQLiteLog.Initialize();
			string str = "3.5.0.0";
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { str };
			SQLiteFactory._dbProviderServicesType = Type.GetType(HelperMethods.StringFormat(invariantCulture, "System.Data.Common.DbProviderServices, System.Data.Entity, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", objArray), false);
		}

		public SQLiteFactory()
		{
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteFactory).Name);
			}
		}

		public override DbCommand CreateCommand()
		{
			this.CheckDisposed();
			return new SQLiteCommand();
		}

		public override DbCommandBuilder CreateCommandBuilder()
		{
			this.CheckDisposed();
			return new SQLiteCommandBuilder();
		}

		public override DbConnection CreateConnection()
		{
			this.CheckDisposed();
			return new SQLiteConnection();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			this.CheckDisposed();
			return new SQLiteConnectionStringBuilder();
		}

		public override DbDataAdapter CreateDataAdapter()
		{
			this.CheckDisposed();
			return new SQLiteDataAdapter();
		}

		public override DbParameter CreateParameter()
		{
			this.CheckDisposed();
			return new SQLiteParameter();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				this.disposed = true;
			}
		}

		protected void Finalize()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//base.Finalize();
			}
		}

		//[ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
		private object GetSQLiteProviderServicesInstance()
		{
			if (SQLiteFactory._sqliteServices == null)
			{
				string settingValue = EntityWorker.SQLite.UnsafeNativeMethods.GetSettingValue("TypeName_SQLiteProviderServices", null);
				Version version = base.GetType().Assembly.GetName().Version;
				if (settingValue == null)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string defaultTypeName = SQLiteFactory.DefaultTypeName;
					object[] objArray = new object[] { version };
					settingValue = HelperMethods.StringFormat(invariantCulture, defaultTypeName, objArray);
				}
				else
				{
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { version };
					settingValue = HelperMethods.StringFormat(cultureInfo, settingValue, objArray1);
				}
				Type type = Type.GetType(settingValue, false);
				if (type != null)
				{
					FieldInfo field = type.GetField("Instance", SQLiteFactory.DefaultBindingFlags);
					if (field != null)
					{
						SQLiteFactory._sqliteServices = field.GetValue(null);
					}
				}
			}
			return SQLiteFactory._sqliteServices;
		}

		object System.IServiceProvider.GetService(Type serviceType)
		{
			if (serviceType != typeof(ISQLiteSchemaExtensions) && (SQLiteFactory._dbProviderServicesType == null || serviceType != SQLiteFactory._dbProviderServicesType))
			{
				return null;
			}
			return this.GetSQLiteProviderServicesInstance();
		}

		public event SQLiteLogEventHandler Log
		{
			add
			{
				this.CheckDisposed();
				SQLiteLog.Log += value;
			}
			remove
			{
				this.CheckDisposed();
				SQLiteLog.Log -= value;
			}
		}
	}
}