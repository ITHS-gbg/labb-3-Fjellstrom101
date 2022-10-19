namespace Labb3_NET22.DataModels;

public class Question
{
    public string Statement { get; }
    public string[] Answers { get; }

    public string ImagePath { get; }
    public string Category { get; }
    public int CorrectAnswer { get; }

    public Question(string statement, string[] answers, int correctAnswer)
    {
        Statement = statement;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
}