using CommunityToolkit.Mvvm.ComponentModel;
using Labb3_NET22.Stores;

namespace Labb3_NET22.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;

    public ObservableObject CurrentViewModel=> _navigationStore.CurrentViewModel;

    public MainViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}