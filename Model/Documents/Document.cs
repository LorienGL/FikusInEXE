using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Model.Documents
{
    public class Document(Guid _id, string _name, bool p_isActive): INotifyPropertyChanged
    {
        public Guid Id { get; set; } = _id;
        public string Name { get; set; } = _name;
        private bool _isActive = p_isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public string Title
        {
            get { return Name + (IsModified ? "*" : ""); }
        }

        public bool IsModified { get; private set; } = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal bool Close()
        {
            return true;
        }
    }
}
