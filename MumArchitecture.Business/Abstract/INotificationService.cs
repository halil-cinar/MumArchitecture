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
    public interface IMailService
    {
        public Task<SystemResult<MailBoxListDto>> SaveMail(MailBoxDto dto);
        public Task<SystemResult<Nothing>> SendMail();
        public Task<SystemResult<Nothing>> SendMail(Filter<MailBox> filter);
        public Task<SystemResult<Nothing>> OpenMail(int id);
        public Task<PaggingResult<MailBoxListDto>> GetAllMails(Filter<MailBox> filter);
        public Task<SystemResult<MailBoxListDto>> GetMail(Filter<MailBox> filter);

    }
}
