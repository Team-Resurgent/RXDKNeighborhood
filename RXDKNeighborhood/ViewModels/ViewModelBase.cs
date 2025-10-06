using ReactiveUI;

namespace RXDKNeighborhood.ViewModels
{
    public class ViewModelBase<T> : ReactiveObject
    {
        public T? Owner { get; set; }
    }
}
