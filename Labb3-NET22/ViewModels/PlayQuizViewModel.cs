using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.ViewModels;

public class PlayQuizViewModel : ObservableObject
{
    private Quiz _quiz;
    public Question _currentQuestion; // Inte planerad

    public int WrongAnswer { get; set; } = -1; //Inte planerad, Snyggare lösning?
    public int RightAnswer { get; set; } = -1; //Inte planerad, Snyggare lösning?
    public Question CurrentQuestion => _currentQuestion;
    public ICommand AnswerQuestionCommand { get; }
    public PlayQuizViewModel(Quiz quiz)
    {

        _quiz = quiz;
        _currentQuestion = _quiz.GetRandomQuestion();
        AnswerQuestionCommand = new RelayCommand<object>(AnswerQuestionCommandHandler);
    }

    private void AnswerQuestionCommandHandler(object parameter)
    {
        //TODO Fortsätt här
        WrongAnswer = Int32.Parse(parameter.ToString());
        OnPropertyChanged(nameof(WrongAnswer));
    }



}