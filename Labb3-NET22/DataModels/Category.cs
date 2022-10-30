using System.Collections.Generic;

namespace Labb3_NET22.DataModels;

public class Category
{
	private IEnumerable<Question> _questions;

    public IEnumerable<Question> Questions => _questions;

    public string Title { get; set; }

    public Category(string title)
    {
        Title = title;
        _questions = new List<Question>();
    }

    public void AddQuestion(Question question)
    {
        (_questions as List<Question>).Add(question);
    }
    public void RemoveQuestion(Question question)
    {
        (_questions as List<Question>).Remove(question);
    }
}