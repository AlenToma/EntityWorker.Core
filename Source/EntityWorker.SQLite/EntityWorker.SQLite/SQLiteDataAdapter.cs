using System;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;

namespace EntityWorker.SQLite
{
	[DefaultEvent("RowUpdated")]
	[Designer("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ToolboxItem("SQLite.Designer.SQLiteDataAdapterToolboxItem, SQLite.Designer, Version=1.0.106.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139")]
	public sealed class SQLiteDataAdapter : DbDataAdapter
	{
		private bool disposeSelect = true;

		private static object _updatingEventPH;

		private static object _updatedEventPH;

		private bool disposed;

		[DefaultValue(null)]
		[Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public new SQLiteCommand DeleteCommand
		{
			get
			{
				this.CheckDisposed();
				return (SQLiteCommand)base.DeleteCommand;
			}
			set
			{
				this.CheckDisposed();
				base.DeleteCommand = value;
			}
		}

		[DefaultValue(null)]
		[Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public new SQLiteCommand InsertCommand
		{
			get
			{
				this.CheckDisposed();
				return (SQLiteCommand)base.InsertCommand;
			}
			set
			{
				this.CheckDisposed();
				base.InsertCommand = value;
			}
		}

		[DefaultValue(null)]
		[Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public new SQLiteCommand SelectCommand
		{
			get
			{
				this.CheckDisposed();
				return (SQLiteCommand)base.SelectCommand;
			}
			set
			{
				this.CheckDisposed();
				base.SelectCommand = value;
			}
		}

		[DefaultValue(null)]
		[Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public new SQLiteCommand UpdateCommand
		{
			get
			{
				this.CheckDisposed();
				return (SQLiteCommand)base.UpdateCommand;
			}
			set
			{
				this.CheckDisposed();
				base.UpdateCommand = value;
			}
		}

		static SQLiteDataAdapter()
		{
			SQLiteDataAdapter._updatingEventPH = new object();
			SQLiteDataAdapter._updatedEventPH = new object();
		}

		public SQLiteDataAdapter()
		{
		}

		public SQLiteDataAdapter(SQLiteCommand cmd)
		{
			this.SelectCommand = cmd;
			this.disposeSelect = false;
		}

		public SQLiteDataAdapter(string commandText, SQLiteConnection connection)
		{
			this.SelectCommand = new SQLiteCommand(commandText, connection);
		}

		public SQLiteDataAdapter(string commandText, string connectionString) : this(commandText, connectionString, false)
		{
		}

		public SQLiteDataAdapter(string commandText, string connectionString, bool parseViaFramework)
		{
			this.SelectCommand = new SQLiteCommand(commandText, new SQLiteConnection(connectionString, parseViaFramework));
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteDataAdapter).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing)
				{
					if (this.disposeSelect && this.SelectCommand != null)
					{
						this.SelectCommand.Dispose();
						this.SelectCommand = null;
					}
					if (this.InsertCommand != null)
					{
						this.InsertCommand.Dispose();
						this.InsertCommand = null;
					}
					if (this.UpdateCommand != null)
					{
						this.UpdateCommand.Dispose();
						this.UpdateCommand = null;
					}
					if (this.DeleteCommand != null)
					{
						this.DeleteCommand.Dispose();
						this.DeleteCommand = null;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		internal static Delegate FindBuilder(MulticastDelegate mcd)
		{
			if (mcd != null)
			{
				Delegate[] invocationList = mcd.GetInvocationList();
				for (int i = 0; i < (int)invocationList.Length; i++)
				{
					if (invocationList[i].Target is DbCommandBuilder)
					{
						return invocationList[i];
					}
				}
			}
			return null;
		}

		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			EventHandler<RowUpdatedEventArgs> item = base.Events[SQLiteDataAdapter._updatedEventPH] as EventHandler<RowUpdatedEventArgs>;
			if (item != null)
			{
				item(this, value);
			}
		}

		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			EventHandler<RowUpdatingEventArgs> item = base.Events[SQLiteDataAdapter._updatingEventPH] as EventHandler<RowUpdatingEventArgs>;
			if (item != null)
			{
				item(this, value);
			}
		}

		public event EventHandler<RowUpdatedEventArgs> RowUpdated
		{
			add
			{
				this.CheckDisposed();
				base.Events.AddHandler(SQLiteDataAdapter._updatedEventPH, value);
			}
			remove
			{
				this.CheckDisposed();
				base.Events.RemoveHandler(SQLiteDataAdapter._updatedEventPH, value);
			}
		}

		public event EventHandler<RowUpdatingEventArgs> RowUpdating
		{
			add
			{
				this.CheckDisposed();
				EventHandler<RowUpdatingEventArgs> item = (EventHandler<RowUpdatingEventArgs>)base.Events[SQLiteDataAdapter._updatingEventPH];
				if (item != null && value.Target is DbCommandBuilder)
				{
					EventHandler<RowUpdatingEventArgs> eventHandler = (EventHandler<RowUpdatingEventArgs>)SQLiteDataAdapter.FindBuilder(item);
					if (eventHandler != null)
					{
						base.Events.RemoveHandler(SQLiteDataAdapter._updatingEventPH, eventHandler);
					}
				}
				base.Events.AddHandler(SQLiteDataAdapter._updatingEventPH, value);
			}
			remove
			{
				this.CheckDisposed();
				base.Events.RemoveHandler(SQLiteDataAdapter._updatingEventPH, value);
			}
		}
	}
}