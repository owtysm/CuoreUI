namespace CuoreUI.Controls
{
    partial class cuiTabControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.scrollbarTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // scrollbarTimer
            // 
            this.scrollbarTimer.Enabled = true;
            this.scrollbarTimer.Interval = 32;
            this.scrollbarTimer.Tick += new System.EventHandler(this.scrollbarTimer_Tick);
            // 
            // cuiTabControl
            // 
            this.Name = "cuiTabControl";
            this.Size = new System.Drawing.Size(500, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer scrollbarTimer;
    }
}
