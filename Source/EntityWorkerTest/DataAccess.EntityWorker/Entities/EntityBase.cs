using System.ComponentModel;
using System.Runtime.CompilerServices;
using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities
{
    public abstract class EntityBase : INotifyPropertyChanged
    {
        [PrimaryKey]
        public int Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}