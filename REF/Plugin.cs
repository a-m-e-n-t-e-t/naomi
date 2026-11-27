using IDLL;
using System.Windows.Forms;

namespace REF
{
    /// <summary>
    /// Плагин-справочник: математика, физика, химия, биология.
    /// Каждая команда открывает универсальную форму с деревом
    /// разделов и текстом выбранного предмета.
    /// </summary>
    public class Plugin : I_Plugin
    {
        private const string P_Name_Value     = "СПРАВОЧНИК";
        private const string P_Discript_Value = "Справочник по математике, физике, химии, биологии";
        private const string P_Version_Value  = "1.0.0";
        private const string P_Type_Value     = "text";

        private string[] _P_Integr =
        {
            "REF|ShowMath|Математика||",
            "REF|ShowPhys|Физика||",
            "REF|ShowChem|Химия||",
            "REF|ShowBio|Биология||"
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
                case "ShowMath":
                    ShowSubject("math");
                    return "";
                case "ShowPhys":
                    ShowSubject("phys");
                    return "";
                case "ShowChem":
                    ShowSubject("chem");
                    return "";
                case "ShowBio":
                    ShowSubject("bio");
                    return "";
                default:
                    return "";
            }
        }

        private static void ShowSubject(string subject)
        {
            string formName = "FM_REF_" + subject;
            Form existing = Application.OpenForms[formName];
            if (existing != null)
            {
                existing.Activate();
            }
            else
            {
                var frm = new FM_REF(subject) { Name = formName };
                frm.Show();
            }
        }

        public void P_Ini()
        {
        }
    }
}
