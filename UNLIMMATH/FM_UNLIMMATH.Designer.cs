namespace UNLIMMATH
{
    partial class FM_UNLIMMATH
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
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.lblInput = new System.Windows.Forms.Label();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.lstHistory = new System.Windows.Forms.ListBox();
            this.lblHelp = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblHistory = new System.Windows.Forms.Label();
            this.btnClearHistory = new System.Windows.Forms.Button();
            this.panelQuick = new System.Windows.Forms.Panel();
            this.lblQuick = new System.Windows.Forms.Label();
            this.btnPi = new System.Windows.Forms.Button();
            this.btnE = new System.Windows.Forms.Button();
            this.btnSqrt = new System.Windows.Forms.Button();
            this.btnPow = new System.Windows.Forms.Button();
            this.btnFact = new System.Windows.Forms.Button();
            this.btnMod = new System.Windows.Forms.Button();
            this.btnSin = new System.Windows.Forms.Button();
            this.btnCos = new System.Windows.Forms.Button();
            this.btnTan = new System.Windows.Forms.Button();
            this.btnLn = new System.Windows.Forms.Button();
            this.btnExp = new System.Windows.Forms.Button();
            this.btnLog = new System.Windows.Forms.Button();
            this.btnAbs = new System.Windows.Forms.Button();
            this.panelSettings = new System.Windows.Forms.Panel();
            this.lblPrecision = new System.Windows.Forms.Label();
            this.nudPrecision = new System.Windows.Forms.NumericUpDown();
            this.btnResetVars = new System.Windows.Forms.Button();
            this.chkDarkTheme = new System.Windows.Forms.CheckBox();
            this.panelTop.SuspendLayout();
            this.panelCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelQuick.SuspendLayout();
            this.panelSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrecision)).BeginInit();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.btnCalculate);
            this.panelTop.Controls.Add(this.txtInput);
            this.panelTop.Controls.Add(this.lblInput);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(900, 64);
            this.panelTop.TabIndex = 0;
            //
            // btnClear
            //
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(715, 30);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 25);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Очистить";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // btnCalculate
            //
            this.btnCalculate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalculate.Location = new System.Drawing.Point(796, 30);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(95, 25);
            this.btnCalculate.TabIndex = 2;
            this.btnCalculate.Text = "Вычислить";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            //
            // txtInput
            //
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Location = new System.Drawing.Point(8, 30);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(701, 20);
            this.txtInput.TabIndex = 1;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            //
            // lblInput
            //
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(8, 11);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(135, 13);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "Выражение (Enter — вычислить):";
            //
            // panelCenter
            //
            this.panelCenter.Controls.Add(this.txtOutput);
            this.panelCenter.Controls.Add(this.lblOutput);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCenter.Location = new System.Drawing.Point(0, 64);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(900, 56);
            this.panelCenter.TabIndex = 1;
            //
            // txtOutput
            //
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.Location = new System.Drawing.Point(8, 22);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(889, 20);
            this.txtOutput.TabIndex = 1;
            //
            // lblOutput
            //
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(8, 6);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(61, 13);
            this.lblOutput.TabIndex = 0;
            this.lblOutput.Text = "Результат:";
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 168);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this.lstHistory);
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.lblHelp);
            this.splitContainer.Size = new System.Drawing.Size(900, 282);
            this.splitContainer.SplitterDistance = 450;
            this.splitContainer.TabIndex = 3;
            //
            // lstHistory
            //
            this.lstHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstHistory.Font = new System.Drawing.Font("Consolas", 9F);
            this.lstHistory.FormattingEnabled = true;
            this.lstHistory.ItemHeight = 14;
            this.lstHistory.Location = new System.Drawing.Point(0, 0);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(450, 282);
            this.lstHistory.TabIndex = 0;
            this.lstHistory.DoubleClick += new System.EventHandler(this.lstHistory_DoubleClick);
            //
            // lblHelp
            //
            this.lblHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHelp.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblHelp.Location = new System.Drawing.Point(0, 0);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.lblHelp.Size = new System.Drawing.Size(446, 282);
            this.lblHelp.TabIndex = 0;
            this.lblHelp.Text = "lblHelp";
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.btnClearHistory);
            this.panelBottom.Controls.Add(this.lblHistory);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 450);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(900, 24);
            this.panelBottom.TabIndex = 4;
            //
            // lblHistory
            //
            this.lblHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHistory.Location = new System.Drawing.Point(0, 0);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblHistory.Size = new System.Drawing.Size(796, 24);
            this.lblHistory.TabIndex = 0;
            this.lblHistory.Text = "История (двойной клик — вернуть в поле ввода)";
            this.lblHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnClearHistory
            //
            this.btnClearHistory.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearHistory.Location = new System.Drawing.Point(796, 0);
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Size = new System.Drawing.Size(104, 24);
            this.btnClearHistory.TabIndex = 1;
            this.btnClearHistory.Text = "Очистить историю";
            this.btnClearHistory.UseVisualStyleBackColor = true;
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);
            //
            // panelQuick — панель быстрых кнопок
            //
            this.panelQuick.Controls.Add(this.lblQuick);
            this.panelQuick.Controls.Add(this.btnPi);
            this.panelQuick.Controls.Add(this.btnE);
            this.panelQuick.Controls.Add(this.btnSqrt);
            this.panelQuick.Controls.Add(this.btnPow);
            this.panelQuick.Controls.Add(this.btnFact);
            this.panelQuick.Controls.Add(this.btnMod);
            this.panelQuick.Controls.Add(this.btnSin);
            this.panelQuick.Controls.Add(this.btnCos);
            this.panelQuick.Controls.Add(this.btnTan);
            this.panelQuick.Controls.Add(this.btnLn);
            this.panelQuick.Controls.Add(this.btnExp);
            this.panelQuick.Controls.Add(this.btnLog);
            this.panelQuick.Controls.Add(this.btnAbs);
            this.panelQuick.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelQuick.Location = new System.Drawing.Point(0, 120);
            this.panelQuick.Name = "panelQuick";
            this.panelQuick.Size = new System.Drawing.Size(900, 48);
            this.panelQuick.TabIndex = 5;
            //
            // lblQuick
            //
            this.lblQuick.AutoSize = true;
            this.lblQuick.Location = new System.Drawing.Point(8, 5);
            this.lblQuick.Name = "lblQuick";
            this.lblQuick.Size = new System.Drawing.Size(99, 13);
            this.lblQuick.TabIndex = 0;
            this.lblQuick.Text = "Быстрые кнопки:";
            //
            // btnPi
            //
            this.btnPi.Location = new System.Drawing.Point(8, 22);
            this.btnPi.Name = "btnPi";
            this.btnPi.Size = new System.Drawing.Size(40, 22);
            this.btnPi.TabIndex = 1;
            this.btnPi.Tag = "pi";
            this.btnPi.Text = "π";
            this.btnPi.UseVisualStyleBackColor = true;
            this.btnPi.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnE
            //
            this.btnE.Location = new System.Drawing.Point(54, 22);
            this.btnE.Name = "btnE";
            this.btnE.Size = new System.Drawing.Size(40, 22);
            this.btnE.TabIndex = 2;
            this.btnE.Tag = "e";
            this.btnE.Text = "e";
            this.btnE.UseVisualStyleBackColor = true;
            this.btnE.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnSqrt
            //
            this.btnSqrt.Location = new System.Drawing.Point(100, 22);
            this.btnSqrt.Name = "btnSqrt";
            this.btnSqrt.Size = new System.Drawing.Size(55, 22);
            this.btnSqrt.TabIndex = 3;
            this.btnSqrt.Tag = "sqrt(";
            this.btnSqrt.Text = "√";
            this.btnSqrt.UseVisualStyleBackColor = true;
            this.btnSqrt.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnPow
            //
            this.btnPow.Location = new System.Drawing.Point(161, 22);
            this.btnPow.Name = "btnPow";
            this.btnPow.Size = new System.Drawing.Size(40, 22);
            this.btnPow.TabIndex = 4;
            this.btnPow.Tag = "^";
            this.btnPow.Text = "^";
            this.btnPow.UseVisualStyleBackColor = true;
            this.btnPow.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnFact
            //
            this.btnFact.Location = new System.Drawing.Point(207, 22);
            this.btnFact.Name = "btnFact";
            this.btnFact.Size = new System.Drawing.Size(55, 22);
            this.btnFact.TabIndex = 5;
            this.btnFact.Tag = "fact(";
            this.btnFact.Text = "x!";
            this.btnFact.UseVisualStyleBackColor = true;
            this.btnFact.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnMod
            //
            this.btnMod.Location = new System.Drawing.Point(268, 22);
            this.btnMod.Name = "btnMod";
            this.btnMod.Size = new System.Drawing.Size(55, 22);
            this.btnMod.TabIndex = 6;
            this.btnMod.Tag = " mod ";
            this.btnMod.Text = "mod";
            this.btnMod.UseVisualStyleBackColor = true;
            this.btnMod.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnSin
            //
            this.btnSin.Location = new System.Drawing.Point(329, 22);
            this.btnSin.Name = "btnSin";
            this.btnSin.Size = new System.Drawing.Size(55, 22);
            this.btnSin.TabIndex = 7;
            this.btnSin.Tag = "sin(";
            this.btnSin.Text = "sin";
            this.btnSin.UseVisualStyleBackColor = true;
            this.btnSin.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnCos
            //
            this.btnCos.Location = new System.Drawing.Point(390, 22);
            this.btnCos.Name = "btnCos";
            this.btnCos.Size = new System.Drawing.Size(55, 22);
            this.btnCos.TabIndex = 8;
            this.btnCos.Tag = "cos(";
            this.btnCos.Text = "cos";
            this.btnCos.UseVisualStyleBackColor = true;
            this.btnCos.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnTan
            //
            this.btnTan.Location = new System.Drawing.Point(451, 22);
            this.btnTan.Name = "btnTan";
            this.btnTan.Size = new System.Drawing.Size(55, 22);
            this.btnTan.TabIndex = 9;
            this.btnTan.Tag = "tan(";
            this.btnTan.Text = "tan";
            this.btnTan.UseVisualStyleBackColor = true;
            this.btnTan.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnLn
            //
            this.btnLn.Location = new System.Drawing.Point(512, 22);
            this.btnLn.Name = "btnLn";
            this.btnLn.Size = new System.Drawing.Size(55, 22);
            this.btnLn.TabIndex = 10;
            this.btnLn.Tag = "ln(";
            this.btnLn.Text = "ln";
            this.btnLn.UseVisualStyleBackColor = true;
            this.btnLn.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnExp
            //
            this.btnExp.Location = new System.Drawing.Point(573, 22);
            this.btnExp.Name = "btnExp";
            this.btnExp.Size = new System.Drawing.Size(55, 22);
            this.btnExp.TabIndex = 11;
            this.btnExp.Tag = "exp(";
            this.btnExp.Text = "exp";
            this.btnExp.UseVisualStyleBackColor = true;
            this.btnExp.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnLog
            //
            this.btnLog.Location = new System.Drawing.Point(634, 22);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(55, 22);
            this.btnLog.TabIndex = 12;
            this.btnLog.Tag = "log(";
            this.btnLog.Text = "log";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // btnAbs
            //
            this.btnAbs.Location = new System.Drawing.Point(695, 22);
            this.btnAbs.Name = "btnAbs";
            this.btnAbs.Size = new System.Drawing.Size(55, 22);
            this.btnAbs.TabIndex = 13;
            this.btnAbs.Tag = "abs(";
            this.btnAbs.Text = "|x|";
            this.btnAbs.UseVisualStyleBackColor = true;
            this.btnAbs.Click += new System.EventHandler(this.QuickButton_Click);
            //
            // panelSettings — панель настроек (точность, тема, сброс)
            //
            this.panelSettings.Controls.Add(this.chkDarkTheme);
            this.panelSettings.Controls.Add(this.btnResetVars);
            this.panelSettings.Controls.Add(this.nudPrecision);
            this.panelSettings.Controls.Add(this.lblPrecision);
            this.panelSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSettings.Location = new System.Drawing.Point(0, 144);
            this.panelSettings.Name = "panelSettings";
            this.panelSettings.Size = new System.Drawing.Size(900, 24);
            this.panelSettings.TabIndex = 6;
            //
            // lblPrecision
            //
            this.lblPrecision.AutoSize = true;
            this.lblPrecision.Location = new System.Drawing.Point(8, 5);
            this.lblPrecision.Name = "lblPrecision";
            this.lblPrecision.Size = new System.Drawing.Size(125, 13);
            this.lblPrecision.TabIndex = 0;
            this.lblPrecision.Text = "Знаков после запятой:";
            //
            // nudPrecision
            //
            this.nudPrecision.Location = new System.Drawing.Point(139, 2);
            this.nudPrecision.Maximum = new decimal(new int[] {50, 0, 0, 0});
            this.nudPrecision.Minimum = new decimal(new int[] {1, 0, 0, 0});
            this.nudPrecision.Name = "nudPrecision";
            this.nudPrecision.Size = new System.Drawing.Size(60, 20);
            this.nudPrecision.TabIndex = 1;
            this.nudPrecision.Value = new decimal(new int[] {20, 0, 0, 0});
            this.nudPrecision.ValueChanged += new System.EventHandler(this.nudPrecision_ValueChanged);
            //
            // btnResetVars
            //
            this.btnResetVars.Location = new System.Drawing.Point(210, 1);
            this.btnResetVars.Name = "btnResetVars";
            this.btnResetVars.Size = new System.Drawing.Size(140, 22);
            this.btnResetVars.TabIndex = 2;
            this.btnResetVars.Text = "Сброс переменных";
            this.btnResetVars.UseVisualStyleBackColor = true;
            this.btnResetVars.Click += new System.EventHandler(this.btnResetVars_Click);
            //
            // chkDarkTheme
            //
            this.chkDarkTheme.AutoSize = true;
            this.chkDarkTheme.Location = new System.Drawing.Point(360, 4);
            this.chkDarkTheme.Name = "chkDarkTheme";
            this.chkDarkTheme.Size = new System.Drawing.Size(101, 17);
            this.chkDarkTheme.TabIndex = 3;
            this.chkDarkTheme.Text = "Тёмная тема";
            this.chkDarkTheme.UseVisualStyleBackColor = true;
            this.chkDarkTheme.CheckedChanged += new System.EventHandler(this.chkDarkTheme_CheckedChanged);
            //
            // FM_UNLIMMATH
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 474);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelSettings);
            this.Controls.Add(this.panelQuick);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FM_UNLIMMATH";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Инженерный калькулятор";
            this.Load += new System.EventHandler(this.FM_UNLIMMATH_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelCenter.ResumeLayout(false);
            this.panelCenter.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelQuick.ResumeLayout(false);
            this.panelQuick.PerformLayout();
            this.panelSettings.ResumeLayout(false);
            this.panelSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrecision)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListBox lstHistory;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblHistory;
        private System.Windows.Forms.Button btnClearHistory;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Panel panelQuick;
        private System.Windows.Forms.Label lblQuick;
        private System.Windows.Forms.Button btnPi;
        private System.Windows.Forms.Button btnE;
        private System.Windows.Forms.Button btnSqrt;
        private System.Windows.Forms.Button btnPow;
        private System.Windows.Forms.Button btnFact;
        private System.Windows.Forms.Button btnMod;
        private System.Windows.Forms.Button btnSin;
        private System.Windows.Forms.Button btnCos;
        private System.Windows.Forms.Button btnTan;
        private System.Windows.Forms.Button btnLn;
        private System.Windows.Forms.Button btnExp;
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.Button btnAbs;
        private System.Windows.Forms.Panel panelSettings;
        private System.Windows.Forms.Label lblPrecision;
        private System.Windows.Forms.NumericUpDown nudPrecision;
        private System.Windows.Forms.Button btnResetVars;
        private System.Windows.Forms.CheckBox chkDarkTheme;
    }
}
