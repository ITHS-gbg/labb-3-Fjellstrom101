using System;
using System.Collections.Generic;
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
            SetProperty(ref _selectedQuiz, value);

            if (value == null) CreateOrEditQuizButtonText = "Skapa Quiz";
            else CreateOrEditQuizButtonText = "Ändra Quiz";
            OnPropertyChanged(nameof(CreateOrEditQuizButtonText));

            PlayQuizCommand.NotifyCanExecuteChanged();
        }
    }

    public String CreateOrEditQuizButtonText { get; set; } = "Skapa Quiz";

    private NavigationStore _navigationStore;
    public IEnumerable<Quiz> Quizzes => _quizStore.Quizzes;
    public IEnumerable<Category> Categories => _quizStore.Categories;

    public IRelayCommand PlayQuizCommand { get; }
    public IRelayCommand CreateOrEditCommand { get; }

    public MainMenuViewModel(QuizStore quizStore, NavigationStore navigationStore)
    {
        _quizStore = quizStore;
        _navigationStore = navigationStore;
        PlayQuizCommand = new RelayCommand(PlayQuizCommandExecute, PlayQuizCommandCanExecute);
        CreateOrEditCommand = new RelayCommand(CreateOrEditCommandExecute);
    }

    public void PlayQuizCommandExecute()
    {
        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _quizStore,SelectedQuiz.Clone());
    }
    public bool PlayQuizCommandCanExecute()
    {
        return SelectedQuiz != null;
    }
    public void CreateOrEditCommandExecute()
    {
        if (_selectedQuiz != null) _navigationStore.CurrentViewModel = new EditQuizViewModel(_navigationStore, _quizStore, _selectedQuiz);
        else _navigationStore.CurrentViewModel = new CreateQuizViewModel(_navigationStore, _quizStore);
    }

}