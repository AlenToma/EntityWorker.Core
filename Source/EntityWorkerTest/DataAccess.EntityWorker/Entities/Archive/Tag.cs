using System;
using System.Collections.ObjectModel;
using System.IO;
using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(Tag))]
    public class Tag : EntityBase
    {
        private SongStatus _status;
        private DateTime? _fileChanged;
        private DateTime _fileCreated = DateTime.Now;

        public Tag()
        {
        }


        [NotNullable]
        public string Filename { get; set; }
        public string Interpret { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }

        public long DurationTicks { get; set; }
        public int Year { get; set; }
        public int Track { get; set; }

        public double Bpm { get; set; }

        public string Genre { get; set; }

        public string Comment { get; set; }

        public string Subtitle { get; set; }

        public int Bitrate { get; set; }

        public byte? Rating { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// This should be nullable. Insert dose not count as changed
        /// </summary>
        public DateTime? FileChanged
        {
            get { return _fileChanged; }
            set { _fileChanged = (value >= System.Data.SqlTypes.SqlDateTime.MinValue.Value ? value : DateTime.Now); }
        }

        public DateTime FileCreated
        {
            get { return _fileCreated; }
            set { _fileCreated = (value >= System.Data.SqlTypes.SqlDateTime.MinValue ? value : DateTime.Now); }
        }

        [ForeignKey(typeof(Image))]
        public int Image_Id { get; set; }

        public virtual Image Image { get; set; }

        [ExcludeFromAbstract]
        public SongStatus Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                NotifyPropertyChanged();
            }
        }

        [ExcludeFromAbstract]
        public string InterpretTitle
        {
            get
            {
                if (string.IsNullOrEmpty(Interpret) && !string.IsNullOrEmpty(Title))
                    return Title;
                if (!string.IsNullOrEmpty(Interpret) && string.IsNullOrEmpty(Title))
                    return Interpret;
                if (string.IsNullOrEmpty(Interpret) && string.IsNullOrEmpty(Title))
                    return Path.GetFileNameWithoutExtension(Filename);
                return $"{Interpret} - {Title}";
            }
        }

        [ExcludeFromAbstract]
        public TimeSpan Duration
        {
            get { return TimeSpan.FromTicks(DurationTicks); }
            set { DurationTicks = value.Ticks; }
        }

        [ExcludeFromAbstract]
        public string All => $"{Interpret}|{Title}|{Album}|{Genre}|{Year}|{Bpm}|{Comment}|{Subtitle}".ToLowerInvariant();

        public override string ToString()
        {
            return InterpretTitle;
        }
    }
}
