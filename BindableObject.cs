/*
 @ 0xCCCCCCCC
*/

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasyKeeper {
    public abstract class BindableObject : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
