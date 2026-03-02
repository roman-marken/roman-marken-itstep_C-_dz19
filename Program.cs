using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class DictionaryEntry
{
    public string Word { get; set; }
    public List<string> Translations { get; set; } = new();
}

class LangDictionary
{
    public string Type { get; set; }
    public List<DictionaryEntry> Entries { get; set; } = new();
}

class User
{
    public string Login { get; set; }
    public string Password { get; set; }
    public DateTime BirthDate { get; set; }
}

class Question
{
    public string Text { get; set; }
    public List<string> Answers { get; set; } = new();
    public List<int> Correct { get; set; } = new();
    public string Category { get; set; }
}

class Result
{
    public string Login { get; set; }
    public string Category { get; set; }
    public int Score { get; set; }
}

class Program
{
    static List<LangDictionary> dictionaries =
        File.Exists("dict.json")
        ? JsonSerializer.Deserialize<List<LangDictionary>>(File.ReadAllText("dict.json"))
        : new();

    static List<User> users =
        File.Exists("users.json")
        ? JsonSerializer.Deserialize<List<User>>(File.ReadAllText("users.json"))
        : new();

    static List<Question> questions =
        File.Exists("quiz.json")
        ? JsonSerializer.Deserialize<List<Question>>(File.ReadAllText("quiz.json"))
        : new();

    static List<Result> results =
        File.Exists("results.json")
        ? JsonSerializer.Deserialize<List<Result>>(File.ReadAllText("results.json"))
        : new();

    static User currentUser;

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("1 Dictionaries");
            Console.WriteLine("2 Quiz");
            Console.WriteLine("0 Exit");

            var c = Console.ReadLine();

            if (c == "1") DictMenu();
            if (c == "2") Auth();
            if (c == "0") return;
        }
    }

    static void SaveAll()
    {
        File.WriteAllText("dict.json", JsonSerializer.Serialize(dictionaries));
        File.WriteAllText("users.json", JsonSerializer.Serialize(users));
        File.WriteAllText("quiz.json", JsonSerializer.Serialize(questions));
        File.WriteAllText("results.json", JsonSerializer.Serialize(results));
    }

    static void DictMenu()
    {
        while (true)
        {
            Console.WriteLine("1 Create");
            Console.WriteLine("2 Add word");
            Console.WriteLine("3 Replace");
            Console.WriteLine("4 Delete");
            Console.WriteLine("5 Search");
            Console.WriteLine("0 Back");

            var c = Console.ReadLine();

            if (c == "1")
            {
                Console.Write("Type: ");
                dictionaries.Add(new LangDictionary { Type = Console.ReadLine() });
            }

            if (c == "2")
            {
                var d = SelectDict();
                Console.Write("Word: ");
                var w = new DictionaryEntry { Word = Console.ReadLine() };

                Console.Write("Translations count: ");
                int n = int.Parse(Console.ReadLine());

                for (int i = 0; i < n; i++)
                    w.Translations.Add(Console.ReadLine());

                d.Entries.Add(w);
            }

            if (c == "3")
            {
                var d = SelectDict();
                Console.Write("Word: ");
                var e = d.Entries.FirstOrDefault(x => x.Word == Console.ReadLine());
                if (e != null)
                {
                    Console.Write("New word: ");
                    e.Word = Console.ReadLine();
                }
            }

            if (c == "4")
            {
                var d = SelectDict();
                Console.Write("Word: ");
                var e = d.Entries.FirstOrDefault(x => x.Word == Console.ReadLine());
                if (e != null) d.Entries.Remove(e);
            }

            if (c == "5")
            {
                var d = SelectDict();
                Console.Write("Word: ");
                var e = d.Entries.FirstOrDefault(x => x.Word == Console.ReadLine());
                if (e != null)
                    foreach (var t in e.Translations)
                        Console.WriteLine(t);
            }

            if (c == "0") break;

            SaveAll();
        }
    }

    static LangDictionary SelectDict()
    {
        for (int i = 0; i < dictionaries.Count; i++)
            Console.WriteLine($"{i} {dictionaries[i].Type}");

        return dictionaries[int.Parse(Console.ReadLine())];
    }

    static void Auth()
    {
        Console.Write("Login: ");
        var login = Console.ReadLine();

        var user = users.FirstOrDefault(x => x.Login == login);

        if (user == null)
        {
            Console.Write("Register password: ");
            var pass = Console.ReadLine();

            Console.Write("Birth yyyy-mm-dd: ");
            var bd = DateTime.Parse(Console.ReadLine());

            user = new User { Login = login, Password = pass, BirthDate = bd };
            users.Add(user);
            SaveAll();
        }
        else
        {
            Console.Write("Password: ");
            if (user.Password != Console.ReadLine()) return;
        }

        currentUser = user;
        QuizMenu();
    }

    static void QuizMenu()
    {
        while (true)
        {
            Console.WriteLine("1 Start quiz");
            Console.WriteLine("2 My results");
            Console.WriteLine("3 Top20");
            Console.WriteLine("4 Settings");
            Console.WriteLine("0 Logout");

            var c = Console.ReadLine();

            if (c == "1") StartQuiz();
            if (c == "2")
                results.Where(x => x.Login == currentUser.Login)
                .ToList()
                .ForEach(x => Console.WriteLine($"{x.Category}:{x.Score}"));

            if (c == "3")
            {
                Console.Write("Category: ");
                var cat = Console.ReadLine();
                results.Where(x => x.Category == cat)
                .OrderByDescending(x => x.Score)
                .Take(20)
                .ToList()
                .ForEach(x => Console.WriteLine($"{x.Login}:{x.Score}"));
            }

            if (c == "4")
            {
                Console.Write("New password: ");
                currentUser.Password = Console.ReadLine();
            }

            if (c == "0") break;

            SaveAll();
        }
    }

    static void StartQuiz()
    {
        Console.Write("Category or mixed: ");
        var cat = Console.ReadLine();

        var rnd = new Random();

        var q = cat == "mixed"
            ? questions.OrderBy(x => rnd.Next()).Take(20).ToList()
            : questions.Where(x => x.Category == cat)
            .OrderBy(x => rnd.Next()).Take(20).ToList();

        int score = 0;

        foreach (var qu in q)
        {
            Console.WriteLine(qu.Text);

            for (int i = 0; i < qu.Answers.Count; i++)
                Console.WriteLine($"{i}:{qu.Answers[i]}");

            var ans = Console.ReadLine()
                .Split(' ')
                .Select(int.Parse)
                .OrderBy(x => x)
                .ToList();

            if (ans.SequenceEqual(qu.Correct.OrderBy(x => x)))
                score++;
        }

        Console.WriteLine($"Score: {score}");

        results.Add(new Result
        {
            Login = currentUser.Login,
            Category = cat,
            Score = score
        });

        SaveAll();
    }
}