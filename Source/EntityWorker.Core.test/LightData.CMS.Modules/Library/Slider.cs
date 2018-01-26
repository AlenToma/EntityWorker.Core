using EntityWorker.Core.Attributes;
using EntityWorker.Core.Object.Library;

namespace LightData.CMS.Modules.Library
{
    public class Slider
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        [ForeignKey(typeof(FileItem))]
        public System.Guid FileItem_Id { get; set; }

        [ForeignKey(typeof(SliderCollection))]
        public System.Guid SliderCollection_Id { get; set; }

        [IndependentData]
        public SliderCollection SliderCollection { get; set; }

        [IndependentData]
        public FileItem File { get; set; }

        [DefaultOnEmpty(1)]
        [PropertyName("ColOrder")]
        public long Order { get; set; }
    }
}
