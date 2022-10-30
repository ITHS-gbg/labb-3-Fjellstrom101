using System;
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
    private int _wrongAnswer = -1;
    private int _rightAnswer = -1;
    private int[] score = new[] { 0, 0 };
    private string _imageFilePath = "";


    public int WrongAnswer
    {
        get => _wrongAnswer;
        set
        {
            SetProperty(ref _wrongAnswer, value);
        }
    }

    public int RightAnswer
    {
        get => _rightAnswer;
        set
        {
            SetProperty(ref _rightAnswer, value);
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

        RightAnswer = CurrentQuestion.CorrectAnswer;
        score[1]++;

        if (Int32.Parse(parameter.ToString()) != RightAnswer)
        {
            WrongAnswer = Int32.Parse(parameter.ToString());
        }
        else
        {
            score[0]++;
            WrongAnswer = -1;
        }

        OnPropertyChanged(nameof(RightAnswer));
        OnPropertyChanged(nameof(WrongAnswer));
        
        //Måste man?
        //(AnswerQuestionCommand as RelayCommand<object>).NotifyCanExecuteChanged();


        //Ny fråga
        /*_currentQuestion = new Question("Test", "", "", new[] { "", "", "", "" }, -1);
        WrongAnswer = -1;
        RightAnswer = -1;*/
        RenderQuestionAsync();
    }

    private bool AnswerQuestionCommandCanExecute(object parameter)
    {
        return WrongAnswer == -1 && RightAnswer == -1;
    }

    private async void RenderQuestionAsync(bool firstQuestion = false)
    {
        if (firstQuestion)
        {
            RightAnswer = -2;
            CurrentQuestion = new Question("Lycka till", "", "",  new []{"", "", "", ""}, -1);
            AnswerQuestionCommand.NotifyCanExecuteChanged();

            await Task.Delay(1000);
        }

        await Task.Delay(1000);
        RightAnswer = -1;
        WrongAnswer = -1;
        ImageFilePath = "";
        CurrentQuestion = _quiz.GetRandomQuestion();

        if (CurrentQuestion == null) // Sista frågan
        {

            CurrentQuestion = new Question($"Bra Jobbat!\n Du svarade rätt på {score[0]} av {score[1]} frågor!", "", "", new[] { "", "", "", "" }, -1);
            await Task.Delay(2000);
            _navigationStore.CurrentViewModel = new MainMenuViewModel(_quizStore, _navigationStore);
        }
        else if (!string.IsNullOrEmpty(CurrentQuestion.ImageFileName))
        {
            RightAnswer = -2;
            AnswerQuestionCommand.NotifyCanExecuteChanged();
            await Task.Delay(2000);
            ImageFilePath = CurrentQuestion.ImageFileName;

            RightAnswer = -1;
            WrongAnswer = -1;

        }

        AnswerQuestionCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(RightAnswer));
        OnPropertyChanged(nameof(WrongAnswer));
    }



}