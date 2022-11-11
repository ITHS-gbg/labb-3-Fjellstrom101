using System;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;
using Labb3_NET22.Stores;
using Microsoft.Win32;

namespace Labb3_NET22.ViewModels;

public class PlayQuizViewModel : ObservableObject
{
    private readonly Quiz _quiz;
    private readonly NavigationStore _navigationStore;
    private readonly QuizStore _quizStore;

    private Question _currentQuestion;
    private int _incorrectAnswer = -1;
    private int _correctAnswer = -1;
    private int[] _score = new[] { 0, 0 };
    private string _imageFilePath;
    private bool _showImageView = false;


    public int IncorrectAnswer
    {
        get => _incorrectAnswer;
        set
        {
            SetProperty(ref _incorrectAnswer, value);
            AnswerQuestionCommand.NotifyCanExecuteChanged();
        }
    }

    public int CorrectAnswer
    {
        get => _correctAnswer;
        set
        {
            SetProperty(ref _correctAnswer, value);
            AnswerQuestionCommand.NotifyCanExecuteChanged();
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

    public string ImageFilePath
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
        //För att slippa att knapparna gråas ut så skriver vi direkt till backing fields och inte till properties. Då slipper vi att AnswerQuestionCommand.NotifyCanExecuteChanged() körs, och knapparna behåller sina fina färger även fast man inte kan trycka på dom
        _correctAnswer = CurrentQuestion.CorrectAnswer;

        if (int.Parse(parameter.ToString()) != CorrectAnswer)
        {
            _incorrectAnswer = int.Parse(parameter.ToString());
            SystemSounds.Exclamation.Play();
        }
        else
        {
            SystemSounds.Hand.Play();
            _score[0]++;
            _incorrectAnswer = -1;
        }

        _score[1]++;

        OnPropertyChanged(nameof(CorrectAnswer));
        OnPropertyChanged(nameof(IncorrectAnswer));
        
        RenderQuestionAsync();
    }

    private bool AnswerQuestionCommandCanExecute(object parameter)
    {
        return IncorrectAnswer == -1 && CorrectAnswer == -1;
    }

    private async Task RenderQuestionAsync(bool firstQuestion = false)
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
            //Om variabeln för felaktigt svar inte är satt, har användaren svarat rätt.
            CurrentQuestion = IncorrectAnswer == -1 ? 
                new Question($"Rätt svar!\n Du har svarat rätt på {_score[0]} av {_score[1]} frågor!", "", "", CurrentQuestion.Answers, -1) :
                new Question($"Fel svar!\n Du har svarat rätt på {_score[0]} av {_score[1]} frågor!", "", "", CurrentQuestion.Answers, -1);
        }

        await Task.Delay(1500);

        //Nollställer färgerna på knapparna.
        CorrectAnswer = -1;
        IncorrectAnswer = -1;

        CurrentQuestion = _quiz.GetRandomQuestion();

        if (CurrentQuestion == null) // Sista frågan
        {
            CorrectAnswer = -2;

            CurrentQuestion = _score[0] > _score[1] / 2 ?
                new Question($"Bra jobbat!\n Du svarade rätt på {_score[0]} av {_score[1]} frågor!", "", "", new[] { "", "", "", "" }, -1) :
                new Question($"Bättre lycka nästa gång!\n Du svarade rätt på {_score[0]} av {_score[1]} frågor!", "", "", new[] { "", "", "", "" }, -1);

            await Task.Delay(2000);
            _navigationStore.CurrentViewModel = new MainMenuViewModel(_quizStore, _navigationStore);

        }
        else if (!string.IsNullOrEmpty(CurrentQuestion.ImageFilePath))
        {
            ShowImageView = true;
            ImageFilePath = CurrentQuestion.ImageFilePath;
        }
    }



}