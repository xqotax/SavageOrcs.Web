using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SavageOrcs.BusinessObjects;
using SavageOrcs.Repositories.Interfaces;
using SavageOrcs.Services.Interfaces;
using SavageOrcs.UnitOfWork;
using SavageOrcs.Web.ViewModels.SystemSetting;

namespace SavageOrcs.Services
{
    public class SystemService : UnitOfWorkService, ISystemService
    {
        private readonly IRepository<SystemSetting> _systemSettingRepository;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Curator> _curatorsRepository;
        private readonly IRepository<Area> _areasRepository;
        private readonly IRepository<Mark> _marksRepository;
        private readonly IRepository<Image> _imagesRepository;

        public SystemService(IUnitOfWork unitOfWork, IRepository<SystemSetting> systemSettingRepository, IConfiguration configuration, IRepository<Curator> curatorsRepository, IRepository<Area> areasRepository, IRepository<Mark> marksRepository, IRepository<Image> imagesRepository) : base(unitOfWork)
        {
            _systemSettingRepository = systemSettingRepository;
            _configuration = configuration;
            _curatorsRepository = curatorsRepository;
            _areasRepository = areasRepository;
            _marksRepository = marksRepository;
            _imagesRepository = imagesRepository;
        }

        public async Task<string[]> GetAllKeysAsync()
        {
            return await (await _systemSettingRepository.GetAllAsync()).Select(x => x.Name).ToArrayAsync();
        }

        public async Task<SystemSettingDto> GetSystemSettingAsync(string key)
        {
            var systemSetting = await _systemSettingRepository.GetTAsync(x => x.Name == key);

            return systemSetting == null ? throw new NullReferenceException() : CreateSystemSettingDto(systemSetting);
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
                string certificatesFolder = _configuration["Certificates:FolderPath"];

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

                await _marksRepository.AddRangeAsync(result.Marks.Values);
                await _imagesRepository.AddRangeAsync(result.Images.Values);

                await UnitOfWork.SaveChangesAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
    }
}
