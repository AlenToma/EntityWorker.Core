using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Interface
{
    public interface IDbRuleTrigger<T> where T: class
    {
        /// <summary>
        /// Event triggered before save an item
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="itemDbEntity"></param>
        void BeforeSave(IRepository repository, T itemDbEntity);

        /// <summary>
        /// triggered after saveing an item
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="itemDbEntity"></param>
        /// <param name="objectId"></param>
        void AfterSave(IRepository repository, T itemDbEntity, object objectId);
    }
}
