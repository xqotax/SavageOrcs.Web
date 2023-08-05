using SavageOrcs.BusinessObjects;
using SavageOrcs.DataTransferObjects.Areas;
using SavageOrcs.DataTransferObjects.Marks;
using SavageOrcs.Repositories.Interfaces;
using SavageOrcs.Services.Interfaces;
using SavageOrcs.UnitOfWork;

namespace SavageOrcs.Services
{
    public class AreaService : UnitOfWorkService, IAreaService
    {
        private readonly IRepository<Area> _areaRepository;
        private readonly IRepository<Mark> _markRepository;
        private readonly IRepository<Cluster> _clusterRepository;
        public AreaService(IUnitOfWork unitOfWork, IRepository<Area> areaRepository, IRepository<Mark> markRepository, IRepository<Cluster> clusterRepository) : base(unitOfWork)
        {
            _areaRepository = areaRepository;
            _markRepository = markRepository;
            _clusterRepository = clusterRepository;
        }

        public async Task<AreaShortDto[]> GetUsedAreasAsync()
        {
            var areasFromMarks = (await _markRepository.GetAllAsync(x => x.Area != null))
                .ToArray()
                .Select(x => new AreaShortDto
                {
                    Community = x.Area.Community,
                    Id = x.Area.Id,
                    Region = x.Area.Region,
                    Name = x.Area.Name,
                    CommunityEng = x.Area.CommunityEng,
                    NameEng = x.Area.NameEng,
                    RegionEng = x.Area.RegionEng
                });

            var areasFromCluster = (await _clusterRepository.GetAllAsync(x => x.Area != null))
                .ToArray()
                .Select(x => new AreaShortDto
                {
                    Community = x.Area.Community,
                    Id = x.Area.Id,
                    Region = x.Area.Region,
                    Name = x.Area.Name,
                    CommunityEng = x.Area.CommunityEng,
                    NameEng = x.Area.NameEng,
                    RegionEng = x.Area.RegionEng
                });

            return areasFromCluster.Concat(areasFromMarks).GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Region).ThenBy(x => x.Name).ToArray();
        }

        public async Task<AreaShortDto[]> GetAreasAsync(string? name = null, string? community = null, string? region = null, int? count = null)
        {
            var areas = await _areaRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(name)) areas = areas.Where(x => x.Name.Contains(name) || x.NameEng.Contains(name));

            if (!string.IsNullOrEmpty(community)) areas = areas.Where(x => x.Community.Contains(community) || x.CommunityEng.Contains(community));

            if (!string.IsNullOrEmpty(region)) areas = areas.Where(x => x.Region.Contains(region) || x.RegionEng.Contains(region));

            if (count.HasValue) areas = areas.Take(count.Value);

            return areas.Select(x => new AreaShortDto
            {
                Community = x.Community,
                Id = x.Id,
                Region = x.Region,
                Name = x.Name,
                RegionEng = x.RegionEng,
                NameEng = x.NameEng,
                CommunityEng = x.CommunityEng
            }).ToArray();
        }

        public async Task SaveArea(AreaSaveDto areaDto)
        {
            var area = new Area();

            if (areaDto.Id is not null)
            {
                area = await _areaRepository.GetTAsync(x => x.Id == areaDto.Id);
                area ??= new Area();
            }
            else
            {
                area.Id = Guid.NewGuid();

                if (!string.IsNullOrEmpty(areaDto.Name) && !string.IsNullOrEmpty(areaDto.Community) && !string.IsNullOrEmpty(areaDto.Region))
                    await _areaRepository.AddAsync(area);
            }

            area.Name = areaDto.Name;
            area.Region = areaDto.Region;
            area.NameEng = areaDto.NameEng;
            area.CommunityEng = areaDto.CommunityEng;
            area.Community = areaDto.Community;
            area.RegionEng = areaDto.RegionEng;

            await UnitOfWork.SaveChangesAsync();
        }
    }
}
