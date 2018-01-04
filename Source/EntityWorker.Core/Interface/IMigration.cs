
using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// Inhert from this class and create a migration File
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMigration
    {
        /// <summary>
        /// Do your db Changes here
        /// </summary>
        /// <param name="repository"></param>
        void ExecuteMigration(IRepository repository);
    }
}
