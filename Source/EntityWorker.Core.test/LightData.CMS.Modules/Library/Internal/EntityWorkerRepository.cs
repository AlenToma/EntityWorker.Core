using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using LightData.CMS.Modules.Interface;

namespace LightData.CMS.Modules.Library.Internal
{
    public class Reposiory : IRepsitory
    {

        private DataBaseTypes  _dbType { get; set; }

        private IRepository _repository;

        public IRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new Repository.Repository();
                return _repository;
            }
        }

        public Reposiory(string dbType = "Mssql")
        {
            _dbType = dbType.ConvertValue<DataBaseTypes>();
        }

        public T ConvertValue<T>(object itemToConvert)
        {
           return MethodHelper.ConvertValue<T>(itemToConvert);
        }


        public void Dispose()
        {
            Repository.Dispose();
            _repository = null;
        }
    }
}
