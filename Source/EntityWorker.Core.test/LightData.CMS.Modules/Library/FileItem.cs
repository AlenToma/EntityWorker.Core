using System;
using System.Collections.Generic;
using System.Text;
using EntityWorker.Core.Attributes;
using LightData.CMS.Modules.Helper;

namespace LightData.CMS.Modules.Library
{
    [Table("FileItems")]
    public class FileItem
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }


        [ForeignKey(typeof(Folder))]
        public System.Guid Folder_Id { get; set; }

        [IndependentData]
        public Folder Folder { get; set; }

        public List<Slider> Slider { get; set; }

        /// <summary>
        /// Path to image on disk
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// PDF, PNG, JPG....
        /// </summary>
        [Stringify]
        public EnumHelper.AllowedFiles FileType { get; set; }


        public byte[] File { get; set; }

        public byte[] ThumpFile { get; set; }

        /// <summary>
        /// Bytes Length
        /// </summary>
        public int Length { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Alt { get; set; }

        public string Title { get; set; }

        public string BorderColor { get; set; }

        public int BorderWidth { get; set; }

        private string _text;
        [ExcludeFromAbstract]
        public string Text
        {
            get
            {
                if (_text != null)
                    return _text;
                if (File != null && (FileType == EnumHelper.AllowedFiles.CSS || FileType == EnumHelper.AllowedFiles.JAVASCRIPT || FileType == EnumHelper.AllowedFiles.HtmlEmbedded))
                {
                    _text = Uri.EscapeDataString(Encoding.UTF8.GetString(File));
                }
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        /// <summary>
        /// load file as 64bytes
        /// </summary>
        /// <returns></returns>
        [ExcludeFromAbstract]
        public string Base64ThumpFile
        {
            get
            {
                return ThumpFile != null ? Convert.ToBase64String(ThumpFile) : null;
            }
        }

        /// <summary>
        /// load file as 64bytes
        /// </summary>
        /// <returns></returns>
        [ExcludeFromAbstract]
        public string Base64File
        {
            get
            {
                return File != null ? Convert.ToBase64String(File) : null;
            }
        }
    }
}
