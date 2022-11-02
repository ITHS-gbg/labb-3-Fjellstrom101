using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Labb3_NET22.DataModels;   

public class Quiz
{
    private IEnumerable<Question> _questions;
    private readonly string _title = string.Empty;


    public IEnumerable<Question> Questions => _questions;
    public string Title => _title;
    public string FolderPath { get; set; }

    private readonly Random _random = new();

    public Quiz()
    {
        _questions = new List<Question>();
    }
    
    public Quiz(string title, IEnumerable<Question> questions)
    {
        _title = title;

        _questions = new List<Question>();
        (_questions as List<Question>)?.AddRange(questions);
    }
    [JsonConstructor]
    public Quiz(string title, IEnumerable<Question> questions, string folderPath)
    {
        _title = title;
        FolderPath = folderPath;

        _questions = new List<Question>();
        (_questions as List<Question>)?.AddRange(questions);
    }

    public Question GetRandomQuestion()
    {
        var index = _random.Next(_questions.Count());
        var randomQuestion = _questions.ElementAtOrDefault(index);

        if((_questions as List<Question>)?.Count!= 0) RemoveQuestion(index);
        return randomQuestion;
    }

    public void AddQuestion(string statement, int correctAnswer, params string[] answers)
    {
        (_questions as List<Question>)?.Add(new Question(statement, "Vanlig allmän fråga", "", answers, correctAnswer));
    }
    public void AddQuestion(string statement, int correctAnswer, string category, string imageFilePath, params string[] answers)
    {
        var tempQuestion = new Question(statement, category, "", answers, correctAnswer);
        tempQuestion.ImageFilePath = imageFilePath;
        (_questions as List<Question>)?.Add(tempQuestion);
    }

    public void RemoveQuestion(int index)
    {
        (_questions as List<Question>)?.RemoveAt(index);
    }

    public Quiz Clone()
    {
        return new Quiz(Title, new List<Question>(_questions)); //TODO Deep copy
    }
}