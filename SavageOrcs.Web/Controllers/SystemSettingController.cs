using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavageOrcs.Services;
using SavageOrcs.Web.ViewModels.Mark;
using SavageOrcs.Web.ViewModels.SystemSetting;
using System.Data;

namespace SavageOrcs.Web.Controllers
{
    public class SystemSettingController : Controller
    {
        private readonly ISystemService _systemService;

        public SystemSettingController(ISystemService systemService)
        {
            _systemService = systemService;
        }

        [Route("SystemSetting")]
        [Authorize(Roles = "Global admin")]
        [EmailConfirmed]
        public async Task<IActionResult> Index()
        {
            var keys = await _systemService.GetAllKeysAsync();

            var systemSettingDtos = new List<SystemSettingDto>();

            foreach (var key in keys)
            {
                var systemSettingDto = await _systemService.GetSystemSettingAsync(key);

                systemSettingDtos.Add(systemSettingDto);
            }

            return View(systemSettingDtos.ToArray());
        }

        [HttpPost]
        public async Task<JsonResult> Save([FromBody] SystemSettingDto systemSettingDto)
        {
            try {
                await _systemService.SetValueAsync(systemSettingDto);

                return Json(new SaveMarkResultViewModel
                {
                    Id = null,
                    Success = true,
                    Url = "",
                    Text = "Зміни успішно збережені."
                });

            }
            catch (Exception ex) {
                return Json(new SaveMarkResultViewModel
                {
                    Id = null,
                    Success = false,
                    Url = "",
                    Text = "Помилка" + ex.Message
                });
            }
}

        [HttpGet]
        public async Task<JsonResult> Parse()
        {
            var result = await _systemService.Parse();

            return Json(new SaveMarkResultViewModel
            {
                Id = null,
                Success = result == "Success",
                Url = "",
                Text = result
            });
        }
    }
}
