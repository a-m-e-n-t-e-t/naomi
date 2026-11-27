using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace REF
{
    /// <summary>
    /// Универсальная форма справочника: TreeView разделов слева,
    /// RichTextBox с текстом справа, поле поиска сверху.
    /// </summary>
    public partial class FM_REF : Form
    {
        private readonly string _subject;
        private readonly List<KeyValuePair<string, string>> _sections;

        public FM_REF(string subject)
        {
            InitializeComponent();
            _subject = subject;
            _sections = RefContent.Sections[subject];
        }

        private void FM_REF_Load(object sender, EventArgs e)
        {
            // Заголовок формы
            SubjectData data = RefContent.Subjects[_subject];
            this.Text = "Справочник — " + data.Name;

            lblTitle.Text = data.Description;

            // Заполняем дерево разделов
            treeSections.BeginUpdate();
            treeSections.Nodes.Clear();
            string lastGroup = null;
            TreeNode lastGroupNode = null;
            foreach (var kv in _sections)
            {
                // Если в названии есть «·», группируем по первой части
                string title = kv.Key;
                if (title.Contains("·"))
                {
                    string[] parts = title.Split(new[] { '·' }, 2);
                    string group = parts[0].Trim();
                    string name = parts[1].Trim();
                    if (group != lastGroup)
                    {
                        lastGroupNode = new TreeNode(group) { NodeFont = new Font(Font, FontStyle.Bold) };
                        treeSections.Nodes.Add(lastGroupNode);
                        lastGroup = group;
                    }
                    TreeNode leaf = new TreeNode(name) { Tag = kv };
                    lastGroupNode.Nodes.Add(leaf);
                }
                else
                {
                    lastGroup = null;
                    lastGroupNode = null;
                    TreeNode leaf = new TreeNode(title) { Tag = kv };
                    treeSections.Nodes.Add(leaf);
                }
            }
            treeSections.ExpandAll();
            treeSections.EndUpdate();

            // Выделяем первый раздел
            if (treeSections.Nodes.Count > 0)
            {
                TreeNode first = treeSections.Nodes[0];
                if (first.Nodes.Count > 0) first = first.Nodes[0];
                treeSections.SelectedNode = first;
                ShowSection(first);
            }

            // Моноширинный шрифт для текста
            txtContent.Font = new Font("Consolas", 10F);
            txtContent.WordWrap = false;
        }

        private void treeSections_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowSection(e.Node);
        }

        private void ShowSection(TreeNode node)
        {
            // Поднимаемся к листу (если выбрана группа)
            while (node != null && node.Tag == null && node.Nodes.Count > 0)
                node = node.Nodes[0];
            if (node == null || node.Tag == null) return;

            var kv = (KeyValuePair<string, string>)node.Tag;
            txtContent.Clear();
            txtContent.Text = kv.Value;
            txtContent.SelectionStart = 0;
            txtContent.ScrollToCaret();

            lblSectionTitle.Text = kv.Key;
        }

        // -----------------------------------------------------------------
        // ПОИСК
        // -----------------------------------------------------------------
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(query))
            {
                lblSearchResult.Text = "";
                lblSearchResult.ForeColor = SystemColors.ControlText;
                return;
            }

            int found = 0;
            TreeNode firstFound = null;
            foreach (TreeNode group in treeSections.Nodes)
            {
                foreach (TreeNode leaf in group.Nodes)
                {
                    var kv = (KeyValuePair<string, string>)leaf.Tag;
                    bool match = kv.Key.ToLower().Contains(query) ||
                                 kv.Value.ToLower().Contains(query);
                    leaf.BackColor = match ? Color.Yellow : Color.Empty;
                    if (match)
                    {
                        found++;
                        if (firstFound == null) firstFound = leaf;
                    }
                }
            }

            lblSearchResult.Text = "Найдено разделов: " + found;
            lblSearchResult.ForeColor = found > 0 ? Color.Green : Color.Firebrick;

            if (firstFound != null)
            {
                treeSections.SelectedNode = firstFound;
                firstFound.EnsureVisible();
            }
        }

        // -----------------------------------------------------------------
        // ВНУТРИ-ТЕКСТОВЫЙ ПОИСК (Ctrl+F)
        // -----------------------------------------------------------------
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            foreach (TreeNode group in treeSections.Nodes)
                foreach (TreeNode leaf in group.Nodes)
                    leaf.BackColor = Color.Empty;
        }
    }
}
