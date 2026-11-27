using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;

namespace UNLIMMATH
{
    /// <summary>
    /// Форма-калькулятор: поле ввода выражения, кнопка «Вычислить»,
    /// поле результата, история вычислений (сохраняется в JSON между
    /// запусками), панель быстрых кнопок, настройка точности и
    /// справка по поддерживаемым функциям.
    /// </summary>
    public partial class FM_UNLIMMATH : Form
    {
        private readonly Plugin _plug;
        private bool _darkTheme = false;

        public FM_UNLIMMATH(Plugin plug)
        {
            InitializeComponent();
            _plug = plug;
        }

        private void FM_UNLIMMATH_Load(object sender, EventArgs e)
        {
            this.Text = "Инженерный калькулятор — Безлимитная математика";

            // Загружаем сохранённую историю
            LoadHistory();

            // Применяем светлую тему
            ApplyTheme();

            // Фокус на поле ввода
            txtInput.Focus();

            // Заполняем справку
            lblHelp.Text =
                "  Поддерживаемые операции и функции\r\n" +
                "  ----------------------------------------\r\n" +
                "  + - * / ^ mod        Арифметика и модуль\r\n" +
                "  ( ... )              Скобки, вложенность любая\r\n" +
                "  pi, e, phi, tau      Константы π, e, φ, 2π\r\n" +
                "  ln2, ln10            Логарифмы 2 и 10\r\n" +
                "  c, g, h, na, r, k    Физические константы (SI)\r\n" +
                "  \r\n" +
                "  sqrt(x)              Квадратный корень\r\n" +
                "  root(x, n)           Корень n-й степени\r\n" +
                "  pow(x, y)            Возведение в степень\r\n" +
                "  exp(x)               Экспонента e^x\r\n" +
                "  ln(x)                Натуральный логарифм\r\n" +
                "  log(x)               log10 (десятичный)\r\n" +
                "  log(x, b)            Логарифм по основанию b\r\n" +
                "  log2(x)              Двоичный логарифм\r\n" +
                "  \r\n" +
                "  sin cos tan          Тригонометрия (рад)\r\n" +
                "  asin acos atan       Обратная тригонометрия\r\n" +
                "  atan2(y, x)          Угол точки (x, y)\r\n" +
                "  sinh cosh tanh       Гиперболические\r\n" +
                "  \r\n" +
                "  floor(x)             Округление вниз\r\n" +
                "  ceil(x)              Округление вверх\r\n" +
                "  trunc(x)             Отбрасывание дроби\r\n" +
                "  round(x, n)          Округление до n знаков\r\n" +
                "  abs(x) sign(x)       Модуль и знак\r\n" +
                "  min(a,b) max(a,b)    Минимум / максимум\r\n" +
                "  hypot(a, b)          sqrt(a² + b²)\r\n" +
                "  \r\n" +
                "  fact(n)              Факториал (n!)\r\n" +
                "  ncr(n,k) npr(n,k)    Сочетания / размещения\r\n" +
                "  combrep(n,k)         Сочетания с повторениями\r\n" +
                "  permrep(n,k)         Размещения с повторениями\r\n" +
                "  \r\n" +
                "  isprime(n)           Проверка простоты\r\n" +
                "  prime(k)             k-е простое число\r\n" +
                "  factorize(n)         Разложение на множители\r\n" +
                "  fib(n)               Числа Фибоначчи\r\n" +
                "  euler(n)             Функция Эйлера φ(n)\r\n" +
                "  sigma(n)             Сумма делителей σ(n)\r\n" +
                "  gcd(a, b)            НОД (алгоритм Евклида)\r\n" +
                "  \r\n" +
                "  mean(...)            Среднее (много аргументов)\r\n" +
                "  median(...)          Медиана\r\n" +
                "  stddev(...)          Стандартное отклонение\r\n" +
                "  \r\n" +
                "  integrate(f, a, b)   ∫ f(x)dx методом Симпсона\r\n" +
                "  solve(f, x0)         Решение f(x)=0 методом Ньютона\r\n" +
                "  \r\n" +
                "  Переменные:\r\n" +
                "    x = 5              Присваивание\r\n" +
                "    y = x + 3          Использование в выражениях\r\n" +
                "  \r\n" +
                "  Примеры:\r\n" +
                "    sin(pi/6) + cos(0) - 2^3 + sqrt(16)\r\n" +
                "    fact(20) / (ncr(20, 5) * 120)\r\n" +
                "    log(1000) + ln(e^2)\r\n" +
                "    2^3^2     (правоассоциативно = 2^9 = 512)\r\n" +
                "    integrate(x^2, 0, 1)        → 1/3\r\n" +
                "    solve(x^2 - 2, 1)           → √2\r\n" +
                "    factorize(360)              → 2^3 * 3^2 * 5\r\n" +
                "    fib(100)                    → 354224848179261915075\r\n" +
                "  \r\n" +
                "  Также доступен плагин 3D:\r\n" +
                "    Плагины → 3D → Показать в окне\r\n" +
                "    Плагины → 3D → Показать на рабочем столе\r\n" +
                "    (составной тетраэдр с логотипом Cyberdyne)\r\n" +
                "  \r\n" +
                "  Точность: 20 знаков после запятой (настраивается).\r\n" +
                "  Разделитель: точка.";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            EvaluateCurrent();
        }

