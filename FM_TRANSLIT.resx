using IDLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SHELL
{
    /// <summary>
    /// Главная форма-хост приложения NAOMI.
    /// Содержит два текстовых поля: верхнее — ввод, нижнее — вывод,
    /// и меню «Плагины», автоматически формируемое из найденных DLL.
    /// </summary>
    public partial class FM_SHELL : Form, I_Plugin_Hostform
    {
        private List<I_Plugin> _plugins;
        private readonly Dictionary<string, I_Plugin> _pluginsByName =
            new Dictionary<string, I_Plugin>(StringComparer.Ordinal);

        public FM_SHELL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Регистрирует плагин в хосте. Дубликаты по имени отбрасываются.
        /// </summary>
        public bool Register(I_Plugin plug)
        {
            if (plug == null || string.IsNullOrEmpty(plug.P_Name))
                return false;
            if (_pluginsByName.ContainsKey(plug.P_Name))
                return false;
            _pluginsByName.Add(plug.P_Name, plug);
            return true;
        }

        private void FM_SHELL_Load(object sender, EventArgs e)
        {
            this.Text = "NAOMI — хост плагинов";
            LoadPlugins();
            BuildPluginMenu();
        }

        /// <summary>
        /// Сканирует каталог PLUGINS\ и загружает каждую DLL, реализующую I_Plugin.
        /// Идентифицирует класс плагина по соглашению: namespace совпадает с именем файла,
        /// класс называется «Plugin». Файл IDLL.dll (контракт) пропускается.
        /// </summary>
        private void LoadPlugins()
        {
            _plugins = new List<I_Plugin>();

            string pluginsRoot = Path.Combine(Application.StartupPath, "PLUGINS");
            if (!Directory.Exists(pluginsRoot))
                return;

            string[] pluginDirs = Directory.GetDirectories(pluginsRoot);
            foreach (string dir in pluginDirs)
            {
                foreach (string dllPath in Directory.GetFiles(dir, "*.dll"))
                {
                    if (Path.GetFileName(dllPath).Equals("IDLL.dll", StringComparison.OrdinalIgnoreCase))
                        continue;
                    TryLoadPlugin(dllPath);
                }
            }
        }

        /// <summary>
        /// Загружает одну DLL плагина, создаёт экземпляр класса Plugin,
        /// инициализирует его и добавляет в общий список.
        /// </summary>
        private void TryLoadPlugin(string dllPath)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(dllPath);
                string typeName = Path.GetFileNameWithoutExtension(dllPath) + ".Plugin";
                Type pluginType = asm.GetType(typeName);
                if (pluginType == null)
                    return;
                if (!typeof(I_Plugin).IsAssignableFrom(pluginType))
                    return;

                I_Plugin plugin = (I_Plugin)Activator.CreateInstance(pluginType);
                plugin.Host = this;
                plugin.P_Ini();
                _plugins.Add(plugin);
            }
            catch (BadImageFormatException)
            {
                // Нативная DLL или не .NET-сборка — просто пропускаем.
            }
            catch (ReflectionTypeLoadException)
            {
                // Не удалось загрузить типы зависимостей — пропускаем плагин.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[NAOMI] Не удалось загрузить плагин '{0}': {1}", dllPath, ex.Message);
            }
        }

        /// <summary>
        /// Строит меню «Плагины» на основе массивов P_Integr загруженных плагинов.
        /// Tag каждого пункта содержит «index|описатель_команды» — этого достаточно,
        /// чтобы потом найти плагин по индексу и передать ему корректный proc_param.
        /// </summary>
        private void BuildPluginMenu()
        {
            плагиныToolStripMenuItem.DropDownItems.Clear();

            for (int i = 0; i < _plugins.Count; i++)
            {
                I_Plugin plugin = _plugins[i];
                ToolStripMenuItem rootItem = new ToolStripMenuItem
                {
                    Name = plugin.P_Name + "_mi",
                    Text = plugin.P_Name
                };

                foreach (string integr in plugin.P_Integr)
                {
                    string[] parts = integr.Split('|');
                    if (parts.Length < 3)
                        continue;

                    ToolStripMenuItem subItem = new ToolStripMenuItem
                    {
                        Name = parts[1] + "_smi",
                        Text = parts[2],
                        Tag = i + "|" + integr
                    };
                    subItem.Click += plug_proc;
                    rootItem.DropDownItems.Add(subItem);
                }

                плагиныToolStripMenuItem.DropDownItems.Add(rootItem);
            }
        }

        /// <summary>
        /// Обработчик клика по пункту меню плагина.
        /// Склеивает Tag пункта с текстом верхнего RichTextBox
        /// и передаёт результат в P_Proc соответствующего плагина.
        /// </summary>
        private void plug_proc(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null || item.Tag == null)
                return;

            string tag = item.Tag.ToString();
            string procParam = tag + richTextBox1.Text;

            string[] parts = procParam.Split('|');
            if (parts.Length == 0)
                return;
            if (!int.TryParse(parts[0], out int plugIndex))
                return;
            if (plugIndex < 0 || plugIndex >= _plugins.Count)
                return;

            try
            {
                richTextBox2.Text = _plugins[plugIndex].P_Proc(procParam);
            }
            catch (Exception ex)
            {
                richTextBox2.Text = "Ошибка плагина: " + ex.Message;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
