using System;
using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(Fade))]
    public class Fade : EntityBase
    {
        public long DurationTicks { get; set; }
        public long StartFadeInTicks { get; set; }
        public long StopFadeInTicks { get; set; }
        public long StartFadeOutTicks { get; set; }
        public long StopFadeOutTicks { get; set; }

        [ExcludeFromAbstract]
        public TimeSpan Duration
        {
            get { return TimeSpan.FromTicks(DurationTicks); }
            set { DurationTicks = value.Ticks; }
        }

        [ExcludeFromAbstract]
        public TimeSpan StartFadeIn
        {
            get { return TimeSpan.FromTicks(StartFadeInTicks); }
            set { StartFadeInTicks = value.Ticks; }
        }

        [ExcludeFromAbstract]
        public TimeSpan StopFadeIn
        {
            get { return TimeSpan.FromTicks(StopFadeInTicks); }
            set { StopFadeInTicks = value.Ticks; }
        }

        [ExcludeFromAbstract]
        public TimeSpan StartFadeOut
        {
            get { return TimeSpan.FromTicks(StartFadeOutTicks); }
            set { StartFadeOutTicks = value.Ticks; }
        }

        [ExcludeFromAbstract]
        public TimeSpan StopFadeOut
        {
            get { return TimeSpan.FromTicks(StopFadeOutTicks); }
            set { StopFadeOutTicks = value.Ticks; }
        }
    }
}