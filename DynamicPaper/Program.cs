namespace Maxstupo.DynamicPaper {

    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Forms;

    public static class Program {
        public const string PipeName = "DynamicPaper_IO";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            // Get the application GUID defined in AssemblyInfo.cs
            string guid = ((GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // id for global mutex (global to machine)
            string mutexId = string.Format("Global\\{{{0}}}", guid);

            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            using (Mutex mutex = new Mutex(false, mutexId, out _, securitySettings)) {
                bool hasHandle = false;
                try {
                    try {
                        hasHandle = mutex.WaitOne(250, false); // Timeout after 250ms.
                        if (!hasHandle) { // Application is already running.
                            //  MessageBox.Show("The application is already running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            SendMessage("bringtofront");

                            Environment.Exit(0);
                            return;
                        }
                    } catch (AbandonedMutexException) {
                        hasHandle = true; // The mutex was abandoned in another process, it will still get acquired.
                    }

                    // Start application normally.
                    Application.Run(new FormMain());
                } finally {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }

        }

        // Connect to the currently running DynamicPaper process and notify it to "bring-to-front"
        private static void SendMessage(string msg) {
            try {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.Identification)) {
                    pipeClient.Connect(1000);
                    using (StreamWriter sw = new StreamWriter(pipeClient, new UTF8Encoding(false))) {

                        sw.WriteLine(msg);
                        sw.Flush();
                        pipeClient.WaitForPipeDrain();
                    }
                }
            } catch (Exception) {
                // Catch exception - Not really important, if it fails nothing happens....
            }
        }

    }

}