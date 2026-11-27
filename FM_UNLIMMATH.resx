using IDLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TRANSLIT
{
    /// <summary>
    /// Плагин транслитерации: преобразует символы исходного алфавита
    /// (например, кириллицу) в один из поддерживаемых стандартов,
    /// загружаемых из translit.csv.
    ///
    /// Файл translit.csv имеет формат:
    ///   заголовок;стандарт_1;стандарт_2;...;стандарт_N
    ///   символ;вариант_1;вариант_2;...;вариант_N
    /// Первая колонка — исходный символ, остальные — его варианты
    /// для каждого стандарта. Каждый стандарт становится отдельным
    /// пунктом меню плагина.
    /// </summary>
    public class Plugin : I_Plugin
    {
        private const string P_Name_Value     = "TRANSLIT";
        private const string P_Discript_Value = "Транслитерация";
        private const string P_Version_Value  = "1.1.0";
        private const string P_Type_Value     = "text";

        // Базовая команда «Открыть окно» — дополнительные команды
        // (по одной на каждый стандарт из CSV) добавляются в P_Ini.
        private string[] _P_Integr =
        {
            "TRANSLIT|Show|Открыть транслитерацию||"
        };

        // translit_dat[i, 0] — исходный символ,
        // translit_dat[i, j>0] — его j-й вариант транслитерации.
        private string[,] _translitDat;

        // Быстрый поиск индекса строки по символу: символ → индекс строки.
        private Dictionary<string, int> _charIndex;

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
            switch (action)
            {
                case "Show":
                    new FM_TRANSLIT(this).Show();
                    return "";

                case "Send":
                    if (pp.Length < 6)
                        return "";
                    // Параметр — это индекс колонки стандарта, разделённый
                    // с подписью меню символом «;»: «индекс;подпись».
                    string[] tempSp = pp[4].Split(';');
                    if (tempSp.Length == 0)
                        return "";
                    if (!int.TryParse(tempSp[0], out int colIndex))
                        return "";
                    return Transliterate(pp[5], colIndex);

                default:
                    return "";
            }
        }

        /// <summary>
        /// Применяет транслитерацию: каждый символ входного текста,
        /// найденный в таблице, заменяется на соответствующий вариант
        /// столбца colIndex. Незнакомые символы переносятся как есть.
        /// </summary>
        private string Transliterate(string text, int colIndex)
        {
            if (_translitDat == null || _charIndex == null)
                return text;

            // Используем StringBuilder для производительности — на больших
            // текстах конкатенация строк даёт квадратичную сложность.
            var sb = new System.Text.StringBuilder(text.Length);
            foreach (char ch in text)
            {
                string key = ch.ToString();
                if (_charIndex.TryGetValue(key, out int row))
                {
                    string val = _translitDat[row, colIndex];
                    if (!string.IsNullOrEmpty(val))
                    {
                        sb.Append(val);
                        continue;
                    }
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Загружает translit.csv из каталога плагина, строит таблицу
        /// вариантов и регистрирует по одному пункту меню на каждый
        /// стандарт транслитерации.
        /// </summary>
        public void P_Ini()
        {
            string csvPath = Path.Combine(
                Application.ExecutablePath, "..", "PLUGINS", "TRANSLIT", "translit.csv");
            csvPath = Path.GetFullPath(csvPath);
            if (!File.Exists(csvPath))
                return;

            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length == 0)
                return;

            // Заголовок: имя;стандарт_1;стандарт_2;...
            string[] header = lines[0].Split(';');

            _translitDat = new string[lines.Length, header.Length];
            _charIndex = new Dictionary<string, int>(lines.Length);

            var integr = new List<string>(_P_Integr);
            for (int k = 1; k < header.Length; k++)
            {
                integr.Add("TRANSLIT|Send|" + header[k] + "|" + k + "|");
            }
            _P_Integr = integr.ToArray();

            // Заполняем таблицу и индекс символов.
            for (int i = 0; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(';');
                for (int j = 0; j < cells.Length && j < header.Length; j++)
                    _translitDat[i, j] = cells[j];

                if (cells.Length > 0)
                {
                    string key = cells[0];
                    if (!string.IsNullOrEmpty(key) && !_charIndex.ContainsKey(key))
                        _charIndex[key] = i;
                }
            }
        }
    }
}
