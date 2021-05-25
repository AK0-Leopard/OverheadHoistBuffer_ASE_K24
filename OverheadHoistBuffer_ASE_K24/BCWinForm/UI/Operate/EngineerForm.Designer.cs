namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class EngineerForm
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
            this.lbl_fromAdr = new System.Windows.Forms.Label();
            this.cmb_fromAdr = new System.Windows.Forms.ComboBox();
            this.lbl_toAdr = new System.Windows.Forms.Label();
            this.cmb_toAdr = new System.Windows.Forms.ComboBox();
            this.txt_Route = new System.Windows.Forms.TextBox();
            this.btn_StartSec = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_startAdr = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lbl_fromAdr
            // 
            this.lbl_fromAdr.AutoSize = true;
            this.lbl_fromAdr.Location = new System.Drawing.Point(177, 1);
            this.lbl_fromAdr.Name = "lbl_fromAdr";
            this.lbl_fromAdr.Size = new System.Drawing.Size(54, 12);
            this.lbl_fromAdr.TabIndex = 0;
            this.lbl_fromAdr.Text = "Form Adr:";
            // 
            // cmb_fromAdr
            // 
            this.cmb_fromAdr.FormattingEnabled = true;
            this.cmb_fromAdr.Location = new System.Drawing.Point(179, 16);
            this.cmb_fromAdr.Name = "cmb_fromAdr";
            this.cmb_fromAdr.Size = new System.Drawing.Size(121, 20);
            this.cmb_fromAdr.TabIndex = 1;
            // 
            // lbl_toAdr
            // 
            this.lbl_toAdr.AutoSize = true;
            this.lbl_toAdr.Location = new System.Drawing.Point(334, 0);
            this.lbl_toAdr.Name = "lbl_toAdr";
            this.lbl_toAdr.Size = new System.Drawing.Size(42, 12);
            this.lbl_toAdr.TabIndex = 0;
            this.lbl_toAdr.Text = "To Adr:";
            // 
            // cmb_toAdr
            // 
            this.cmb_toAdr.FormattingEnabled = true;
            this.cmb_toAdr.Location = new System.Drawing.Point(336, 15);
            this.cmb_toAdr.Name = "cmb_toAdr";
            this.cmb_toAdr.Size = new System.Drawing.Size(121, 20);
            this.cmb_toAdr.TabIndex = 1;
            // 
            // txt_Route
            // 
            this.txt_Route.Location = new System.Drawing.Point(16, 59);
            this.txt_Route.Multiline = true;
            this.txt_Route.Name = "txt_Route";
            this.txt_Route.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.txt_Route.Size = new System.Drawing.Size(624, 125);
            this.txt_Route.TabIndex = 3;
            // 
            // btn_StartSec
            // 
            this.btn_StartSec.Location = new System.Drawing.Point(17, 205);
            this.btn_StartSec.Name = "btn_StartSec";
            this.btn_StartSec.Size = new System.Drawing.Size(98, 30);
            this.btn_StartSec.TabIndex = 2;
            this.btn_StartSec.Text = "Adr To Adr";
            this.btn_StartSec.UseVisualStyleBackColor = true;
            this.btn_StartSec.Click += new System.EventHandler(this.btn_StartSec_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Adr:";
            // 
            // cmb_startAdr
            // 
            this.cmb_startAdr.FormattingEnabled = true;
            this.cmb_startAdr.Location = new System.Drawing.Point(17, 16);
            this.cmb_startAdr.Name = "cmb_startAdr";
            this.cmb_startAdr.Size = new System.Drawing.Size(121, 20);
            this.cmb_startAdr.TabIndex = 1;
            // 
            // EngineerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 261);
            this.Controls.Add(this.txt_Route);
            this.Controls.Add(this.btn_StartSec);
            this.Controls.Add(this.cmb_toAdr);
            this.Controls.Add(this.cmb_startAdr);
            this.Controls.Add(this.cmb_fromAdr);
            this.Controls.Add(this.lbl_toAdr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_fromAdr);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EngineerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmRouteSearchTool";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EngineerForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_fromAdr;
        private System.Windows.Forms.ComboBox cmb_fromAdr;
        private System.Windows.Forms.Label lbl_toAdr;
        private System.Windows.Forms.ComboBox cmb_toAdr;
        private System.Windows.Forms.TextBox txt_Route;
        private System.Windows.Forms.Button btn_StartSec;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_startAdr;
    }
}