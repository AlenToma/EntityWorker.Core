using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(CueLoopInfo))]
    public class CueLoopInfo : EntityBase
    {
        [ForeignKey(typeof(Tag))]
        public int TagId { get; set; }
        public Tag Tag { get; set; }

        public double? CueIn { get; set; }
        public double? Cue1 { get; set; }
        public double? Cue2 { get; set; }
        public double? Cue3 { get; set; }
        public double? Cue4 { get; set; }

        public double? LoopIn1 { get; set; }
        public double? LoopIn2 { get; set; }
        public double? LoopIn3 { get; set; }
        public double? LoopIn4 { get; set; }
        public double? LoopOut1 { get; set; }
        public double? LoopOut2 { get; set; }
        public double? LoopOut3 { get; set; }
        public double? LoopOut4 { get; set; }
    }
}