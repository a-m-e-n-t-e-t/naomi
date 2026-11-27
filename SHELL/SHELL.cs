using System;
using System.Windows.Forms;

namespace SHELL
{
    internal static class SHELL
    {
        /// <summary>
        /// Главная точка входа для приложения NAOMI.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FM_SHELL());
        }
    }
}
