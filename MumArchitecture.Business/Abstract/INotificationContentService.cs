using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Abstract
{
    public interface INotificationContentService
    {
        //aynı tipte ve aynı dilde ve aynı target ta birden fazla kayıt olamaz
        //varaiables denilen şey content içindeki değiştirilebilir kısımlardır. örneğin [username] gibi 
        public Task<SystemResult<NotificationContentListDto>> Save(NotificationContentDto dto);
        public Task<SystemResult<NotificationContentListDto>> Get(int id);
        public Task<SystemResult<NotificationContentListDto>> Get(Filter<NotificationContent> filter);
        
        //filter ile bulunan content içerisindeki değiştirilebilir alanların dictionarydeki karşılığına göre değiştirilerek return edileceği fonksiyondur.
        //Aynı işlemler content ve subject için uygulanacak 
        //değişkenlerin neler olduğu bilgisine Varaiables değişkeninden ulaşabilirsin. 
        public Task<SystemResult<List<NotificationContentListDto>>> Get(Filter<NotificationContent> filter, Dictionary<string, string> variables);

        public Task<SystemResult<NotificationContentListDto>> Delete(int id);
        public Task<PaggingResult<NotificationContentListDto>> GetAll(Filter<NotificationContent> filter);
    }
}
