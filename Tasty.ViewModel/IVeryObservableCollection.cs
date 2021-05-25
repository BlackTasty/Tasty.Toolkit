namespace Tasty.ViewModel
{
    public interface IVeryObservableCollection
    {
        bool UnsavedChanged { get; }

        int Count { get; }

        IVeryObservableCollection Copy();
    }
}
