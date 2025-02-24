﻿using WMS.Business.Settings;
using WMS.Business.CommonManagement.UploadBlobFiles;
using WMS.Business.CommonManagement.UploadBlobFiles.Dto;
using WMS.Business.Settings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace WMS.Business.UploadBlobFiles
{
    /// <summary>
    /// 基础Blob二进制文件存储
    /// </summary>
    [Area(ABPVNextRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = ABPVNextRemoteServiceConsts.RemoteServiceName)]
    [Route($"{ApiConsts.RootPath}fileblob")]
    [ApiExplorerSettings(GroupName = "Infra", IgnoreApi = false)]
    public class FileStorageBlobController : ABPVNextController, IFileStorageBlobAppService
    {
        private readonly IFileStorageBlobAppService _fileContainer;//文件类型的Blob容器
        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="fileContainer"></param>
        public FileStorageBlobController(IFileStorageBlobAppService fileContainer)
        {
            _fileContainer = fileContainer;
        }
        /// <summary>
        /// 保存二进制
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SaveBlobFile")]
        public virtual async Task SaveBlobAsync(SaveFileBlobInputDto input)
        {
            await _fileContainer.SaveBlobAsync(input);
        }
        /// <summary>
        /// 读取二进制
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBlobFile")]
        public virtual async Task<BlobFilesDto> GetBlobAsync(GetBlobFileRequestDto input)
        {
            return await _fileContainer.GetBlobAsync(input);

        }
    }
}
