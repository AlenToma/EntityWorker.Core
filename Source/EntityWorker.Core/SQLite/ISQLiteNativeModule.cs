using System;

namespace EntityWorker.Core.SQLite
{
	public interface ISQLiteNativeModule
	{
		SQLiteErrorCode xBegin(IntPtr pVtab);

		SQLiteErrorCode xBestIndex(IntPtr pVtab, IntPtr pIndex);

		SQLiteErrorCode xClose(IntPtr pCursor);

		SQLiteErrorCode xColumn(IntPtr pCursor, IntPtr pContext, int index);

		SQLiteErrorCode xCommit(IntPtr pVtab);

		SQLiteErrorCode xConnect(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

		SQLiteErrorCode xCreate(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

		SQLiteErrorCode xDestroy(IntPtr pVtab);

		SQLiteErrorCode xDisconnect(IntPtr pVtab);

		int xEof(IntPtr pCursor);

		SQLiteErrorCode xFilter(IntPtr pCursor, int idxNum, IntPtr idxStr, int argc, IntPtr argv);

		int xFindFunction(IntPtr pVtab, int nArg, IntPtr zName, ref SQLiteCallback callback, ref IntPtr pClientData);

		SQLiteErrorCode xNext(IntPtr pCursor);

		SQLiteErrorCode xOpen(IntPtr pVtab, ref IntPtr pCursor);

		SQLiteErrorCode xRelease(IntPtr pVtab, int iSavepoint);

		SQLiteErrorCode xRename(IntPtr pVtab, IntPtr zNew);

		SQLiteErrorCode xRollback(IntPtr pVtab);

		SQLiteErrorCode xRollbackTo(IntPtr pVtab, int iSavepoint);

		SQLiteErrorCode xRowId(IntPtr pCursor, ref long rowId);

		SQLiteErrorCode xSavepoint(IntPtr pVtab, int iSavepoint);

		SQLiteErrorCode xSync(IntPtr pVtab);

		SQLiteErrorCode xUpdate(IntPtr pVtab, int argc, IntPtr argv, ref long rowId);
	}
}