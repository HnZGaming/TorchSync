using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Torch;

namespace TorchSync
{
    public class ViewModelCollection<T> : ObservableCollection<T> where T : ViewModel
    {
        public ViewModelCollection()
        {
            CollectionChangedEventManager.AddHandler(this, OnMyCollectionChanged);
        }

        void OnMyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var remotePort in this)
            {
                PropertyChangedEventManager.RemoveHandler(remotePort, OnRemotePortPropertyChanged, nameof(RemotePort.Number));
                PropertyChangedEventManager.AddHandler(remotePort, OnRemotePortPropertyChanged, nameof(RemotePort.Number));
            }
        }

        void OnRemotePortPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}