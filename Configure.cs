using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace SpawnEditor
{
	/// <summary>
	/// Summary description for Configure.
	/// </summary>
	public class Configure : System.Windows.Forms.Form
	{
        private readonly string UOTDRegistryKey = @"Software\Origin Worlds Online\Ultima Online Third Dawn\1.0";
        private readonly string T2ARegistryKey = @"Software\Origin Worlds Online\Ultima Online\1.0";
        private readonly string UOExePathValue = "ExePath";

        private readonly string AppRegistryKey = @"Software\Spawn Editor";
        private readonly string AppRunUoPathValue = "RunUO Exe Path";
        private readonly string AppUoClientPathValue = "Ultima Client Exe Path";
        private readonly string AppZoomLevelValue = "Zoom Level";
        private readonly string AppRunUoCmdPrefixValue = "RunUO Cmd Prefix";
        private readonly string AppUoClientWindowValue = "Ultima Client Window";
        private readonly string AppSpawnNameValue = "Spawn Name";
        private readonly string AppSpawnHomeRangeValue = "Spawn Home Range";
        private readonly string AppSpawnMaxCountValue = "Spawn Max Count";
        private readonly string AppSpawnMinDelayValue = "Spawn Min Delay";
        private readonly string AppSpawnMaxDelayValue = "Spawn Max Delay";
        private readonly string AppSpawnTeamValue = "Spawn Team";
        private readonly string AppSpawnGroupValue = "Spawn Group";
        private readonly string AppSpawnRunningValue = "Spawn Running";
        private readonly string AppSpawnRelativeHomeValue = "Spawn Relative Home";

        public string CfgRunUoPathValue;
        public string CfgUoClientPathValue;
        public string CfgUoClientWindowValue = "Ultima Online Third Dawn";
        public short CfgZoomLevelValue = -4;
        public string CfgRunUoCmdPrefix = "[";
        public string CfgSpawnNameValue = "Spawn";
        public int CfgSpawnHomeRangeValue = 10;
        public int CfgSpawnMaxCountValue = 1;
        public int CfgSpawnMinDelayValue = 5;
        public int CfgSpawnMaxDelayValue = 10;
        public int CfgSpawnTeamValue = 0;
        public bool CfgSpawnGroupValue = false;
        public bool CfgSpawnRunningValue = true;
        public bool CfgSpawnRelativeHomeValue = true;

        private bool _IsValidConfiguration = false;
        RegistryKey _HKLMKey = null;
        RegistryKey _HKCUKey = null;

        private System.Windows.Forms.OpenFileDialog ofdOpenFile;
        private System.Windows.Forms.TextBox txtRunUOExe;
        private System.Windows.Forms.Button btnRunUOExe;
        private System.Windows.Forms.Label lblRunUOExe;
        private System.Windows.Forms.TextBox txtUltimaClient;
        private System.Windows.Forms.Label lblUltimaClient;
        private System.Windows.Forms.Button btnUltimaClient;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trkZoom;
        private System.Windows.Forms.GroupBox grpSpawnEdit;
        private System.Windows.Forms.Label lblMaxDelay;
        private System.Windows.Forms.Label lblHomeRange;
        private System.Windows.Forms.Label lblTeam;
        private System.Windows.Forms.Label lblMaxCount;
        private System.Windows.Forms.Label lblMinDelay;
        private System.Windows.Forms.CheckBox chkSpawnRunning;
        private System.Windows.Forms.NumericUpDown spnSpawnMaxCount;
        private System.Windows.Forms.TextBox txtSpawnName;
        private System.Windows.Forms.NumericUpDown spnSpawnRange;
        private System.Windows.Forms.NumericUpDown spnSpawnMinDelay;
        private System.Windows.Forms.NumericUpDown spnSpawnTeam;
        private System.Windows.Forms.CheckBox chkSpawnGroup;
        private System.Windows.Forms.NumericUpDown spnSpawnMaxDelay;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkHomeRangeIsRelative;
        private System.Windows.Forms.TextBox txtCmdPrefix;
        private System.Windows.Forms.Label lblCmdPrefix;
        private System.Windows.Forms.Label lblClientWindow;
        private System.Windows.Forms.TextBox txtClientWindow;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Configure()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            // Check if the spawn editor has ever run on this machine
            // Look for the spawn editor registry value
            this._HKCUKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( this.AppRegistryKey, true );

            // Check if the key was found and that there are values in the key
            if( ( this._HKCUKey != null ) && ( this._HKCUKey.ValueCount == 14 ) )
            {
                // Load all of the registry values required
                this.CfgRunUoPathValue = (string)this._HKCUKey.GetValue( this.AppRunUoPathValue, string.Empty );
                this.CfgUoClientPathValue = (string)this._HKCUKey.GetValue( this.AppUoClientPathValue, string.Empty );
                this.CfgUoClientWindowValue = (string)this._HKCUKey.GetValue( this.AppUoClientWindowValue, "Ultima Online Third Dawn" );
                this.CfgZoomLevelValue = short.Parse( this._HKCUKey.GetValue( this.AppZoomLevelValue, "-4" ) as string );
                this.CfgRunUoCmdPrefix = (string)this._HKCUKey.GetValue( this.AppRunUoCmdPrefixValue, "[" );
                this.CfgSpawnNameValue = (string)this._HKCUKey.GetValue( this.AppSpawnNameValue, "Spawner" );
                this.CfgSpawnHomeRangeValue = (int)this._HKCUKey.GetValue( this.AppSpawnHomeRangeValue, 5 );
                this.CfgSpawnMaxCountValue = (int)this._HKCUKey.GetValue( this.AppSpawnMaxCountValue, 1 );
                this.CfgSpawnMinDelayValue = (int)this._HKCUKey.GetValue( this.AppSpawnMinDelayValue, 5 );
                this.CfgSpawnMaxDelayValue = (int)this._HKCUKey.GetValue( this.AppSpawnMaxDelayValue, 10 );
                this.CfgSpawnTeamValue = (int)this._HKCUKey.GetValue( this.AppSpawnTeamValue, 0 );
                this.CfgSpawnGroupValue = bool.Parse( this._HKCUKey.GetValue( this.AppSpawnGroupValue, bool.FalseString ) as string );
                this.CfgSpawnRunningValue = bool.Parse( this._HKCUKey.GetValue( this.AppSpawnRunningValue, bool.TrueString ) as string );
                this.CfgSpawnRelativeHomeValue = bool.Parse( this._HKCUKey.GetValue( this.AppSpawnRelativeHomeValue, bool.TrueString ) as string );
                
                // Verify that the RunUO path is valid and that the UO client path is still valid
                // If either of the file do not exist anymore, return false
                if( ( File.Exists( this.CfgRunUoPathValue ) == true ) &&
                    ( this.CfgUoClientPathValue.Length > 0 ) )
                {
                    // If everything is good, set that the confuration is valid
                    this._IsValidConfiguration = true;
                }
            }
		}

        public bool IsValidConfiguration
        {
            get{ return this._IsValidConfiguration; }
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
            this.ofdOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.txtRunUOExe = new System.Windows.Forms.TextBox();
            this.btnRunUOExe = new System.Windows.Forms.Button();
            this.lblRunUOExe = new System.Windows.Forms.Label();
            this.txtUltimaClient = new System.Windows.Forms.TextBox();
            this.lblUltimaClient = new System.Windows.Forms.Label();
            this.btnUltimaClient = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trkZoom = new System.Windows.Forms.TrackBar();
            this.grpSpawnEdit = new System.Windows.Forms.GroupBox();
            this.chkHomeRangeIsRelative = new System.Windows.Forms.CheckBox();
            this.lblMaxDelay = new System.Windows.Forms.Label();
            this.chkSpawnRunning = new System.Windows.Forms.CheckBox();
            this.lblHomeRange = new System.Windows.Forms.Label();
            this.spnSpawnMaxCount = new System.Windows.Forms.NumericUpDown();
            this.txtSpawnName = new System.Windows.Forms.TextBox();
            this.spnSpawnRange = new System.Windows.Forms.NumericUpDown();
            this.lblTeam = new System.Windows.Forms.Label();
            this.lblMaxCount = new System.Windows.Forms.Label();
            this.spnSpawnMinDelay = new System.Windows.Forms.NumericUpDown();
            this.spnSpawnTeam = new System.Windows.Forms.NumericUpDown();
            this.chkSpawnGroup = new System.Windows.Forms.CheckBox();
            this.spnSpawnMaxDelay = new System.Windows.Forms.NumericUpDown();
            this.lblMinDelay = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtCmdPrefix = new System.Windows.Forms.TextBox();
            this.lblCmdPrefix = new System.Windows.Forms.Label();
            this.lblClientWindow = new System.Windows.Forms.Label();
            this.txtClientWindow = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).BeginInit();
            this.grpSpawnEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMaxCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMinDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnTeam)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMaxDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // ofdOpenFile
            // 
            this.ofdOpenFile.DefaultExt = "exe";
            this.ofdOpenFile.Filter = "Executable (*.exe)|*.exe|All Files (*.*)|*.*";
            this.ofdOpenFile.ReadOnlyChecked = true;
            // 
            // txtRunUOExe
            // 
            this.txtRunUOExe.Location = new System.Drawing.Point(80, 8);
            this.txtRunUOExe.Name = "txtRunUOExe";
            this.txtRunUOExe.ReadOnly = true;
            this.txtRunUOExe.Size = new System.Drawing.Size(184, 20);
            this.txtRunUOExe.TabIndex = 1;
            this.txtRunUOExe.Text = "";
            // 
            // btnRunUOExe
            // 
            this.btnRunUOExe.Location = new System.Drawing.Point(264, 8);
            this.btnRunUOExe.Name = "btnRunUOExe";
            this.btnRunUOExe.Size = new System.Drawing.Size(24, 20);
            this.btnRunUOExe.TabIndex = 2;
            this.btnRunUOExe.Text = "...";
            this.btnRunUOExe.Click += new System.EventHandler(this.btnRunUOExe_Click);
            // 
            // lblRunUOExe
            // 
            this.lblRunUOExe.Location = new System.Drawing.Point(8, 8);
            this.lblRunUOExe.Name = "lblRunUOExe";
            this.lblRunUOExe.Size = new System.Drawing.Size(80, 20);
            this.lblRunUOExe.TabIndex = 0;
            this.lblRunUOExe.Text = "RunUO.EXE:";
            this.lblRunUOExe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUltimaClient
            // 
            this.txtUltimaClient.Location = new System.Drawing.Point(80, 32);
            this.txtUltimaClient.Name = "txtUltimaClient";
            this.txtUltimaClient.ReadOnly = true;
            this.txtUltimaClient.Size = new System.Drawing.Size(184, 20);
            this.txtUltimaClient.TabIndex = 4;
            this.txtUltimaClient.Text = "";
            // 
            // lblUltimaClient
            // 
            this.lblUltimaClient.Location = new System.Drawing.Point(8, 32);
            this.lblUltimaClient.Name = "lblUltimaClient";
            this.lblUltimaClient.Size = new System.Drawing.Size(80, 20);
            this.lblUltimaClient.TabIndex = 3;
            this.lblUltimaClient.Text = "Ultima Client:";
            this.lblUltimaClient.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnUltimaClient
            // 
            this.btnUltimaClient.Location = new System.Drawing.Point(264, 32);
            this.btnUltimaClient.Name = "btnUltimaClient";
            this.btnUltimaClient.Size = new System.Drawing.Size(24, 20);
            this.btnUltimaClient.TabIndex = 5;
            this.btnUltimaClient.Text = "...";
            this.btnUltimaClient.Click += new System.EventHandler(this.btnUltimaClient_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Default Zoom Level:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trkZoom
            // 
            this.trkZoom.LargeChange = 2;
            this.trkZoom.Location = new System.Drawing.Point(112, 56);
            this.trkZoom.Maximum = 4;
            this.trkZoom.Minimum = -4;
            this.trkZoom.Name = "trkZoom";
            this.trkZoom.Size = new System.Drawing.Size(146, 42);
            this.trkZoom.TabIndex = 7;
            this.trkZoom.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // grpSpawnEdit
            // 
            this.grpSpawnEdit.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                       this.chkHomeRangeIsRelative,
                                                                                       this.lblMaxDelay,
                                                                                       this.chkSpawnRunning,
                                                                                       this.lblHomeRange,
                                                                                       this.spnSpawnMaxCount,
                                                                                       this.txtSpawnName,
                                                                                       this.spnSpawnRange,
                                                                                       this.lblTeam,
                                                                                       this.lblMaxCount,
                                                                                       this.spnSpawnMinDelay,
                                                                                       this.spnSpawnTeam,
                                                                                       this.chkSpawnGroup,
                                                                                       this.spnSpawnMaxDelay,
                                                                                       this.lblMinDelay});
            this.grpSpawnEdit.Location = new System.Drawing.Point(112, 104);
            this.grpSpawnEdit.Name = "grpSpawnEdit";
            this.grpSpawnEdit.Size = new System.Drawing.Size(152, 200);
            this.grpSpawnEdit.TabIndex = 10;
            this.grpSpawnEdit.TabStop = false;
            this.grpSpawnEdit.Text = "Default Spawn Details";
            // 
            // chkHomeRangeIsRelative
            // 
            this.chkHomeRangeIsRelative.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkHomeRangeIsRelative.Checked = true;
            this.chkHomeRangeIsRelative.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHomeRangeIsRelative.Location = new System.Drawing.Point(8, 176);
            this.chkHomeRangeIsRelative.Name = "chkHomeRangeIsRelative";
            this.chkHomeRangeIsRelative.Size = new System.Drawing.Size(102, 16);
            this.chkHomeRangeIsRelative.TabIndex = 13;
            this.chkHomeRangeIsRelative.Text = "Relative Home:";
            // 
            // lblMaxDelay
            // 
            this.lblMaxDelay.Location = new System.Drawing.Point(8, 104);
            this.lblMaxDelay.Name = "lblMaxDelay";
            this.lblMaxDelay.Size = new System.Drawing.Size(80, 16);
            this.lblMaxDelay.TabIndex = 7;
            this.lblMaxDelay.Text = "Max Delay (m)";
            // 
            // chkSpawnRunning
            // 
            this.chkSpawnRunning.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSpawnRunning.Checked = true;
            this.chkSpawnRunning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSpawnRunning.Location = new System.Drawing.Point(8, 160);
            this.chkSpawnRunning.Name = "chkSpawnRunning";
            this.chkSpawnRunning.Size = new System.Drawing.Size(102, 16);
            this.chkSpawnRunning.TabIndex = 12;
            this.chkSpawnRunning.Text = "Running:";
            // 
            // lblHomeRange
            // 
            this.lblHomeRange.Location = new System.Drawing.Point(8, 44);
            this.lblHomeRange.Name = "lblHomeRange";
            this.lblHomeRange.Size = new System.Drawing.Size(80, 16);
            this.lblHomeRange.TabIndex = 1;
            this.lblHomeRange.Text = "Home Range:";
            // 
            // spnSpawnMaxCount
            // 
            this.spnSpawnMaxCount.Location = new System.Drawing.Point(96, 60);
            this.spnSpawnMaxCount.Maximum = new System.Decimal(new int[] {
                                                                             10000,
                                                                             0,
                                                                             0,
                                                                             0});
            this.spnSpawnMaxCount.Name = "spnSpawnMaxCount";
            this.spnSpawnMaxCount.Size = new System.Drawing.Size(48, 20);
            this.spnSpawnMaxCount.TabIndex = 4;
            this.spnSpawnMaxCount.Value = new System.Decimal(new int[] {
                                                                           1,
                                                                           0,
                                                                           0,
                                                                           0});
            // 
            // txtSpawnName
            // 
            this.txtSpawnName.Location = new System.Drawing.Point(8, 16);
            this.txtSpawnName.Name = "txtSpawnName";
            this.txtSpawnName.Size = new System.Drawing.Size(136, 20);
            this.txtSpawnName.TabIndex = 0;
            this.txtSpawnName.Text = "Spawn";
            // 
            // spnSpawnRange
            // 
            this.spnSpawnRange.Location = new System.Drawing.Point(96, 40);
            this.spnSpawnRange.Maximum = new System.Decimal(new int[] {
                                                                          10000,
                                                                          0,
                                                                          0,
                                                                          0});
            this.spnSpawnRange.Minimum = new System.Decimal(new int[] {
                                                                          1,
                                                                          0,
                                                                          0,
                                                                          0});
            this.spnSpawnRange.Name = "spnSpawnRange";
            this.spnSpawnRange.Size = new System.Drawing.Size(48, 20);
            this.spnSpawnRange.TabIndex = 2;
            this.spnSpawnRange.Value = new System.Decimal(new int[] {
                                                                        10,
                                                                        0,
                                                                        0,
                                                                        0});
            // 
            // lblTeam
            // 
            this.lblTeam.Location = new System.Drawing.Point(8, 124);
            this.lblTeam.Name = "lblTeam";
            this.lblTeam.Size = new System.Drawing.Size(80, 16);
            this.lblTeam.TabIndex = 9;
            this.lblTeam.Text = "Team:";
            // 
            // lblMaxCount
            // 
            this.lblMaxCount.Location = new System.Drawing.Point(8, 64);
            this.lblMaxCount.Name = "lblMaxCount";
            this.lblMaxCount.Size = new System.Drawing.Size(80, 16);
            this.lblMaxCount.TabIndex = 3;
            this.lblMaxCount.Text = "Max Count:";
            // 
            // spnSpawnMinDelay
            // 
            this.spnSpawnMinDelay.Location = new System.Drawing.Point(96, 80);
            this.spnSpawnMinDelay.Maximum = new System.Decimal(new int[] {
                                                                             65535,
                                                                             0,
                                                                             0,
                                                                             0});
            this.spnSpawnMinDelay.Name = "spnSpawnMinDelay";
            this.spnSpawnMinDelay.Size = new System.Drawing.Size(48, 20);
            this.spnSpawnMinDelay.TabIndex = 6;
            this.spnSpawnMinDelay.Value = new System.Decimal(new int[] {
                                                                           5,
                                                                           0,
                                                                           0,
                                                                           0});
            // 
            // spnSpawnTeam
            // 
            this.spnSpawnTeam.Location = new System.Drawing.Point(96, 120);
            this.spnSpawnTeam.Maximum = new System.Decimal(new int[] {
                                                                         65535,
                                                                         0,
                                                                         0,
                                                                         0});
            this.spnSpawnTeam.Name = "spnSpawnTeam";
            this.spnSpawnTeam.Size = new System.Drawing.Size(48, 20);
            this.spnSpawnTeam.TabIndex = 10;
            // 
            // chkSpawnGroup
            // 
            this.chkSpawnGroup.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSpawnGroup.Location = new System.Drawing.Point(8, 144);
            this.chkSpawnGroup.Name = "chkSpawnGroup";
            this.chkSpawnGroup.Size = new System.Drawing.Size(102, 16);
            this.chkSpawnGroup.TabIndex = 11;
            this.chkSpawnGroup.Text = "Group:";
            // 
            // spnSpawnMaxDelay
            // 
            this.spnSpawnMaxDelay.Location = new System.Drawing.Point(96, 100);
            this.spnSpawnMaxDelay.Maximum = new System.Decimal(new int[] {
                                                                             65535,
                                                                             0,
                                                                             0,
                                                                             0});
            this.spnSpawnMaxDelay.Name = "spnSpawnMaxDelay";
            this.spnSpawnMaxDelay.Size = new System.Drawing.Size(48, 20);
            this.spnSpawnMaxDelay.TabIndex = 8;
            this.spnSpawnMaxDelay.Value = new System.Decimal(new int[] {
                                                                           10,
                                                                           0,
                                                                           0,
                                                                           0});
            // 
            // lblMinDelay
            // 
            this.lblMinDelay.Location = new System.Drawing.Point(8, 84);
            this.lblMinDelay.Name = "lblMinDelay";
            this.lblMinDelay.Size = new System.Drawing.Size(80, 16);
            this.lblMinDelay.TabIndex = 5;
            this.lblMinDelay.Text = "Min Delay (m)";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(112, 312);
            this.btnOk.Name = "btnOk";
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "&Ok";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(192, 312);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "&Cancel";
            // 
            // txtCmdPrefix
            // 
            this.txtCmdPrefix.Location = new System.Drawing.Point(8, 120);
            this.txtCmdPrefix.Name = "txtCmdPrefix";
            this.txtCmdPrefix.Size = new System.Drawing.Size(96, 20);
            this.txtCmdPrefix.TabIndex = 9;
            this.txtCmdPrefix.Text = "[";
            // 
            // lblCmdPrefix
            // 
            this.lblCmdPrefix.Location = new System.Drawing.Point(8, 104);
            this.lblCmdPrefix.Name = "lblCmdPrefix";
            this.lblCmdPrefix.Size = new System.Drawing.Size(96, 16);
            this.lblCmdPrefix.TabIndex = 8;
            this.lblCmdPrefix.Text = "Command Prefix:";
            // 
            // lblClientWindow
            // 
            this.lblClientWindow.Location = new System.Drawing.Point(8, 152);
            this.lblClientWindow.Name = "lblClientWindow";
            this.lblClientWindow.Size = new System.Drawing.Size(88, 16);
            this.lblClientWindow.TabIndex = 13;
            this.lblClientWindow.Text = "Client Window:";
            // 
            // txtClientWindow
            // 
            this.txtClientWindow.Location = new System.Drawing.Point(8, 168);
            this.txtClientWindow.Name = "txtClientWindow";
            this.txtClientWindow.Size = new System.Drawing.Size(96, 20);
            this.txtClientWindow.TabIndex = 14;
            this.txtClientWindow.Text = "Ultima Online";
            // 
            // Configure
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(292, 346);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.txtClientWindow,
                                                                          this.lblClientWindow,
                                                                          this.lblCmdPrefix,
                                                                          this.txtCmdPrefix,
                                                                          this.btnCancel,
                                                                          this.btnOk,
                                                                          this.grpSpawnEdit,
                                                                          this.trkZoom,
                                                                          this.label1,
                                                                          this.txtUltimaClient,
                                                                          this.lblUltimaClient,
                                                                          this.btnUltimaClient,
                                                                          this.txtRunUOExe,
                                                                          this.lblRunUOExe,
                                                                          this.btnRunUOExe});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Configure";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spawn Editor Configuration";
            this.Load += new System.EventHandler(this.Configure_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).EndInit();
            this.grpSpawnEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMaxCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMinDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnTeam)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnSpawnMaxDelay)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

        private void btnRunUOExe_Click(object sender, System.EventArgs e)
        {
            if( this.ofdOpenFile.ShowDialog( this ) == DialogResult.OK )
            {
                this.txtRunUOExe.Text = this.ofdOpenFile.FileName;
            }
        }

        private void btnUltimaClient_Click(object sender, System.EventArgs e)
        {
            if( this.ofdOpenFile.ShowDialog( this ) == DialogResult.OK )
            {
                this.txtUltimaClient.Text = this.ofdOpenFile.FileName;
                this.SetClientWindowName();
            }
        }

        private void Configure_Load(object sender, System.EventArgs e)
        {
            if( this._IsValidConfiguration == false )
            {
                // Try to find the uo registry information
                // Look for the Third Dawn key first
                this._HKLMKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( this.UOTDRegistryKey );
                this.CfgUoClientWindowValue = "Ultima Online Third Dawn";

                // Check if the key was not found
                if( this._HKLMKey == null )
                {
                    // Try to find the T2A client (pre-3rd Dawn client)
                    this._HKLMKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( this.T2ARegistryKey );
                }

                // Was the key found this time?
                if( this._HKLMKey != null )
                {
                    this.CfgUoClientPathValue = (string)this._HKLMKey.GetValue( this.UOExePathValue );
                    this.txtUltimaClient.Text = this.CfgUoClientPathValue;
                    this.SetClientWindowName();
                }
            }

            // Load all of the registry values required
            this.txtRunUOExe.Text = this.CfgRunUoPathValue;
            this.txtUltimaClient.Text = this.CfgUoClientPathValue;
            this.txtClientWindow.Text = this.CfgUoClientWindowValue;
            this.trkZoom.Value = this.CfgZoomLevelValue;
            this.txtCmdPrefix.Text = this.CfgRunUoCmdPrefix;
            this.txtSpawnName.Text = this.CfgSpawnNameValue;
            this.spnSpawnRange.Value = this.CfgSpawnHomeRangeValue;
            this.spnSpawnMaxCount.Value = this.CfgSpawnMaxCountValue;
            this.spnSpawnMinDelay.Value = this.CfgSpawnMinDelayValue;
            this.spnSpawnMaxDelay.Value = this.CfgSpawnMaxDelayValue;
            this.spnSpawnTeam.Value = this.CfgSpawnTeamValue;
            this.chkSpawnGroup.Checked = this.CfgSpawnGroupValue;
            this.chkSpawnRunning.Checked = this.CfgSpawnRunningValue;
            this.chkHomeRangeIsRelative.Checked = this.CfgSpawnRelativeHomeValue;
        }

        private void SetClientWindowName()
        {
            // Determine what client is being used so that the window
            // name can be set to the proper value ("Ultima Online" for client.exe and "Ultima Online Third Dawn" for uotd.exe)
            string ClientExe = System.IO.Path.GetFileName( this.txtUltimaClient.Text ).ToLower();

            switch( ClientExe )
            {
                case "uotd.exe":
                    this.CfgUoClientWindowValue = "Ultima Online Third Dawn";
                    break;

                case "client.exe":
                    this.CfgUoClientWindowValue = "Ultima Online";
                    break;

                default:
                    this.CfgUoClientWindowValue = "???";
                    break;
            }

            // Set the text box to the new name
            this.txtClientWindow.Text = this.CfgUoClientWindowValue;
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            // Update all of the configuration values
            this.CfgRunUoPathValue = this.txtRunUOExe.Text;
            this.CfgUoClientPathValue = this.txtUltimaClient.Text;
            this.CfgUoClientWindowValue = this.txtClientWindow.Text;
            this.CfgZoomLevelValue = (short)this.trkZoom.Value;
            this.CfgRunUoCmdPrefix = this.txtCmdPrefix.Text;
            this.CfgSpawnNameValue = this.txtSpawnName.Text;
            this.CfgSpawnHomeRangeValue = (int)this.spnSpawnRange.Value;
            this.CfgSpawnMaxCountValue = (int)this.spnSpawnMaxCount.Value;
            this.CfgSpawnMinDelayValue = (int)this.spnSpawnMinDelay.Value;
            this.CfgSpawnMaxDelayValue = (int)this.spnSpawnMaxDelay.Value;
            this.CfgSpawnTeamValue = (int)this.spnSpawnTeam.Value;
            this.CfgSpawnGroupValue = this.chkSpawnGroup.Checked;
            this.CfgSpawnRunningValue = this.chkSpawnRunning.Checked;
            this.CfgSpawnRelativeHomeValue = this.chkHomeRangeIsRelative.Checked;
        
            // Check to see if all of the required information was provided
            if( this.CfgRunUoPathValue.Length == 0 )
            {
                MessageBox.Show( this, "You must set the path to the RunUO EXE before proceeding!", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return;
            }
            else if( this.CfgUoClientPathValue.Length == 0 )
            {
                MessageBox.Show( this, "You must set the path to the Ultima Online client EXE before proceeding!", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return;
            }

            // Store all of the registry values
            if( this._HKCUKey == null )
                this._HKCUKey = Registry.CurrentUser.CreateSubKey( this.AppRegistryKey );

            this._HKCUKey.SetValue( this.AppRunUoPathValue, (string)CfgRunUoPathValue );
            this._HKCUKey.SetValue( this.AppUoClientPathValue, (string)this.CfgUoClientPathValue );
            this._HKCUKey.SetValue( this.AppUoClientWindowValue, (string)this.CfgUoClientWindowValue );
            this._HKCUKey.SetValue( this.AppZoomLevelValue, this.CfgZoomLevelValue.ToString() );
            this._HKCUKey.SetValue( this.AppRunUoCmdPrefixValue, this.CfgRunUoCmdPrefix );
            this._HKCUKey.SetValue( this.AppSpawnNameValue, (string)this.CfgSpawnNameValue );
            this._HKCUKey.SetValue( this.AppSpawnHomeRangeValue, (int)this.CfgSpawnHomeRangeValue );
            this._HKCUKey.SetValue( this.AppSpawnMaxCountValue, (int)this.CfgSpawnMaxCountValue );
            this._HKCUKey.SetValue( this.AppSpawnMinDelayValue, (int)this.CfgSpawnMinDelayValue );
            this._HKCUKey.SetValue( this.AppSpawnMaxDelayValue, (int)this.CfgSpawnMaxDelayValue );
            this._HKCUKey.SetValue( this.AppSpawnTeamValue, (int)this.CfgSpawnTeamValue );
            this._HKCUKey.SetValue( this.AppSpawnGroupValue, (bool)this.CfgSpawnGroupValue );
            this._HKCUKey.SetValue( this.AppSpawnRunningValue, (bool)this.CfgSpawnRunningValue );
            this._HKCUKey.SetValue( this.AppSpawnRelativeHomeValue, (bool)this.CfgSpawnRelativeHomeValue );

            // Set the configuration to valid
            this._IsValidConfiguration = true;

            // Close the dialog
            this.Close();
        }
	}
}
