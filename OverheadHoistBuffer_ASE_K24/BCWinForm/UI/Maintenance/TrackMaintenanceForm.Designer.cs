﻿namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class TrackMaintenanceForm
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
            this.dgv_trackData = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_track_id = new System.Windows.Forms.Label();
            this.txt_tracks = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TrackDir = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.last_updateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trackData)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_trackData
            // 
            this.dgv_trackData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_trackData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.TrackDir,
            this.last_updateTime,
            this.Column1});
            this.tableLayoutPanel3.SetColumnSpan(this.dgv_trackData, 4);
            this.dgv_trackData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_trackData.Location = new System.Drawing.Point(3, 45);
            this.dgv_trackData.MultiSelect = false;
            this.dgv_trackData.Name = "dgv_trackData";
            this.dgv_trackData.ReadOnly = true;
            this.dgv_trackData.RowTemplate.Height = 24;
            this.dgv_trackData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_trackData.Size = new System.Drawing.Size(517, 742);
            this.dgv_trackData.TabIndex = 0;
            this.dgv_trackData.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgv_shelfData_RowPrePaint);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 819);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Track";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.01721F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.01912F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.65009F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.12237F));
            this.tableLayoutPanel3.Controls.Add(this.dgv_trackData, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.lbl_track_id, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txt_tracks, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 26);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.335157F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.66484F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(523, 790);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // lbl_track_id
            // 
            this.lbl_track_id.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_track_id.AutoSize = true;
            this.lbl_track_id.Location = new System.Drawing.Point(46, 10);
            this.lbl_track_id.Name = "lbl_track_id";
            this.lbl_track_id.Size = new System.Drawing.Size(40, 22);
            this.lbl_track_id.TabIndex = 1;
            this.lbl_track_id.Text = "ID:";
            // 
            // txt_tracks
            // 
            this.txt_tracks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.SetColumnSpan(this.txt_tracks, 3);
            this.txt_tracks.Location = new System.Drawing.Point(92, 6);
            this.txt_tracks.Name = "txt_tracks";
            this.txt_tracks.Size = new System.Drawing.Size(428, 30);
            this.txt_tracks.TabIndex = 2;
            this.txt_tracks.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(535, 825);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ID
            // 
            this.ID.DataPropertyName = "UNIT_ID";
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            // 
            // TrackDir
            // 
            this.TrackDir.DataPropertyName = "TrackDir";
            this.TrackDir.HeaderText = "Dir.";
            this.TrackDir.Name = "TrackDir";
            this.TrackDir.ReadOnly = true;
            // 
            // last_updateTime
            // 
            this.last_updateTime.DataPropertyName = "LastUpdateTime";
            this.last_updateTime.HeaderText = "Last Update Time";
            this.last_updateTime.Name = "last_updateTime";
            this.last_updateTime.ReadOnly = true;
            this.last_updateTime.Width = 300;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // TrackMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 825);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "TrackMaintenanceForm";
            this.Text = "TrackMaintenanceForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TrackMaintenanceForm_FormClosed);
            this.Load += new System.EventHandler(this.TrackMaintenanceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trackData)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_trackData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lbl_track_id;
        private System.Windows.Forms.TextBox txt_tracks;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn TrackDir;
        private System.Windows.Forms.DataGridViewTextBoxColumn last_updateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}