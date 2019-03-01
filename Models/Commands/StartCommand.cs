using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoymaxTestBot.Models.Commands
{
    public class StartCommand : Command
    {
        public override string Name => @"/start";

        public override async void Execute(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            await botClient.SendTextMessageAsync(chatId,
                                                "Welcome to Loymax Test Bot. First, push the Register button",
                                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}
