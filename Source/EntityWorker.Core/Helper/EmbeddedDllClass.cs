using System;
using System.IO;
using System.Linq;
using System.Reflection;


namespace EntityWorker.Core.Helper
{
    internal class EmbeddedDllClass
    {
        public static void LoadAllDll()
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            if (IntPtr.Size == 8)
            {
                var path = Path.Combine(string.Join("\\", assem.Location.Split('\\').Reverse().Skip(1).Reverse()), "x64");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, "SQLite.Interop.dll");

                if (!File.Exists(path))
                    File.WriteAllBytes(path, Properties.x64.SQLite_Interop);
            }
            else if (IntPtr.Size == 4)
            {
                var path = Path.Combine(string.Join("\\", assem.Location.Split('\\').Reverse().Skip(1).Reverse()), "x86");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, "SQLite.Interop.dll");

                if (!File.Exists(path))
                    File.WriteAllBytes(path, Properties.x86.SQLite_Interop);

            }
        }
    }
}