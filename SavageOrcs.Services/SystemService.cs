using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SavageOrcs.BusinessObjects;
using SavageOrcs.Repositories.Interfaces;
using SavageOrcs.Services.Interfaces;
using SavageOrcs.UnitOfWork;
using SavageOrcs.Web.ViewModels.SystemSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.Services
{
    public class SystemService : UnitOfWorkService, ISystemService
    {
        private readonly IRepository<SystemSetting> _systemSettingRepository;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Curator> _curatorsRepository;
        private readonly IRepository<Area> _areasRepository;

        public SystemService(IUnitOfWork unitOfWork, IRepository<SystemSetting> systemSettingRepository, IConfiguration configuration, IRepository<Curator> curatorsRepository, IRepository<Area> areasRepository) : base(unitOfWork)
        {
            _systemSettingRepository = systemSettingRepository;
            _configuration = configuration;
            _curatorsRepository = curatorsRepository;
            _areasRepository = areasRepository;
        }

        public async Task<string[]> GetAllKeysAsync()
        {
            return await (await _systemSettingRepository.GetAllAsync()).Select(x => x.Name).ToArrayAsync();
        }

        public async Task<SystemSettingDto> GetSystemSettingAsync(string key)
        {
            var systemSetting = await _systemSettingRepository.GetTAsync(x => x.Name == key);

            if (systemSetting == null) throw new NullReferenceException();

            return CreateSystemSettingDto(systemSetting);
        }

        public async Task<SystemSettingDto> GetSystemSettingAsync(Guid id)
        {
            var systemSetting = await _systemSettingRepository.GetTAsync(x => x.Id == id);

            if (systemSetting == null) throw new NullReferenceException();

            return CreateSystemSettingDto(systemSetting);
        }
        private static SystemSettingDto CreateSystemSettingDto(SystemSetting systemSetting)
        {
            return new SystemSettingDto
            {
                Id = systemSetting.Id,
                Name = systemSetting.Name,
                Value = systemSetting.Value,
            };
        }

        public async Task SetValueAsync(SystemSettingDto systemSettingDto)
        {
            var systemSetting = await _systemSettingRepository.GetTAsync(x => x.Id == systemSettingDto.Id);
            if (systemSetting == null)
            {
                if (string.IsNullOrEmpty(systemSettingDto.Name)) return;
                systemSetting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Name = systemSettingDto.Name
                };
            }

            systemSetting.Value = systemSettingDto.Value;

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<string> Parse()
        {
            try
            {
                string certificatesFolder = _configuration["Certificates:ServerFolderPath"];

#if DEBUG
                certificatesFolder = _configuration["Certificates:DeveloperFolderPath"];
#endif
                var sheetId = (await GetSystemSettingAsync("SpreadsheetId")).Value;
                var sheetName = (await GetSystemSettingAsync("SheetName")).Value;

                var parser = new Parser.Parser
                {
                    CertificatesFolder = certificatesFolder,
                    CertificateDriveEmail = "googledriveautoparser@wallevidence-mizhvukhamy.iam.gserviceaccount.com",
                    CertificateDriveName = "wallevidence-mizhvukhamy-drive.p12",
                    CertificateDriveProjectName = "Wallevidence Mizhvukhamy",
                    CertificateSheetEmail = "googlesheetautoparser@wallevidence-mizhvukhamy.iam.gserviceaccount.com",
                    CertificateSheetName = "wallevidence-mizhvukhamy-sheet.p12",
                    CertificateSheetProjectName = "Wallevidence Mizhvukhamy",
                    SheetId = sheetId,
                    SheetName = sheetName,
                    Areas = (await _areasRepository.GetAllAsync()).ToArray(),
                    Curators = (await _curatorsRepository.GetAllAsync()).ToArray()
                };

                var result = parser.Start();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
    }
}
