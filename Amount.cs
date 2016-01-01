using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SpawnEditor
{
	/// <summary>
	/// Summary description for Amount.
	/// </summary>
	public class Amount : System.Windows.Forms.Form
	{
        public int SpawnAmount
        {
            get{ return (int)this.spnSpawnAmount.Value; }
        }

        public string SpawnName
        {
            get{ return this.txtSpawnObject.Text; }
        }

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtSpawnObject;
        private System.Windows.Forms.NumericUpDown spnSpawnAmount;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Amount( string Name, int Amount )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            this.txtSpawnObject.Text = Name;
            this.spnSpawnAmount.Value = Amount;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.txtSpawnObject = new System.Windows.Forms.TextBox();
            this.spnSpawnAmount = new System.Windows.Forms.NumericUpDown();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnAmount)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSpawnObject
            // 
            this.txtSpawnObject.Location = new System.Drawing.Point(3, 8);
            this.txtSpawnObject.Name = "txtSpawnObject";
            this.txtSpawnObject.ReadOnly = true;
            this.txtSpawnObject.Size = new System.Drawing.Size(208, 20);
            this.txtSpawnObject.TabIndex = 0;
            this.txtSpawnObject.TabStop = false;
            this.txtSpawnObject.Text = "";
            // 
            // spnSpawnAmount
            // 
            this.spnSpawnAmount.Location = new System.Drawing.Point(213, 8);
            this.spnSpawnAmount.Name = "spnSpawnAmount";
            this.spnSpawnAmount.Size = new System.Drawing.Size(75, 20);
            this.spnSpawnAmount.TabIndex = 1;
            this.spnSpawnAmount.Enter += new System.EventHandler(this.spnSpawnAmount_Enter);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(136, 32);
            this.btnOk.Name = "btnOk";
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "&Ok";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(213, 32);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            // 
            // Amount
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(292, 61);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.btnCancel,
                                                                          this.btnOk,
                                                                          this.spnSpawnAmount,
                                                                          this.txtSpawnObject});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Amount";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Amount";
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnAmount)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void spnSpawnAmount_Enter(object sender, System.EventArgs e)
        {
            this.spnSpawnAmount.Select( 0, int.MaxValue );
        }

	}
}
