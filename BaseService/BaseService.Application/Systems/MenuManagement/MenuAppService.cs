﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseService.Permissions;
using BaseService.Systems.MenuManagement.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BaseService.Systems.MenuManagement
{
    [Authorize(BaseServicePermissions.Menu.Default)]
    public class MenuAppService : ApplicationService, IMenuAppService
    {
        private readonly IRepository<Menu, Guid> _repository;

        public MenuAppService(IRepository<Menu, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<MenuDto> Create(CreateOrUpdateMenuDto input)
        {
            var result = await _repository.InsertAsync(new Menu(GuidGenerator.Create())
            {
                FormId = input.FormId,
                Pid = input.Pid,
                Name = input.Name,
                Label = input.Label,
                CategoryId = input.CategoryId,
                Sort = input.Sort,
                Path = input.Path,
                Component = input.Component,
                Permission = input.Permission,
                Icon = input.Icon,
                AlwaysShow = input.Pid == null ? true : false,
                Hidden = input.CategoryId == 2 ? true : false,
                AppId = input.AppId,//新增业务Id
            });
            return ObjectMapper.Map<Menu, MenuDto>(result);
        }

        public async Task Delete(List<Guid> ids)
        {
            //TODO：删除子集
            foreach (var id in ids)
            {
                await _repository.DeleteAsync(id);
            }
        }

        public async Task<ListResultDto<MenuDto>> GetAll(GetMenuInputDto input)
        {
            var query = (await _repository.GetQueryableAsync()).WhereIf(!string.IsNullOrWhiteSpace(input.Filter), _ => _.Name.Contains(input.Filter));
            var items = await query.OrderBy(input.Sorting ?? "Sort")
                                   .ToListAsync();
            var dtos = ObjectMapper.Map<List<Menu>, List<MenuDto>>(items);
            return new ListResultDto<MenuDto>(dtos);
        }

        /// <summary>
        /// 扩展-用于区分接入的不同项目
        /// </summary>
        /// <param name="input"></param>
        /// <param name="AppId">保持唯一</param>
        /// <returns></returns>
        public async Task<ListResultDto<MenuDto>> GetAll(GetMenuInputDto input, string AppId)
        {
            if (string.IsNullOrEmpty(AppId))
                throw new BusinessException(message: "传入的项目ID为空");
            var _query = (await _repository.GetQueryableAsync()).Where(p => p.AppId == AppId);
            var query = _query.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), _ => _.Name.Contains(input.Filter));
            var items = await query.OrderBy(input.Sorting ?? "Sort")
                                   .ToListAsync();
            var dtos = ObjectMapper.Map<List<Menu>, List<MenuDto>>(items);
            return new ListResultDto<MenuDto>(dtos);
        }

        public async Task<MenuDto> Get(Guid id)
        {
            var result = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<Menu, MenuDto>(result);
            if (dto.Pid.HasValue)
                dto.ParentLabel = (await _repository.FirstOrDefaultAsync(_ => _.Id == result.Pid))?.Label;
            return dto;
        }

        public async Task<ListResultDto<MenuDto>> LoadAll(Guid? id)
        {
            var items = await (await _repository.GetQueryableAsync()).Where(_ => _.Pid == id).OrderBy(_ => _.Sort).ToListAsync();
            var dtos = ObjectMapper.Map<List<Menu>, List<MenuDto>>(items);
            return new ListResultDto<MenuDto>(dtos);
        }

        /// <summary>
        /// 区分接入的不同系统
        /// </summary>
        /// <param name="id"></param>
        /// <param name="AppId">项目ID</param>
        /// <returns></returns>
        public async Task<ListResultDto<MenuDto>> LoadAll(Guid? id, string AppId)
        {
            if (string.IsNullOrEmpty(AppId))
                throw new BusinessException(message: "传入的项目ID为空");
            var items = await (await _repository.GetQueryableAsync()).Where(_ => _.Pid == id && _.AppId == AppId).OrderBy(_ => _.Sort).ToListAsync();
            var dtos = ObjectMapper.Map<List<Menu>, List<MenuDto>>(items);
            return new ListResultDto<MenuDto>(dtos);
        }

        public async Task<MenuDto> Update(Guid id, CreateOrUpdateMenuDto input)
        {
            var menu = await _repository.GetAsync(id);
            menu.Pid = input.Pid;
            menu.CategoryId = input.CategoryId;
            menu.Name = input.Name;
            menu.Label = input.Label;
            menu.Sort = input.Sort;
            menu.Path = input.Path;
            menu.Component = input.Component;
            menu.Icon = input.Icon;
            menu.Permission = input.Permission;
            menu.AppId = input.AppId;//新增业务Id

            return ObjectMapper.Map<Menu, MenuDto>(menu);
        }
    }
}