        /// <summary>
        /// Вычисляет выражение из txtInput и обновляет txtOutput и lstHistory.
        /// </summary>
        private void EvaluateCurrent()
        {
            string expr = txtInput.Text.Trim();
            if (expr.Length == 0)
            {
                txtOutput.Text = "";
                return;
            }

            string result = CalcEngine.Evaluate(expr);

            txtOutput.Text = result;

            // История ограничена 100 последними вычислениями.
            if (!result.StartsWith("ERR"))
            {
                string entry = expr + "  =  " + result;
                lstHistory.Items.Insert(0, entry);
                while (lstHistory.Items.Count > 100)
                    lstHistory.Items.RemoveAt(lstHistory.Items.Count - 1);
                SaveHistory();
            }

            // Подсветка ошибок.
            txtOutput.ForeColor = result.StartsWith("ERR")
                ? Color.Firebrick
                : (_darkTheme ? Color.LightGreen : Color.Green);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
            txtInput.Focus();
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
            SaveHistory();
        }

        /// <summary>
        /// Двойной клик по истории возвращает выражение в поле ввода.
        /// </summary>
        private void lstHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lstHistory.SelectedItem == null) return;
            string entry = lstHistory.SelectedItem.ToString();
            int eq = entry.IndexOf("  =  ");
            if (eq > 0)
            {
                txtInput.Text = entry.Substring(0, eq);
                txtInput.Focus();
            }
        }

        /// <summary>
        /// Enter в поле ввода запускает вычисление.
        /// </summary>
        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                EvaluateCurrent();
            }
        }

        // -----------------------------------------------------------------
        // ПАНЕЛЬ БЫСТРЫХ КНОПОК
        // Клик вставляет текст в текущую позицию курсора поля ввода.
        // -----------------------------------------------------------------
        private void QuickButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            string insert = btn.Tag as string;
            if (string.IsNullOrEmpty(insert)) return;

            int selStart = txtInput.SelectionStart;
            txtInput.Text = txtInput.Text.Insert(selStart, insert);
            txtInput.SelectionStart = selStart + insert.Length;
            txtInput.Focus();
        }

        // -----------------------------------------------------------------
        // НАСТРОЙКА ТОЧНОСТИ
        // -----------------------------------------------------------------
        private void nudPrecision_ValueChanged(object sender, EventArgs e)
        {
            CalcEngine.PREC = (int)nudPrecision.Value;
        }

        // -----------------------------------------------------------------
        // СБРОС ПЕРЕМЕННЫХ
        // -----------------------------------------------------------------
        private void btnResetVars_Click(object sender, EventArgs e)
        {
            CalcEngine.ResetVariables();
            txtOutput.Text = "Пользовательские переменные сброшены";
            txtOutput.ForeColor = _darkTheme ? Color.LightBlue : Color.Blue;
        }

        // -----------------------------------------------------------------
        // ТЁМНАЯ ТЕМА
        // -----------------------------------------------------------------
        private void chkDarkTheme_CheckedChanged(object sender, EventArgs e)
        {
            _darkTheme = chkDarkTheme.Checked;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            Color bg, fg, panelBg, inputBg;
            if (_darkTheme)
            {
                bg = Color.FromArgb(32, 32, 32);
                fg = Color.FromArgb(220, 220, 220);
                panelBg = Color.FromArgb(45, 45, 45);
                inputBg = Color.FromArgb(50, 50, 50);
            }
            else
            {
                bg = SystemColors.Control;
                fg = SystemColors.ControlText;
                panelBg = SystemColors.ControlLight;
                inputBg = SystemColors.Window;
            }
            this.BackColor = bg;
            this.ForeColor = fg;
            panelTop.BackColor = panelBg;
            panelCenter.BackColor = panelBg;
            panelBottom.BackColor = panelBg;
            panelQuick.BackColor = panelBg;
            txtInput.BackColor = inputBg;
            txtInput.ForeColor = fg;
            txtOutput.BackColor = inputBg;
            txtOutput.ForeColor = fg;
            lstHistory.BackColor = inputBg;
            lstHistory.ForeColor = fg;
            lblHelp.BackColor = inputBg;
            lblHelp.ForeColor = fg;
        }

        // -----------------------------------------------------------------
        // СОХРАНЕНИЕ / ЗАГРУЗКА ИСТОРИИ В JSON
        // -----------------------------------------------------------------
        private string HistoryFilePath
        {
            get
            {
                string dir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "PLUGINS", "UNLIMMATH");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "history.json");
            }
        }

        private void SaveHistory()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < lstHistory.Items.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append("\"");
                    sb.Append(lstHistory.Items[i].ToString()
                        .Replace("\\", "\\\\")
                        .Replace("\"", "\\\""));
                    sb.Append("\"");
                }
                sb.Append("]");
                File.WriteAllText(HistoryFilePath, sb.ToString());
            }
            catch
            {
                // Игнорируем ошибки записи истории — это не критично.
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (!File.Exists(HistoryFilePath)) return;
                string json = File.ReadAllText(HistoryFilePath);
                // Простой парсер JSON-массива строк (без внешних зависимостей).
                if (json.StartsWith("[") && json.EndsWith("]"))
                {
                    json = json.Substring(1, json.Length - 2).Trim();
                    if (json.Length == 0) return;
                    var items = new List<string>();
                    int i = 0;
                    while (i < json.Length)
                    {
                        while (i < json.Length && json[i] != '"') i++;
                        if (i >= json.Length) break;
                        i++; // пропускаем открывающую кавычку
                        var sb = new StringBuilder();
                        while (i < json.Length)
                        {
                            char ch = json[i];
                            if (ch == '\\' && i + 1 < json.Length)
                            {
                                char next = json[i + 1];
                                sb.Append(next);
                                i += 2;
                            }
                            else if (ch == '"')
                            {
                                i++;
                                break;
                            }
                            else
                            {
                                sb.Append(ch);
                                i++;
                            }
                        }
                        items.Add(sb.ToString());
                        while (i < json.Length && json[i] != ',') i++;
                        if (i < json.Length) i++;
                    }
                    lstHistory.Items.AddRange(items.ToArray());
                }
            }
            catch
            {
                // Игнорируем ошибки чтения истории.
            }
        }
    }
}
