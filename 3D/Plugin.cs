using IDLL;
using System.Windows.Forms;

namespace _3D
{
    /// <summary>
    /// Плагин «3D-визуализация»: составной правильный тетраэдр из 4 одинаковых
    /// треугольных пирамид с логотипом Cyberdyne на гранях.
    ///
    /// Поддерживает две команды:
    ///   • Show      — открывает обычное окно с GLControl.
    ///   • Desktop   — открывает fullscreen-оверлей прямо поверх рабочего
    ///                 стола (прозрачный фон, опциональный click-through).
    /// </summary>
    public class Plugin : I_Plugin
    {
        private const string P_Name_Value     = "3D";
        private const string P_Discript_Value = "3D-визуализация: составной тетраэдр с логотипом Cyberdyne";
        private const string P_Version_Value  = "1.0.0";
        private const string P_Type_Value     = "image";

        private string[] _P_Integr =
        {
            "3D|Show|Показать в окне||",
            "3D|Desktop|Показать на рабочем столе||"
        };

        private I_Plugin_Hostform _Host;

        public string P_Name     { get { return P_Name_Value; } }
        public string P_Discript { get { return P_Discript_Value; } }
        public string P_Version  { get { return P_Version_Value; } }
        public string P_Type     { get { return P_Type_Value; } }

        public string[] P_Integr
        {
            get { return _P_Integr; }
            set { _P_Integr = value; }
        }

        public I_Plugin_Hostform Host
        {
            get { return _Host; }
            set
            {
                _Host = value;
                if (_Host != null) _Host.Register(this);
            }
        }

        public string P_Proc(string proc_param)
        {
            string[] pp = proc_param.Split(new char[] { '|' }, 6);
            if (pp.Length < 3) return "";

            switch (pp[2])
            {
                case "Show":
                    Form existing = Application.OpenForms["FM_3D"];
                    if (existing != null)
                    {
                        existing.Activate();
                    }
                    else
                    {
                        new FM_3D().Show();
                    }
                    return "";

                case "Desktop":
                    Form deskExisting = Application.OpenForms["DesktopOverlay"];
                    if (deskExisting != null)
                    {
                        deskExisting.Close();
                    }
                    else
                    {
                        new DesktopOverlay().Show();
                    }
                    return "";

                default:
                    return "";
            }
        }

        public void P_Ini()
        {
        }
    }
}
