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
            this.grb_Shelpanel = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_bay_id = new System.Windows.Forms.Label();
            this.cmb_bay_id = new System.Windows.Forms.ComboBox();
            this.lbl_zoneID = new System.Windows.Forms.Label();
            this.cmb_zoneID = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_shelfID = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_enable = new System.Windows.Forms.Button();
            this.btn_disable = new System.Windows.Forms.Button();
            this.btn_EnableAll = new System.Windows.Forms.Button();
            this.btn_DisableAll = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_reason = new System.Windows.Forms.TextBox();
            this.ShelfID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Enable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ZoneID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ADR_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Remark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).BeginInit();
            this.grb_Shelpanel.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
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
            this.Time,
            this.Remark});
            this.tableLayoutPanel3.SetColumnSpan(this.dgv_shelfData, 6);
            this.dgv_shelfData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_shelfData.Location = new System.Drawing.Point(3, 40);
            this.dgv_shelfData.MultiSelect = false;
            this.dgv_shelfData.Name = "dgv_shelfData";
            this.dgv_shelfData.ReadOnly = true;
            this.dgv_shelfData.RowTemplate.Height = 24;
            this.dgv_shelfData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_shelfData.Size = new System.Drawing.Size(1196, 661);
            this.dgv_shelfData.TabIndex = 0;
            this.dgv_shelfData.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgv_shelfData_RowPrePaint);
            // 
            // grb_Shelpanel
            // 
            this.grb_Shelpanel.Controls.Add(this.tableLayoutPanel3);
            this.grb_Shelpanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grb_Shelpanel.Location = new System.Drawing.Point(3, 3);
            this.grb_Shelpanel.Name = "grb_Shelpanel";
            this.grb_Shelpanel.Size = new System.Drawing.Size(1208, 733);
            this.grb_Shelpanel.TabIndex = 0;
            this.grb_Shelpanel.TabStop = false;
            this.grb_Shelpanel.Text = "Shelf";
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1202, 704);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // lbl_bay_id
            // 
            this.lbl_bay_id.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_bay_id.AutoSize = true;
            this.lbl_bay_id.Location = new System.Drawing.Point(17, 7);
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
            this.cmb_bay_id.Size = new System.Drawing.Size(295, 30);
            this.cmb_bay_id.TabIndex = 2;
            this.cmb_bay_id.SelectedIndexChanged += new System.EventHandler(this.cmb_bay_id_SelectedIndexChanged);
            // 
            // lbl_zoneID
            // 
            this.lbl_zoneID.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_zoneID.AutoSize = true;
            this.lbl_zoneID.Location = new System.Drawing.Point(408, 7);
            this.lbl_zoneID.Name = "lbl_zoneID";
            this.lbl_zoneID.Size = new System.Drawing.Size(90, 22);
            this.lbl_zoneID.TabIndex = 3;
            this.lbl_zoneID.Text = "Zone ID:";
            // 
            // cmb_zoneID
            // 
            this.cmb_zoneID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_zoneID.FormattingEnabled = true;
            this.cmb_zoneID.Location = new System.Drawing.Point(504, 3);
            this.cmb_zoneID.Name = "cmb_zoneID";
            this.cmb_zoneID.Size = new System.Drawing.Size(289, 30);
            this.cmb_zoneID.TabIndex = 4;
            this.cmb_zoneID.SelectedIndexChanged += new System.EventHandler(this.cmb_zoneID_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(799, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Shelf ID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_shelfID
            // 
            this.txt_shelfID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_shelfID.Location = new System.Drawing.Point(899, 3);
            this.txt_shelfID.Name = "txt_shelfID";
            this.txt_shelfID.Size = new System.Drawing.Size(300, 30);
            this.txt_shelfID.TabIndex = 6;
            this.txt_shelfID.TextChanged += new System.EventHandler(this.txt_shelfID_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.grb_Shelpanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1214, 825);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.Controls.Add(this.btn_enable, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_disable, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_EnableAll, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_DisableAll, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 778);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1208, 44);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btn_enable
            // 
            this.btn_enable.Location = new System.Drawing.Point(3, 3);
            this.btn_enable.Name = "btn_enable";
            this.btn_enable.Size = new System.Drawing.Size(94, 38);
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
            this.btn_disable.Size = new System.Drawing.Size(94, 38);
            this.btn_disable.TabIndex = 1;
            this.btn_disable.Text = "Disable";
            this.btn_disable.UseVisualStyleBackColor = true;
            this.btn_disable.Click += new System.EventHandler(this.btn_disable_Click);
            // 
            // btn_EnableAll
            // 
            this.btn_EnableAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_EnableAll.Location = new System.Drawing.Point(911, 3);
            this.btn_EnableAll.Name = "btn_EnableAll";
            this.btn_EnableAll.Size = new System.Drawing.Size(144, 38);
            this.btn_EnableAll.TabIndex = 0;
            this.btn_EnableAll.Text = "Enable All";
            this.btn_EnableAll.UseVisualStyleBackColor = true;
            this.btn_EnableAll.Click += new System.EventHandler(this.btn_EnableAll_Click);
            // 
            // btn_DisableAll
            // 
            this.btn_DisableAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_DisableAll.Location = new System.Drawing.Point(1061, 3);
            this.btn_DisableAll.Name = "btn_DisableAll";
            this.btn_DisableAll.Size = new System.Drawing.Size(144, 38);
            this.btn_DisableAll.TabIndex = 0;
            this.btn_DisableAll.Text = "Disable All";
            this.btn_DisableAll.UseVisualStyleBackColor = true;
            this.btn_DisableAll.Click += new System.EventHandler(this.btn_DisableAll_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.25387F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.74613F));
            this.tableLayoutPanel5.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.txt_reason, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 739);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1214, 36);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(114, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 22);
            this.label5.TabIndex = 0;
            this.label5.Text = "Reason:";
            // 
            // txt_reason
            // 
            this.txt_reason.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_reason.Location = new System.Drawing.Point(200, 3);
            this.txt_reason.MaxLength = 80;
            this.txt_reason.Name = "txt_reason";
            this.txt_reason.Size = new System.Drawing.Size(1011, 30);
            this.txt_reason.TabIndex = 1;
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
            // Time
            // 
            this.Time.DataPropertyName = "sDISABLE_TIME";
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 210;
            // 
            // Remark
            // 
            this.Remark.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Remark.DataPropertyName = "Remark";
            this.Remark.HeaderText = "Remark";
            this.Remark.Name = "Remark";
            this.Remark.ReadOnly = true;
            this.Remark.Width = 95;
            // 
            // ShelfMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1214, 825);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "ShelfMaintenanceForm";
            this.Text = "ShelfMaintenanceForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShelfMaintenanceForm_FormClosed);
            this.Load += new System.EventHandler(this.ShelfMaintenanceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_shelfData)).EndInit();
            this.grb_Shelpanel.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_shelfData;
        private System.Windows.Forms.GroupBox grb_Shelpanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lbl_bay_id;
        private System.Windows.Forms.ComboBox cmb_bay_id;
        private System.Windows.Forms.Label lbl_zoneID;
        private System.Windows.Forms.ComboBox cmb_zoneID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_shelfID;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btn_enable;
        private System.Windows.Forms.Button btn_disable;
        private System.Windows.Forms.Button btn_EnableAll;
        private System.Windows.Forms.Button btn_DisableAll;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_reason;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShelfID;
        private System.Windows.Forms.DataGridViewTextBoxColumn CSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ZoneID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ADR_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Remark;
    }
}