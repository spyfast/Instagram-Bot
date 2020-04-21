using InstaBot.Config;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace InstaBot
{
    class Program
    {
        private static UserSessionData UserSessionData;
        private static IInstaApi _instaAPI;
        private static Random Random { get; set; }
        static void Main(string[] args)
        {
            Console.Title = "Instagram Bot";

            if (!File.Exists("InstaSharper.dll") || !File.Exists("Newtonsoft.Json.dll"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log.Push("Отсутствуют необходимые .dll для работы приложения: Newtonsoft.Json.dll, InstaSharper.dll");
            }
            else
            {
                UserSessionData = new UserSessionData();
                Random = new Random();

                if (!File.Exists("accounts.txt"))
                    File.Create("accounts.txt").Close();
                if (!File.Exists("messages.txt"))
                    File.Create("messages.txt").Close();

                Console.WriteLine("Введите задержку в миллисекундах");
                var Delay = int.Parse(Console.ReadLine());

                foreach (var item in File.ReadAllLines("accounts.txt"))
                {
                    var UserName = item.Split(':')[0];
                    var Password = item.Split(':')[1];

                    UserSessionData.UserName = UserName;
                    UserSessionData.Password = Password;

                    Routes(UserName, Delay);
                }
            }
            Console.ReadKey(true);
        }

        public static async void Routes(string UserName, int Delay)
        {
            _instaAPI = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData)
                .UseLogger(new DebugLogger(LogLevel.Exceptions)).Build();

            var request = await _instaAPI.LoginAsync();
         
            var userId = File.ReadAllLines("Users.txt").ToList();

            if (request.Succeeded)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Log.Push($"Успешная авторизация: {UserName}");

                while (true)
                {
                    try
                    {
                        var user = await _instaAPI.GetUserAsync(
                            userId[Random.Next(0, userId.Count)]);
                        var message = File.ReadAllLines("messages.txt").ToList();

                        if (message.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Log.Push("Отсутствуют фразы.");
                            break;
                        }

                        if (userId.Count == 0 || !File.Exists("Users.txt"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Log.Push("База пользователей отсутствует или ещё не была содана.");
                            break;
                        }

                        if (user.Value != null)
                        {
                            var sendMessage = await _instaAPI.SendDirectMessage(user.Value.Pk.ToString(), null, 
                                message[Random.Next(0, message.Count)]);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            if (sendMessage.Succeeded)
                                Log.Push($"Сообщение было успешно отправлено на аккаунте: {UserName}");
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Log.Push($"Ошибка при отправке сообщения:\n\tАккаунт: {UserName}\n\tСообщение {user.Info.Message}");
                            }
                        }

                        Thread.Sleep(Delay);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Log.Push($"Неизвестная ошибка: {ex.Message}");
                        _instaAPI = null;
                    }
                }
            }
            else
                Log.Push($"Ошибка авторизации: {request.Info.Message}");
        }
    }
}
