﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comm.ConsulConfig.Registry
{
    /// <summary>
    /// 服务注册
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        void Register();

        /// <summary>
        /// 撤销服务
        /// </summary>
        void Deregister();
    }
}
