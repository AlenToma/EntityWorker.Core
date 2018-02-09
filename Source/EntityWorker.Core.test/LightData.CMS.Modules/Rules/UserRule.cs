using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using LightData.CMS.Modules.Library;

namespace LightData.CMS.Modules.Rules
{
    public class UserRule : IDbRuleTrigger<User>
    {
        public void AfterSave(IRepository repository, User itemDbEntity, object objectId)
        {

        }

        public void BeforeSave(IRepository repository, User itemDbEntity)
        {

        }
    }
}
