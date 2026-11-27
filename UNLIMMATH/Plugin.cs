using IDLL;
using System;
using System.Windows.Forms;

namespace UNLIMMATH
{
    /// <summary>
    /// Плагин «Безлимитная математика»: инженерный калькулятор
    /// с произвольной точностью (числа как строки, табличная арифметика).
    ///
    /// Поддерживает две команды, регистрируемые в меню хоста:
    ///   • Show — открывает отдельную форму калькулятора;
    ///   • Calc — вычисляет выражение, взятое из верхнего RichTextBox
    ///            хоста, и возвращает результат в нижний RichTextBox.
    /// </summary>
    public class Plugin : I_Plugin
    {
        private const string P_Name_Value       = "UNLIMMATH";
        private const string P_Discript_Value   = "Безлимитная математика (инженерный калькулятор)";
        private const string P_Version_Value    = "3.0.0";
        private const string P_Type_Value       = "text";

        // Формат строки: «Имя|Действие|Подпись_меню|Параметр|»
        private string[] _P_Integr =
        {
            "UNLIMMATH|Show|Открыть калькулятор||",
            "UNLIMMATH|Calc|Вычислить выражение||"
        };

        private I_Plugin_Hostform _Host;

        public string P_Name       { get { return P_Name_Value; } }
        public string P_Discript   { get { return P_Discript_Value; } }
        public string P_Version    { get { return P_Version_Value; } }
        public string P_Type       { get { return P_Type_Value; } }

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

        /// <summary>
        /// Диспетчер команд. Формат proc_param:
        /// «index|Имя|Действие|Подпись|Параметр|Входной_текст».
        /// </summary>
        public string P_Proc(string proc_param)
        {
            string[] pp = proc_param.Split(new char[] { '|' }, 6);
            if (pp.Length < 3)
                return "";

            string action = pp[2];
            string input  = pp.Length > 5 ? pp[5] : "";

            switch (action)
            {
                case "Show":
                    // Открываем немодальную форму калькулятора.
                    Form existing = Application.OpenForms["FM_UNLIMMATH"];
                    if (existing != null)
                    {
                        existing.Activate();
                    }
                    else
                    {
                        new FM_UNLIMMATH(this).Show();
                    }
                    return "";

                case "Calc":
                    // Вычисляем выражение из верхнего RichTextBox хоста
                    // и возвращаем результат в нижний RichTextBox.
                    return CalcEngine.Evaluate(input);

                default:
                    return "";
            }
        }

        /// <summary>
        /// Инициализация плагина.
        /// </summary>
        public void P_Ini()
        {
        }
    }
}
