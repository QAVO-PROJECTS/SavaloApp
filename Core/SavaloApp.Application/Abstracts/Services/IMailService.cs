using AutoMapper.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavaloApp.Domain.HelperEntities;

namespace SavaloApp.Application.Abstracts.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }

}
