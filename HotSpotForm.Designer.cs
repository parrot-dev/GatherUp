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
            this.radiusNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.listboxFlyingDest = new System.Windows.Forms.ListBox();
            this.btnAddFlyingDest = new System.Windows.Forms.Button();
            this.lblRadius = new System.Windows.Forms.Label();
            this.chkboxDisableMount = new System.Windows.Forms.CheckBox();
            this.chkboxLand = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.radiusNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // radiusNumericUpDown
            // 
            this.radiusNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radiusNumericUpDown.Location = new System.Drawing.Point(98, 7);
            this.radiusNumericUpDown.Name = "radiusNumericUpDown";
            this.radiusNumericUpDown.Size = new System.Drawing.Size(93, 20);
            this.radiusNumericUpDown.TabIndex = 15;
            this.radiusNumericUpDown.ValueChanged += new System.EventHandler(this.radiusNumericUpDown_ValueChanged_1);
            // 
            // listboxFlyingDest
            // 
            this.listboxFlyingDest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listboxFlyingDest.FormattingEnabled = true;
            this.listboxFlyingDest.Location = new System.Drawing.Point(8, 53);
            this.listboxFlyingDest.Name = "listboxFlyingDest";
            this.listboxFlyingDest.Size = new System.Drawing.Size(183, 108);
            this.listboxFlyingDest.TabIndex = 11;
            this.listboxFlyingDest.SelectedValueChanged += new System.EventHandler(this.listboxFlyingDest_SelectedValueChanged_1);
            this.listboxFlyingDest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listboxFlyingDest_KeyDown_1);
            // 
            // btnAddFlyingDest
            // 
            this.btnAddFlyingDest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFlyingDest.Location = new System.Drawing.Point(8, 167);
            this.btnAddFlyingDest.Name = "btnAddFlyingDest";
            this.btnAddFlyingDest.Size = new System.Drawing.Size(183, 23);
            this.btnAddFlyingDest.TabIndex = 12;
            this.btnAddFlyingDest.Text = "Add flying destination";
            this.btnAddFlyingDest.UseVisualStyleBackColor = true;
            this.btnAddFlyingDest.Click += new System.EventHandler(this.btnAddFlyingDest_Click);
            // 
            // lblRadius
            // 
            this.lblRadius.AutoSize = true;
            this.lblRadius.Location = new System.Drawing.Point(8, 9);
            this.lblRadius.Name = "lblRadius";
            this.lblRadius.Size = new System.Drawing.Size(43, 13);
            this.lblRadius.TabIndex = 14;
            this.lblRadius.Text = "Radius:";
            // 
            // chkboxDisableMount
            // 
            this.chkboxDisableMount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkboxDisableMount.AutoSize = true;
            this.chkboxDisableMount.Location = new System.Drawing.Point(99, 34);
            this.chkboxDisableMount.Name = "chkboxDisableMount";
            this.chkboxDisableMount.Size = new System.Drawing.Size(93, 17);
            this.chkboxDisableMount.TabIndex = 10;
            this.chkboxDisableMount.Text = "Disable mount";
            this.chkboxDisableMount.UseVisualStyleBackColor = true;
            this.chkboxDisableMount.CheckedChanged += new System.EventHandler(this.chkboxDisableMount_CheckedChanged_1);
            // 
            // chkboxLand
            // 
            this.chkboxLand.AutoSize = true;
            this.chkboxLand.Location = new System.Drawing.Point(8, 34);
            this.chkboxLand.Name = "chkboxLand";
            this.chkboxLand.Size = new System.Drawing.Size(50, 17);
            this.chkboxLand.TabIndex = 13;
            this.chkboxLand.Text = "Land";
            this.chkboxLand.UseVisualStyleBackColor = true;
            this.chkboxLand.CheckedChanged += new System.EventHandler(this.chkboxLand_CheckedChanged_1);
            // 
            // HotSpotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 200);
            this.Controls.Add(this.radiusNumericUpDown);
            this.Controls.Add(this.listboxFlyingDest);
            this.Controls.Add(this.btnAddFlyingDest);
            this.Controls.Add(this.lblRadius);
            this.Controls.Add(this.chkboxDisableMount);
            this.Controls.Add(this.chkboxLand);
            this.Name = "HotSpotForm";
            this.Text = "Edit";
            ((System.ComponentModel.ISupportInitialize)(this.radiusNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown radiusNumericUpDown;
        private System.Windows.Forms.ListBox listboxFlyingDest;
        private System.Windows.Forms.Button btnAddFlyingDest;
        private System.Windows.Forms.Label lblRadius;
        private System.Windows.Forms.CheckBox chkboxDisableMount;
        private System.Windows.Forms.CheckBox chkboxLand;
    }
}