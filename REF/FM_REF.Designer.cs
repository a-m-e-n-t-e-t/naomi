namespace REF
{
    partial class FM_REF
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support — do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSectionTitle = new System.Windows.Forms.Label();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnClearSearch = new System.Windows.Forms.Button();
            this.lblSearchResult = new System.Windows.Forms.Label();
            this.lblSearchHint = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeSections = new System.Windows.Forms.TreeView();
            this.txtContent = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.panelSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.lblSectionTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 48);
            this.panelTop.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(8, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(120, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Справочник";
            //
            // lblSectionTitle
            //
            this.lblSectionTitle.AutoSize = true;
            this.lblSectionTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblSectionTitle.ForeColor = System.Drawing.Color.DimGray;
            this.lblSectionTitle.Location = new System.Drawing.Point(8, 30);
            this.lblSectionTitle.Name = "lblSectionTitle";
            this.lblSectionTitle.Size = new System.Drawing.Size(80, 15);
            this.lblSectionTitle.TabIndex = 1;
            this.lblSectionTitle.Text = "раздел";
            //
            // panelSearch
            //
            this.panelSearch.Controls.Add(this.btnClearSearch);
            this.panelSearch.Controls.Add(this.txtSearch);
            this.panelSearch.Controls.Add(this.lblSearchHint);
            this.panelSearch.Controls.Add(this.lblSearchResult);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Location = new System.Drawing.Point(0, 48);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(1000, 32);
            this.panelSearch.TabIndex = 1;
            //
            // lblSearchHint
            //
            this.lblSearchHint.AutoSize = true;
            this.lblSearchHint.Location = new System.Drawing.Point(8, 8);
            this.lblSearchHint.Name = "lblSearchHint";
            this.lblSearchHint.Size = new System.Drawing.Size(56, 13);
            this.lblSearchHint.TabIndex = 0;
            this.lblSearchHint.Text = "Поиск:";
            //
            // txtSearch
            //
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(70, 5);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(750, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            //
            // btnClearSearch
            //
            this.btnClearSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSearch.Location = new System.Drawing.Point(826, 4);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(75, 23);
            this.btnClearSearch.TabIndex = 2;
            this.btnClearSearch.Text = "Очистить";
            this.btnClearSearch.UseVisualStyleBackColor = true;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);
            //
            // lblSearchResult
            //
            this.lblSearchResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchResult.Location = new System.Drawing.Point(910, 8);
            this.lblSearchResult.Name = "lblSearchResult";
            this.lblSearchResult.Size = new System.Drawing.Size(85, 18);
            this.lblSearchResult.TabIndex = 3;
            this.lblSearchResult.Text = "";
            this.lblSearchResult.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 80);
            this.splitMain.Name = "splitMain";
            //
            // splitMain.Panel1 — дерево разделов
            //
            this.splitMain.Panel1.Controls.Add(this.treeSections);
            //
            // splitMain.Panel2 — текст
            //
            this.splitMain.Panel2.Controls.Add(this.txtContent);
            this.splitMain.Size = new System.Drawing.Size(1000, 410);
            this.splitMain.SplitterDistance = 320;
            this.splitMain.TabIndex = 2;
            //
            // treeSections
            //
            this.treeSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSections.HideSelection = false;
            this.treeSections.Location = new System.Drawing.Point(0, 0);
            this.treeSections.Name = "treeSections";
            this.treeSections.ShowNodeToolTips = true;
            this.treeSections.Size = new System.Drawing.Size(320, 410);
            this.treeSections.TabIndex = 0;
            this.treeSections.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeSections_AfterSelect);
            //
            // txtContent
            //
            this.txtContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtContent.Location = new System.Drawing.Point(0, 0);
            this.txtContent.Name = "txtContent";
            this.txtContent.ReadOnly = true;
            this.txtContent.Size = new System.Drawing.Size(676, 410);
            this.txtContent.TabIndex = 0;
            this.txtContent.Text = "";
            //
            // FM_REF
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 490);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.panelSearch);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FM_REF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Справочник";
            this.Load += new System.EventHandler(this.FM_REF_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSectionTitle;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Label lblSearchHint;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnClearSearch;
        private System.Windows.Forms.Label lblSearchResult;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TreeView treeSections;
        private System.Windows.Forms.RichTextBox txtContent;
    }
}
