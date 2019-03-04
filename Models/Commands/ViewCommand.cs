using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoymaxTestBot.Models.Commands
{
    public class ViewCommand : Command
    {
        public override string Name => "/view";

        public override void Execute(Message message, TelegramBotClient botClient)
        {
            throw new NotImplementedException();
        }
    }
}
