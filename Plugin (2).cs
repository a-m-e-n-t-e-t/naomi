using System;
using System.Windows.Forms;

namespace TRANSLIT
{
    /// <summary>
    /// Окно транслитерации: поле ввода, выбор стандарта из выпадающего
    /// списка, кнопка «Транслит» и поле результата.
    /// </summary>
    public partial class FM_TRANSLIT : Form
    {
        private readonly Plugin _plug;

        public FM_TRANSLIT(Plugin plug)
        {
            InitializeComponent();
            _plug = plug;
        }

        private void FM_TRANSLIT_Load(object sender, EventArgs e)
        {
            this.Text = "Транслитерация";
            btnTranslit.Text = "Транслит";

            // Нулевой пункт P_Integr — это команда «Открыть окно», её в список
            // стандартов не выводим. Остальные элементы — стандарты из CSV.
            for (int i = 1; i < _plug.P_Integr.Length; i++)
            {
                string[] parts = _plug.P_Integr[i].Split('|');
                if (parts.Length >= 4)
                    comboBox1.Items.Add(parts[2]);
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void btnTranslit_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
                return;

            // Индекс стандарта в P_Integr = (SelectedIndex в комбобоксе) + 1,
            // т. к. нулевой элемент P_Integr — это команда «Show».
            int standardIndex = comboBox1.SelectedIndex + 1;
            richTextBox2.Text = _plug.P_Proc(
                "-1|TRANSLIT|Send|" + comboBox1.SelectedItem + "|" + standardIndex + "|" + richTextBox1.Text);
        }
    }
}
