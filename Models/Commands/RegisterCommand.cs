using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using LoymaxTestBot.Controllers;
using Microsoft.EntityFrameworkCore;

namespace LoymaxTestBot.Models.Commands
{
    public class RegisterCommand : Command
    {
        public override string Name => "/register";

        public override async void Execute(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            try
            {
            await botClient.SendTextMessageAsync(chatId, "Enter first name");
            }
            catch(Exception er)
            {
                await botClient.SendTextMessageAsync(chatId, er.ToString());
            }



        }
    }
}
