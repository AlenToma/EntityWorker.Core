using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// Interface for Entity Rules
    /// read https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/modules.md for more information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbRuleTrigger<T> where T: class
    {
        /// <summary>
        /// Event triggered before saving an item
        /// No need to commit your changes here as there is already a transaction create and a commit operation will be triggered 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="itemDbEntity"></param>
        void BeforeSave(IRepository repository, T itemDbEntity);

        /// <summary>
        /// triggered after saving an item
        /// No need to commit your changes here as there is already a transaction create and a commit operation will be triggered 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="itemDbEntity"></param>
        /// <param name="objectId"></param>
        void AfterSave(IRepository repository, T itemDbEntity, object objectId);


        /// <summary>
        /// trigger before deleting an item
        /// No need to commit your changes here as there is already a transaction create and a commit operation will be triggered 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="itemDbEntity"></param>
        void Delete(IRepository repository, T itemDbEntity);
    }
}
