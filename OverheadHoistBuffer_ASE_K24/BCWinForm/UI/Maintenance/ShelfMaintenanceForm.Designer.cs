namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class ShelfMaintenanceForm
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgv_shelfData = new System.Windows.Forms.DataGridView();
            this.ShelfID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Enable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ADR_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_enable = new System.Windows.Forms.Button();
            this.btn_disable = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_bay_id = new System.Windows.Forms.Label();
            this.cmb_bay_id = new System.Windows.Forms.ComboBox();
            this.lbl_zoneID = new System.Windows.Forms.Label();
            this.cmb_zoneID = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_shelfData
            // 
            this.dgv_shelfData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_shelfData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ShelfID,
            this.Enable,
            this.ADR_ID,
            this.Column1});
            this.tableLayoutPanel3.SetColumnSpan(this.dgv_shelfData, 4);
            this.dgv_shelfData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_shelfData.Location = new System.Drawing.Point(3, 42);
            this.dgv_shelfData.MultiSelect = false;
            this.dgv_shelfData.Name = "dgv_shelfData";
            this.dgv_shelfData.ReadOnly = true;
            this.dgv_shelfData.RowTemplate.Height = 24;
            this.dgv_shelfData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_shelfData.Size = new System.Drawing.Size(517, 686);
            this.dgv_shelfData.TabIndex = 0;
            this.dgv_shelfData.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgv_shelfData_RowPrePaint);
            // 
            // ShelfID
            // 
            this.ShelfID.DataPropertyName = "ShelfID";
            this.ShelfID.HeaderText = "ShelfID";
            this.ShelfID.Name = "ShelfID";
            this.ShelfID.ReadOnly = true;
            // 
            // Enable
            // 
            this.Enable.DataPropertyName = "Enable";
            this.Enable.HeaderText = "Enable";
            this.Enable.Name = "Enable";
            this.Enable.ReadOnly = true;
            // 
            // ADR_ID
            // 
            this.ADR_ID.DataPropertyName = "ADR_ID";
            this.ADR_ID.HeaderText = "ADR_ID";
            this.ADR_ID.Name = "ADR_ID";
            this.ADR_ID.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 760);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Shelf";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.85714F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.142857F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(535, 825);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.btn_enable, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_disable, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 769);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(529, 53);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btn_enable
            // 
            this.btn_enable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_enable.Location = new System.Drawing.Point(3, 3);
            this.btn_enable.Name = "btn_enable";
            this.btn_enable.Size = new System.Drawing.Size(94, 47);
            this.btn_enable.TabIndex = 0;
            this.btn_enable.Text = "Enable";
            this.btn_enable.UseVisualStyleBackColor = true;
            this.btn_enable.Click += new System.EventHandler(this.btn_enable_Click);
            // 
            // btn_disable
            // 
            this.btn_disable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_disable.Location = new System.Drawing.Point(103, 3);
            this.btn_disable.Name = "btn_disable";
            this.btn_disable.Size = new System.Drawing.Size(94, 47);
            this.btn_disable.TabIndex = 1;
            this.btn_disable.Text = "Disable";
            this.btn_disable.UseVisualStyleBackColor = true;
            this.btn_disable.Click += new System.EventHandler(this.btn_disable_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.01721F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.69598F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.54684F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.12237F));
            this.tableLayoutPanel3.Controls.Add(this.dgv_shelfData, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.lbl_bay_id, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmb_bay_id, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.lbl_zoneID, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmb_zoneID, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 26);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.335157F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.66484F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(523, 731);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // lbl_bay_id
            // 
            this.lbl_bay_id.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_bay_id.AutoSize = true;
            this.lbl_bay_id.Location = new System.Drawing.Point(5, 8);
            this.lbl_bay_id.Name = "lbl_bay_id";
            this.lbl_bay_id.Size = new System.Drawing.Size(80, 22);
            this.lbl_bay_id.TabIndex = 1;
            this.lbl_bay_id.Text = "Bay ID:";
            // 
            // cmb_bay_id
            // 
            this.cmb_bay_id.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_bay_id.FormattingEnabled = true;
            this.cmb_bay_id.Location = new System.Drawing.Point(91, 3);
            this.cmb_bay_id.Name = "cmb_bay_id";
            this.cmb_bay_id.Size = new System.Drawing.Size(164, 30);
            this.cmb_bay_id.TabIndex = 2;
            this.cmb_bay_id.SelectedIndexChanged += new System.EventHandler(this.cmb_bay_id_SelectedIndexChanged);
            // 
            // lbl_zoneID
            // 
            this.lbl_zoneID.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_zoneID.AutoSize = true;
            this.lbl_zoneID.Location = new System.Drawing.Point(261, 8);
            this.lbl_zoneID.Name = "lbl_zoneID";
            this.lbl_zoneID.Size = new System.Drawing.Size(90, 22);
            this.lbl_zoneID.TabIndex = 3;
            this.lbl_zoneID.Text = "Zone ID:";
            this.lbl_zoneID.Visible = false;
            // 
            // cmb_zoneID
            // 
            this.cmb_zoneID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_zoneID.FormattingEnabled = true;
            this.cmb_zoneID.Location = new System.Drawing.Point(357, 3);
            this.cmb_zoneID.Name = "cmb_zoneID";
            this.cmb_zoneID.Size = new System.Drawing.Size(163, 30);
            this.cmb_zoneID.TabIndex = 4;
            this.cmb_zoneID.Visible = false;
            // 
            // ShelfMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 825);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "ShelfMaintenanceForm";
            this.Text = "ShelfMaintenanceForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShelfMaintenanceForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_shelfData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btn_enable;
        private System.Windows.Forms.Button btn_disable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShelfID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ADR_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lbl_bay_id;
        private System.Windows.Forms.ComboBox cmb_bay_id;
        private System.Windows.Forms.Label lbl_zoneID;
        private System.Windows.Forms.ComboBox cmb_zoneID;
    }
}