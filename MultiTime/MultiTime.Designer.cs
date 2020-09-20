namespace MultiTime
{
    partial class MultiTime
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
            this.btnDone = new System.Windows.Forms.Button();
            this.labMsg = new System.Windows.Forms.Label();
            this.timUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.ottoTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(290, 32);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(75, 23);
            this.btnDone.TabIndex = 28;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // labMsg
            // 
            this.labMsg.AutoSize = true;
            this.labMsg.Location = new System.Drawing.Point(100, 37);
            this.labMsg.Name = "labMsg";
            this.labMsg.Size = new System.Drawing.Size(61, 13);
            this.labMsg.TabIndex = 29;
            this.labMsg.Text = "Checking...";
            // 
            // timUpdateUI
            // 
            this.timUpdateUI.Tick += new System.EventHandler(this.timUpdateUI_Tick);
            // 
            // ottoTimer
            // 
            this.ottoTimer.Interval = 1000;
            this.ottoTimer.Tick += new System.EventHandler(this.ottosTimer);
            // 
            // MultiTime
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(429, 59);
            this.Controls.Add(this.labMsg);
            this.Controls.Add(this.btnDone);
            this.Name = "MultiTime";
            this.Text = "MultiTime";
            this.Load += new System.EventHandler(this.MultiTime_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label labMsg;
        private System.Windows.Forms.Timer timUpdateUI;
        private System.Windows.Forms.Timer ottoTimer;
    }
}

