using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ShareX
{
    public class WindowsRectangleList
    {
        public IntPtr IgnoreHandle { get; set; }
        public bool IncludeChildWindows { get; set; }

        private List<SimpleWindowInfo> windows;
        private HashSet<IntPtr> parentHandles;

        public List<SimpleWindowInfo> GetWindowInfoListAsync(int timeout)
        {
            List<SimpleWindowInfo> windowInfoList = null;

            var t = new Thread(() =>
            {
                try
                {
                    windowInfoList = GetWindowInfoList();
                }
                catch
                {
                    // ignored
                }
            });

            t.Start();

            if (!t.Join(timeout))
            {
                t.Abort();
            }

            return windowInfoList;
        }

        public List<SimpleWindowInfo> GetWindowInfoList()
        {
            windows = new List<SimpleWindowInfo>();
            parentHandles = new HashSet<IntPtr>();

            EnumWindowsProc ewp = EvalWindow;
            NativeMethods.EnumWindows(ewp, IntPtr.Zero);

            var result = new List<SimpleWindowInfo>();

            foreach (var window in windows)
            {
                var rectVisible = true;

                if (!window.IsWindow)
                {
                    if (result.Any(window2 => window2.Rectangle.Contains(window.Rectangle)))
                    {
                        rectVisible = false;
                    }
                }
                if (rectVisible)
                {
                    result.Add(window);
                }
            }
            return result;
        }

        private bool EvalWindow(IntPtr hWnd, IntPtr lParam)
        {
            return CheckHandle(hWnd, true);
        }

        private bool EvalControl(IntPtr hWnd, IntPtr lParam)
        {
            return CheckHandle(hWnd, false);
        }

        private bool CheckHandle(IntPtr handle, bool isWindow)
        {
            if (handle == IgnoreHandle || !NativeMethods.IsWindowVisible(handle) || (isWindow && NativeMethods.IsWindowCloaked(handle)))
            {
                return true;
            }

            var windowInfo = new SimpleWindowInfo(handle);

            if (isWindow)
            {
                windowInfo.IsWindow = true;
                windowInfo.Rectangle = CaptureHelpers.GetWindowRectangle(handle);
            }
            else
            {
                windowInfo.Rectangle = NativeMethods.GetWindowRect(handle);
            }

            if (!windowInfo.Rectangle.IsValid())
            {
                return true;
            }

            if (IncludeChildWindows && !parentHandles.Contains(handle))
            {
                parentHandles.Add(handle);

                EnumWindowsProc ewp = EvalControl;
                NativeMethods.EnumChildWindows(handle, ewp, IntPtr.Zero);
            }

            if (isWindow)
            {
                var clientRect = NativeMethods.GetClientRect(handle);

                if (clientRect.IsValid() && clientRect != windowInfo.Rectangle)
                {
                    windows.Add(new SimpleWindowInfo(handle, clientRect));
                }
            }
            windows.Add(windowInfo);
            return true;
        }
    }
}