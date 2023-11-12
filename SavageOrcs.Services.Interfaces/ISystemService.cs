using SavageOrcs.Web.ViewModels.SystemSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.Services
{
    public interface ISystemService
    {
        Task<string[]> GetAllKeysAsync();

        Task<SystemSettingDto> GetSystemSettingAsync(string key);

        Task<SystemSettingDto> GetSystemSettingAsync(Guid id);

        Task SetValueAsync(SystemSettingDto systemSettingDto);

        Task<string> Parse();
    }
}
