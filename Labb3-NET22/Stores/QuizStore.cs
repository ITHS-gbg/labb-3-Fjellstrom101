using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.Stores;

public class QuizStore
{
    private ObservableCollection<Quiz> _quizCollection { get; }
    public ObservableCollection<Quiz> QuizCollection => _quizCollection;

    public QuizStore()
    {
        _quizCollection = new ObservableCollection<Quiz>();
        Quiz tempQuiz = new Quiz("Svenska kungar");
        tempQuiz.AddQuestion("Vilket år dog Gustav Vasa?", 3, new string[] { "1650", "1593", "1539", "1560" });
        _quizCollection.Add(tempQuiz);
        _quizCollection.Add(new Quiz("TV reklam"));

    }
    public void SaveQuizzesToFile()
    {
        throw new NotImplementedException();
    }
    public void LoadQuizzesFromFile()
    {
        throw new NotImplementedException();
    }
    //Kanske?
    public void ImportQuizFromFile()
    {
        throw new NotImplementedException();
    }
    //Kanske?
    public void ExportQuizToFile()
    {
        throw new NotImplementedException();
    }
}