using EntityWorker.Core.InterFace;
using System;


namespace LightData.CMS.Modules.Interface
{
    public interface IRepsitory : IDisposable
    {
        IRepository Repository { get; }

        T ConvertValue<T>(object itemToConvert);
    }
}
