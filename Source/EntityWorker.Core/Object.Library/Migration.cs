
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Object.Library
{
    /// <inheritdoc />
    public class Migration : IMigration
    {
        /// <summary>
        /// Default cto
        /// </summary>
        public Migration() { }

        /// <summary>
        ///  Make sure that the key dose not exist in the database
        /// </summary>
        public string MigrationIdentifier { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Do your db Changes here
        /// </summary>
        /// <param name="repository"></param>
        public virtual void ExecuteMigration(IRepository repository)
        {
            return;
        }
    }
}
