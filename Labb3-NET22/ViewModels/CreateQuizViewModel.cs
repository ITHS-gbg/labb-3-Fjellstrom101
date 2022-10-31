using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;
using Labb3_NET22.Stores;
using Microsoft.Win32;

namespace Labb3_NET22.ViewModels;

public class CreateQuizViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly QuizStore _quizStore;

    private string _title = "";
    private string _statement = "";
    private ObservableCollection<string> _answers = new ObservableCollection<string>() {"", "", "", ""};
    private string _imageFileName = "";
    private int _correctAnswer;
    private string _category = "";
    private Question? _selectedQuestion;
    private string _saveQuestionButtonText = "Spara";
    private readonly string _defaultImagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SuperDuperQuizzenNo1\\noimage.jpg");


    public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }


    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            SetProperty(ref _selectedQuestion, value);
            DeleteQuestionCommand.NotifyCanExecuteChanged();

            if (value != null)
            {
                Statement = value.Statement;
                Category = value.Category;
                ImageFileName = value.ImageFileName;
                CorrectAnswer = value.CorrectAnswer;
                Answers[0] = value.Answers[0];
                Answers[1] = value.Answers[1];
                Answers[2] = value.Answers[2];
                Answers[3] = value.Answers[3];

                SaveQuestionButtonText = "Redigera";
            }
            else
            {
                ClearQuestionFields();
                SaveQuestionButtonText = "Spara";
            }
        }
    }
    public string Statement
    {
        get => _statement;
        set
        {
            SetProperty(ref _statement, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public ObservableCollection<string> Answers
    {
        get => _answers;
        set
        {
            SetProperty(ref _answers, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public string ImageFileName
    {
        get
        {
            if (string.IsNullOrEmpty(_imageFileName)) return _defaultImagePath;
            return _imageFileName;
        }
        set
        {
            SetProperty(ref _imageFileName, value);
            DeleteImageCommand.NotifyCanExecuteChanged();
        }
    }
    public int CorrectAnswer
    {
        get => _correctAnswer;
        set
        {
            SetProperty(ref _correctAnswer, value);
        }
    }
    public string Category
    {
        get => _category;
        set
        {
            SetProperty(ref _category, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public string SaveQuestionButtonText
    {
        get => _saveQuestionButtonText;
        set
        {
            SetProperty(ref _saveQuestionButtonText, value);
        }
    }

    public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
    public bool QuestionIsSelected => SelectedQuestion != null;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddImageCommand { get; }
    public IRelayCommand DeleteImageCommand { get; }
    public IRelayCommand AddQuestionCommand { get; }
    public IRelayCommand DeleteQuestionCommand { get; }

    public CreateQuizViewModel(NavigationStore navigationStore, QuizStore quizStore)
    {
        _quizStore = quizStore;
        _navigationStore = navigationStore;

        SaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        CancelCommand = new RelayCommand(CancelCommandExecute);
        AddImageCommand = new RelayCommand(AddImageCommandExecute);
        DeleteImageCommand = new RelayCommand(DeleteImageCommandExecute, DeleteImageCommandCanExecute);
        AddQuestionCommand = new RelayCommand(AddQuestionCommandExecute, AddQuestionCommandCanExecute);
        DeleteQuestionCommand = new RelayCommand(DeleteQuestionCommandExecute, DeleteQuestionCommandCanExecute);

        Answers.CollectionChanged += (sender, e) => { AddQuestionCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged();};
        Categories.Add("Historia");
        Categories.Add("Sport");
        Categories.Add("Ekonomi");
        Categories.Add("Data/IT");
        Categories.Add("Geografi");
    }

    public void SaveCommandExecute()
    {
        //Finns det ändringar som inte är sparade?
        if (QuestionIsSelected && 
            (!SelectedQuestion.Statement.Equals(Statement) ||
             !SelectedQuestion.Category.Equals(Category) ||
             !SelectedQuestion.ImageFileName.Equals(_imageFileName) ||
             !SelectedQuestion.Answers.Equals(Answers) ||
             SelectedQuestion.CorrectAnswer != CorrectAnswer))
        {
            if (MessageBox.Show("Du har gjort ändringar som inte har sparats. Vill du spara ändringarna?", "Osparade Ändringar", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                AddQuestionCommandExecute();
            }
        }
        //Spara Quiz
        _quizStore.AddQuiz(new Quiz(Title, Questions));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_quizStore, _navigationStore);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Title) && Questions.Count > 0;
    }
    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_quizStore, _navigationStore);
    }
    public void AddImageCommandExecute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        if (openFileDialog.ShowDialog() == true)
            ImageFileName = openFileDialog.FileName;
    }
    public void DeleteImageCommandExecute()
    {
        ImageFileName = string.Empty;
    }
    public bool DeleteImageCommandCanExecute()
    {
        return !string.IsNullOrEmpty(_imageFileName);
    }
    public void AddQuestionCommandExecute()
    {
        if (QuestionIsSelected)
        {
            Questions[Questions.IndexOf(SelectedQuestion)] = 
                new Question(Statement, Category, _imageFileName, Answers.ToArray(), CorrectAnswer);
        }
        else
        {
            Questions.Add(new Question(Statement, Category, _imageFileName, Answers.ToArray(), CorrectAnswer));
        }
        ClearQuestionFields();
    }
    public bool AddQuestionCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Statement) &&
               !string.IsNullOrEmpty(Category) &&
               !string.IsNullOrEmpty(Answers[0]) &&
               !string.IsNullOrEmpty(Answers[1]) &&
               !string.IsNullOrEmpty(Answers[2]) &&
               !string.IsNullOrEmpty(Answers[3]);
    }
    public void DeleteQuestionCommandExecute()
    {
        Questions.Remove(SelectedQuestion);
    }
    public bool DeleteQuestionCommandCanExecute()
    {
        return QuestionIsSelected;
    }

    public void ClearQuestionFields()
    {
        Statement = "";
        Category = "";
        ImageFileName = "";
        Answers[0] = "";
        Answers[1] = "";
        Answers[2] = "";
        Answers[3] = "";
    }

}