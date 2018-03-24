namespace GatherUp
{
    partial class HotSpotForm
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
            this.chkboxStealth = new System.Windows.Forms.CheckBox();
            this.chkboxDisableMount = new System.Windows.Forms.CheckBox();
            this.listboxFlyingDest = new System.Windows.Forms.ListBox();
            this.btnAddFlyingDest = new System.Windows.Forms.Button();
            this.chkboxLand = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkboxStealth
            // 
            this.chkboxStealth.AutoSize = true;
            this.chkboxStealth.Location = new System.Drawing.Point(12, 12);
            this.chkboxStealth.Name = "chkboxStealth";
            this.chkboxStealth.Size = new System.Drawing.Size(59, 17);
            this.chkboxStealth.TabIndex = 0;
            this.chkboxStealth.Text = "Stealth";
            this.chkboxStealth.UseVisualStyleBackColor = true;
            this.chkboxStealth.CheckedChanged += new System.EventHandler(this.chkboxStealth_CheckedChanged);
            // 
            // chkboxDisableMount
            // 
            this.chkboxDisableMount.AutoSize = true;
            this.chkboxDisableMount.Location = new System.Drawing.Point(101, 12);
            this.chkboxDisableMount.Name = "chkboxDisableMount";
            this.chkboxDisableMount.Size = new System.Drawing.Size(93, 17);
            this.chkboxDisableMount.TabIndex = 1;
            this.chkboxDisableMount.Text = "Disable mount";
            this.chkboxDisableMount.UseVisualStyleBackColor = true;
            this.chkboxDisableMount.CheckedChanged += new System.EventHandler(this.chkboxDisableMount_CheckedChanged);
            // 
            // listboxFlyingDest
            // 
            this.listboxFlyingDest.FormattingEnabled = true;
            this.listboxFlyingDest.Location = new System.Drawing.Point(12, 63);
            this.listboxFlyingDest.Name = "listboxFlyingDest";
            this.listboxFlyingDest.Size = new System.Drawing.Size(212, 95);
            this.listboxFlyingDest.TabIndex = 4;
            this.listboxFlyingDest.SelectedValueChanged += new System.EventHandler(this.listboxFlyingDest_SelectedValueChanged);
            this.listboxFlyingDest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listboxFlyingDest_KeyDown);
            // 
            // btnAddFlyingDest
            // 
            this.btnAddFlyingDest.Location = new System.Drawing.Point(101, 35);
            this.btnAddFlyingDest.Name = "btnAddFlyingDest";
            this.btnAddFlyingDest.Size = new System.Drawing.Size(123, 23);
            this.btnAddFlyingDest.TabIndex = 5;
            this.btnAddFlyingDest.Text = "Add flying destination";
            this.btnAddFlyingDest.UseVisualStyleBackColor = true;
            this.btnAddFlyingDest.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkboxLand
            // 
            this.chkboxLand.AutoSize = true;
            this.chkboxLand.Location = new System.Drawing.Point(12, 35);
            this.chkboxLand.Name = "chkboxLand";
            this.chkboxLand.Size = new System.Drawing.Size(50, 17);
            this.chkboxLand.TabIndex = 7;
            this.chkboxLand.Text = "Land";
            this.chkboxLand.UseVisualStyleBackColor = true;
            this.chkboxLand.CheckedChanged += new System.EventHandler(this.chkboxLand_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(200, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "?";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // HotSpotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 169);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkboxLand);
            this.Controls.Add(this.btnAddFlyingDest);
            this.Controls.Add(this.listboxFlyingDest);
            this.Controls.Add(this.chkboxDisableMount);
            this.Controls.Add(this.chkboxStealth);
            this.Name = "HotSpotForm";
            this.Text = "HotSpotForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HotSpotForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkboxStealth;
        private System.Windows.Forms.CheckBox chkboxDisableMount;
        private System.Windows.Forms.ListBox listboxFlyingDest;
        private System.Windows.Forms.Button btnAddFlyingDest;
        private System.Windows.Forms.CheckBox chkboxLand;
        private System.Windows.Forms.Label label1;
    }
}