using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using MumArchitecture.Domain;

namespace MumArchitecture.Business.Notification
{
    public class TelegramNotify 
    {
        private readonly TelegramBotClient _botClient;
        private readonly long _chatId;

        public TelegramNotify(long chatId)
        {
            _botClient = new TelegramBotClient(AppSettings.instance!.Telegram!.ApiToken!);
            _chatId = chatId;
        }
        public TelegramNotify()
        {
            _botClient = new TelegramBotClient(AppSettings.instance!.Telegram!.ApiToken!);
            _chatId = AppSettings.instance.Telegram.DevChatId.GetValueOrDefault();
        }

        public async Task SendMessage(string message, ParseMode parseMode=ParseMode.Markdown)
        {
            try
            {
                await _botClient.SendMessage(
                    chatId: _chatId,
                    text: message,
                    parseMode: parseMode
                );
            }
            catch
            {
                
            }
        }

       

    }
}
