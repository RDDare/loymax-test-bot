using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LoymaxTestBot.Models.Commands
{
    public class StartCommand : Command
    {
        public override string Name => "/start";

        public override async void Execute(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            InlineKeyboardButton button = new InlineKeyboardButton();
            button.Text = "\U0001F4DD Register";
            button.CallbackData = "register";
            
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(button);

            await botClient.SendTextMessageAsync(
                chatId,
                "Welcome to Loymax Test Bot. First, push the /register button",
                replyMarkup: keyboard);


        }
    }
}
