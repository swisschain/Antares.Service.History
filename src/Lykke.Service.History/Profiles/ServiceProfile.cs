using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.PostgresRepositories.Entities;

namespace Lykke.Service.History.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<SaveCashinCommand, HistoryEntity>(MemberList.Source);
        }
    }
}
