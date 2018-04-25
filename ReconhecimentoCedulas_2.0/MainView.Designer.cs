namespace ReconhecimentoCedulas_2._0
{
    partial class MainView
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.operationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sVMTrainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sVMEvaluateEvalPasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sVMEvaluateOneImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.operationsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // operationsToolStripMenuItem
            // 
            this.operationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sVMTrainToolStripMenuItem,
            this.sVMEvaluateEvalPasteToolStripMenuItem,
            this.sVMEvaluateOneImageToolStripMenuItem});
            this.operationsToolStripMenuItem.Name = "operationsToolStripMenuItem";
            this.operationsToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.operationsToolStripMenuItem.Text = "Operations";
            // 
            // sVMTrainToolStripMenuItem
            // 
            this.sVMTrainToolStripMenuItem.Name = "sVMTrainToolStripMenuItem";
            this.sVMTrainToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.sVMTrainToolStripMenuItem.Text = "SVM Train";
            this.sVMTrainToolStripMenuItem.Click += new System.EventHandler(this.sVMTrainToolStripMenuItem_Click);
            // 
            // sVMEvaluateEvalPasteToolStripMenuItem
            // 
            this.sVMEvaluateEvalPasteToolStripMenuItem.Name = "sVMEvaluateEvalPasteToolStripMenuItem";
            this.sVMEvaluateEvalPasteToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.sVMEvaluateEvalPasteToolStripMenuItem.Text = "SVM Evaluate (Eval Paste)";
            this.sVMEvaluateEvalPasteToolStripMenuItem.Click += new System.EventHandler(this.sVMEvaluateEvalPasteToolStripMenuItem_Click);
            // 
            // sVMEvaluateOneImageToolStripMenuItem
            // 
            this.sVMEvaluateOneImageToolStripMenuItem.Name = "sVMEvaluateOneImageToolStripMenuItem";
            this.sVMEvaluateOneImageToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.sVMEvaluateOneImageToolStripMenuItem.Text = "SVM Evaluate (One Image)";
            this.sVMEvaluateOneImageToolStripMenuItem.Click += new System.EventHandler(this.sVMEvaluateOneImageToolStripMenuItem_Click);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainView";
            this.Text = "Reconhecimento de Cédulas";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem operationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sVMTrainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sVMEvaluateEvalPasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sVMEvaluateOneImageToolStripMenuItem;
    }
}

