
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
            richTextBox1 = new RichTextBox();
            btn_random_routing = new Button();
            btn_avalanche_routing = new Button();
            btn_experience_routing = new Button();
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
            panelGraph.Paint += panelGraph_Paint_1;
            // 
            // dgvIncidenceMatrix
            // 
            dgvIncidenceMatrix.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvIncidenceMatrix.Dock = DockStyle.Fill;
            dgvIncidenceMatrix.Location = new Point(859, 87);
            dgvIncidenceMatrix.Name = "dgvIncidenceMatrix";
            dgvIncidenceMatrix.RowHeadersWidth = 51;
            dgvIncidenceMatrix.Size = new Size(238, 159);
            dgvIncidenceMatrix.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Bottom;
            richTextBox1.Location = new Point(859, 246);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(238, 273);
            richTextBox1.TabIndex = 2;
            richTextBox1.Text = "";
            // 
            // btn_random_routing
            // 
            btn_random_routing.Dock = DockStyle.Top;
            btn_random_routing.Location = new Point(859, 0);
            btn_random_routing.Name = "btn_random_routing";
            btn_random_routing.Size = new Size(238, 29);
            btn_random_routing.TabIndex = 0;
            btn_random_routing.Text = "Случайная маршрутизация";
            btn_random_routing.UseVisualStyleBackColor = true;
            btn_random_routing.Click += btn_random_routing_Click;
            // 
            // btn_avalanche_routing
            // 
            btn_avalanche_routing.Dock = DockStyle.Top;
            btn_avalanche_routing.Location = new Point(859, 29);
            btn_avalanche_routing.Name = "btn_avalanche_routing";
            btn_avalanche_routing.Size = new Size(238, 29);
            btn_avalanche_routing.TabIndex = 3;
            btn_avalanche_routing.Text = "Лавинная маршрутизация";
            btn_avalanche_routing.UseVisualStyleBackColor = true;
            btn_avalanche_routing.Click += btn_avalanche_routing_Click;
            // 
            // btn_experience_routing
            // 
            btn_experience_routing.Dock = DockStyle.Top;
            btn_experience_routing.Location = new Point(859, 58);
            btn_experience_routing.Name = "btn_experience_routing";
            btn_experience_routing.Size = new Size(238, 29);
            btn_experience_routing.TabIndex = 4;
            btn_experience_routing.Text = "Маршрутизация по предыдущему опыту";
            btn_experience_routing.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1097, 519);
            Controls.Add(dgvIncidenceMatrix);
            Controls.Add(btn_experience_routing);
            Controls.Add(btn_avalanche_routing);
            Controls.Add(btn_random_routing);
            Controls.Add(richTextBox1);
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
        private RichTextBox richTextBox1;
        private Button btn_random_routing;
        private Button btn_avalanche_routing;
        private Button btn_experience_routing;
    }
}
