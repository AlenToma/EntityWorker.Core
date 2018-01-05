using System;

namespace EntityWorker.SQLite
{
    public sealed class SQLiteValue : ISQLiteNativeHandle
    {
        private IntPtr pValue;

        private bool persisted;

        private object @value;

        public IntPtr NativeHandle
        {
            get
            {
                return this.pValue;
            }
        }

        public bool Persisted
        {
            get
            {
                return this.persisted;
            }
        }

        public object Value
        {
            get
            {
                if (!this.persisted)
                {
                    throw new InvalidOperationException("value was not persisted");
                }
                return this.@value;
            }
        }

        private SQLiteValue(IntPtr pValue)
        {
            this.pValue = pValue;
        }

        internal static SQLiteValue[] ArrayFromSizeAndIntPtr(int argc, IntPtr argv)
        {
            SQLiteValue sQLiteValue;
            if (argc < 0)
            {
                return null;
            }
            if (argv == IntPtr.Zero)
            {
                return null;
            }
            SQLiteValue[] sQLiteValueArray = new SQLiteValue[argc];
            int num = 0;
            int size = 0;
            while (num < (int)sQLiteValueArray.Length)
            {
                IntPtr intPtr = SQLiteMarshal.ReadIntPtr(argv, size);
                SQLiteValue[] sQLiteValueArray1 = sQLiteValueArray;
                int num1 = num;
                if (intPtr != IntPtr.Zero)
                {
                    sQLiteValue = new SQLiteValue(intPtr);
                }
                else
                {
                    sQLiteValue = null;
                }
                sQLiteValueArray1[num1] = sQLiteValue;
                num++;
                size += IntPtr.Size;
            }
            return sQLiteValueArray;
        }

        internal static SQLiteValue FromIntPtr(IntPtr pValue)
        {
            if (pValue == IntPtr.Zero)
            {
                return null;
            }
            return new SQLiteValue(pValue);
        }

        public byte[] GetBlob()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return null;
            }
            return SQLiteBytes.FromIntPtr(UnsafeNativeMethods.sqlite3_value_blob(this.pValue), this.GetBytes());
        }

        public int GetBytes()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.sqlite3_value_bytes(this.pValue);
        }

        public double GetDouble()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.sqlite3_value_double(this.pValue);
        }

        public int GetInt()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.sqlite3_value_int(this.pValue);
        }

        public long GetInt64()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return (long)0;
            }
            return UnsafeNativeMethods.sqlite3_value_int64(this.pValue);
        }

        public object GetObject()
        {
            switch (this.GetTypeAffinity())
            {
                case TypeAffinity.Uninitialized:
                    {
                        return null;
                    }
                case TypeAffinity.Int64:
                    {
                        return this.GetInt64();
                    }
                case TypeAffinity.Double:
                    {
                        return this.GetDouble();
                    }
                case TypeAffinity.Text:
                    {
                        return this.GetString();
                    }
                case TypeAffinity.Blob:
                    {
                        return this.GetBytes();
                    }
                case TypeAffinity.Null:
                    {
                        return DBNull.Value;
                    }
            }
            return null;
        }

        public string GetString()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return null;
            }
            int num = 0;
            IntPtr intPtr = UnsafeNativeMethods.sqlite3_value_text_interop(this.pValue, ref num);
            return SQLiteString.StringFromUtf8IntPtr(intPtr, num);
        }

        public TypeAffinity GetTypeAffinity()
        {
            if (this.pValue == IntPtr.Zero)
            {
                return TypeAffinity.None;
            }
            return UnsafeNativeMethods.sqlite3_value_type(this.pValue);
        }

        public bool Persist()
        {
            switch (this.GetTypeAffinity())
            {
                case TypeAffinity.Uninitialized:
                    {
                        this.@value = null;
                        this.PreventNativeAccess();
                        var num = true;
                        bool flag = num;
                        this.persisted = num;
                        return flag;
                    }
                case TypeAffinity.Int64:
                    {
                        this.@value = this.GetInt64();
                        this.PreventNativeAccess();
                        var num1 = true;
                        bool flag1 = (bool)num1;
                        this.persisted = (bool)num1;
                        return flag1;
                    }
                case TypeAffinity.Double:
                    {
                        this.@value = this.GetDouble();
                        this.PreventNativeAccess();
                        var num2 = true;
                        bool flag2 = (bool)num2;
                        this.persisted = (bool)num2;
                        return flag2;
                    }
                case TypeAffinity.Text:
                    {
                        this.@value = this.GetString();
                        this.PreventNativeAccess();
                        var num3 = true;
                        bool flag3 = (bool)num3;
                        this.persisted = (bool)num3;
                        return flag3;
                    }
                case TypeAffinity.Blob:
                    {
                        this.@value = this.GetBytes();
                        this.PreventNativeAccess();
                        var num4 = true;
                        bool flag4 = (bool)num4;
                        this.persisted = (bool)num4;
                        return flag4;
                    }
                case TypeAffinity.Null:
                    {
                        this.@value = DBNull.Value;
                        this.PreventNativeAccess();
                        var num5 = true;
                        bool flag5 = (bool)num5;
                        this.persisted = (bool)num5;
                        return flag5;
                    }
            }
            return false;
        }

        private void PreventNativeAccess()
        {
            this.pValue = IntPtr.Zero;
        }
    }
}