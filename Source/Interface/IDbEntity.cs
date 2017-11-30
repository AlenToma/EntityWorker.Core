using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using FastDeepCloner;

namespace EntityWorker.Core.InterFace
{
    public interface IDbEntity
    {
        /// <summary>
        /// Triggered when primary id changed
        /// </summary>
        event Events.IdChanged OnIdChanged;

        /// <summary>
        /// invoked when the id get changed only
        /// </summary>
        /// event MethodHelper.IdChanged OnIdChanged;
        /// <summary>
        /// Primary Id, when overrides you have to implement PropertyName Attribute
        /// </summary>
        [PrimaryKey]
        long Id { get; set; }

        /// <summary>
        /// Change the State, this is useful when using in DbRuleTrigger only
        /// </summary>
        [ExcludeFromAbstract]
        ItemState State { get; set; }

        /// <summary>
        /// Clone the object
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        IDbEntity Clone(FieldType fieldType = FieldType.PropertyInfo);
    }
}
