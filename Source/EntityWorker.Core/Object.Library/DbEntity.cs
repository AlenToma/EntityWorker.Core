using System.Runtime.Serialization;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using FastDeepCloner;
using System;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// All Tables Should inherit from this class
    /// </summary>
    public class DbEntity : IDbEntity
    {
        /// <summary>
        /// Notify when Id has been changed
        /// </summary>
        public event Events.IdChanged OnIdChanged;

        private long _id;

        /// <summary>
        /// Entity PrimaryKey
        /// </summary>
        [PrimaryKey]
        public virtual long Id
        {
            get => _id;
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnIdChanged?.Invoke(_id);
                }
            }
        }

        /// <summary>
        /// This should only be used IDbRuleTrigger
        /// Se https://github.com/AlenToma/EntityWorker.Core for full documentation
        /// </summary>
        [ExcludeFromAbstract]
        public virtual ItemState State { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DbEntity() { }

        /// <inheritdoc />
        /// <summary>
        /// Clone the object
        /// </summary>
        /// <param name="cloneLevel"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public IDbEntity Clone(CloneLevel cloneLevel = CloneLevel.Hierarki, FieldType fieldType = FieldType.PropertyInfo)
        {
            return DeepCloner.Clone(this, new FastDeepClonerSettings()
            {
                FieldType = fieldType,
                CloneLevel = cloneLevel,
                OnCreateInstance = new Extensions.CreateInstance(FormatterServices.GetUninitializedObject)
            });
        }

    }
}
