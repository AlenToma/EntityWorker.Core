using System;
using static EntityWorker.Core.FastDeepCloner.Extensions;

namespace EntityWorker.Core.FastDeepCloner
{
    public class FastDeepClonerSettings
    {
        public FieldType FieldType { get; set; }

        public CloneLevel CloneLevel { get; set; }

        /// <summary>
        /// override Activator CreateInstance and use your own method
        /// </summary>
        public CreateInstance OnCreateInstance { get; set; }


        public FastDeepClonerSettings()
        {
            OnCreateInstance = new CreateInstance((Type type) =>
            {
                return type.Creator();
            });
        }

    }
}
