using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    public class ArchiveGroup : GroupBase
    {
        public const string SearchGroupName = "Search_BB245149-995C-42EA-AF49-9258A4559D12";
        private Tag _selectedTag;
        private bool _isBusy;
        private bool _isSelected;

        public ArchiveGroup()
        {
            ArchivGroups = new Collection<ArchiveGroup>();
        }

        public ArchiveGroup(string name, string fullPath, int? parentId, DateTime importedAt, string orderName1, string orderName2, Color color)
            : base(name)
        {
            FullPath = fullPath;
            ParentId = parentId;
            ImportedAt = importedAt;
            OrderName1 = orderName1;
            OrderName2 = orderName2;
            ColorA = color.A;
            ColorR = color.R;
            ColorG = color.G;
            ColorB = color.B;
            ArchivGroups = new Collection<ArchiveGroup>();
        }

        [ForeignKey(typeof(Tag))]
        public int? SelectedTagId { get; set; }

        public virtual Tag SelectedTag
        {
            get { return _selectedTag; }
            set { _selectedTag = value; }
        }

        public string FullPath { get; set; }

        public DateTime ImportedAt { get; set; }

        [ForeignKey(typeof(ArchiveGroup))]
        public int? ParentId { get; set; }

        public ArchiveGroup Parent { get; set; }

        [ExcludeFromAbstract]
        public IEnumerable<ArchiveGroup> ArchiveGroupsOrderedByName => ArchivGroups.OrderBy(x => x.Name);
        public virtual Collection<ArchiveGroup> ArchivGroups { get; set; }

        public string OrderName1 { get; set; }
        public string OrderName2 { get; set; }
        public bool OrderDescending { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(); }
        }

        public byte ColorA { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        
        public void SetColor(Color color)
        {
            ColorA = color.A;
            ColorR = color.R;
            ColorG = color.G;
            ColorB = color.B;
        }
        public List<int> GetAllSubgroupIds(List<int> groups)
        {
            groups.Add(Id);
            foreach (var g in ArchivGroups)
            {
                groups = g.GetAllSubgroupIds(groups);
            }
            return groups;
        }

        [ExcludeFromAbstract]
        public Color Color => Color.FromArgb(ColorA, ColorR, ColorG, ColorB);

        [ExcludeFromAbstract]
        public SolidColorBrush ColorBrush => new SolidColorBrush(Color);

        public static ArchiveGroup Default => new ArchiveGroup {Name = SearchGroupName, OrderName1 = nameof(Tag.Interpret), OrderName2 = nameof(Tag.Title)};
    }
}