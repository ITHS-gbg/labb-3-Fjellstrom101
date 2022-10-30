using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.Stores;

public class QuizStore
{
    private IEnumerable<Quiz> _quizzes = new List<Quiz>();
    private IEnumerable<Category> _categories = new List<Category>();

    private string _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SuperDuperQuizzenNo1");
    public IEnumerable<Category> Categories => _categories;
    public IEnumerable<Quiz> Quizzes => _quizzes;

    public QuizStore()
    {
        LoadAllQuizzesAsync();
    }

    public void AddQuiz(Quiz quiz)
    {
        if (string.IsNullOrEmpty(quiz.FolderName))
        {
            quiz.FolderName = GenerateFolderName(quiz.Title);
        }

        SaveQuizAsync(quiz);

        (_quizzes as List<Quiz>).Add(quiz);
    }
    public void RemoveQuiz(Quiz quiz, bool removeFiles = false)
    {
        (_quizzes as List<Quiz>).Remove(quiz);
    }

    public void ReplaceQuiz(Quiz toBeReplaced, Quiz replacement)
    {
        if (toBeReplaced.Title.Equals(replacement.Title))
        {
            replacement.FolderName = toBeReplaced.FolderName;

            var filesToBeRemoved =
                toBeReplaced.Questions.Where(a => replacement.Questions.All(b => b.ImageFileName != a.ImageFileName && !string.IsNullOrEmpty(a.ImageFileName)));

            foreach (var question in filesToBeRemoved)
            {
                File.Delete(question.ImageFileName);
            }

            AddQuiz(replacement);
            RemoveQuiz(toBeReplaced);
        }
        else
        {
            AddQuiz(replacement);
            RemoveQuiz(toBeReplaced, true);
        }
    }
    public async void SaveQuizAsync(Quiz quiz)
    {
        

        if (!Directory.Exists(quiz.FolderName))
        {
            Directory.CreateDirectory(quiz.FolderName);
        }

        foreach (var question in quiz.Questions)
        {
            if (!string.IsNullOrEmpty(question.ImageFileName) && 
                !FileIsInsideFolder(new DirectoryInfo(question.ImageFileName), new DirectoryInfo(quiz.FolderName)))
            {
                string newImagePath = GenerateImagePath(quiz, question.ImageFileName);

                File.Copy(question.ImageFileName, newImagePath);

                question.ImageFileName = newImagePath;
            }
        }

        string json = JsonSerializer.Serialize(quiz, new JsonSerializerOptions() { WriteIndented = true });

        using (var writer = File.CreateText(Path.Combine(quiz.FolderName, "Quiz.json")))
        {
            await writer.WriteAsync(json);
        }
    }
    public async void LoadAllQuizzesAsync()
    {
        string[] directories = Directory.GetDirectories(_appFolder, "*", SearchOption.TopDirectoryOnly);

        foreach (var directory in directories)
        {
            if (File.Exists(Path.Combine(directory, "Quiz.json")))
            {
                using (var reader = new StreamReader(Path.Combine(directory, "Quiz.json")))
                {
                    string json = await reader.ReadToEndAsync();
                    Quiz temp = JsonSerializer.Deserialize<Quiz>(json);

                    (_quizzes as List<Quiz>).Add(temp);
                    foreach (var question in temp.Questions)
                    {
                        AddQuestionToCategory(question);
                    }
                }
            }
        }
    }

    public Quiz GenerateQuizByCategories(Category[] categories, int amount)
    {
        throw new NotImplementedException();
    }

    public string GenerateFolderName(string title)
    {
        string legalFileName = title;

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            legalFileName = legalFileName.Replace(c.ToString(), "");
        }

        if (Directory.Exists(Path.Combine(_appFolder, legalFileName)))
        {
            for (int i = 1; i < 100; i++) //Varför 100? Vem vet?
            {
                if (!Directory.Exists(Path.Combine(_appFolder, $"{legalFileName} ({i})")))
                {
                    return Path.Combine(_appFolder, $"{legalFileName} ({i})");
                }
            }
        }

        return (Path.Combine(_appFolder, legalFileName));
    }

    public string GenerateImagePath(Quiz quiz, string imagePath)
    {
        var newFileName = String.Empty;
        var directory = new DirectoryInfo(Path.Combine(quiz.FolderName, "Images"));

        if (!directory.Exists)
        {
            Directory.CreateDirectory(Path.Combine(quiz.FolderName, "Images"));
        }

        if (directory.GetFiles().Length==0)
        {
            newFileName = "1" + Path.GetExtension(imagePath);
        }
        else
        {
            var filename = directory.GetFiles()
                .OrderBy(x => x.Name)
                .Last().Name;
            newFileName = (int.Parse(Path.GetFileNameWithoutExtension(filename)) + 1) + Path.GetExtension(imagePath);
        }

        return Path.Combine(quiz.FolderName, "Images", newFileName); ;
    }

    public void AddQuestionToCategory(Question question)
    {
        if (!_categories.Any(a => a.Title.Equals(question.Category)))
        {
            (_categories as List<Category>).Add(new Category(question.Category));
        }

        _categories.First(a => a.Title.Equals(question.Category)).AddQuestion(question);
    }
    //Kanske?
    public void ImportQuizFromFileAsync()
    {
        //TODO Om tid finns
        throw new NotImplementedException();
    }
    //Kanske?
    public void ExportQuizToFileAsync()
    {
        //TODO Om tid finns
        throw new NotImplementedException();
    }
    private bool FileIsInsideFolder(DirectoryInfo file, DirectoryInfo folder)
    {
        if (file.Parent == null)
        {
            return false;
        }

        if (String.Equals(file.Parent.FullName, folder.FullName, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return FileIsInsideFolder(file.Parent, folder);
    }
}