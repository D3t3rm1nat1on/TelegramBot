using System;
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
            
            while (Console.ReadLine() != "exit");
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
            
            Console.WriteLine($"{name}: {text}");

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
                    var inlineKeyboard = new InlineKeyboardMarkup(new []
                    {
                        InlineKeyboardButton.WithUrl("Анкета на сайте", "https://yandex.ru"),
                        InlineKeyboardButton.WithCallbackData("Пункт 1")
                    });
                    await _botClient.SendTextMessageAsync(message.From.Id, "Выберите пункт меню",
                        replyMarkup: inlineKeyboard);
                    break;
                case "/QR":
                    break;
                
            }
        }
    }
}