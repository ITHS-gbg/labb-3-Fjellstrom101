using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.Stores;

public class QuizStore
{
    private IEnumerable<Quiz> _quizzes = new ObservableCollection<Quiz>();
    private IEnumerable<Category> _categories = new ObservableCollection<Category>();

    private string _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SuperDuperQuizzenNo1");
    public IEnumerable<Category> Categories => _categories;
    public IEnumerable<Quiz> Quizzes => _quizzes;

    public QuizStore()
    {
        InitAppFoldersAndFiles();
        LoadAllQuizzesAsync();
    }

    public void AddQuiz(Quiz quiz)
    {
        if (string.IsNullOrEmpty(quiz.FolderPath))
        {
            quiz.FolderPath = GenerateFolderName(quiz.Title);
        }

        SaveQuizAsync(quiz);

        (_quizzes as ObservableCollection<Quiz>)?.Add(quiz);

        foreach (var question in quiz.Questions)
        {
            if (!Categories.Any(a => a.Title.Equals(question.Category)))
            {
                (Categories as ObservableCollection<Category>)?.Add(new Category(question.Category));
            }

            Categories.First(a => a.Title.Equals(question.Category))?.AddQuestion(question);
        }
    }
    public void RemoveQuiz(Quiz quiz, bool removeFiles = false)
    {
        (_quizzes as ObservableCollection<Quiz>)?.Remove(quiz);

        foreach (var question in quiz.Questions)
        {
            var category = Categories.FirstOrDefault(a => a.Title.Equals(question.Category));

            if (category == null) continue;

            category.RemoveQuestion(question);

            if (!(category.Questions.Any()))
            {
                (Categories as ObservableCollection<Category>)?.Remove(category);
            }
        }

        if (removeFiles && !string.IsNullOrEmpty(quiz.FolderPath) && Directory.Exists(quiz.FolderPath))
        {
            Directory.Delete(quiz.FolderPath, true);
        }
    }

    public void ReplaceQuiz(Quiz toBeReplaced, Quiz replacement)
    {
        if (toBeReplaced.Title.Equals(replacement.Title))
        {
            replacement.FolderPath = toBeReplaced.FolderPath;

            var filesToBeRemoved =
                toBeReplaced.Questions.Where(a => replacement.Questions.All(b => b.ImageFilePath != a.ImageFilePath && !string.IsNullOrEmpty(a.ImageFilePath)));

            foreach (var question in filesToBeRemoved)
            {
                File.Delete(question.ImageFilePath);
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
        

        if (!Directory.Exists(quiz.FolderPath))
        {
            Directory.CreateDirectory(quiz.FolderPath);
        }

        foreach (var question in quiz.Questions)
        {
            if (!string.IsNullOrEmpty(question.ImageFilePath) && 
                !FileIsInsideFolder(new DirectoryInfo(question.ImageFilePath), new DirectoryInfo(quiz.FolderPath)))
            {
                string newImagePath = GenerateImagePath(quiz, question.ImageFilePath);

                File.Copy(question.ImageFilePath, newImagePath);

                question.ImageFilePath = newImagePath;
            }
        }

        string json = JsonSerializer.Serialize(quiz, new JsonSerializerOptions() { WriteIndented = true });

        using (var writer = File.CreateText(Path.Combine(quiz.FolderPath, "Quiz.json")))
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
                    Quiz? temp = JsonSerializer.Deserialize<Quiz>(json);

                    (_quizzes as ObservableCollection<Quiz>)?.Add(temp);
                    foreach (var question in temp.Questions)
                    {
                        AddQuestionToCategory(question);
                    }
                }
            }
        }
    }

    public async void ExportQuizAsync(Quiz quiz, string path)
    {
        await Task.Run(() => ZipFile.CreateFromDirectory(quiz.FolderPath, path));
    }
    public async void ImportQuizAsync(string path)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "SuperDuperQuizzenNo1");
        await Task.Run(() => ZipFile.ExtractToDirectory(path, tempPath, true));
        
        if (File.Exists(Path.Combine(tempPath, "Quiz.json")))
        {
            using (var reader = new StreamReader(Path.Combine(tempPath, "Quiz.json")))
            {
                string json = await reader.ReadToEndAsync();
                Quiz temp = JsonSerializer.Deserialize<Quiz>(json);

                temp.FolderPath = GenerateFolderName(temp.Title);


                foreach (var question in temp.Questions)
                {
                    if (!string.IsNullOrEmpty(question.ImageFilePath))
                    {
                        question.ImageFilePath = Path.Combine(tempPath,
                            "Images",
                            Path.GetFileName(question.ImageFilePath));
                    }
                }

                AddQuiz(temp);
            }
        }

    }

    public Quiz GenerateQuizByCategories(Category[] categories, int amount)
    {
        List<Question> questions = new List<Question>();
        Quiz returnQuiz = new Quiz("Custom Quiz", new List<Question>());
        Random random = new Random();

        foreach (var category in categories)
        {
            questions.AddRange(category.Questions);
        }

        for (int i = 0; i < amount && questions.Count!=0; i++)
        {
            var randomQuestion = questions[random.Next(questions.Count)];
            questions.Remove(randomQuestion);
            returnQuiz.AddQuestion(randomQuestion.Statement, randomQuestion.CorrectAnswer, randomQuestion.Category, randomQuestion.ImageFilePath, randomQuestion.Answers);
        }

        return returnQuiz;
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
        var directory = new DirectoryInfo(Path.Combine(quiz.FolderPath, "Images"));

        if (!directory.Exists)
        {
            Directory.CreateDirectory(Path.Combine(quiz.FolderPath, "Images"));
        }

        if (directory.GetFiles().Length==0)
        {
            newFileName = "1" + Path.GetExtension(imagePath);
        }
        else
        {
            var filename = directory.GetFiles()
                .OrderBy(a => Path.GetFileNameWithoutExtension(a.Name).Length).ThenBy(a => a.Name)
                .Last().Name;
            newFileName = (int.Parse(Path.GetFileNameWithoutExtension(filename)) + 1) + Path.GetExtension(imagePath);
        }

        return Path.Combine(quiz.FolderPath, "Images", newFileName); ;
    }

    public void AddQuestionToCategory(Question question)
    {
        if (!_categories.Any(a => a.Title.Equals(question.Category)))
        {
            (_categories as ObservableCollection<Category>).Add(new Category(question.Category));
        }

        _categories.First(a => a.Title.Equals(question.Category)).AddQuestion(question);
    }

    public void InitAppFoldersAndFiles()
    {

        if (!Directory.Exists(_appFolder))
        {
            Directory.CreateDirectory(_appFolder);
        }

        if (!File.Exists(Path.Combine(_appFolder, "noimage.jpg")))
        {
            string? currentDirr = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.Move(Path.Combine(currentDirr, "Files\\noimage.jpg"), Path.Combine(_appFolder, "noimage.jpg"));
        }
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

    public IEnumerable<string> GetCategoriesStringList()
    {
        List<string> retList = new List<string>();

        retList.AddRange(new []{
            "Underhållning",
            "Natur/Vetenskap",
            "Kultur/Litteratur",
            "Geografi",
            "Historia",
            "Sport/Fritid"
        });

        return _categories.Select(a => a.Title).Concat(retList).Distinct();
    }
}