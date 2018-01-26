using EntityWorker.Core.Attributes;
using EntityWorker.Core.Object.Library;
using System.Collections.Generic;

namespace LightData.CMS.Modules.Library
{
    public class SliderCollection 
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Identifier { get; set; }

        public List<Slider> Sliders { get; set; }
    }
}
