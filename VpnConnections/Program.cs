using System;
using System.Windows.Forms;

namespace VpnConnections
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            if (!SingleInstance.Start())
            {
                SingleInstance.ShowFirstInstance();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new TrayApplicationContext());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Terminated Unexpectedly",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SingleInstance.Stop(); // all finished so release the mutex

        }
    }
}
