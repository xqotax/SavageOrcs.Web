﻿using SavageOrcs.BusinessObjects;
using SavageOrcs.DataTransferObjects.Curators;
using SavageOrcs.DataTransferObjects.Marks;
using SavageOrcs.DataTransferObjects.Texts;
using SavageOrcs.Repositories.Interfaces;
using SavageOrcs.Services.Interfaces;
using SavageOrcs.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.Services
{
    public class CuratorService : UnitOfWorkService, ICuratorService
    {
        private readonly IRepository<Curator> _curatorRepository;
        private readonly IRepository<Mark> _markRepository;
        private readonly IRepository<Cluster> _clusterRepository;
        private readonly IRepository<Text> _textRepository;


        public CuratorService(IUnitOfWork unitOfWork, IRepository<Curator> curatorRepositor, IRepository<Cluster> clusterRepository, IRepository<Mark> markRepository, IRepository<Text> textRepository) : base(unitOfWork)
        {
            _curatorRepository = curatorRepositor;
            _clusterRepository = clusterRepository;
            _markRepository = markRepository;
            _textRepository = textRepository;
        }

        public async Task<CuratorDto[]> GetCurators()
        {
            var curators = await _curatorRepository.GetAllAsync();

            return curators.Select(x => CreateCuratorDto(x)).ToArray();
        }

        public async Task<CuratorDto> GetCuratorById(Guid id)
        {
            var curator = await _curatorRepository.GetTAsync(x => x.Id == id);

            curator ??= new Curator();

            return CreateCuratorDto(curator);
        }


        private static CuratorDto CreateCuratorDto(Curator curator)
        {
            return new CuratorDto
            {
                Id = curator.Id,
                DisplayNameEng = curator.NameEng,
                DisplayName = curator.Name,
                Description = curator.Description,
                DescriptionEng = curator.DescriptionEng,
                Image = curator.Image,
                TextDtos = curator.Texts.Select(x => new TextShortDto { 
                    Id = x.Id,
                    Name = x.Name,
                    Subject = x.Subject
                }).ToArray(),
                MarkDtos = curator.Marks.Select(x => new MarkShortDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameEng = x.NameEng,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    Description = x.Description,
                    DescriptionEng = x.DescriptionEng,
                    CuratorName = x.Curator is null ? "" : x.Curator.Name,
                    CuratorNameEng = x.Curator is null ? "" : x.Curator.NameEng,
                    ClusterName = x.Cluster?.Name,
                    ClusterNameEng = x.Cluster?.NameEng,
                    ResourceUrl = x.ResourceUrl,
                    ResourceName = x.ResourceName,
                }).ToArray()
            };
        }

        public async Task<CuratorSaveResultDto> SaveCurator(CuratorSaveDto curatorSaveDto)
        {
            try
            {
                var curator = new Curator();

                if (curatorSaveDto.Id is not null)
                {
                    curator = await _curatorRepository.GetTAsync(x => x.Id == curatorSaveDto.Id);
                    curator ??= new Curator();
                }
                else
                {
                    curator.Id = Guid.NewGuid();
                    await _curatorRepository.AddAsync(curator);
                }

                curator.Name = curatorSaveDto.DisplayName;
                curator.NameEng = curatorSaveDto.DisplayNameEng;
                curator.Description = curatorSaveDto.Description;
                curator.DescriptionEng = curatorSaveDto.DescriptionEng;
                curator.Image = curatorSaveDto.Image;

                await UnitOfWork.SaveChangesAsync();
                return new CuratorSaveResultDto()
                {
                    Success = true,
                    Id = curator.Id
                };
            }
            catch
            {
                return new CuratorSaveResultDto()
                {
                    Success = false,
                    Id = null
                };
            }
        }

        public async Task<bool> DeleteCurator(Guid id)
        {
            try
            {
                var curator = _curatorRepository.GetT(x => x.Id == id);

                var marks = _markRepository.GetAll(x => x.CuratorId == id);
                var clusters = _clusterRepository.GetAll(x => x.CuratorId == id);
                var texts = _textRepository.GetAll(x => x.CuratorId == id);

                foreach (var mark in marks)
                {
                    mark.CuratorId = null;
                }

                foreach (var cluster in clusters)
                {
                    cluster.CuratorId = null;
                }

                foreach (var text in texts)
                {
                    text.CuratorId = null;
                }

                _curatorRepository.Delete(curator);

                await UnitOfWork.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
