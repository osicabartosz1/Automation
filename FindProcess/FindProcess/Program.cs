using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

/*
 using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
// More info about Window Classes at http://msdn.microsoft.com/en-us/library/ms633574(VS.85).aspx 

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        const uint WM_CLOSE = 0x10;

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        public Form1()
        {
            InitializeComponent();
        }

        // This event will silently kill any alert dialog box 
        private void button2_Click(object sender, EventArgs e)
        {
            string dialogBoxText = "Rename File"; // Windows would give you this alert when you try to set to files to the same name 
            IntPtr hwnd = FindWindow("#32770", dialogBoxText);
            SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

    }
}
 */
namespace FindProcess
{
    class Program
    {
        
        [DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

       // [DllImport("User32.dll")]
       // static extern uint PeekMessageA(out System.Windows.Forms.Message lpdwProcessId, IntPtr hWnd, uint a1, uint a2, uint a3);
        


        static Object GetObj(int i, String progID)
        {
            Object obj = null;

            Console.WriteLine("\n" + i + ") Object obj = GetActiveObject(\"" + progID + "\")");
            try
            { obj = Marshal.GetActiveObject(progID);
            
            }
            catch (Exception e)
            {
                Console.WriteLine("\n   Failure: obj did not get initialized\n" +
                              "   Exception = " + e.ToString().Substring(0, 43), 0);
            }

            if (obj != null)
            { Console.WriteLine("\n   Success: obj = " + obj.ToString(), 1); }
            return obj;
        }
        static void Main(string[] args)
        {

            IRunningObjectTable Rot = null;

            GetRunningObjectTable(0, out Rot);//pobranie obiektu Rot
            //Co my dostajemy w obiekcie pod nazwa ROT? => nadzieje na lepsze jutro            
            /*
             HRESULT GetRunningObjectTable(
                  DWORD                reserved,
                  LPRUNNINGOBJECTTABLE *pprot
                );
                
                reserved

                This parameter is reserved and must be 0.

                pprot

                The address of an IRunningObjectTable* pointer variable that receives the interface pointer to the local ROT.
                When the function is successful, the caller is responsible for calling Release on the interface pointer. 
                If an error occurs, *pprot is undefined.
             */

            if (Rot == null) return;
            IEnumMoniker monikerEnumerator = null;//inform. moniker (obiekt pełniący rolę identyfikatora)
            //Enumerates the components of a moniker or the monikers in a table of monikers.
            Rot.EnumRunning(out monikerEnumerator);//pobranie obiektu IEnumMoniker => dowiedzisc sie wiecej o nim....
            if (monikerEnumerator == null) return;
            monikerEnumerator.Reset();// Resets the enumeration sequence to the beginning

            List<object> instances = new List<object>();

            IntPtr pNumFetched = new IntPtr();//Handle - Typ specyficzny dla platformy, który jest używany do reprezentowania wskaźnika lub uchwytu.

            IMoniker[] monikers = new IMoniker[1];
            while (monikerEnumerator.Next(1, monikers, pNumFetched) == 0)//Retrieves the specified number of items in the enumeration sequence.
            {
                /*
                HRESULT Next(
                    ULONG    celt,
                    IMoniker **rgelt,
                    ULONG    *pceltFetched
                  );
                 celt -> The number of items to be retrieved.
                 rgelt -> An array of enumerated items.
                 pceltFetched -> This parameter can be NULL if celt is 1
              
                */
                IBindCtx bindCtx;//Provides access to a bind context, which is an object that stores information about a particular moniker binding operation.
                CreateBindCtx(0, out bindCtx);//po co?
                if (bindCtx == null) continue;// sprawdzenie warunkow pocztakowy nastepnej funkcji
                string displayName;//clsid 
                monikers[0].GetDisplayName(bindCtx, null, out displayName);
                //Console.WriteLine(displayName + "@#$" + pNumFetched);

                /*
                 HRESULT GetDisplayName(
                      IBindCtx *pbc,
                      IMoniker *pmkToLeft,
                      LPOLESTR *ppszDisplayName
                    );
                    Parameters
                    pbc

                    A pointer to the IBindCtx interface on the bind context to be used in this operation. The bind context caches objects bound during the binding process, contains parameters that apply to all operations using the bind context, and provides the means by which the moniker implementation should retrieve information about its environment.

                    pmkToLeft

                    If the moniker is part of a composite moniker, pointer to the moniker to the left of this moniker. This parameter is used primarily by moniker implementers to enable cooperation between the various components of a composite moniker. Moniker clients should pass NULL.

                    ppszDisplayName

                    The address of a pointer variable that receives a pointer to the display name string for the moniker. The implementation must use IMalloc::Alloc to allocate the string returned in ppszDisplayName, and the caller is responsible for calling IMalloc::Free to free it. Both the caller and the implementation of this method use the COM task allocator returned by CoGetMalloc. If an error occurs, the implementation must set *ppszDisplayName should be set to NULL.
                  */
                //object ComObject;
                //SYSTEMTIME stUTC;
                System.Runtime.InteropServices.ComTypes.FILETIME a = new System.Runtime.InteropServices.ComTypes.FILETIME();
                Rot.GetTimeOfLastChange(monikers[0],out a);
                //Rot.GetObject(monikers[0], out ComObject);
                //if (ComObject == null) continue;
                //instances.Add(ComObject);
               // break;//nie wiem czy po mojej przrerubce bedzie spelnial to samo zadanie
            }



            test();

        }



        [DllImport("ole32.dll")]
        static extern int CreateBindCtx(uint reserved,out IBindCtx ppbc);
        
        [DllImport("ole32.dll")]
        public static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
        // Requires Using System.Runtime.InteropServices.ComTypes
        // Get all running instance by querying ROT

        private List<object> GetRunningInstances(string[] progIds)
        {   List<string> clsIds = new List<string>();
            // get the app clsid

            foreach (string progId in progIds)
            {   Type type = Type.GetTypeFromProgID(progId);
                if (type != null) clsIds.Add(type.GUID.ToString().ToUpper());
            }
            // get Running Object Table ...

            IRunningObjectTable Rot = null;

            GetRunningObjectTable(0, out Rot);
            if (Rot == null) return null;
            // get enumerator for ROT entries

            IEnumMoniker monikerEnumerator = null;
            Rot.EnumRunning(out monikerEnumerator);
            if (monikerEnumerator == null) return null;
            monikerEnumerator.Reset();
            
            List<object> instances = new List<object>();
            
            IntPtr pNumFetched = new IntPtr();

            IMoniker[] monikers = new IMoniker[1];
            
            // go through all entries and identifies app instances

            while (monikerEnumerator.Next(1, monikers, pNumFetched) == 0)
            {
                IBindCtx bindCtx;
                CreateBindCtx(0, out bindCtx);
                if (bindCtx == null) continue;                
                string displayName;
                monikers[0].GetDisplayName(bindCtx, null, out displayName);                
                foreach (string clsId in clsIds)
                {
                    if (displayName.ToUpper().IndexOf(clsId) > 0)
                    {   object ComObject;
                        Rot.GetObject(monikers[0], out ComObject);
                        if (ComObject == null) continue;
                        instances.Add(ComObject);
                        break;
                    }
                }
            }
            return instances;
        }
        void TestROT()
        {  // Look for acad 2009 & 2010 & 2014

            string[] progIds =
                {
                    "AutoCAD.Application.17.2",
                    "AutoCAD.Application.18",
                    "AutoCAD.Application.19.1"
                };
            List<object> instances = GetRunningInstances(progIds);
            
            foreach (object acadObj in instances)
            {
                try
                {
                    // do some stuff ...  


                }

                catch
                {



                }

            }

        }


        public static void test()
        {
            //uint a=0;
            IntPtr b = new IntPtr(0);
            //a=Program.GetWindowThreadProcessId(b, a);
            string result = "";
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                result=process.MainWindowTitle;
                Console.WriteLine(result);

                    result = process.MainWindowTitle;
                    if (result == "Kalkulator")
                    {
                        Console.WriteLine(result);
                        Console.WriteLine(process.Handle);
                        Console.WriteLine(process.MainWindowHandle);
                        Console.WriteLine(process.Id);
                        Console.WriteLine(process.ProcessName);
                        Console.WriteLine(process.Threads);
                        Console.WriteLine(process.GetType());
                        Console.WriteLine(process.Handle);
                        Console.WriteLine(process.HandleCount);

                        foreach (ProcessThread ab in process.Threads)
                        {
                            Console.WriteLine(ab.Id);
                            try
                            {
                                //Console.WriteLine(ab.GetType());

                                String sProgId = Marshal.GenerateProgIdForType(ab.GetType());
                                Object obj = GetObj(1, sProgId);
                            }
                            catch (Exception e) { }
                        }
                        //process.Handle
                    }
                
            }
            System.Threading.Thread.Sleep(50000);
            //GetObj(1, "Word.Application");
        }
/*
        public void tt3() {
            HWND hwnd;
            BOOL fDone;
            MSG msg;

            // Begin the operation and continue until it is complete 
            // or until the user clicks the mouse or presses a key. 

            fDone = FALSE;
            while (!fDone)
            {
                fDone = DoLengthyOperation(); // application-defined function 

                // Remove any messages that may be in the queue. If the 
                // queue contains any mouse or keyboard 
                // messages, end the operation. 

                while (PeekMessage(&msg, hwnd, 0, 0, PM_REMOVE))
                {
                    switch (msg.message)
                    {
                        case WM_LBUTTONDOWN:
                        case WM_RBUTTONDOWN:
                        case WM_KEYDOWN:
                            // 
                            // Perform any required cleanup. 
                            // 
                            fDone = TRUE;
                    }
                }
            } 
        }
        */
    }
}
