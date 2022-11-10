using System;
using System.IO;

namespace Labb3_NET22.DataModels;

public class Question
{
    public string Statement { get; }
    public string[] Answers { get; }

    public string ImageFilePath { get; set; }
    public string Category { get; }
    public int CorrectAnswer { get; }

    public static string NoImageFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"SuperDuperQuizzenNo1\noimage.jpg");

    public Question(string statement, string category, string imageFilePath, string[] answers, int correctAnswer)
    {
        Statement = statement;
        Category = category;
        ImageFilePath = imageFilePath;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
}