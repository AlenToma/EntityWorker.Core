using System;

namespace System.Data.SQLite
{
	public class SQLiteReadArrayEventArgs : SQLiteReadEventArgs
	{
		private long dataOffset;

		private byte[] byteBuffer;

		private char[] charBuffer;

		private int bufferOffset;

		private int length;

		public int BufferOffset
		{
			get
			{
				return this.bufferOffset;
			}
			set
			{
				this.bufferOffset = value;
			}
		}

		public byte[] ByteBuffer
		{
			get
			{
				return this.byteBuffer;
			}
		}

		public char[] CharBuffer
		{
			get
			{
				return this.charBuffer;
			}
		}

		public long DataOffset
		{
			get
			{
				return this.dataOffset;
			}
			set
			{
				this.dataOffset = value;
			}
		}

		public int Length
		{
			get
			{
				return this.length;
			}
			set
			{
				this.length = value;
			}
		}

		internal SQLiteReadArrayEventArgs(long dataOffset, byte[] byteBuffer, int bufferOffset, int length)
		{
			this.dataOffset = dataOffset;
			this.byteBuffer = byteBuffer;
			this.bufferOffset = bufferOffset;
			this.length = length;
		}

		internal SQLiteReadArrayEventArgs(long dataOffset, char[] charBuffer, int bufferOffset, int length)
		{
			this.dataOffset = dataOffset;
			this.charBuffer = charBuffer;
			this.bufferOffset = bufferOffset;
			this.length = length;
		}
	}
}