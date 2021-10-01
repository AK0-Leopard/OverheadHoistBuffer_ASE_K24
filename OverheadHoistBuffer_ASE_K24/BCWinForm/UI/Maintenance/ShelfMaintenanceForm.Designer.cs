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
            this.components = new System.ComponentModel.Container();
            this.dgv_shelfData = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_bay_id = new System.Windows.Forms.Label();
            this.cmb_bay_id = new System.Windows.Forms.ComboBox();
            this.lbl_zoneID = new System.Windows.Forms.Label();
            this.cmb_zoneID = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_enable = new System.Windows.Forms.Button();
            this.btn_disable = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_shelfID = new System.Windows.Forms.TextBox();
            this.ShelfID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Enable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ZoneID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ADR_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_shelfData
            // 
            this.dgv_shelfData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_shelfData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ShelfID,
            this.CSTID,
            this.Enable,
            this.ZoneID,
            this.ADR_ID,
            this.Column1});
            this.tableLayoutPanel3.SetColumnSpan(this.dgv_shelfData, 6);
            this.dgv_shelfData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_shelfData.Location = new System.Drawing.Point(3, 41);
            this.dgv_shelfData.MultiSelect = false;
            this.dgv_shelfData.Name = "dgv_shelfData";
            this.dgv_shelfData.ReadOnly = true;
            this.dgv_shelfData.RowTemplate.Height = 24;
            this.dgv_shelfData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_shelfData.Size = new System.Drawing.Size(821, 687);
            this.dgv_shelfData.TabIndex = 0;
            this.dgv_shelfData.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgv_shelfData_RowPrePaint);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(833, 760);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Shelf";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 6;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.38214F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.79649F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.82138F));
            this.tableLayoutPanel3.Controls.Add(this.dgv_shelfData, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.lbl_bay_id, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmb_bay_id, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.lbl_zoneID, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmb_zoneID, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.label1, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.txt_shelfID, 5, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 26);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.335157F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.66484F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(827, 731);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // lbl_bay_id
            // 
            this.lbl_bay_id.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_bay_id.AutoSize = true;
            this.lbl_bay_id.Location = new System.Drawing.Point(17, 8);
            this.lbl_bay_id.Name = "lbl_bay_id";
            this.lbl_bay_id.Size = new System.Drawing.Size(80, 22);
            this.lbl_bay_id.TabIndex = 1;
            this.lbl_bay_id.Text = "Bay ID:";
            // 
            // cmb_bay_id
            // 
            this.cmb_bay_id.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_bay_id.FormattingEnabled = true;
            this.cmb_bay_id.Location = new System.Drawing.Point(103, 3);
            this.cmb_bay_id.Name = "cmb_bay_id";
            this.cmb_bay_id.Size = new System.Drawing.Size(169, 30);
            this.cmb_bay_id.TabIndex = 2;
            this.cmb_bay_id.SelectedIndexChanged += new System.EventHandler(this.cmb_bay_id_SelectedIndexChanged);
            // 
            // lbl_zoneID
            // 
            this.lbl_zoneID.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_zoneID.AutoSize = true;
            this.lbl_zoneID.Location = new System.Drawing.Point(282, 8);
            this.lbl_zoneID.Name = "lbl_zoneID";
            this.lbl_zoneID.Size = new System.Drawing.Size(90, 22);
            this.lbl_zoneID.TabIndex = 3;
            this.lbl_zoneID.Text = "Zone ID:";
            // 
            // cmb_zoneID
            // 
            this.cmb_zoneID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_zoneID.FormattingEnabled = true;
            this.cmb_zoneID.Location = new System.Drawing.Point(378, 3);
            this.cmb_zoneID.Name = "cmb_zoneID";
            this.cmb_zoneID.Size = new System.Drawing.Size(166, 30);
            this.cmb_zoneID.TabIndex = 4;
            this.cmb_zoneID.SelectedIndexChanged += new System.EventHandler(this.cmb_zoneID_SelectedIndexChanged);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(839, 825);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(833, 53);
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
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(550, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Shelf ID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_shelfID
            // 
            this.txt_shelfID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_shelfID.Location = new System.Drawing.Point(650, 3);
            this.txt_shelfID.Name = "txt_shelfID";
            this.txt_shelfID.Size = new System.Drawing.Size(174, 30);
            this.txt_shelfID.TabIndex = 6;
            this.txt_shelfID.TextChanged += new System.EventHandler(this.txt_shelfID_TextChanged);
            // 
            // ShelfID
            // 
            this.ShelfID.DataPropertyName = "ShelfID";
            this.ShelfID.HeaderText = "ShelfID";
            this.ShelfID.Name = "ShelfID";
            this.ShelfID.ReadOnly = true;
            // 
            // CSTID
            // 
            this.CSTID.DataPropertyName = "CSTID";
            this.CSTID.HeaderText = "CST ID";
            this.CSTID.Name = "CSTID";
            this.CSTID.ReadOnly = true;
            this.CSTID.Width = 150;
            // 
            // Enable
            // 
            this.Enable.DataPropertyName = "Enable";
            this.Enable.HeaderText = "Enable";
            this.Enable.Name = "Enable";
            this.Enable.ReadOnly = true;
            // 
            // ZoneID
            // 
            this.ZoneID.DataPropertyName = "ZoneID";
            this.ZoneID.HeaderText = "Zone ID";
            this.ZoneID.Name = "ZoneID";
            this.ZoneID.ReadOnly = true;
            this.ZoneID.Width = 200;
            // 
            // ADR_ID
            // 
            this.ADR_ID.DataPropertyName = "ADR_ID";
            this.ADR_ID.HeaderText = "ADR ID";
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
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ShelfMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 825);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "ShelfMaintenanceForm";
            this.Text = "ShelfMaintenanceForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShelfMaintenanceForm_FormClosed);
            this.Load += new System.EventHandler(this.ShelfMaintenanceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_shelfData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btn_enable;
        private System.Windows.Forms.Button btn_disable;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lbl_bay_id;
        private System.Windows.Forms.ComboBox cmb_bay_id;
        private System.Windows.Forms.Label lbl_zoneID;
        private System.Windows.Forms.ComboBox cmb_zoneID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_shelfID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShelfID;
        private System.Windows.Forms.DataGridViewTextBoxColumn CSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ZoneID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ADR_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Timer timer1;
    }
}