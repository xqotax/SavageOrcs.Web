﻿using SavageOrcs.DataTransferObjects.Curators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.Services.Interfaces
{
    public interface ICuratorService
    {
        Task<CuratorDto[]> GetCurators();

        Task<CuratorDto> GetCuratorById(Guid id);

        Task<CuratorSaveResultDto> SaveCurator(CuratorSaveDto curatorSaveDto);

        Task<bool> DeleteCurator(Guid id);
    }
}
