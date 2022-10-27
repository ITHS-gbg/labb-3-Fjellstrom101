using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.ViewModels;

public class PlayQuizViewModel : ObservableObject
{
    private Quiz _quiz;
    private Question _currentQuestion; // Inte planerad

    public int WrongAnswer { get; set; } = -1; //Inte planerad, Snyggare lösning?
    public int RightAnswer { get; set; } = -1; //Inte planerad, Snyggare lösning?
    public Question CurrentQuestion => _currentQuestion;
    public ICommand AnswerQuestionCommand { get; }
    public PlayQuizViewModel(Quiz quiz)
    {

        _quiz = quiz;
        _currentQuestion = _quiz.GetRandomQuestion();
        AnswerQuestionCommand = new RelayCommand<object>(AnswerQuestionCommandHandler, AnswerQuestionCommandCanExecute);
        RenderQuestionAsync(true);
    }

    private void AnswerQuestionCommandHandler(object parameter)
    {

        RightAnswer = CurrentQuestion.CorrectAnswer;

        if (Int32.Parse(parameter.ToString()) != RightAnswer)
        {
            WrongAnswer = Int32.Parse(parameter.ToString());
        }
        else
        {
            WrongAnswer = -1;
        }

        OnPropertyChanged(nameof(RightAnswer));
        OnPropertyChanged(nameof(WrongAnswer));
        //Måste man?
        //(AnswerQuestionCommand as RelayCommand<object>).NotifyCanExecuteChanged();


        //Ny fråga
        _currentQuestion = new Question("TEST", new string[]{"1", "2", "3", "4"}, 0);
        WrongAnswer = -1;
        RightAnswer = -1;
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
            Question temp = _currentQuestion;
            _currentQuestion = new Question("Lycka till");
            OnPropertyChanged(nameof(RightAnswer));
            OnPropertyChanged(nameof(WrongAnswer));
            OnPropertyChanged(nameof(CurrentQuestion));
            await Task.Delay(2000);

            _currentQuestion = temp;
        }
        else if (CurrentQuestion == null) // Sista frågan
        {

            return;
        }
        OnPropertyChanged(nameof(RightAnswer));
        OnPropertyChanged(nameof(WrongAnswer));
        OnPropertyChanged(nameof(CurrentQuestion));
    }



}