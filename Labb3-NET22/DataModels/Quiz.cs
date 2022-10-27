using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Labb3_NET22.DataModels;   

public class Quiz
{
    private IEnumerable<Question> _questions;
    private string _title = string.Empty;
    public IEnumerable<Question> Questions => _questions;
    public int QuestionCount => _questions.Count();
    public string Title => _title;

    private Random _random = new();

    public Quiz(string title)
    {
        _title = title;
        _questions = new List<Question>();
    }

    public Question GetRandomQuestion()
    {
        return _questions.ElementAtOrDefault(_random.Next(_questions.Count()));
    }

    public void AddQuestion(string statement, int correctAnswer, params string[] answers)
    {
        (_questions as List<Question>)?.Add(new Question(statement, answers, correctAnswer));
    }

    public void RemoveQuestion(int index)
    {
        (_questions as List<Question>)?.RemoveAt(index);
    } 
}