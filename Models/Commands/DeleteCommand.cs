using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoymaxTestBot.Models.Commands
{
    public class DeleteCommand : Command
    {
        public override string Name => @"/delete";

        public override void Execute(Message message, TelegramBotClient botClient)
        {
            throw new NotImplementedException();
        }
    }
}
