
namespace TEST_GRAPH
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelGraph = new DoubleBufferedPanel();
            dgvIncidenceMatrix = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvIncidenceMatrix).BeginInit();
            SuspendLayout();
            // 
            // panelGraph
            // 
            panelGraph.BorderStyle = BorderStyle.FixedSingle;
            panelGraph.Dock = DockStyle.Left;
            panelGraph.Location = new Point(0, 0);
            panelGraph.Name = "panelGraph";
            panelGraph.Size = new Size(859, 519);
            panelGraph.TabIndex = 0;
            // 
            // dgvIncidenceMatrix
            // 
            dgvIncidenceMatrix.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvIncidenceMatrix.Dock = DockStyle.Fill;
            dgvIncidenceMatrix.Location = new Point(859, 0);
            dgvIncidenceMatrix.Name = "dgvIncidenceMatrix";
            dgvIncidenceMatrix.RowHeadersWidth = 51;
            dgvIncidenceMatrix.Size = new Size(238, 519);
            dgvIncidenceMatrix.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1097, 519);
            Controls.Add(dgvIncidenceMatrix);
            Controls.Add(panelGraph);
            Name = "Form1";
            Text = "СУПЕР МОДНЫЕ ДИЗАЙНЕРСКИЕ ГРАФЫ";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvIncidenceMatrix).EndInit();
            ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #endregion

        private DoubleBufferedPanel panelGraph;
        private DataGridView dgvIncidenceMatrix;
    }
}
