using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace EntityWorker.SQLite
{
	internal static class SQLiteConnectionPool
	{
		private readonly static object _syncRoot;

		private static ISQLiteConnectionPool _connectionPool;

		private static SortedList<string, SQLiteConnectionPool.PoolQueue> _queueList;

		private static int _poolVersion;

		private static int _poolOpened;

		private static int _poolClosed;

		static SQLiteConnectionPool()
		{
			SQLiteConnectionPool._syncRoot = new object();
			SQLiteConnectionPool._connectionPool = null;
			SQLiteConnectionPool._queueList = new SortedList<string, SQLiteConnectionPool.PoolQueue>(StringComparer.OrdinalIgnoreCase);
			SQLiteConnectionPool._poolVersion = 1;
			SQLiteConnectionPool._poolOpened = 0;
			SQLiteConnectionPool._poolClosed = 0;
		}

		internal static void Add(string fileName, SQLiteConnectionHandle handle, int version)
		{
			SQLiteConnectionPool.PoolQueue poolQueue;
			ISQLiteConnectionPool connectionPool = SQLiteConnectionPool.GetConnectionPool();
			if (connectionPool != null)
			{
				connectionPool.Add(fileName, handle, version);
				return;
			}
			lock (SQLiteConnectionPool._syncRoot)
			{
				if (!SQLiteConnectionPool._queueList.TryGetValue(fileName, out poolQueue) || version != poolQueue.PoolVersion)
				{
					handle.Close();
				}
				else
				{
					SQLiteConnectionPool.ResizePool(poolQueue, true);
					Queue<WeakReference> queue = poolQueue.Queue;
					if (queue != null)
					{
						queue.Enqueue(new WeakReference(handle, false));
						Interlocked.Increment(ref SQLiteConnectionPool._poolClosed);
					}
					else
					{
						return;
					}
				}
				GC.KeepAlive(handle);
			}
		}

		internal static void ClearAllPools()
		{
			ISQLiteConnectionPool connectionPool = SQLiteConnectionPool.GetConnectionPool();
			if (connectionPool != null)
			{
				connectionPool.ClearAllPools();
				return;
			}
			lock (SQLiteConnectionPool._syncRoot)
			{
				foreach (KeyValuePair<string, SQLiteConnectionPool.PoolQueue> keyValuePair in SQLiteConnectionPool._queueList)
				{
					if (keyValuePair.Value == null)
					{
						continue;
					}
					Queue<WeakReference> queue = keyValuePair.Value.Queue;
					while (queue.Count > 0)
					{
						WeakReference weakReference = queue.Dequeue();
						if (weakReference == null)
						{
							continue;
						}
						SQLiteConnectionHandle target = weakReference.Target as SQLiteConnectionHandle;
						if (target != null)
						{
							target.Dispose();
						}
						GC.KeepAlive(target);
					}
					if (SQLiteConnectionPool._poolVersion > keyValuePair.Value.PoolVersion)
					{
						continue;
					}
					SQLiteConnectionPool._poolVersion = keyValuePair.Value.PoolVersion + 1;
				}
				SQLiteConnectionPool._queueList.Clear();
			}
		}

		internal static void ClearPool(string fileName)
		{
			SQLiteConnectionPool.PoolQueue poolQueue;
			ISQLiteConnectionPool connectionPool = SQLiteConnectionPool.GetConnectionPool();
			if (connectionPool != null)
			{
				connectionPool.ClearPool(fileName);
				return;
			}
			lock (SQLiteConnectionPool._syncRoot)
			{
				if (SQLiteConnectionPool._queueList.TryGetValue(fileName, out poolQueue))
				{
					poolQueue.PoolVersion++;
					Queue<WeakReference> queue = poolQueue.Queue;
					if (queue != null)
					{
						while (queue.Count > 0)
						{
							WeakReference weakReference = queue.Dequeue();
							if (weakReference == null)
							{
								continue;
							}
							SQLiteConnectionHandle target = weakReference.Target as SQLiteConnectionHandle;
							if (target != null)
							{
								target.Dispose();
							}
							GC.KeepAlive(target);
						}
					}
					else
					{
						return;
					}
				}
			}
		}

		internal static ISQLiteConnectionPool GetConnectionPool()
		{
			ISQLiteConnectionPool sQLiteConnectionPool;
			lock (SQLiteConnectionPool._syncRoot)
			{
				sQLiteConnectionPool = SQLiteConnectionPool._connectionPool;
			}
			return sQLiteConnectionPool;
		}

		internal static void GetCounts(string fileName, ref Dictionary<string, int> counts, ref int openCount, ref int closeCount, ref int totalCount)
		{
			SQLiteConnectionPool.PoolQueue poolQueue;
			ISQLiteConnectionPool connectionPool = SQLiteConnectionPool.GetConnectionPool();
			if (connectionPool != null)
			{
				connectionPool.GetCounts(fileName, ref counts, ref openCount, ref closeCount, ref totalCount);
				return;
			}
			lock (SQLiteConnectionPool._syncRoot)
			{
				openCount = SQLiteConnectionPool._poolOpened;
				closeCount = SQLiteConnectionPool._poolClosed;
				if (counts == null)
				{
					counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				}
				if (fileName == null)
				{
					foreach (KeyValuePair<string, SQLiteConnectionPool.PoolQueue> keyValuePair in SQLiteConnectionPool._queueList)
					{
						if (keyValuePair.Value == null)
						{
							continue;
						}
						Queue<WeakReference> queue = keyValuePair.Value.Queue;
						int num = (queue != null ? queue.Count : 0);
						counts.Add(keyValuePair.Key, num);
						totalCount += num;
					}
				}
				else if (SQLiteConnectionPool._queueList.TryGetValue(fileName, out poolQueue))
				{
					Queue<WeakReference> weakReferences = poolQueue.Queue;
					int num1 = (weakReferences != null ? weakReferences.Count : 0);
					counts.Add(fileName, num1);
					totalCount += num1;
				}
			}
		}

		internal static SQLiteConnectionHandle Remove(string fileName, int maxPoolSize, out int version)
		{
			int num;
			Queue<WeakReference> queue;
			SQLiteConnectionPool.PoolQueue poolQueue;
			SQLiteConnectionPool.PoolQueue poolQueue1;
			bool flag;
			SQLiteConnectionHandle sQLiteConnectionHandle;
			ISQLiteConnectionPool connectionPool = SQLiteConnectionPool.GetConnectionPool();
			if (connectionPool != null)
			{
				return connectionPool.Remove(fileName, maxPoolSize, out version) as SQLiteConnectionHandle;
			}
			lock (SQLiteConnectionPool._syncRoot)
			{
				version = SQLiteConnectionPool._poolVersion;
				if (SQLiteConnectionPool._queueList.TryGetValue(fileName, out poolQueue))
				{
					int poolVersion = poolQueue.PoolVersion;
					num = poolVersion;
					version = poolVersion;
					poolQueue.MaxPoolSize = maxPoolSize;
					SQLiteConnectionPool.ResizePool(poolQueue, false);
					queue = poolQueue.Queue;
					if (queue != null)
					{
						SQLiteConnectionPool._queueList.Remove(fileName);
						queue = new Queue<WeakReference>(queue);
						goto Label0;
					}
					else
					{
						sQLiteConnectionHandle = null;
					}
				}
				else
				{
					poolQueue = new SQLiteConnectionPool.PoolQueue(SQLiteConnectionPool._poolVersion, maxPoolSize);
					SQLiteConnectionPool._queueList.Add(fileName, poolQueue);
					sQLiteConnectionHandle = null;
				}
			}
			return sQLiteConnectionHandle;
		Label0:
			try
			{
				while (queue.Count > 0)
				{
					WeakReference weakReference = queue.Dequeue();
					if (weakReference == null)
					{
						continue;
					}
					SQLiteConnectionHandle target = weakReference.Target as SQLiteConnectionHandle;
					if (target == null)
					{
						continue;
					}
					GC.SuppressFinalize(target);
					try
					{
						GC.WaitForPendingFinalizers();
						if (!target.IsInvalid && !target.IsClosed)
						{
							Interlocked.Increment(ref SQLiteConnectionPool._poolOpened);
							sQLiteConnectionHandle = target;
							return sQLiteConnectionHandle;
						}
					}
					finally
					{
						GC.ReRegisterForFinalize(target);
					}
					GC.KeepAlive(target);
				}
			}
			finally
			{
				lock (SQLiteConnectionPool._syncRoot)
				{
					if (!SQLiteConnectionPool._queueList.TryGetValue(fileName, out poolQueue1))
					{
						flag = true;
						poolQueue1 = new SQLiteConnectionPool.PoolQueue(num, maxPoolSize);
					}
					else
					{
						flag = false;
					}
					Queue<WeakReference> weakReferences = poolQueue1.Queue;
					while (queue.Count > 0)
					{
						weakReferences.Enqueue(queue.Dequeue());
					}
					SQLiteConnectionPool.ResizePool(poolQueue1, false);
					if (flag)
					{
						SQLiteConnectionPool._queueList.Add(fileName, poolQueue1);
					}
				}
			}
			return null;
		}

		private static void ResizePool(SQLiteConnectionPool.PoolQueue queue, bool add)
		{
			int maxPoolSize = queue.MaxPoolSize;
			if (add && maxPoolSize > 0)
			{
				maxPoolSize--;
			}
			Queue<WeakReference> weakReferences = queue.Queue;
			if (weakReferences == null)
			{
				return;
			}
			while (weakReferences.Count > maxPoolSize)
			{
				WeakReference weakReference = weakReferences.Dequeue();
				if (weakReference == null)
				{
					continue;
				}
				SQLiteConnectionHandle target = weakReference.Target as SQLiteConnectionHandle;
				if (target != null)
				{
					target.Dispose();
				}
				GC.KeepAlive(target);
			}
		}

		internal static void SetConnectionPool(ISQLiteConnectionPool connectionPool)
		{
			lock (SQLiteConnectionPool._syncRoot)
			{
				SQLiteConnectionPool._connectionPool = connectionPool;
			}
		}

		private sealed class PoolQueue
		{
			internal readonly Queue<WeakReference> Queue;

			internal int PoolVersion;

			internal int MaxPoolSize;

			internal PoolQueue(int version, int maxSize)
			{
				this.PoolVersion = version;
				this.MaxPoolSize = maxSize;
			}
		}
	}
}