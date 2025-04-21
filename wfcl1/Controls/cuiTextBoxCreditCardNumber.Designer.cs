namespace CuoreUI.Controls
{
    partial class cuiTextBoxCreditCardNumber
    {
        /// <summary> 
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        /// <summary> 
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować 
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(cuiTextBoxCreditCardNumber));
            this.SuspendLayout();
            // 
            // contentTextField
            // 
            this.contentTextField.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.contentTextField.Location = new System.Drawing.Point(41, 15);
            this.contentTextField.Size = new System.Drawing.Size(184, 15);
            // 
            // cuiTextBoxCreditCardNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Image = ((System.Drawing.Image)(resources.GetObject("$this.Image")));
            this.ImageExpand = new System.Drawing.Point(2, 2);
            this.Name = "cuiTextBoxCreditCardNumber";
            this.Padding = new System.Windows.Forms.Padding(41, 15, 41, 0);
            this.PlaceholderColor = System.Drawing.Color.Gray;
            this.TextOffset = new System.Drawing.Size(26, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
