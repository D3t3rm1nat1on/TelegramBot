using System;
using System.Collections.Generic;
using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        private static TelegramBotClient _botClient;

        private static string[] _questions = {
            "Как мы можем к тебе обращаться? Напиши свои Фамилию и Имя.",
            "Пожалуйста, напиши свой мобильный телефон, чтобы могли с тобой связаться и обсудить все детали.",
            "Напиши, пожалуйста, в какой компании ты сейчас работаешь.",
            "Пожалуйста, напиши свой город, чтобы мы могли подобрать для тебя ближайшую открытую вакансию.",
            "Напиши, пожалуйста, свою дату рождения: Число/Месяц/Год рождения.",
            "Напиши, пожалуйста, свое гражданство.",
            "У тебя есть Мед.Книжка?",
            "Спасибо за ответы, мы получили твою анкету!\n" +
            "Мы свяжемся с тобой в ближайшее время, чтобы обсудить все детали.\n" +
            "Хорошего дня!"
        };

        private static Dictionary<int, string[]> userDictionaryAnswers = new Dictionary<int, string[]>();

        private static Dictionary<int, int> userDictionary = new Dictionary<int, int>();

        static void Main(string[] args)
        {
            var proxy = new HttpToSocks5Proxy("154.95.16.184", 443);
            //1204106611:AAEctHiDVZ6dmreSFI_w2OFMU0GKHc3gaTI
            _botClient = new TelegramBotClient("1177094519:AAGJBqg0hAV_CiYLPnEae_dsYeAHBdscp5k", proxy);
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

            var messageId = e.CallbackQuery.From.Id;

            if (userDictionary.ContainsKey(e.CallbackQuery.From.Id))
            {
                await _botClient.SendTextMessageAsync(messageId, "*Вы уже заполняете анкету*");
                return;
            }

            if (buttonText == "Заполнить у бота")
            {
                userDictionary.Add(messageId, 0);
                userDictionaryAnswers.Add(messageId, new string[7]);
                await _botClient.SendTextMessageAsync(messageId, _questions[userDictionary[messageId]]);
            }
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
                    string hello = "Привет :)\n" +
                                   "Чтобы найти для тебя самую подходящую вакансию мы хотели бы задать несколько уточняющих вопросов, это займет не более 1 минуты.";
                    await _botClient.SendTextMessageAsync(message.From.Id, hello);
                    Hello(message.From.Id);
                    string dataMessage =
                        "Обращаем внимание, что продолжая наше общение, в соответствии с требованиями статьи 9 Федерального закона от 27.07.2006 № 152-ФЗ «О персональных данных», ты подтверждаешь свое согласие на обработку персональных данных компанией ООО \"АШАН\"";
                    await _botClient.SendTextMessageAsync(message.From.Id, dataMessage);
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
                Hello(fromId);
            }
        }

        private static async void Send(string[] answers)
        {
            var chatId = 173111653; //Саша
            chatId = 1293493721; //Саша2
            var new_chatId = "-1001283303838"; //группа
            // chatId = 575903766; //Я
            string text = "Новая анкета!\n" + string.Join('\n', answers);
            await _botClient.SendTextMessageAsync(new_chatId, text);
        }

        private static async void Hello(int messageId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Анкета на сайте",
                            "https://docs.google.com/forms/d/e/1FAIpQLSfQxh28HQ600V2cVyh6GXDaQUzVQpnjkWQwEQkmQwkYfnyRpg/viewform"),
                        InlineKeyboardButton.WithCallbackData("Заполнить у бота")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Положение о персональных данных",
                            "https://www.auchan.ru/pokupki/personalnye-dannye.html?ndspecialkey=1")
                    }
                });
            await _botClient.SendTextMessageAsync(messageId, "Выберите пункт меню",
                replyMarkup: inlineKeyboard);
        }
    }
}