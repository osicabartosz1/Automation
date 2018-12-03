using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;//add
using System.Text;
using System.Threading.Tasks;//add

namespace ConsoleApplication1
{
    class Program
    {

        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public static String GetWindowClass(IntPtr hWnd)
        {
            const int size = 255;
            StringBuilder buffer = new StringBuilder(size + 1);
            GetClassName(hWnd, buffer, size);/// Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return buffer.ToString();
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent){
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");

            list.Add(handle);
            return true;
        }

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate(IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        } 


        static void Main(string[] args)
        {
            long count=0;
            while(true){
                count++;
                if(count>=1000000){break;}

                IEnumerable<IntPtr> windows = FindWindowsWithText("XYZ");

                foreach(IntPtr window in windows){
                    Console.WriteLine(window);

                    Console.WriteLine(GetWindowClass(window));
                }

                IntPtr a= new IntPtr(0);
                a = FindWindow("#32770", "XYZ");
                Console.WriteLine("###");
                StringBuilder ClassName = new StringBuilder(256);// Pre-allocate 256 characters, since this is the maximum class name length.
                int nRet;
                IntPtr child= new IntPtr(0);
                if (a.ToInt32() != 0)
                {
                    foreach (IntPtr c in GetChildWindows(a))
                    {

                        //Get the window class name
                        nRet = GetClassName(c, ClassName, ClassName.Capacity);
                        if ("Button" == ClassName.ToString())
                        {
                            child = c;

                            break;
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
                int wMsg = 245;  //BM_CLICK => https://wiki.winehq.org/List_Of_Windows_Messages
                IntPtr wParam = new IntPtr(0);
                IntPtr lParam = new IntPtr(0);

                int b = SendMessage(child, wMsg, wParam, lParam);
                b = SendMessage(child, wMsg, wParam, lParam);
            }
        }
    }
}
