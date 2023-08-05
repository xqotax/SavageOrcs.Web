using SavageOrcs.DataTransferObjects.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.Services.Interfaces
{
    public interface IAreaService
    {
        Task<AreaShortDto[]> GetAreasAsync(string? name = null, string? community = null, string? region = null, int? count = null);

        Task<AreaShortDto[]> GetUsedAreasAsync();

        Task SaveArea(AreaSaveDto areaDto);
    }
}
