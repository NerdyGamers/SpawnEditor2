using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SpawnEditor
{
	/// <summary>
	/// Summary description for Area.
	/// </summary>
	public class Area : System.Windows.Forms.Form
	{
        private SpawnEditor _Editor;
        private SpawnPoint _Spawn;
        private bool _IsConstructed;
        private System.Windows.Forms.NumericUpDown spnX;
        private System.Windows.Forms.NumericUpDown spnY;
        private System.Windows.Forms.NumericUpDown spnWidth;
        private System.Windows.Forms.NumericUpDown spnHeight;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Area( SpawnPoint Spawn, SpawnEditor Editor )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            this._Spawn = Spawn;
            this._Editor = Editor;

            int LeftSide = this._Editor.grpSpawnEdit.Left + this._Editor.grpSpawnEdit.Parent.Left + this._Editor.Left;
            int TopSide = this._Editor.grpSpawnEdit.Top + this._Editor.grpSpawnEdit.Parent.Top + this._Editor.btnUpdateSpawn.Top + this._Editor.Top;
            
            this.Left = LeftSide;
            this.Top = TopSide;

            this.spnX.Value = this._Spawn.Bounds.X;
            this.spnY.Value = this._Spawn.Bounds.Y;
            this.spnWidth.Value = this._Spawn.Bounds.Width;
            this.spnHeight.Value = this._Spawn.Bounds.Height;
            this._IsConstructed = true;
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
            this.spnX = new System.Windows.Forms.NumericUpDown();
            this.spnY = new System.Windows.Forms.NumericUpDown();
            this.spnWidth = new System.Windows.Forms.NumericUpDown();
            this.spnHeight = new System.Windows.Forms.NumericUpDown();
            this.lblY = new System.Windows.Forms.Label();
            this.lblX = new System.Windows.Forms.Label();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spnX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // spnX
            // 
            this.spnX.Location = new System.Drawing.Point(8, 64);
            this.spnX.Maximum = new System.Decimal(new int[] {
                                                                 10000,
                                                                 0,
                                                                 0,
                                                                 0});
            this.spnX.Name = "spnX";
            this.spnX.Size = new System.Drawing.Size(64, 20);
            this.spnX.TabIndex = 1;
            this.spnX.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            this.spnX.ValueChanged += new System.EventHandler(this.SpinBox_ValueChanged);
            // 
            // spnY
            // 
            this.spnY.Location = new System.Drawing.Point(48, 24);
            this.spnY.Maximum = new System.Decimal(new int[] {
                                                                 10000,
                                                                 0,
                                                                 0,
                                                                 0});
            this.spnY.Name = "spnY";
            this.spnY.Size = new System.Drawing.Size(64, 20);
            this.spnY.TabIndex = 3;
            this.spnY.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            this.spnY.ValueChanged += new System.EventHandler(this.SpinBox_ValueChanged);
            // 
            // spnWidth
            // 
            this.spnWidth.Location = new System.Drawing.Point(88, 64);
            this.spnWidth.Maximum = new System.Decimal(new int[] {
                                                                     10000,
                                                                     0,
                                                                     0,
                                                                     0});
            this.spnWidth.Name = "spnWidth";
            this.spnWidth.Size = new System.Drawing.Size(64, 20);
            this.spnWidth.TabIndex = 5;
            this.spnWidth.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            this.spnWidth.ValueChanged += new System.EventHandler(this.SpinBox_ValueChanged);
            // 
            // spnHeight
            // 
            this.spnHeight.Location = new System.Drawing.Point(48, 104);
            this.spnHeight.Maximum = new System.Decimal(new int[] {
                                                                      10000,
                                                                      0,
                                                                      0,
                                                                      0});
            this.spnHeight.Name = "spnHeight";
            this.spnHeight.Size = new System.Drawing.Size(64, 20);
            this.spnHeight.TabIndex = 7;
            this.spnHeight.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            this.spnHeight.ValueChanged += new System.EventHandler(this.SpinBox_ValueChanged);
            // 
            // lblY
            // 
            this.lblY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.lblY.Location = new System.Drawing.Point(48, 8);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(64, 16);
            this.lblY.TabIndex = 2;
            this.lblY.Text = "Y";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblX
            // 
            this.lblX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.lblX.Location = new System.Drawing.Point(8, 48);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(64, 16);
            this.lblX.TabIndex = 0;
            this.lblX.Text = "X";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWidth
            // 
            this.lblWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.lblWidth.Location = new System.Drawing.Point(88, 48);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(64, 16);
            this.lblWidth.TabIndex = 4;
            this.lblWidth.Text = "Width";
            this.lblWidth.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblHeight
            // 
            this.lblHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.lblHeight.Location = new System.Drawing.Point(48, 88);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(64, 16);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "Height";
            this.lblHeight.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(176, 128);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(8, 8);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Cancel";
            // 
            // Area
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(160, 141);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.lblHeight,
                                                                          this.lblWidth,
                                                                          this.lblX,
                                                                          this.lblY,
                                                                          this.spnHeight,
                                                                          this.spnWidth,
                                                                          this.spnY,
                                                                          this.spnX,
                                                                          this.btnCancel});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "Area";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Bounds";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Area_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.spnX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHeight)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion
	
        private void TextEntryControl_Enter(object sender, System.EventArgs e)
        {
            if( sender is TextBox )
            {
                TextBox TheControl = (TextBox)sender;
                TheControl.Select( 0, TheControl.MaxLength );
            }
            else if( sender is NumericUpDown )
            {
                NumericUpDown TheControl = (NumericUpDown)sender;
                TheControl.Select( 0, int.MaxValue );
            }
        }

        private void SpinBox_ValueChanged(object sender, System.EventArgs e)
        {
            // Only do the following code if the form is completly constructed
            if( this._IsConstructed == true )
            {
                // Handle the value changing in the spin control
                NumericUpDown SpinBox = sender as NumericUpDown;

                if( SpinBox != null )
                {
                    if( SpinBox == this.spnX )
                    {
                        this._Spawn.Bounds = new Rectangle( (int)this.spnX.Value, this._Spawn.Bounds.Y, this._Spawn.Bounds.Width, this._Spawn.Bounds.Height );
                    }
                    else if( SpinBox == this.spnY )
                    {
                        this._Spawn.Bounds = new Rectangle( this._Spawn.Bounds.X, (int)this.spnY.Value, this._Spawn.Bounds.Width, this._Spawn.Bounds.Height );
                    }
                    else if( SpinBox == this.spnWidth )
                    {
                        this._Spawn.Bounds = new Rectangle( this._Spawn.Bounds.X, this._Spawn.Bounds.Y, (int)this.spnWidth.Value, this._Spawn.Bounds.Height );
                    }
                    else if( SpinBox == this.spnHeight )
                    {
                        this._Spawn.Bounds = new Rectangle( this._Spawn.Bounds.X, this._Spawn.Bounds.Y, this._Spawn.Bounds.Width, (int)this.spnHeight.Value );
                    }

                    // Refresh the spawn points 
                    this._Editor.RefreshSpawnPoints();
                }
            }
        }

        private void Area_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Handle the arrow keys to resize the area
            // Check if the shift key is pressed (turns on higher increment)
            int Increment = 1;

            if( e.Shift == true )
                Increment = 5;

            if( e.KeyCode == Keys.Down )
            {
                if( e.Control == true )
                    this.spnHeight.Value += Increment;
                else
                    this.spnY.Value += Increment;

                e.Handled = true;
            }
            else if( e.KeyCode == Keys.Up )
            {
                if( e.Control == true )
                    this.spnHeight.Value -= Increment;
                else
                    this.spnY.Value -= Increment;

                e.Handled = true;
            }
            else if( e.KeyCode == Keys.Left )
            {
                if( e.Control == true )
                    this.spnWidth.Value -= Increment;
                else
                    this.spnX.Value -= Increment;

                e.Handled = true;
            }
            else if( e.KeyCode == Keys.Right )
            {
                if( e.Control == true )
                    this.spnWidth.Value += Increment;
                else
                    this.spnX.Value += Increment;

                e.Handled = true;
            }
        }
    }
}
