using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	public class SQLiteModuleNoop : SQLiteModule
	{
		private Dictionary<string, SQLiteErrorCode> resultCodes;

		private bool disposed;

		public SQLiteModuleNoop(string name) : base(name)
		{
			this.resultCodes = new Dictionary<string, SQLiteErrorCode>();
		}

		public override SQLiteErrorCode Begin(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Begin");
		}

		public override SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("BestIndex");
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteModuleNoop).Name);
			}
		}

		public override SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Close");
		}

		public override SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Column");
		}

		public override SQLiteErrorCode Commit(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Commit");
		}

		public override SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Connect");
		}

		public override SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Create");
		}

		public override SQLiteErrorCode Destroy(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Destroy");
		}

		public override SQLiteErrorCode Disconnect(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Disconnect");
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				bool flag = this.disposed;
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		public override bool Eof(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			return this.ResultCodeToEofResult(this.GetMethodResultCode("Eof"));
		}

		public override SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Filter");
		}

		public override bool FindFunction(SQLiteVirtualTable table, int argumentCount, string name, ref SQLiteFunction function, ref IntPtr pClientData)
		{
			this.CheckDisposed();
			return this.ResultCodeToFindFunctionResult(this.GetMethodResultCode("FindFunction"));
		}

		protected virtual SQLiteErrorCode GetDefaultResultCode()
		{
			return SQLiteErrorCode.Ok;
		}

		protected virtual SQLiteErrorCode GetMethodResultCode(string methodName)
		{
			SQLiteErrorCode sQLiteErrorCode;
			if (methodName == null || this.resultCodes == null)
			{
				return this.GetDefaultResultCode();
			}
			if (this.resultCodes != null && this.resultCodes.TryGetValue(methodName, out sQLiteErrorCode))
			{
				return sQLiteErrorCode;
			}
			return this.GetDefaultResultCode();
		}

		public override SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Next");
		}

		public override SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Open");
		}

		public override SQLiteErrorCode Release(SQLiteVirtualTable table, int savepoint)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Release");
		}

		public override SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Rename");
		}

		protected virtual bool ResultCodeToEofResult(SQLiteErrorCode resultCode)
		{
			if (resultCode != SQLiteErrorCode.Ok)
			{
				return true;
			}
			return false;
		}

		protected virtual bool ResultCodeToFindFunctionResult(SQLiteErrorCode resultCode)
		{
			if (resultCode != SQLiteErrorCode.Ok)
			{
				return false;
			}
			return true;
		}

		public override SQLiteErrorCode Rollback(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Rollback");
		}

		public override SQLiteErrorCode RollbackTo(SQLiteVirtualTable table, int savepoint)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("RollbackTo");
		}

		public override SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("RowId");
		}

		public override SQLiteErrorCode Savepoint(SQLiteVirtualTable table, int savepoint)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Savepoint");
		}

		protected virtual bool SetMethodResultCode(string methodName, SQLiteErrorCode resultCode)
		{
			if (methodName == null || this.resultCodes == null)
			{
				return false;
			}
			this.resultCodes[methodName] = resultCode;
			return true;
		}

		public override SQLiteErrorCode Sync(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Sync");
		}

		public override SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId)
		{
			this.CheckDisposed();
			return this.GetMethodResultCode("Update");
		}
	}
}