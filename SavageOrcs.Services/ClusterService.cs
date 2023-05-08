﻿using SavageOrcs.BusinessObjects;
using SavageOrcs.DataTransferObjects._Constants;
using SavageOrcs.DataTransferObjects.Areas;
using SavageOrcs.DataTransferObjects.Cluster;
using SavageOrcs.DataTransferObjects.Clusters;
using SavageOrcs.DataTransferObjects.Marks;
using SavageOrcs.DbContext.Migrations;
using SavageOrcs.Repositories.Interfaces;
using SavageOrcs.Services.Interfaces;
using SavageOrcs.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SavageOrcs.Services
{
    public class ClusterService : UnitOfWorkService, IClusterService
    {
        private readonly IRepository<Cluster> _clusterRepository;
        private readonly IRepository<Mark> _markRepository;
        private readonly IHelperService _helperService;
        private readonly IRepository<Image> _imageRepository;
        private readonly IRepository<TextToCluster> _textsToClustersRepository;

        public ClusterService(IUnitOfWork unitOfWork, IRepository<Cluster> clusterRepository, IRepository<Image> imageRepository, IRepository<Mark> markRepository, IRepository<TextToCluster> textsToClustersRepository, IHelperService helperService) : base(unitOfWork)
        {
            _clusterRepository = clusterRepository;
            _imageRepository = imageRepository;
            _markRepository = markRepository;
            _textsToClustersRepository = textsToClustersRepository;
            _helperService = helperService;
        }

        public async Task<ClusterDto[]> GetClusters()
        {
            var clusters = await _clusterRepository.GetAllAsync();

            return clusters.Select(x => CreateClusterDto(x)).ToArray();
        }

        public async Task<ClusterDto?> GetClusterById(Guid id, bool withImage)
        {
            var cluster = await _clusterRepository.GetTAsync(x => x.Id == id);

            return cluster is null ? null : CreateClusterDto(cluster, withImage);
        }

        public async Task<ClusterDto[]> GetClustersByFilters(Guid[]? keyWordIds,  Guid[]? clusterIds, Guid[]? areaIds)
        {
            var clusters = await _clusterRepository.GetAllAsync();

            var resultClusters = new List<Cluster>();

            var filterByClusterIds = clusterIds is not null && clusterIds.Length > 0;
            var filterByKeyWordIds = keyWordIds is not null && keyWordIds.Length > 0;
            var filterByAreaIds = areaIds is not null && areaIds.Length > 0;

            if ( !filterByClusterIds && !filterByKeyWordIds && !filterByAreaIds)
                return clusters.Select(x => CreateClusterDto(x)).ToArray();

            if (filterByKeyWordIds)
            {
                if (filterByKeyWordIds)
                {
                    var keyWords = (await _helperService.GetAllKeyWords()).Where(x => keyWordIds.Contains(x.Id) && !string.IsNullOrEmpty(x.Name)).Select(x => _helperService.GetTranslation(x.Name, x.NameEng)).ToArray();

                    foreach (var cluster in clusters)
                    {
                        if (!string.IsNullOrEmpty(cluster.DescriptionEng)
                            && keyWords.Any(y => Regex.IsMatch(cluster.DescriptionEng, @"\b" + Regex.Escape(y) + @"\b", RegexOptions.IgnoreCase)
                            || Regex.IsMatch(y, @"\b" + Regex.Escape(cluster.DescriptionEng) + @"\b", RegexOptions.IgnoreCase)))
                        {
                            resultClusters.Add(cluster);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(cluster.Description)
                            && keyWords.Any(y => Regex.IsMatch(cluster.Description, @"\b" + Regex.Escape(y) + @"\b", RegexOptions.IgnoreCase)
                            || Regex.IsMatch(y, @"\b" + Regex.Escape(cluster.Description) + @"\b", RegexOptions.IgnoreCase)))
                        {
                            resultClusters.Add(cluster);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(cluster.Name)
                            && keyWords.Any(y => Regex.IsMatch(cluster.Name, @"\b" + Regex.Escape(y) + @"\b", RegexOptions.IgnoreCase)
                            || Regex.IsMatch(y, @"\b" + Regex.Escape(cluster.Name) + @"\b", RegexOptions.IgnoreCase)))
                        {
                            resultClusters.Add(cluster);
                            continue;
                        }
                    }
                }
            }

            if (filterByClusterIds)
            {
                resultClusters.AddRange(clusters.Where(x => clusterIds.Contains(x.Id)).ToList());
            }

            if (filterByAreaIds)
            {
                resultClusters.AddRange(clusters.Where(x => x.AreaId.HasValue && areaIds.Contains(x.AreaId.Value)).ToList());
            }

            resultClusters = resultClusters.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            return resultClusters.Select(x => CreateClusterDto(x)).ToArray();
        }

        public async Task<ClusterSaveResultDto> SaveCluster(ClusterSaveDto clusterSaveDto)
        {
            var cluster = new Cluster();

            if (clusterSaveDto.Id is not null)
            {
                cluster = await _clusterRepository.GetTAsync(x => x.Id == clusterSaveDto.Id);
                cluster ??= new Cluster();
            }
            else
            {
                cluster.Id = Guid.NewGuid();
                cluster.CreatedDate = DateTime.Now;
                await _clusterRepository.AddAsync(cluster);
            }

            cluster.UpdatedDate = DateTime.Now;
            cluster.Name = clusterSaveDto.Name;
            cluster.NameEng = clusterSaveDto.NameEng;
            cluster.Description = clusterSaveDto.Description;
            cluster.DescriptionEng = clusterSaveDto.DescriptionEng;
            cluster.ResourceName = clusterSaveDto.ResourceName;
            cluster.ResourceUrl = clusterSaveDto.ResourceUrl;
            cluster.Lat = clusterSaveDto.Lat;
            cluster.Lng = clusterSaveDto.Lng;
            cluster.MapId = clusterSaveDto.MapId;

            if (clusterSaveDto.AreaId == _Constants.EmptySelect)
                cluster.AreaId = null;
            else
                cluster.AreaId = clusterSaveDto.AreaId;

            if (clusterSaveDto.CuratorId == _Constants.EmptySelect)
                cluster.CuratorId = null;
            else
                cluster.CuratorId = clusterSaveDto.CuratorId;


            await UnitOfWork.SaveChangesAsync();

            return new ClusterSaveResultDto
            {
                Id = cluster.Id,
                Success = true
            };
        }



        private static ClusterDto CreateClusterDto(Cluster cluster, bool withImage = false)
        {
            return new ClusterDto
            {
                Id = cluster.Id,
                Name = cluster.Name,
                NameEng = cluster.NameEng,
                Description = cluster.Description,
                DescriptionEng = cluster.DescriptionEng,
                ResourceName = cluster.ResourceName,
                ResourceNameEng = cluster.ResourceNameEng,
                ResourceUrl = cluster.ResourceUrl,
                Curator = cluster.Curator is null
                    ? new GuidIdAndStringNameWithEnglishName { } 
                    : new GuidIdAndStringNameWithEnglishName { 
                        Name = cluster.Curator.Name, 
                        Id = cluster.Curator.Id, 
                        NameEng = cluster .Curator.NameEng
                    },
                Lat = cluster.Lat,
                Lng = cluster.Lng,
                Area = cluster.Area is null ? null : new AreaShortDto
                {
                    Id = cluster.Area.Id,
                    Name = cluster.Area.Name,
                    Region = cluster.Area.Region,
                    Community = cluster.Area.Community
                },
                Marks = cluster.Marks.Select(x => new ClusterMarkDto
                { 
                    Id = x.Id,
                    Name = x.Name,
                    NameEng = x.NameEng,
                    Description = x.Description,
                    ResourceName = x.ResourceName,
                    ResourceNameEng= x.ResourceNameEng,
                    IsVisible = x.IsVisible,
                    CuratorName = x.Curator?.Name,
                    DescriptionEng = x.DescriptionEng,
                    ResourceUrl = x.ResourceUrl,
                    Images = withImage ? x.Images.Select( y => new ByteContentAndBooIsVisible
                    {
                        IsVisible = y.IsVisible,
                        Content = y.Content
                    }).ToArray() : Array.Empty<ByteContentAndBooIsVisible>(),
                    Area = x.Area is null? null : new AreaShortDto
                    {
                        Id = x.Area.Id,
                        Name = x.Area.Name,
                        Community = x.Area.Community,
                        Region = x.Area.Region,
                        RegionEng = x.Area.RegionEng,
                        CommunityEng = x.Area.CommunityEng,
                        NameEng = x.Area.NameEng
                    }
                }).ToArray()
            };
        }

        public async Task<bool> DeleteCluster(Guid id, bool withMarks)
        {
            try
            {
                var cluster = await _clusterRepository.GetTAsync(x => x.Id == id);

                if (cluster is null) return false;

                if (withMarks)
                {
                    foreach (var mark in cluster.Marks)
                    {
                        _imageRepository.DeleteRange(mark.Images);
                        _markRepository.Delete(mark);
                    }
                }
                
                else
                {
                    foreach (var mark in cluster.Marks)
                    {
                        if (mark.Lat is null)
                            mark.Lat = cluster.Lat;
                        if (mark.Lng is null)
                            mark.Lng = cluster.Lng;
                        if (mark.AreaId is null)
                            mark.AreaId = cluster.AreaId;
                        mark.ClusterId = null;
                    }
                }

                foreach (var textToCluster in cluster.TextsToClusters)
                {
                    _textsToClustersRepository.Delete(textToCluster);
                }

                _clusterRepository.Delete(cluster);

                await UnitOfWork.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
