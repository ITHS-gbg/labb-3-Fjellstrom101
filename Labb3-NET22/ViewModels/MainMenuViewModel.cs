using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;
using Labb3_NET22.Stores;

namespace Labb3_NET22.ViewModels;

public class MainMenuViewModel : ObservableObject
{
    private QuizStore _quizStore;
    private Quiz _selectedQuiz; //Inte planerad
    public Quiz SelectedQuiz
    {
        get => _selectedQuiz;
        set
        {
            if (value == _selectedQuiz)
            {
                return;
            }

            SetProperty(ref _selectedQuiz, value);
            (PlayQuizCommand as RelayCommand).NotifyCanExecuteChanged();
        }
    }

    private NavigationStore _navigationStore;
    public ObservableCollection<Quiz> QuizCollection => _quizStore.QuizCollection;

    public int SelectedIndex { get; set; }

    public ICommand PlayQuizCommand { get; }
    public MainMenuViewModel(QuizStore quizStore, NavigationStore navigationStore)
    {
        _quizStore = quizStore;
        _navigationStore = navigationStore;
        PlayQuizCommand = new RelayCommand(PlayQuizCommandExecute, PlayQuizCommandCanExecute);
    }

    public void PlayQuizCommandExecute()
    {
        _navigationStore.CurrentViewModel = new PlayQuizViewModel(SelectedQuiz);
    }
    public bool PlayQuizCommandCanExecute()
    {
        return SelectedQuiz != null;
    }
    
}