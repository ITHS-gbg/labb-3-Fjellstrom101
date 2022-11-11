using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;
using Labb3_NET22.Stores;
using Microsoft.Win32;

namespace Labb3_NET22.ViewModels;

public class MainMenuViewModel : ObservableObject
{
    private readonly QuizStore _quizStore;

    private Quiz _selectedQuiz; 
    private int _selectedCategoryIndex = -1;

    public Quiz SelectedQuiz
    {
        get => _selectedQuiz;
        set
        {
            SetProperty(ref _selectedQuiz, value);

            CreateOrEditQuizButtonText = value == null ? "Skapa Quiz" : "Ändra Quiz";

            OnPropertyChanged(nameof(CreateOrEditQuizButtonText));
            RemoveQuizCommand.NotifyCanExecuteChanged();
            PlayQuizCommand.NotifyCanExecuteChanged();
            ExportQuizCommand.NotifyCanExecuteChanged();
        }
    }
    public string CreateOrEditQuizButtonText { get; set; } = "Skapa Quiz";
    public int CategoryQuestionAmount { get; set; } = 10;
    private readonly NavigationStore _navigationStore;
    public IEnumerable<Quiz> Quizzes => _quizStore.Quizzes;
    public IEnumerable<Category> Categories => _quizStore.Categories;
    public int SelectedCategoryIndex
    {
        get => _selectedCategoryIndex;
        set
        {
            SetProperty(ref _selectedCategoryIndex, value);
            GenerateQuizCommand.NotifyCanExecuteChanged();
        }
    }


    public IRelayCommand PlayQuizCommand { get; }
    public IRelayCommand CreateOrEditCommand { get; }
    public IRelayCommand RemoveQuizCommand { get; }
    public IRelayCommand GenerateQuizCommand { get; }
    public IRelayCommand ExportQuizCommand { get; }
    public IRelayCommand ImportQuizCommand { get; }


    public MainMenuViewModel(QuizStore quizStore, NavigationStore navigationStore)
    {
        _quizStore = quizStore;
        _navigationStore = navigationStore;
        PlayQuizCommand = new RelayCommand(PlayQuizCommandExecute, QuizIsSelected);
        CreateOrEditCommand = new RelayCommand(CreateOrEditCommandExecute);
        GenerateQuizCommand = new RelayCommand<object>((param) => { GenerateQuizCommandExecute(param); }, GenerateQuizCommandCanExecute);
        RemoveQuizCommand = new RelayCommand(DeleteQuizCommandExecute, QuizIsSelected);
        ExportQuizCommand = new RelayCommand(ExportQuizCommandExecute, QuizIsSelected);
        ImportQuizCommand = new RelayCommand(ImportQuizCommandExecute);
    }


    public void PlayQuizCommandExecute()
    {
        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _quizStore,SelectedQuiz.Clone());
    }
    public bool QuizIsSelected()
    {
        return SelectedQuiz != null;
    }

    public void GenerateQuizCommandExecute(object param)
    {
        System.Collections.IList items = (System.Collections.IList)param;
        Quiz generatedQuiz = _quizStore.GenerateQuizByCategories(items.Cast<Category>().ToArray(),CategoryQuestionAmount);

        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _quizStore, generatedQuiz);
    }
    public bool GenerateQuizCommandCanExecute(object param)
    {
        return SelectedCategoryIndex != -1;
    }
    public void CreateOrEditCommandExecute()
    {
        if (_selectedQuiz != null) _navigationStore.CurrentViewModel = new EditQuizViewModel(_navigationStore, _quizStore, _selectedQuiz);
        else _navigationStore.CurrentViewModel = new CreateQuizViewModel(_navigationStore, _quizStore);
    }
    public void DeleteQuizCommandExecute()
    {
        _quizStore.RemoveQuiz(SelectedQuiz, true);
    }
    private void ExportQuizCommandExecute()
    {
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        saveFileDialog1.Filter = "Quiz File|*.quiz";
        saveFileDialog1.Title = "Export Quiz";
        saveFileDialog1.ShowDialog();

        if (saveFileDialog1.FileName != "")
        {
            _quizStore.ExportQuizAsync(SelectedQuiz, saveFileDialog1.FileName);
        }
    }
    private void ImportQuizCommandExecute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Quiz File|*.quiz";
        openFileDialog.Title = "Import Quiz";
        openFileDialog.ShowDialog();

        if (openFileDialog.FileName != "")
        {
            _quizStore.ImportQuizAsync(openFileDialog.FileName);
        }
    }
}