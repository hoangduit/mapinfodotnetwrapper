﻿using Mapinfo.Wrapper.Core.Extensions;
using System;
using Mapinfo.Wrapper.Mapinfo;

namespace Mapinfo.Wrapper.Embedding
{
    /// <summary>
    /// Contains methods for retiving application information about
    /// the current running instance of Mapinfo.
    /// </summary>
    public class SystemInfo
    {
        private readonly MapinfoSession misession;

        /// <summary>
        /// Creates a new SystemInfo object used to retive information
        /// about the current instace of mapinfo.
        /// </summary>
        /// <param name="mapinfoWrapper">A wrapper object containing the running instace of mapinfo.</param>
        public SystemInfo(MapinfoSession MISession)
        {
            this.misession = MISession;
        }

        /// <summary>
        /// Returns a <see cref="IntPtr"/> for the current Mapinfo application frame.
        /// </summary>
        public IntPtr MapinfoFrameHandle
        {
            get 
            {
                int temphandle = Convert.ToInt32(RunSystemInfoCommand(SystemInfoEnum.SYS_INFO_MAPINFOWND));
                IntPtr handle = new IntPtr(temphandle);
                return handle;
            }
        }

        /// <summary>
        /// Returns a <see cref="IntPtr"/> for the current Mapinfo Mdi client window.
        /// </summary>
        public IntPtr MdiHandle
        {
            get
            {
                int temphandle = Convert.ToInt32(RunSystemInfoCommand(SystemInfoEnum.SYS_INFO_MDICLIENTWND));
                IntPtr handle = new IntPtr(temphandle);
                return handle;
            }
        }

        public object RunSystemInfoCommand(SystemInfoEnum systemInfoEnum)
        {
            string command = "SystemInfo({0})".FormatWith((int)systemInfoEnum);
            return misession.Eval(command);
        }
    }
}
