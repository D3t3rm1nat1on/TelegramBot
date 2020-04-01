using System;
using System.Collections.Generic;
using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        private static TelegramBotClient _botClient;

        private static string[] _questions = new[]
        {
            "Как мы можем к тебе обращаться? Напиши свои фамилию и имя",
            "Пожалуйста, напиши свой мобильный телефон, чтобы могли с тобой связаться и обсудить все детали",
            "Пожалуйста, укажи, в какой компании ты сейчас работаешь",
            "Пожалуйста, укажи название города, чтобы мы могли подобрать для тебя ближайшую открытую вакансию",
            "Укажи, пожалуйста, дату рождения Число/Месяц/Год рождения",
            "Гражданство",
            "Медицинская книжка",
            "Анкета заполнена, спасибо"
        };

        private static string[] _answers = new string[7];

        private static Dictionary<int, string[]> userDictionaryAnswers = new Dictionary<int, string[]>();

        private static Dictionary<int, int> userDictionary = new Dictionary<int, int>();

        static void Main(string[] args)
        {
            var proxy = new HttpToSocks5Proxy("154.95.16.184", 443);
            _botClient = new TelegramBotClient("1204106611:AAEctHiDVZ6dmreSFI_w2OFMU0GKHc3gaTI", proxy);
            _botClient.OnMessage += BotOnMessageRecived;
            _botClient.OnCallbackQuery += BotOnCallbackQueryRecived;
            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine($"Bot id: {me.Id}. Bot name: {me.FirstName}");
            Console.WriteLine(me.Username);
            _botClient.StartReceiving();

            while (Console.ReadLine() != "exit") ;
            _botClient.StartReceiving();
        }

        private static async void BotOnCallbackQueryRecived(object? sender, CallbackQueryEventArgs e)
        {

            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} нажал: {buttonText}");

            await _botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Вы нажали кнопку {buttonText}");
        }

        private static async void BotOnMessageRecived(object? sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            var text = message.Text;
            var name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"id: {message.From.Id}");
            Console.WriteLine($"{name}: {text}");

            if (userDictionary.ContainsKey(message.From.Id))
            {
                Qr(message.From.Id, text);
                return;
            }

            switch (text)
            {
                case "/start":
                    string hello = @"Список команд:
/start - запуск бота
/inline - вывод меню
/QR - заполнить анкету";
                    await _botClient.SendTextMessageAsync(message.From.Id, hello);
                    break;
                case "/inline":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithUrl("Анкета на сайте",
                            "https://docs.google.com/forms/d/e/1FAIpQLSfQxh28HQ600V2cVyh6GXDaQUzVQpnjkWQwEQkmQwkYfnyRpg/viewform"),
                    });
                    await _botClient.SendTextMessageAsync(message.From.Id, "Выберите пункт меню",
                        replyMarkup: inlineKeyboard);
                    break;
                case "/QR":
                    userDictionary.Add(message.From.Id, 0);
                    userDictionaryAnswers.Add(message.From.Id, new string[7]);
                    await _botClient.SendTextMessageAsync(message.From.Id, _questions[userDictionary[message.From.Id]]);
                    break;

            }
        }

        private static void Qr(int fromId, string answer)
        {
            userDictionaryAnswers[fromId][userDictionary[fromId]] = answer;
            userDictionary[fromId]++;
            _botClient.SendTextMessageAsync(fromId, _questions[userDictionary[fromId]]);
            if (userDictionary[fromId] > 6)
            {
                string[] next = new string[7];
                Array.Copy(userDictionaryAnswers[fromId], next, 7);
                Send(next);
                userDictionary.Remove(fromId);
                userDictionaryAnswers.Remove(fromId);
                string hello = @"Список команд:
/start - запуск бота
/inline - вывод меню
/QR - заполнить анкету";
                _botClient.SendTextMessageAsync(fromId, hello);
            }
        }

        private static async void Send(string[] answers)
        {
            var chatId = 173111653; //Саша
            chatId = 1293493721; //Саша2
            var new_chatId = "-1001283303838"; //группа
            // chatId = 575903766;
            string text = "Новая анкета!\n" + string.Join('\n', answers);
            await _botClient.SendTextMessageAsync(new_chatId, text);
        }
    }
}