﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MapInfo.Wrapper.Core
{
    /// <summary>
    /// A base callback class which can be used with Mapinfo,
    /// provides StatusText changed event and WindowContentsChanged event.
    /// 
    /// <para>If you need to implement you own callback event, inherit from this class and implement any events you need.
    /// You can also override the defult implementation of the base events if need be. </para>
    /// </summary>
    [ComVisible(true)]
    public class MapinfoCallback
    {
        public event Action<string> OnStatusChanged;

        public virtual void SetStatusText(string text)
        {
            Action<string> statuschanged = OnStatusChanged;
            if (statuschanged != null)
            {
                statuschanged(text);
            }
        }

        public event Action<int> OnWindowChanged;

        public virtual int WindowContentsChanged(int windowId)
        {
            Action<int> windowchanged = OnWindowChanged;
            if (windowchanged != null)
            {
                windowchanged(windowId);
            }
            return 1;
        }
    }
}
