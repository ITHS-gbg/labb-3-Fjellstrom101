using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;
using Labb3_NET22.Stores;

namespace Labb3_NET22.ViewModels;

public class PlayQuizViewModel : ObservableObject
{
    private Quiz _quiz;
    private Question _currentQuestion; // Inte planerad
    private NavigationStore _navigationStore;
    private QuizStore _quizStore;
    private int _incorrectAnswer = -1;
    private int _correctAnswer = -1;
    private int[] score = new[] { 0, 0 };
    private string _imageFilePath = "";
    private bool _showImageView = false;


    public int IncorrectAnswer
    {
        get => _incorrectAnswer;
        set
        {
            SetProperty(ref _incorrectAnswer, value);
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

    public Question CurrentQuestion
    {
        set
        {
            SetProperty(ref _currentQuestion, value);
        }
        get => _currentQuestion;
    }

    public String ImageFilePath
    {
        get => _imageFilePath;
        set
        {
            SetProperty(ref _imageFilePath, value);
        }
    }
    public bool ShowImageView
    {
        get => _showImageView;
        set
        {
            SetProperty(ref _showImageView, value);
        }
    }
    public IRelayCommand AnswerQuestionCommand { get; }
    public PlayQuizViewModel(NavigationStore navigationStore, QuizStore quizStore, Quiz quiz)
    {
        _navigationStore = navigationStore;
        _quiz = quiz;
        _quizStore = quizStore;

        AnswerQuestionCommand = new RelayCommand<object>(AnswerQuestionCommandHandler, AnswerQuestionCommandCanExecute);
        RenderQuestionAsync(true);
    }

    private void AnswerQuestionCommandHandler(object parameter)
    {

        CorrectAnswer = CurrentQuestion.CorrectAnswer;

        if (Int32.Parse(parameter.ToString()) != CorrectAnswer)
        {
            IncorrectAnswer = Int32.Parse(parameter.ToString());
        }
        else
        {
            score[0]++;
            IncorrectAnswer = -1;
        }

        score[1]++;

        OnPropertyChanged(nameof(CorrectAnswer));
        OnPropertyChanged(nameof(IncorrectAnswer));
        
        RenderQuestionAsync();
    }

    private bool AnswerQuestionCommandCanExecute(object parameter)
    {
        return IncorrectAnswer == -1 && CorrectAnswer == -1;
    }

    private async void RenderQuestionAsync(bool firstQuestion = false)
    {
        ShowImageView = false;

        if (firstQuestion)
        {
            CorrectAnswer = -2;
            CurrentQuestion = new Question("Lycka till!", "", "",  new []{"", "", "", ""}, -1);
            AnswerQuestionCommand.NotifyCanExecuteChanged();

            await Task.Delay(500);
        }
        else
        {
            ImageFilePath = "";
            if (IncorrectAnswer == -1)
            {
                CurrentQuestion = new Question($"Rätt svar!\n Du har svarat rätt på {score[0]} av {score[1]} frågor!", "", "", CurrentQuestion.Answers, -1);
            }
            else
            {
                CurrentQuestion = new Question($"Fel svar!\n Du har svarat rätt på {score[0]} av {score[1]} frågor!", "", "", CurrentQuestion.Answers, -1);
            }
        }

        await Task.Delay(1500);
        CorrectAnswer = -1;
        IncorrectAnswer = -1;
        ImageFilePath = "";
        CurrentQuestion = _quiz.GetRandomQuestion();

        if (CurrentQuestion == null) // Sista frågan
        {

            if (score[0] > score[1] / 2)
            {
                CurrentQuestion = new Question($"Bra jobbat!\n Du svarade rätt på {score[0]} av {score[1]} frågor!", "", "", new[] { "", "", "", "" }, -1);
            }
            else
            {
                CurrentQuestion = new Question($"Bättre lycka nästa gång!\n Du svarade rätt på {score[0]} av {score[1]} frågor!", "", "", new[] { "", "", "", "" }, -1);
            }
            await Task.Delay(2000);
            _navigationStore.CurrentViewModel = new MainMenuViewModel(_quizStore, _navigationStore);
        }
        else if (!string.IsNullOrEmpty(CurrentQuestion.ImageFilePath))
        {
            ShowImageView = true;
            ImageFilePath = CurrentQuestion.ImageFilePath;
        }

        AnswerQuestionCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CorrectAnswer));
        OnPropertyChanged(nameof(IncorrectAnswer));
    }



}