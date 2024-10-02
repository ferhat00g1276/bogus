using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;

class Program
{
    static void Main(string[] args)
    {
        generateUsers(5);
        Console.WriteLine("Choose threading mode: (1) Single, (2) Multiple");
        var choice = Console.ReadLine();
        bool useMultipleThreads = choice == "2";
        LoadUsers(useMultipleThreads);
    }

    static void generateUsers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Faker<User> faker = new Faker<User>()
                .RuleFor(u => u.Name, f => f.Person.FirstName)
                .RuleFor(u => u.Surname, f => f.Person.LastName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.DateOfBirth, f => f.Person.DateOfBirth);
            var users = faker.Generate(50); 
            var json = JsonSerializer.Serialize(users); 
            File.WriteAllText($"users{i + 1}.json", json); 
        }
    }

    static List<User> ReadJsonFile(string filePath)
    {
        var json = File.ReadAllText(filePath); 
        return JsonSerializer.Deserialize<List<User>>(json); 
    }

    static void SingleThreadRead(List<User> userList, int fileCount)
    {
        for (int i = 1; i <= fileCount; i++)
        {
            var filePath = $"users{i}.json";
            var usersFromFile = ReadJsonFile(filePath);
            foreach (var user in usersFromFile)
            {
                userList.Add(user);  
            }
        }
    }

    static void MultiThreadRead(List<User> userList, int fileCount)
    {
        List<Task> tasks = new List<Task>();
        for (int i = 1; i <= fileCount; i++)
        {
            var index = i; 
            tasks.Add(Task.Run(() =>
            {
                var filePath = $"users{index}.json";
                var usersFromFile = ReadJsonFile(filePath);
                lock (userList)
                {  
                    foreach (var user in usersFromFile)
                    {
                        userList.Add(user); 
                    }
                }
            }));
        }
        Task.WaitAll(tasks.ToArray()); 
    }

    static void LoadUsers(bool useMultipleThreads)
    {
        List<User> allUsers = new List<User>(); 
        int fileCount = 5;  
        if (useMultipleThreads)
        {
            MultiThreadRead(allUsers, fileCount); 
        }
        else
        {
            SingleThreadRead(allUsers, fileCount); 
        }
        Console.WriteLine($"Loaded {allUsers.Count} users");
    }
}

class User
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
}
