using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpawnEditor
{
    public enum WorldMap
    {
        Internal = -1,
        Trammel = 0,
        Felucca = 1,
        Ilshenar = 2,
        Malas = 3,
    }

	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class SpawnEditor : System.Windows.Forms.Form
	{
        private const string SpawnEditorTitle = "Spawn Editor";
        private const string SpawnDataSetName = "Spawns";
        private const string SpawnTablePointName = "Points";
        private const string SpawnTableObjectName = "Objects";
        private readonly string DefaultZoomLevelText = "Zoom Level:  ";
        private Configure _CfgDialog;
        private Type[] _RunUOScriptTypes;
        private SelectionWindow _SelectionWindow = null;

        private ManagedMap axUOMap;
        private System.Windows.Forms.ToolTip ttpSpawnInfo;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.TrackBar trkZoom;
        private System.Windows.Forms.CheckBox chkDrawStatics;
        private System.Windows.Forms.GroupBox grpMapControl;
        private System.Windows.Forms.CheckedListBox clbRunUOTypes;
        private System.Windows.Forms.Label lblTotalTypesLoaded;
        private System.Windows.Forms.RadioButton radShowAll;
        private System.Windows.Forms.RadioButton radShowItemsOnly;
        private System.Windows.Forms.RadioButton radShowMobilesOnly;
        private System.Windows.Forms.Panel pnlSpawnDetails;
        private System.Windows.Forms.Label lblMaxCount;
        private System.Windows.Forms.NumericUpDown spnMaxCount;
        private System.Windows.Forms.CheckBox chkGroup;
        private System.Windows.Forms.Label lblHomeRange;
        private System.Windows.Forms.NumericUpDown spnHomeRange;
        private System.Windows.Forms.NumericUpDown spnMinDelay;
        private System.Windows.Forms.Label lblMinDelay;
        private System.Windows.Forms.NumericUpDown spnMaxDelay;
        private System.Windows.Forms.Label lblMaxDelay;
        private System.Windows.Forms.CheckBox chkRunning;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblTotalSpawn;
        private System.Windows.Forms.NumericUpDown spnTeam;
        private System.Windows.Forms.Label lblTeam;
        private System.Windows.Forms.Button btnLoadSpawn;
        private System.Windows.Forms.Button btnSaveSpawn;
        private System.Windows.Forms.TreeView tvwSpawnPoints;
        private System.Windows.Forms.Button btnResetTypes;
        private System.Windows.Forms.Button btnMergeSpawn;
        private System.Windows.Forms.OpenFileDialog ofdLoadFile;
        private System.Windows.Forms.SaveFileDialog sfdSaveFile;
        private System.Windows.Forms.ContextMenuStrip mncSpawns;
        private System.Windows.Forms.GroupBox grpSpawnTypes;
        internal System.Windows.Forms.Button btnUpdateSpawn;
        private System.Windows.Forms.Button btnDeleteSpawn;
        private System.Windows.Forms.GroupBox grpSpawnList;
        internal System.Windows.Forms.GroupBox grpSpawnEdit;
        private System.Windows.Forms.StatusStrip stbMain;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.ToolStripSeparator menuItem3;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteAllSpawns;
        private System.Windows.Forms.ToolStripMenuItem mniSetSpawnAmount;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteSpawn;
        private System.Windows.Forms.Button btnRestoreSpawnDefaults;
        private System.Windows.Forms.CheckBox chkShowMapTip;
        private System.Windows.Forms.CheckBox chkShowSpawns;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.ComboBox cbxMap;
        private System.Windows.Forms.CheckBox chkHomeRangeIsRelative;
        private System.Windows.Forms.CheckBox chkSyncUO;
        private System.Windows.Forms.ContextMenuStrip mncLoad;
        private System.Windows.Forms.ToolStripMenuItem mniForceLoad;
        private System.Windows.Forms.ContextMenuStrip mncMerge;
        private System.Windows.Forms.ToolStripMenuItem mniForceMerge;
        private System.Windows.Forms.ToolStripStatusLabel stbMainLabel;
        private System.Windows.Forms.Splitter splLeft;
        private System.Windows.Forms.Splitter splRight;
        private System.ComponentModel.IContainer components;

		public SpawnEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            // Create the configuration dialog
            this._CfgDialog = new Configure();
		}

        private void SpawnEditor_Load(object sender, System.EventArgs e)
        {
            // Check if the configuration is valid
            if( this._CfgDialog.IsValidConfiguration == false )
            {
                // Show the configuraion dialog
                this._CfgDialog.ShowDialog();

                // Check if the configuration is complete
                if( this._CfgDialog.IsValidConfiguration == false )
                {
                    // Spawn editor can not continue
                    MessageBox.Show( this, "Spawn Editor has not been configured properly." + Environment.NewLine + "Exiting...", "Configuration Failure", MessageBoxButtons.OK, MessageBoxIcon.Error );
                    Application.Exit();
                }
            }

            try
            {
                // Populate the map combo box
                Array Maps = Enum.GetValues( typeof( WorldMap ) );
                foreach( WorldMap m in Maps )
                    if( m != WorldMap.Internal )
                        this.cbxMap.Items.Add( m );

                // Set the default map to Trammel
                this.cbxMap.SelectedIndex = 0;

                this.axUOMap.SetClientPath( System.IO.Path.GetDirectoryName( this._CfgDialog.CfgUoClientPathValue ) );
                this.axUOMap.ZoomLevel = this._CfgDialog.CfgZoomLevelValue;
                this.trkZoom.Value = this.axUOMap.ZoomLevel;
            
                // Load the assembly information for RunUO
                Assembly CoreAssembly = Assembly.LoadFrom( this._CfgDialog.CfgRunUoPathValue );
                Assembly ScriptAssembly = null;
                ArrayList AllTypes = new ArrayList();
                string RunUORootPath = System.IO.Path.GetDirectoryName( this._CfgDialog.CfgRunUoPathValue );

                // Try to load Scripts.dll
                if( System.IO.File.Exists( RunUORootPath + @"\Scripts\Output\Scripts.dll" ) == true )
                {
                    ScriptAssembly = Assembly.LoadFrom( RunUORootPath + @"\Scripts\Output\Scripts.dll" );
                    if( ScriptAssembly != null )
                        AllTypes.AddRange( ScriptAssembly.GetTypes() );
                }

                // Try to load Scripts.CS.dll
                if( System.IO.File.Exists( RunUORootPath + @"\Scripts\Output\Scripts.CS.dll" ) == true )
                {
                    ScriptAssembly = Assembly.LoadFrom( RunUORootPath + @"\Scripts\Output\Scripts.CS.dll" );
                    if( ScriptAssembly != null )
                        AllTypes.AddRange( ScriptAssembly.GetTypes() );
                }
            
                // Try to load Scripts.VB.dll
                if( System.IO.File.Exists( RunUORootPath + @"\Scripts\Output\Scripts.VB.dll" ) == true )
                {
                    ScriptAssembly = Assembly.LoadFrom( RunUORootPath + @"\Scripts\Output\Scripts.VB.dll" );
                    if( ScriptAssembly != null )
                        AllTypes.AddRange( ScriptAssembly.GetTypes() );
                }

                this._RunUOScriptTypes = (Type[])AllTypes.ToArray( typeof(Type) );

                // Load all types by default
                this.LoadTypes();

                // Update the total count of the spawners loaded
                this.lblTotalSpawn.Text = "Total Spawns = " + this.tvwSpawnPoints.Nodes.Count;

                // Load the default spawner settings
                this.LoadDefaultSpawnValues();

                // Set the load and save dialog default paths to the RunUO directory
                if( System.IO.Directory.Exists( System.IO.Path.GetDirectoryName( this._CfgDialog.CfgRunUoPathValue ) ) == true )
                {
                    this.ofdLoadFile.InitialDirectory = this._CfgDialog.CfgRunUoPathValue;
                    this.sfdSaveFile.InitialDirectory = this._CfgDialog.CfgRunUoPathValue;
                }
            }
            catch( System.Exception se )
            {
                // Show a message box indicating that there is a configuration problem
                MessageBox.Show( this, "Error loading the required RunUO executables. Please check that the paths specified in Setup are valid." + Environment.NewLine + se.ToString(), "Configuration Error!", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void LoadDefaultSpawnValues()
        {
            this.txtName.Text = this._CfgDialog.CfgSpawnNameValue + this.tvwSpawnPoints.Nodes.Count;
            this.spnHomeRange.Value = this._CfgDialog.CfgSpawnHomeRangeValue;
            this.spnMaxCount.Value = this._CfgDialog.CfgSpawnMaxCountValue;
            this.spnMinDelay.Value = this._CfgDialog.CfgSpawnMinDelayValue;
            this.spnMaxDelay.Value = this._CfgDialog.CfgSpawnMaxDelayValue;
            this.spnTeam.Value = this._CfgDialog.CfgSpawnTeamValue;
            this.chkGroup.Checked = this._CfgDialog.CfgSpawnGroupValue;
            this.chkRunning.Checked = this._CfgDialog.CfgSpawnRunningValue;
            this.chkHomeRangeIsRelative.Checked = this._CfgDialog.CfgSpawnRelativeHomeValue;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SpawnEditor));
            this.axUOMap = new ManagedMap();
            this.ttpSpawnInfo = new System.Windows.Forms.ToolTip(this.components);
            this.btnSaveSpawn = new System.Windows.Forms.Button();
            this.btnLoadSpawn = new System.Windows.Forms.Button();
            this.mncLoad = new System.Windows.Forms.ContextMenuStrip();
            this.mniForceLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.trkZoom = new System.Windows.Forms.TrackBar();
            this.chkDrawStatics = new System.Windows.Forms.CheckBox();
            this.radShowMobilesOnly = new System.Windows.Forms.RadioButton();
            this.radShowItemsOnly = new System.Windows.Forms.RadioButton();
            this.radShowAll = new System.Windows.Forms.RadioButton();
            this.clbRunUOTypes = new System.Windows.Forms.CheckedListBox();
            this.spnTeam = new System.Windows.Forms.NumericUpDown();
            this.txtName = new System.Windows.Forms.TextBox();
            this.chkRunning = new System.Windows.Forms.CheckBox();
            this.spnMaxDelay = new System.Windows.Forms.NumericUpDown();
            this.spnMinDelay = new System.Windows.Forms.NumericUpDown();
            this.spnHomeRange = new System.Windows.Forms.NumericUpDown();
            this.spnMaxCount = new System.Windows.Forms.NumericUpDown();
            this.chkGroup = new System.Windows.Forms.CheckBox();
            this.tvwSpawnPoints = new System.Windows.Forms.TreeView();
            this.btnResetTypes = new System.Windows.Forms.Button();
            this.btnMergeSpawn = new System.Windows.Forms.Button();
            this.btnUpdateSpawn = new System.Windows.Forms.Button();
            this.btnDeleteSpawn = new System.Windows.Forms.Button();
            this.btnRestoreSpawnDefaults = new System.Windows.Forms.Button();
            this.chkShowMapTip = new System.Windows.Forms.CheckBox();
            this.chkShowSpawns = new System.Windows.Forms.CheckBox();
            this.btnMove = new System.Windows.Forms.Button();
            this.cbxMap = new System.Windows.Forms.ComboBox();
            this.chkHomeRangeIsRelative = new System.Windows.Forms.CheckBox();
            this.chkSyncUO = new System.Windows.Forms.CheckBox();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.grpMapControl = new System.Windows.Forms.GroupBox();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.grpSpawnTypes = new System.Windows.Forms.GroupBox();
            this.lblTotalTypesLoaded = new System.Windows.Forms.Label();
            this.lblTotalSpawn = new System.Windows.Forms.Label();
            this.pnlSpawnDetails = new System.Windows.Forms.Panel();
            this.grpSpawnEdit = new System.Windows.Forms.GroupBox();
            this.lblMaxDelay = new System.Windows.Forms.Label();
            this.lblHomeRange = new System.Windows.Forms.Label();
            this.lblTeam = new System.Windows.Forms.Label();
            this.lblMaxCount = new System.Windows.Forms.Label();
            this.lblMinDelay = new System.Windows.Forms.Label();
            this.grpSpawnList = new System.Windows.Forms.GroupBox();
            this.mncSpawns = new System.Windows.Forms.ContextMenuStrip();
            this.mniSetSpawnAmount = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mniDeleteSpawn = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteAllSpawns = new System.Windows.Forms.ToolStripMenuItem();
            this.ofdLoadFile = new System.Windows.Forms.OpenFileDialog();
            this.sfdSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.stbMain = new System.Windows.Forms.StatusStrip();
            this.stbMainLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mncMerge = new System.Windows.Forms.ContextMenuStrip();
            this.mniForceMerge = new System.Windows.Forms.ToolStripMenuItem();
            this.splLeft = new System.Windows.Forms.Splitter();
            this.splRight = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnTeam)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMaxDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMinDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHomeRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMaxCount)).BeginInit();
            this.pnlControls.SuspendLayout();
            this.grpMapControl.SuspendLayout();
            this.grpSpawnTypes.SuspendLayout();
            this.pnlSpawnDetails.SuspendLayout();
            this.grpSpawnEdit.SuspendLayout();
            this.grpSpawnList.SuspendLayout();
            this.SuspendLayout();
            // 
            // axUOMap
            // 
            this.axUOMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axUOMap.Enabled = true;
            this.axUOMap.Location = new System.Drawing.Point(168, 0);
            this.axUOMap.Name = "axUOMap";
            this.axUOMap.Size = new System.Drawing.Size(456, 551);
            this.axUOMap.TabIndex = 1;
            this.axUOMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.axUOMap_MouseMoveEvent);
            this.axUOMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axUOMap_MouseDownEvent);
            this.axUOMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axUOMap_MouseUpEvent);
            // 
            // ttpSpawnInfo
            // 
            this.ttpSpawnInfo.AutoPopDelay = 5000;
            this.ttpSpawnInfo.InitialDelay = 500;
            this.ttpSpawnInfo.ReshowDelay = 100;
            // 
            // btnSaveSpawn
            // 
            this.btnSaveSpawn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSpawn.Location = new System.Drawing.Point(104, 16);
            this.btnSaveSpawn.Name = "btnSaveSpawn";
            this.btnSaveSpawn.Size = new System.Drawing.Size(40, 24);
            this.btnSaveSpawn.TabIndex = 2;
            this.btnSaveSpawn.Text = "&Save";
            this.ttpSpawnInfo.SetToolTip(this.btnSaveSpawn, "Saves the current spawn list.");
            this.btnSaveSpawn.Click += new System.EventHandler(this.btnSaveSpawn_Click);
            // 
            // btnLoadSpawn
            // 
            this.btnLoadSpawn.ContextMenuStrip = this.mncLoad;
            this.btnLoadSpawn.Location = new System.Drawing.Point(8, 16);
            this.btnLoadSpawn.Name = "btnLoadSpawn";
            this.btnLoadSpawn.Size = new System.Drawing.Size(40, 24);
            this.btnLoadSpawn.TabIndex = 0;
            this.btnLoadSpawn.Text = "&Load";
            this.ttpSpawnInfo.SetToolTip(this.btnLoadSpawn, "Clears the currently defined spawns, if any, and loads a spawn file.  Right-Click" +
                " on the Load button to bring up a menu to force loading a spawn file into the cu" +
                "rrently selected map.  This can be used to convert a spawn file from one map to " +
                "another.");
            this.btnLoadSpawn.Click += new System.EventHandler(this.btnLoadSpawn_Click);
            // 
            // mncLoad
            // 
            this.mncLoad.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                    this.mniForceLoad});
            // 
            // mniForceLoad
            // 
            this.mniForceLoad.Text = "Force Load Into Current Map...";
            this.mniForceLoad.Click += new System.EventHandler(this.mniForceLoad_Click);
            // 
            // trkZoom
            // 
            this.trkZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.trkZoom.AutoSize = false;
            this.trkZoom.LargeChange = 2;
            this.trkZoom.Location = new System.Drawing.Point(8, 88);
            this.trkZoom.Maximum = 4;
            this.trkZoom.Minimum = -4;
            this.trkZoom.Name = "trkZoom";
            this.trkZoom.Size = new System.Drawing.Size(136, 32);
            this.trkZoom.TabIndex = 5;
            this.trkZoom.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.ttpSpawnInfo.SetToolTip(this.trkZoom, "Zooms in/out of map.");
            this.trkZoom.ValueChanged += new System.EventHandler(this.trkZoom_ValueChanged);
            // 
            // chkDrawStatics
            // 
            this.chkDrawStatics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDrawStatics.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkDrawStatics.Location = new System.Drawing.Point(64, 16);
            this.chkDrawStatics.Name = "chkDrawStatics";
            this.chkDrawStatics.Size = new System.Drawing.Size(80, 16);
            this.chkDrawStatics.TabIndex = 1;
            this.chkDrawStatics.Text = "Statics";
            this.ttpSpawnInfo.SetToolTip(this.chkDrawStatics, "Draws static tiles on the map.");
            this.chkDrawStatics.CheckedChanged += new System.EventHandler(this.chkDrawStatics_CheckedChanged);
            // 
            // radShowMobilesOnly
            // 
            this.radShowMobilesOnly.Location = new System.Drawing.Point(8, 48);
            this.radShowMobilesOnly.Name = "radShowMobilesOnly";
            this.radShowMobilesOnly.Size = new System.Drawing.Size(136, 16);
            this.radShowMobilesOnly.TabIndex = 2;
            this.radShowMobilesOnly.Text = "Show Mobiles";
            this.ttpSpawnInfo.SetToolTip(this.radShowMobilesOnly, "Shows only mobile based spawn objects.");
            this.radShowMobilesOnly.CheckedChanged += new System.EventHandler(this.TypeSelectionChanged);
            // 
            // radShowItemsOnly
            // 
            this.radShowItemsOnly.Location = new System.Drawing.Point(8, 32);
            this.radShowItemsOnly.Name = "radShowItemsOnly";
            this.radShowItemsOnly.Size = new System.Drawing.Size(136, 16);
            this.radShowItemsOnly.TabIndex = 1;
            this.radShowItemsOnly.Text = "Show Items";
            this.ttpSpawnInfo.SetToolTip(this.radShowItemsOnly, "Shows only item based spawn objects.");
            this.radShowItemsOnly.CheckedChanged += new System.EventHandler(this.TypeSelectionChanged);
            // 
            // radShowAll
            // 
            this.radShowAll.Checked = true;
            this.radShowAll.Location = new System.Drawing.Point(8, 16);
            this.radShowAll.Name = "radShowAll";
            this.radShowAll.Size = new System.Drawing.Size(136, 16);
            this.radShowAll.TabIndex = 0;
            this.radShowAll.TabStop = true;
            this.radShowAll.Text = "Show All";
            this.ttpSpawnInfo.SetToolTip(this.radShowAll, "Shows all types of spawn objects (items/mobiles).");
            this.radShowAll.CheckedChanged += new System.EventHandler(this.TypeSelectionChanged);
            // 
            // clbRunUOTypes
            // 
            this.clbRunUOTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.clbRunUOTypes.CheckOnClick = true;
            this.clbRunUOTypes.HorizontalScrollbar = true;
            this.clbRunUOTypes.IntegralHeight = false;
            this.clbRunUOTypes.Location = new System.Drawing.Point(8, 88);
            this.clbRunUOTypes.Name = "clbRunUOTypes";
            this.clbRunUOTypes.Size = new System.Drawing.Size(136, 288);
            this.clbRunUOTypes.TabIndex = 4;
            this.clbRunUOTypes.ThreeDCheckBoxes = true;
            this.ttpSpawnInfo.SetToolTip(this.clbRunUOTypes, "List of all spawnable objects.");
            // 
            // spnTeam
            // 
            this.spnTeam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.spnTeam.Location = new System.Drawing.Point(96, 120);
            this.spnTeam.Maximum = new System.Decimal(new int[] {
                                                                    65535,
                                                                    0,
                                                                    0,
                                                                    0});
            this.spnTeam.Name = "spnTeam";
            this.spnTeam.Size = new System.Drawing.Size(48, 20);
            this.spnTeam.TabIndex = 10;
            this.ttpSpawnInfo.SetToolTip(this.spnTeam, "Team that spawned object will belong to.");
            this.spnTeam.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(8, 16);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(136, 20);
            this.txtName.TabIndex = 0;
            this.txtName.Text = "Spawn";
            this.ttpSpawnInfo.SetToolTip(this.txtName, "Name of the spawner.");
            this.txtName.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // chkRunning
            // 
            this.chkRunning.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkRunning.Checked = true;
            this.chkRunning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunning.Location = new System.Drawing.Point(8, 160);
            this.chkRunning.Name = "chkRunning";
            this.chkRunning.Size = new System.Drawing.Size(104, 16);
            this.chkRunning.TabIndex = 12;
            this.chkRunning.Text = "Running:";
            this.ttpSpawnInfo.SetToolTip(this.chkRunning, "Check if the spawner should be running.");
            // 
            // spnMaxDelay
            // 
            this.spnMaxDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.spnMaxDelay.Location = new System.Drawing.Point(96, 100);
            this.spnMaxDelay.Maximum = new System.Decimal(new int[] {
                                                                        65535,
                                                                        0,
                                                                        0,
                                                                        0});
            this.spnMaxDelay.Name = "spnMaxDelay";
            this.spnMaxDelay.Size = new System.Drawing.Size(48, 20);
            this.spnMaxDelay.TabIndex = 8;
            this.ttpSpawnInfo.SetToolTip(this.spnMaxDelay, "Maximum delay to respawn (in minutes).");
            this.spnMaxDelay.Value = new System.Decimal(new int[] {
                                                                      10,
                                                                      0,
                                                                      0,
                                                                      0});
            this.spnMaxDelay.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // spnMinDelay
            // 
            this.spnMinDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.spnMinDelay.Location = new System.Drawing.Point(96, 80);
            this.spnMinDelay.Maximum = new System.Decimal(new int[] {
                                                                        65535,
                                                                        0,
                                                                        0,
                                                                        0});
            this.spnMinDelay.Name = "spnMinDelay";
            this.spnMinDelay.Size = new System.Drawing.Size(48, 20);
            this.spnMinDelay.TabIndex = 6;
            this.ttpSpawnInfo.SetToolTip(this.spnMinDelay, "Minimum delay to respawn (in minutes).");
            this.spnMinDelay.Value = new System.Decimal(new int[] {
                                                                      5,
                                                                      0,
                                                                      0,
                                                                      0});
            this.spnMinDelay.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // spnHomeRange
            // 
            this.spnHomeRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.spnHomeRange.Location = new System.Drawing.Point(96, 40);
            this.spnHomeRange.Maximum = new System.Decimal(new int[] {
                                                                         10000,
                                                                         0,
                                                                         0,
                                                                         0});
            this.spnHomeRange.Minimum = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         0});
            this.spnHomeRange.Name = "spnHomeRange";
            this.spnHomeRange.Size = new System.Drawing.Size(48, 20);
            this.spnHomeRange.TabIndex = 2;
            this.ttpSpawnInfo.SetToolTip(this.spnHomeRange, "Maximum wandering range of the spawn from its spawned location.");
            this.spnHomeRange.Value = new System.Decimal(new int[] {
                                                                       10,
                                                                       0,
                                                                       0,
                                                                       0});
            this.spnHomeRange.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // spnMaxCount
            // 
            this.spnMaxCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.spnMaxCount.Location = new System.Drawing.Point(96, 60);
            this.spnMaxCount.Maximum = new System.Decimal(new int[] {
                                                                        10000,
                                                                        0,
                                                                        0,
                                                                        0});
            this.spnMaxCount.Name = "spnMaxCount";
            this.spnMaxCount.Size = new System.Drawing.Size(48, 20);
            this.spnMaxCount.TabIndex = 4;
            this.ttpSpawnInfo.SetToolTip(this.spnMaxCount, "Absolute maximum number of objects to be spawned by this spawner.");
            this.spnMaxCount.Value = new System.Decimal(new int[] {
                                                                      1,
                                                                      0,
                                                                      0,
                                                                      0});
            this.spnMaxCount.Enter += new System.EventHandler(this.TextEntryControl_Enter);
            // 
            // chkGroup
            // 
            this.chkGroup.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGroup.Location = new System.Drawing.Point(8, 144);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(104, 16);
            this.chkGroup.TabIndex = 11;
            this.chkGroup.Text = "Group:";
            this.ttpSpawnInfo.SetToolTip(this.chkGroup, "Check if the spawned object belongs to a group.");
            // 
            // tvwSpawnPoints
            // 
            this.tvwSpawnPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.tvwSpawnPoints.ImageIndex = -1;
            this.tvwSpawnPoints.Location = new System.Drawing.Point(8, 40);
            this.tvwSpawnPoints.Name = "tvwSpawnPoints";
            this.tvwSpawnPoints.SelectedImageIndex = -1;
            this.tvwSpawnPoints.Size = new System.Drawing.Size(136, 216);
            this.tvwSpawnPoints.Sorted = true;
            this.tvwSpawnPoints.TabIndex = 3;
            this.ttpSpawnInfo.SetToolTip(this.tvwSpawnPoints, "List of currently defined spawns.  Right-Click for a context menu based on the cu" +
                "rrently selected spawn.");
            this.tvwSpawnPoints.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvwSpawnPoints_MouseUp);
            // 
            // btnResetTypes
            // 
            this.btnResetTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetTypes.Location = new System.Drawing.Point(8, 64);
            this.btnResetTypes.Name = "btnResetTypes";
            this.btnResetTypes.Size = new System.Drawing.Size(136, 24);
            this.btnResetTypes.TabIndex = 3;
            this.btnResetTypes.Text = "&Clear Selection";
            this.ttpSpawnInfo.SetToolTip(this.btnResetTypes, "Clears current selections from the type list.");
            this.btnResetTypes.Click += new System.EventHandler(this.btnResetTypes_Click);
            // 
            // btnMergeSpawn
            // 
            this.btnMergeSpawn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMergeSpawn.Location = new System.Drawing.Point(48, 16);
            this.btnMergeSpawn.Name = "btnMergeSpawn";
            this.btnMergeSpawn.Size = new System.Drawing.Size(56, 24);
            this.btnMergeSpawn.TabIndex = 1;
            this.btnMergeSpawn.Text = "&Merge";
            this.ttpSpawnInfo.SetToolTip(this.btnMergeSpawn, "Loads a spawn file WITHOUT clearing the current spawn list.  Right-Click on the M" +
                "erge button to bring up a menu to force merging a spawn file into the currently " +
                "selected map.  This can be used to convert a spawn file from one map to another." +
                "");
            this.btnMergeSpawn.Click += new System.EventHandler(this.btnMergeSpawn_Click);
            // 
            // btnUpdateSpawn
            // 
            this.btnUpdateSpawn.Enabled = false;
            this.btnUpdateSpawn.Location = new System.Drawing.Point(8, 224);
            this.btnUpdateSpawn.Name = "btnUpdateSpawn";
            this.btnUpdateSpawn.Size = new System.Drawing.Size(56, 23);
            this.btnUpdateSpawn.TabIndex = 15;
            this.btnUpdateSpawn.Text = "&Update";
            this.ttpSpawnInfo.SetToolTip(this.btnUpdateSpawn, "Updates the currently selected spawn with the spawn details provided.");
            this.btnUpdateSpawn.Click += new System.EventHandler(this.btnUpdateSpawn_Click);
            // 
            // btnDeleteSpawn
            // 
            this.btnDeleteSpawn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteSpawn.Enabled = false;
            this.btnDeleteSpawn.Location = new System.Drawing.Point(64, 224);
            this.btnDeleteSpawn.Name = "btnDeleteSpawn";
            this.btnDeleteSpawn.Size = new System.Drawing.Size(48, 23);
            this.btnDeleteSpawn.TabIndex = 16;
            this.btnDeleteSpawn.Text = "&Delete";
            this.ttpSpawnInfo.SetToolTip(this.btnDeleteSpawn, "Deletes the currently selected spawn.");
            this.btnDeleteSpawn.Click += new System.EventHandler(this.btnDeleteSpawn_Click);
            // 
            // btnRestoreSpawnDefaults
            // 
            this.btnRestoreSpawnDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestoreSpawnDefaults.Location = new System.Drawing.Point(8, 200);
            this.btnRestoreSpawnDefaults.Name = "btnRestoreSpawnDefaults";
            this.btnRestoreSpawnDefaults.Size = new System.Drawing.Size(136, 23);
            this.btnRestoreSpawnDefaults.TabIndex = 14;
            this.btnRestoreSpawnDefaults.Text = "Restore Defaults";
            this.ttpSpawnInfo.SetToolTip(this.btnRestoreSpawnDefaults, "Restores the spawn details to the default values.");
            this.btnRestoreSpawnDefaults.Click += new System.EventHandler(this.btnRestoreSpawnDefaults_Click);
            // 
            // chkShowMapTip
            // 
            this.chkShowMapTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowMapTip.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkShowMapTip.Checked = true;
            this.chkShowMapTip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMapTip.Location = new System.Drawing.Point(64, 32);
            this.chkShowMapTip.Name = "chkShowMapTip";
            this.chkShowMapTip.Size = new System.Drawing.Size(80, 16);
            this.chkShowMapTip.TabIndex = 2;
            this.chkShowMapTip.Text = "Spawn Tip";
            this.ttpSpawnInfo.SetToolTip(this.chkShowMapTip, "Turns on/off the spawn tool tip when hovering over a spawn.");
            // 
            // chkShowSpawns
            // 
            this.chkShowSpawns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowSpawns.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkShowSpawns.Checked = true;
            this.chkShowSpawns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowSpawns.Location = new System.Drawing.Point(64, 48);
            this.chkShowSpawns.Name = "chkShowSpawns";
            this.chkShowSpawns.Size = new System.Drawing.Size(80, 16);
            this.chkShowSpawns.TabIndex = 3;
            this.chkShowSpawns.Text = "Spawns";
            this.ttpSpawnInfo.SetToolTip(this.chkShowSpawns, "Turns on/off drawing of spawn points.");
            this.chkShowSpawns.CheckedChanged += new System.EventHandler(this.chkShowSpawns_CheckedChanged);
            // 
            // btnMove
            // 
            this.btnMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMove.Enabled = false;
            this.btnMove.Location = new System.Drawing.Point(112, 224);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(32, 23);
            this.btnMove.TabIndex = 17;
            this.btnMove.Text = "&XY";
            this.ttpSpawnInfo.SetToolTip(this.btnMove, "Adjusted the spawners boundaries.");
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // cbxMap
            // 
            this.cbxMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMap.Location = new System.Drawing.Point(64, 64);
            this.cbxMap.Name = "cbxMap";
            this.cbxMap.Size = new System.Drawing.Size(80, 21);
            this.cbxMap.TabIndex = 4;
            this.ttpSpawnInfo.SetToolTip(this.cbxMap, "Changes the current map.");
            this.cbxMap.SelectedIndexChanged += new System.EventHandler(this.cbxMap_SelectedIndexChanged);
            // 
            // chkHomeRangeIsRelative
            // 
            this.chkHomeRangeIsRelative.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkHomeRangeIsRelative.Checked = true;
            this.chkHomeRangeIsRelative.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHomeRangeIsRelative.Location = new System.Drawing.Point(8, 176);
            this.chkHomeRangeIsRelative.Name = "chkHomeRangeIsRelative";
            this.chkHomeRangeIsRelative.Size = new System.Drawing.Size(104, 16);
            this.chkHomeRangeIsRelative.TabIndex = 13;
            this.chkHomeRangeIsRelative.Text = "Relative Home:";
            this.ttpSpawnInfo.SetToolTip(this.chkHomeRangeIsRelative, "Check if the object to be spawned should set its home point base on its spawned l" +
                "ocation and not the spawners location.");
            // 
            // chkSyncUO
            // 
            this.chkSyncUO.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSyncUO.Location = new System.Drawing.Point(8, 69);
            this.chkSyncUO.Name = "chkSyncUO";
            this.chkSyncUO.Size = new System.Drawing.Size(48, 16);
            this.chkSyncUO.TabIndex = 6;
            this.chkSyncUO.Text = "Sync:";
            this.ttpSpawnInfo.SetToolTip(this.chkSyncUO, "Turns on/off sending [go commands to the UO client.");
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.grpMapControl);
            this.pnlControls.Controls.Add(this.grpSpawnTypes);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(168, 551);
            this.pnlControls.TabIndex = 0;
            // 
            // grpMapControl
            // 
            this.grpMapControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMapControl.Controls.Add(this.cbxMap);
            this.grpMapControl.Controls.Add(this.chkShowSpawns);
            this.grpMapControl.Controls.Add(this.chkShowMapTip);
            this.grpMapControl.Controls.Add(this.chkDrawStatics);
            this.grpMapControl.Controls.Add(this.trkZoom);
            this.grpMapControl.Controls.Add(this.chkSyncUO);
            this.grpMapControl.Controls.Add(this.btnConfigure);
            this.grpMapControl.Location = new System.Drawing.Point(8, 8);
            this.grpMapControl.Name = "grpMapControl";
            this.grpMapControl.Size = new System.Drawing.Size(152, 128);
            this.grpMapControl.TabIndex = 0;
            this.grpMapControl.TabStop = false;
            this.grpMapControl.Text = "Map Settings";
            // 
            // btnConfigure
            // 
            this.btnConfigure.Location = new System.Drawing.Point(8, 16);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Size = new System.Drawing.Size(48, 24);
            this.btnConfigure.TabIndex = 0;
            this.btnConfigure.Text = "&Setup";
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // grpSpawnTypes
            // 
            this.grpSpawnTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSpawnTypes.Controls.Add(this.radShowMobilesOnly);
            this.grpSpawnTypes.Controls.Add(this.radShowItemsOnly);
            this.grpSpawnTypes.Controls.Add(this.radShowAll);
            this.grpSpawnTypes.Controls.Add(this.btnResetTypes);
            this.grpSpawnTypes.Controls.Add(this.clbRunUOTypes);
            this.grpSpawnTypes.Controls.Add(this.lblTotalTypesLoaded);
            this.grpSpawnTypes.Location = new System.Drawing.Point(8, 144);
            this.grpSpawnTypes.Name = "grpSpawnTypes";
            this.grpSpawnTypes.Size = new System.Drawing.Size(152, 400);
            this.grpSpawnTypes.TabIndex = 1;
            this.grpSpawnTypes.TabStop = false;
            this.grpSpawnTypes.Text = "Spawn Types";
            // 
            // lblTotalTypesLoaded
            // 
            this.lblTotalTypesLoaded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalTypesLoaded.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTotalTypesLoaded.Location = new System.Drawing.Point(8, 376);
            this.lblTotalTypesLoaded.Name = "lblTotalTypesLoaded";
            this.lblTotalTypesLoaded.Size = new System.Drawing.Size(136, 16);
            this.lblTotalTypesLoaded.TabIndex = 5;
            // 
            // lblTotalSpawn
            // 
            this.lblTotalSpawn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalSpawn.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTotalSpawn.Location = new System.Drawing.Point(8, 256);
            this.lblTotalSpawn.Name = "lblTotalSpawn";
            this.lblTotalSpawn.Size = new System.Drawing.Size(136, 16);
            this.lblTotalSpawn.TabIndex = 4;
            // 
            // pnlSpawnDetails
            // 
            this.pnlSpawnDetails.Controls.Add(this.grpSpawnEdit);
            this.pnlSpawnDetails.Controls.Add(this.grpSpawnList);
            this.pnlSpawnDetails.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSpawnDetails.Location = new System.Drawing.Point(624, 0);
            this.pnlSpawnDetails.Name = "pnlSpawnDetails";
            this.pnlSpawnDetails.Size = new System.Drawing.Size(168, 551);
            this.pnlSpawnDetails.TabIndex = 2;
            // 
            // grpSpawnEdit
            // 
            this.grpSpawnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSpawnEdit.Controls.Add(this.chkHomeRangeIsRelative);
            this.grpSpawnEdit.Controls.Add(this.btnMove);
            this.grpSpawnEdit.Controls.Add(this.btnRestoreSpawnDefaults);
            this.grpSpawnEdit.Controls.Add(this.btnDeleteSpawn);
            this.grpSpawnEdit.Controls.Add(this.btnUpdateSpawn);
            this.grpSpawnEdit.Controls.Add(this.lblMaxDelay);
            this.grpSpawnEdit.Controls.Add(this.chkRunning);
            this.grpSpawnEdit.Controls.Add(this.lblHomeRange);
            this.grpSpawnEdit.Controls.Add(this.spnMaxCount);
            this.grpSpawnEdit.Controls.Add(this.txtName);
            this.grpSpawnEdit.Controls.Add(this.spnHomeRange);
            this.grpSpawnEdit.Controls.Add(this.lblTeam);
            this.grpSpawnEdit.Controls.Add(this.lblMaxCount);
            this.grpSpawnEdit.Controls.Add(this.spnMinDelay);
            this.grpSpawnEdit.Controls.Add(this.spnTeam);
            this.grpSpawnEdit.Controls.Add(this.chkGroup);
            this.grpSpawnEdit.Controls.Add(this.spnMaxDelay);
            this.grpSpawnEdit.Controls.Add(this.lblMinDelay);
            this.grpSpawnEdit.Location = new System.Drawing.Point(8, 8);
            this.grpSpawnEdit.Name = "grpSpawnEdit";
            this.grpSpawnEdit.Size = new System.Drawing.Size(152, 256);
            this.grpSpawnEdit.TabIndex = 0;
            this.grpSpawnEdit.TabStop = false;
            this.grpSpawnEdit.Text = "Spawn Details";
            // 
            // lblMaxDelay
            // 
            this.lblMaxDelay.Location = new System.Drawing.Point(8, 104);
            this.lblMaxDelay.Name = "lblMaxDelay";
            this.lblMaxDelay.Size = new System.Drawing.Size(80, 16);
            this.lblMaxDelay.TabIndex = 7;
            this.lblMaxDelay.Text = "Max Delay (m)";
            // 
            // lblHomeRange
            // 
            this.lblHomeRange.Location = new System.Drawing.Point(8, 44);
            this.lblHomeRange.Name = "lblHomeRange";
            this.lblHomeRange.Size = new System.Drawing.Size(80, 16);
            this.lblHomeRange.TabIndex = 1;
            this.lblHomeRange.Text = "Home Range:";
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
            // lblMinDelay
            // 
            this.lblMinDelay.Location = new System.Drawing.Point(8, 84);
            this.lblMinDelay.Name = "lblMinDelay";
            this.lblMinDelay.Size = new System.Drawing.Size(80, 16);
            this.lblMinDelay.TabIndex = 5;
            this.lblMinDelay.Text = "Min Delay (m)";
            // 
            // grpSpawnList
            // 
            this.grpSpawnList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSpawnList.Controls.Add(this.tvwSpawnPoints);
            this.grpSpawnList.Controls.Add(this.btnLoadSpawn);
            this.grpSpawnList.Controls.Add(this.btnMergeSpawn);
            this.grpSpawnList.Controls.Add(this.btnSaveSpawn);
            this.grpSpawnList.Controls.Add(this.lblTotalSpawn);
            this.grpSpawnList.Location = new System.Drawing.Point(8, 264);
            this.grpSpawnList.Name = "grpSpawnList";
            this.grpSpawnList.Size = new System.Drawing.Size(152, 280);
            this.grpSpawnList.TabIndex = 1;
            this.grpSpawnList.TabStop = false;
            this.grpSpawnList.Text = "Spawn List";
            // 
            // mncSpawns
            // 
            this.mncSpawns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                      this.mniSetSpawnAmount,
                                                                                      this.menuItem3,
                                                                                      this.mniDeleteSpawn,
                                                                                      this.mniDeleteAllSpawns});
            this.mncSpawns.Opening += new System.ComponentModel.CancelEventHandler(this.mncSpawns_Opening);
            // 
            // mniSetSpawnAmount
            // 
            this.mniSetSpawnAmount.Text = "&Set Amount...";
            this.mniSetSpawnAmount.Click += new System.EventHandler(this.mniSetSpawnAmount_Click);
            // 
            // menuItem3
            // 
            
            // 
            // mniDeleteSpawn
            // 
            this.mniDeleteSpawn.Text = "&Delete";
            this.mniDeleteSpawn.Click += new System.EventHandler(this.mniDeleteSpawn_Click);
            // 
            // mniDeleteAllSpawns
            // 
            this.mniDeleteAllSpawns.Text = "Delete &All";
            this.mniDeleteAllSpawns.Click += new System.EventHandler(this.mniDeleteAllSpawns_Click);
            // 
            // ofdLoadFile
            // 
            this.ofdLoadFile.DefaultExt = "xml";
            this.ofdLoadFile.Filter = "Spawn Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.ofdLoadFile.Title = "Load Spawn File";
            // 
            // sfdSaveFile
            // 
            this.sfdSaveFile.DefaultExt = "xml";
            this.sfdSaveFile.FileName = "Spawns";
            this.sfdSaveFile.Filter = "Spawn Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.sfdSaveFile.Title = "Save Spawn File";
            // 
            // stbMain
            //
            this.stbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                            this.stbMainLabel});
            this.stbMain.Location = new System.Drawing.Point(0, 551);
            this.stbMain.Name = "stbMain";
            this.stbMain.Size = new System.Drawing.Size(792, 22);
            this.stbMain.TabIndex = 3;
            //
            // stbMainLabel
            //
            this.stbMainLabel.Name = "stbMainLabel";
            this.stbMainLabel.Size = new System.Drawing.Size(78, 17);
            this.stbMainLabel.Text = "Spawn Editor";
            // 
            // mncMerge
            // 
            this.mncMerge.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.mniForceMerge});
            // 
            // mniForceMerge
            // 
            this.mniForceMerge.Text = "Force Merge Into Current Map...";
            this.mniForceMerge.Click += new System.EventHandler(this.mniForceMerge_Click);
            // 
            // splLeft
            // 
            this.splLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splLeft.Location = new System.Drawing.Point(168, 0);
            this.splLeft.MinSize = 168;
            this.splLeft.Name = "splLeft";
            this.splLeft.Size = new System.Drawing.Size(5, 551);
            this.splLeft.TabIndex = 4;
            this.splLeft.TabStop = false;
            // 
            // splRight
            // 
            this.splRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.splRight.Location = new System.Drawing.Point(619, 0);
            this.splRight.MinSize = 168;
            this.splRight.Name = "splRight";
            this.splRight.Size = new System.Drawing.Size(5, 551);
            this.splRight.TabIndex = 5;
            this.splRight.TabStop = false;
            // 
            // SpawnEditor
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Controls.Add(this.splLeft);
            this.Controls.Add(this.splRight);
            this.Controls.Add(this.axUOMap);
            this.Controls.Add(this.pnlSpawnDetails);
            this.Controls.Add(this.pnlControls);
            this.Controls.Add(this.stbMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SpawnEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spawn Editor";
            this.Load += new System.EventHandler(this.SpawnEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnTeam)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMaxDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMinDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnHomeRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnMaxCount)).EndInit();
            this.pnlControls.ResumeLayout(false);
            this.grpMapControl.ResumeLayout(false);
            this.grpSpawnTypes.ResumeLayout(false);
            this.pnlSpawnDetails.ResumeLayout(false);
            this.grpSpawnEdit.ResumeLayout(false);
            this.grpSpawnList.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new SpawnEditor());
		}

        private void axUOMap_MouseDownEvent(object sender, MouseEventArgs e)
        {
            // Calculate the map location
            short MapCentreX = this.axUOMap.CtrlToMapX( (short)e.X );
            short MapCentreY = this.axUOMap.CtrlToMapY( (short)e.Y );
            short MapCentreZ = this.axUOMap.GetMapHeight( MapCentreX, MapCentreY );

            // Check which mouse button was pressed down
            if( e.Button == MouseButtons.Left )
            {
                // Left mouse button

                // Check if a selection has been started
                if( this._SelectionWindow != null )
                {
                    // Erase the old selection window if any
                    if( this._SelectionWindow.Index > -1 )
                    {
                        this.axUOMap.RemoveDrawRectAt( this._SelectionWindow.Index );
                        this._SelectionWindow = null;
                    }
                }

                // Create a new selection window
                this._SelectionWindow = new SelectionWindow();
                this._SelectionWindow.X = MapCentreX;
                this._SelectionWindow.Y = MapCentreY;

                // Draw the single point
                this._SelectionWindow.Index = this.axUOMap.AddDrawRect( this._SelectionWindow.X, this._SelectionWindow.Y, (short)1, (short)1, 2, 0x00FFFFFF );

            }
            else if( e.Button == MouseButtons.Right )
            {
                // Right mouse button

                // Check if there is currently a selection window
                // Check if the point where the cursor was clicked is
                // within the selection window
                if( ( this._SelectionWindow != null ) && ( this._SelectionWindow.IsWithinWindow( MapCentreX, MapCentreY ) == true ) )
                {
                    // Trim the spawners name
                    this.txtName.Text = this.txtName.Text.Trim();

                    // Make sure there is a name for the spawner
                    if( this.txtName.Text.Length == 0 )
                    {
                        MessageBox.Show( this, "You must specify a name for the spawner!", "Spawn Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                        return;
                    }

                    // Clear the currently selected spawn point
                    foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
                        spn.Spawn.IsSelected = false;

                    // Add the spawner point
                    SpawnPointNode NewNode = new SpawnPointNode( new SpawnPoint( Guid.NewGuid(), (WorldMap)this.cbxMap.SelectedItem, this._SelectionWindow.Bounds ) );

                    // Set the spawn node properties
                    this.SetSpawn( NewNode, false );

                    // Set the spawns default CentreZ to the minimum short value
                    // to indicate that it should be calculated when loaded
                    NewNode.Spawn.CentreZ = short.MinValue;

                    // Add the spawn point to the list
                    this.tvwSpawnPoints.Nodes.Add( NewNode );
                
                    // Update the total count of the spawners loaded
                    this.lblTotalSpawn.Text = "Total Spawns = " + this.tvwSpawnPoints.Nodes.Count;

                    // Clear the selection window
                    this._SelectionWindow = null;
                }
                else
                {
                    // Clear the currently selected spawn points
                    foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
                        spn.Spawn.IsSelected = false;

                    // Sort the spawn points by the area they take up
                    // then use the spawn points from smallest to largest area
                    ArrayList TheNodes = new ArrayList( this.tvwSpawnPoints.Nodes );
                    TheNodes.Sort( new SpawnPointAreaComparer() );

                    // Check to see if this spawn point already exists
                    foreach( SpawnPointNode spn in TheNodes )
                    {
                        // Make sure the spawn point is on the same map as the current map
                        if( spn.Spawn.Map == (WorldMap)this.cbxMap.SelectedItem )
                        {
                            if( spn.Spawn.IsSameArea( MapCentreX, MapCentreY ) == true )
                            {
                                // Mark this spawn point as the selected one
                                spn.Spawn.IsSelected = true;

                                // Send the go command if possible
                                this.SendGoCommand( spn.Spawn );

                                // Centre the map on the centre point of the spawner
                                this.axUOMap.SetCenter( spn.Spawn.CentreX, spn.Spawn.CentreY );

                                // Set the properties of the spawner in the right window
                                this.txtName.Text = spn.Spawn.SpawnName;
                                this.spnHomeRange.Value = spn.Spawn.SpawnHomeRange;
                                this.spnMaxCount.Value = spn.Spawn.SpawnMaxCount;
                                this.spnMinDelay.Value = spn.Spawn.SpawnMinDelay;
                                this.spnMaxDelay.Value = spn.Spawn.SpawnMaxDelay;
                                this.spnTeam.Value = spn.Spawn.SpawnTeam;
                                this.chkGroup.Checked = spn.Spawn.SpawnIsGroup;
                                this.chkRunning.Checked = spn.Spawn.SpawnIsRunning;
                                this.chkHomeRangeIsRelative.Checked = spn.Spawn.SpawnHomeRangeIsRelative;

                                // Set the selected node
                                this.tvwSpawnPoints.SelectedNode = spn;
                                this.tvwSpawnPoints.SelectedNode.EnsureVisible();
                                this.SetSelectedSpawnTypes();

                                break;
                            }
                        }
                    }
                }
            }
            
            // Refresh the spawn points
            this.RefreshSpawnPoints();
        }

        private void axUOMap_MouseUpEvent(object sender, MouseEventArgs e)
        {
            // Calculate the map location
            short MapCentreX = this.axUOMap.CtrlToMapX( (short)e.X );
            short MapCentreY = this.axUOMap.CtrlToMapY( (short)e.Y );
            short MapCentreZ = this.axUOMap.GetMapHeight( MapCentreX, MapCentreY );

            // Check which mouse button was pressed down ( don't bother, it is always 0)
            // Check if the coordinates are the same as the mouse down event
            if( this._SelectionWindow != null )
            {
                // Right mouse button with no selection window, centres the map
                if( ( this._SelectionWindow.X == MapCentreX ) && ( this._SelectionWindow.Y == MapCentreY ) )
                {
                    // Erase the old selection window if any
                    if( this._SelectionWindow.Index > -1 )
                    {
                        this.axUOMap.RemoveDrawRectAt( this._SelectionWindow.Index );
                        this._SelectionWindow = null;
                    }

                    // Centre the map
                    this.axUOMap.SetCenter( MapCentreX, MapCentreY );

                    // Refresh the spawn points
                    this.RefreshSpawnPoints();
                }
            }
        }

        private void axUOMap_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            // Calculate the map location
            short MapCentreX = this.axUOMap.CtrlToMapX( (short)e.X );
            short MapCentreY = this.axUOMap.CtrlToMapY( (short)e.Y );
            short MapCentreZ = this.axUOMap.GetMapHeight( MapCentreX, MapCentreY );

            if( e.Button == MouseButtons.None )
            {
                string Tip = string.Empty;

                // Check if the tool tip summary should be displayed
                if( this.chkShowMapTip.Checked == true )
                {
                    // Sort the spawn points by the area they take up
                    // then use the spawn points from smallest to largest area
                    ArrayList TheNodes = new ArrayList( this.tvwSpawnPoints.Nodes );
                    TheNodes.Sort( new SpawnPointAreaComparer() );

                    // Check to see if the mouse is above a particular spawn point
                    foreach( SpawnPointNode spn in TheNodes )
                    {
                        // Make sure the spawn point is on the same map as the current map
                        if( spn.Spawn.Map == (WorldMap)this.cbxMap.SelectedItem )
                        {
                            if( spn.Spawn.IsSameArea( MapCentreX, MapCentreY, 1 ) == true )
                            {
                                Tip = spn.Spawn.ToString();
                                break;
                            }
                        }
                    }

                }
                
                // Set the tool tip for the spawn point, if any
                this.ttpSpawnInfo.SetToolTip( this.axUOMap, Tip );

                // Set the current map location in the status bar if there is no selection window
                if( this._SelectionWindow == null )
                    this.stbMainLabel.Text = string.Format( "[X={0} Y={1} H={2}]", MapCentreX, MapCentreY, MapCentreZ );
            }
            else if( e.button == 1 )
            {
                // Check if a selection has been started
                if( this._SelectionWindow != null )
                {
                    // Erase the old window and draw a new one
                    if( this._SelectionWindow.Index > -1 )
                    {
                        this.axUOMap.RemoveDrawRectAt( this._SelectionWindow.Index );
                        this._SelectionWindow.Index = -1;
                    }

                    // Set the 2nd coordinates of the window
                    this._SelectionWindow.Width = (short)(MapCentreX - this._SelectionWindow.X);
                    this._SelectionWindow.Height = (short)(MapCentreY - this._SelectionWindow.Y);

                    // Reset the selected existing spawn point if any
                    foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
                        spn.Spawn.IsSelected = false;

                    // Refresh the spawn points
                    this.RefreshSpawnPoints();

                    // Reset the next spawners default name
                    this.txtName.Text = this._CfgDialog.CfgSpawnNameValue + this.tvwSpawnPoints.Nodes.Count;
                    this.txtName.Refresh();

                    // Draw the window
                    this._SelectionWindow.Index = this.axUOMap.AddDrawRect( this._SelectionWindow.X, this._SelectionWindow.Y, this._SelectionWindow.Width, this._SelectionWindow.Height, 2, 0x00FFFFFF );

                    // Set the current map location in the status bar
                    this.stbMainLabel.Text = string.Format( "[X1={0} Y1={1}] TO [X2={2} Y2={3}] (Width={4}, Height={5})", this._SelectionWindow.X, this._SelectionWindow.Y, ( this._SelectionWindow.X + this._SelectionWindow.Width ), ( this._SelectionWindow.Y + this._SelectionWindow.Height ), this._SelectionWindow.Width, this._SelectionWindow.Height );
                }
            }
        }

        internal void RefreshSpawnPoints()
        {
            // Erase all of the rectangles and redraw them
            this.axUOMap.RemoveDrawRects();
            this.axUOMap.RemoveDrawObjects();

            // Flag to indicate a selected spawn point
            bool FoundSelection = false;
            
            // Redraw all of the other spawn points the unselected colour
            foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
            {
                // Draw the new rectangle based on if this is the selected index or not
                if( spn.Spawn.IsSelected == true )
                {
                    if( ( spn.Spawn.Map == (WorldMap)this.cbxMap.SelectedItem ) &&
                        ( this.chkShowSpawns.Checked == true ) )
                        spn.Spawn.Index = this.axUOMap.AddDrawRect( (short)spn.Spawn.Bounds.X, (short)spn.Spawn.Bounds.Y, (short)spn.Spawn.Bounds.Width, (short)spn.Spawn.Bounds.Height, 2, 0x00FFFF00 );

                    FoundSelection = true;

                    // Check if the currently selected node is a child node
                    if( ( this.tvwSpawnPoints.SelectedNode != null ) &&
                        ( ( this.tvwSpawnPoints.SelectedNode.Parent == null ) ||
                          ( this.tvwSpawnPoints.SelectedNode.Parent != spn ) ) )
                    {
                        this.tvwSpawnPoints.SelectedNode = spn;
                        spn.BackColor = Color.Yellow;
                        spn.EnsureVisible();
                    }
                }
                else
                {
                    // Check that the current spawn point belongs to this map
                    // and that the user wants the spawn points drawn
                    if( ( spn.Spawn.Map == (WorldMap)this.cbxMap.SelectedItem ) &&
                        ( this.chkShowSpawns.Checked == true ) )
                        spn.Spawn.Index = this.axUOMap.AddDrawRect( (short)spn.Spawn.Bounds.X, (short)spn.Spawn.Bounds.Y, (short)spn.Spawn.Bounds.Width, (short)spn.Spawn.Bounds.Height, 2, 0x000000FF );

                    spn.BackColor = this.tvwSpawnPoints.BackColor;
                }
            }

            
            // Enable the update and delete buttons if something is selected
            if( FoundSelection == true )
            {
                this.btnUpdateSpawn.Enabled = true;
                this.btnDeleteSpawn.Enabled = true;
                this.btnMove.Enabled = true;
            }
            else
            {
                this.btnUpdateSpawn.Enabled = false;
                this.btnDeleteSpawn.Enabled = false;
                this.btnMove.Enabled = false;
            }

            this.axUOMap.Refresh();
        }

        private short GetZoomAdjustedSize( short DefaultSize )
        {
            // Zoom level zero is no change, just use the default size
            // any other level must be compensated for
            if( this.axUOMap.ZoomLevel == 0 )
                return DefaultSize;
            else
            {
                if( this.axUOMap.ZoomLevel > 0 )
                {
                    // If the zoom factor is greater than zero, increase the size
                    short NewValue = (short)(Math.Pow( 2, this.axUOMap.ZoomLevel ) * DefaultSize);
                    return NewValue;
                }
                else
                {
                    // If the zoom factor is smaller than zero, decrease the size
                    short NewValue = (short)(Math.Pow( 2, this.axUOMap.ZoomLevel ) * DefaultSize);
                    return ( NewValue <= 0 ? (short)1 : NewValue );
                }
            }
        }

        private void trkZoom_ValueChanged(object sender, System.EventArgs e)
        {
            this.axUOMap.ZoomLevel = (short)this.trkZoom.Value;
            this.stbMainLabel.Text = this.DefaultZoomLevelText + this.axUOMap.ZoomLevel;
            this.RefreshSpawnPoints();
        }

        private void chkDrawStatics_CheckedChanged(object sender, System.EventArgs e)
        {
            this.axUOMap.DrawStatics = this.chkDrawStatics.Checked;
        }

        private void TypeSelectionChanged(object sender, System.EventArgs e)
        {
            if( sender is RadioButton )
            {
                RadioButton radButton = (RadioButton)sender;
                if( radButton.Checked == true )
                    this.LoadTypes();
            }
        }

        private void LoadTypes()
        {
            // Sort the check list
            this.clbRunUOTypes.Sorted = false;

            // Clear the list
            this.clbRunUOTypes.Items.Clear();

            // Load all types by default
            foreach( Type t in this._RunUOScriptTypes )
            {
                if( ( t.IsAbstract == true ) || ( t.IsPublic == false ) || ( t.IsClass == false ) )
                    continue;

                // Check if the current type has the [Constructable] attribute its default constructor.
                // Only want to show classes with constructable types in the list.
                ConstructorInfo ci = t.GetConstructor( Type.EmptyTypes );
                if( ci == null )
                    continue;

                object[] attributes = ci.GetCustomAttributes( true );
                bool FoundConstructable = false;
                foreach( Attribute a in attributes )
                {
                    if( string.Compare( a.GetType().Name, "ConstructableAttribute", true ) == 0 )
                    {
                        FoundConstructable = true;
                        break;
                    }
                }

                // Add any constructable objects
                if( ( FoundConstructable == true ) &&
                    ( ( this.radShowAll.Checked == true ) ||
                      ( this.radShowItemsOnly.Checked == true ) ) )
                {
                    if( ( t.BaseType != null ) &&
                        ( t.BaseType.FullName.StartsWith( "Server.Item" ) ) )
                    {
                        // Add the item
                        this.clbRunUOTypes.Items.Add( t.Name );
                    }
                }

                // Add any mobiles required
                if( ( FoundConstructable == true ) &&
                    ( ( this.radShowAll.Checked == true ) ||
                      ( this.radShowMobilesOnly.Checked == true ) ) )
                {
                    if( ( t.BaseType != null ) &&
                        ( t.BaseType.FullName.StartsWith( "Server.Mobile" ) ) )
                    {
                        // Add the mobile
                        this.clbRunUOTypes.Items.Add( t.Name );
                    }
                }
            }

            // Sort the check list
            this.clbRunUOTypes.Sorted = true;

            // Update the total coun of the types loaded
            this.lblTotalTypesLoaded.Text = "Types Loaded = " + this.clbRunUOTypes.Items.Count;

            // Set the selected items in the type list based on the currently
            this.SetSelectedSpawnTypes();
        }

        private void SetSelectedSpawnTypes()
        {
            // Set the selected items in the type list based on the currently
            // selected tree node if any
            // Check if there is a node selected
            if( this.tvwSpawnPoints.SelectedNode != null )
            {
                SpawnPointNode SelectedSpawnNode = this.tvwSpawnPoints.SelectedNode as SpawnPointNode;
                SpawnObjectNode SelectedObjectNode = this.tvwSpawnPoints.SelectedNode as SpawnObjectNode;

                // Check if the selected node is a object node
                if( SelectedObjectNode != null )
                    SelectedSpawnNode = (SpawnPointNode)SelectedObjectNode.Parent;

                this.clbRunUOTypes.ClearSelected();
                for( int x = 0; x < this.clbRunUOTypes.Items.Count; x++ )
                {
                    // Check if the current item is one of the spawn objects
                    bool IsObject = false;
                    foreach( SpawnObject so in SelectedSpawnNode.Spawn.SpawnObjects )
                    {
                        if( so.TypeName.ToUpper() == this.clbRunUOTypes.Items[x].ToString().ToUpper() )
                        {
                            IsObject = true;
                            break;
                        }
                    }

                    this.clbRunUOTypes.SetItemChecked( x, IsObject );
                }
            }
            else
            {
                // Nothing is selected so clear the type selection
                // Clear the checked items in the type list
                this.clbRunUOTypes.ClearSelected();
                for( int x = 0; x < this.clbRunUOTypes.Items.Count; x++ )
                    this.clbRunUOTypes.SetItemChecked( x, false );
            }
        }

        private void SaveSpawnFile( string FilePath )
        {
            try
            {
                // Save the spawn file
                DataSet ds = new DataSet( SpawnDataSetName );
                ds.Tables.Add( SpawnTablePointName );

                // Create spawn point schema
                ds.Tables[SpawnTablePointName].Columns.Add( "Name" );
                ds.Tables[SpawnTablePointName].Columns.Add( "UniqueId" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Map" );
                ds.Tables[SpawnTablePointName].Columns.Add( "X" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Y" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Width" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Height" );
                ds.Tables[SpawnTablePointName].Columns.Add( "CentreX" );
                ds.Tables[SpawnTablePointName].Columns.Add( "CentreY" );
                ds.Tables[SpawnTablePointName].Columns.Add( "CentreZ" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Range" );
                ds.Tables[SpawnTablePointName].Columns.Add( "MaxCount" );
                ds.Tables[SpawnTablePointName].Columns.Add( "MinDelay" );
                ds.Tables[SpawnTablePointName].Columns.Add( "MaxDelay" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Team" );
                ds.Tables[SpawnTablePointName].Columns.Add( "IsGroup" );
                ds.Tables[SpawnTablePointName].Columns.Add( "IsRunning" );
                ds.Tables[SpawnTablePointName].Columns.Add( "IsHomeRangeRelative" );
                ds.Tables[SpawnTablePointName].Columns.Add( "Objects" );

                // Add each spawn point to the new table
                foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
                {
                    SpawnPoint sp = spn.Spawn;
                    DataRow dr = ds.Tables[SpawnTablePointName].NewRow();
                    dr["Name"] = sp.SpawnName;
                    dr["UniqueId"] = sp.UnqiueId.ToString();
                    dr["Map"] = sp.Map.ToString();
                    dr["X"] = sp.Bounds.X;
                    dr["Y"] = sp.Bounds.Y;
                    dr["Width"] = sp.Bounds.Width;
                    dr["Height"] = sp.Bounds.Height;
                    dr["CentreX"] = sp.CentreX;
                    dr["CentreY"] = sp.CentreY;
                    dr["CentreZ"] = sp.CentreZ;
                    dr["Range"] = sp.SpawnHomeRange;
                    dr["MaxCount"] = sp.SpawnMaxCount;
                    dr["MinDelay"] = sp.SpawnMinDelay;
                    dr["MaxDelay"] = sp.SpawnMaxDelay;
                    dr["Team"] = sp.SpawnTeam;
                    dr["IsGroup"] = sp.SpawnIsGroup;
                    dr["IsRunning"] = sp.SpawnIsRunning;
                    dr["IsHomeRangeRelative"] = sp.SpawnHomeRangeIsRelative;
                    dr["Objects"] = sp.GetSerializedObjectList();
                    ds.Tables[SpawnTablePointName].Rows.Add( dr );
                }

                // Write the data set out as XML
                ds.WriteXml( FilePath );
            }
            catch( System.Exception se )
            {
                MessageBox.Show( this, "Failed to save file [" + FilePath + "] for the following reason:" + Environment.NewLine + se.ToString(), "Save Failure", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void LoadSpawnFile( string FilePath, WorldMap ForceMap )
        {
            try
            {
                // Load the spawn file
                DataSet ds = new DataSet( SpawnDataSetName );
                ds.ReadXml( FilePath );

                RectangleConverter rectConvert = new RectangleConverter();

                // Add each spawn point to the new table
                foreach( DataRow dr in ds.Tables[SpawnTablePointName].Rows )
                {
                    // Check if the map should be forced to the specified map
                    if( ForceMap != WorldMap.Internal )
                    {
                        // Check if the Map column exists
                        if( ds.Tables[SpawnTablePointName].Columns.Contains( "Map" ) == false )
                            ds.Tables[SpawnTablePointName].Columns.Add( "Map" );

                        // Check if the UnqiueId column exists
                        if( ds.Tables[SpawnTablePointName].Columns.Contains( "UniqueId" ) == false )
                            ds.Tables[SpawnTablePointName].Columns.Add( "UniqueId" );

                        dr["Map"] = ForceMap.ToString();
                        dr["UniqueId"] = Guid.NewGuid().ToString();
                    }

                    // Create the spawn point from the data row
                    SpawnPoint sp = new SpawnPoint( dr );

                    // Add it to the list of spawn points
                    SpawnPointNode NewNode = new SpawnPointNode( sp );
                    this.tvwSpawnPoints.Nodes.Add( NewNode );
                }

                // Update the total count of the spawners loaded
                this.lblTotalSpawn.Text = "Total Spawns = " + this.tvwSpawnPoints.Nodes.Count;

                // Set the next spawners default name
                this.txtName.Text = this._CfgDialog.CfgSpawnNameValue + this.tvwSpawnPoints.Nodes.Count;
            
                // Refresh the spawn points
                this.RefreshSpawnPoints();
            }
            catch( System.Exception se )
            {
                MessageBox.Show( this, "Failed to load file [" + FilePath + "] for the following reason:" + Environment.NewLine + se.ToString(), "Load Failure", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void btnLoadSpawn_Click(object sender, System.EventArgs e)
        {
            try
            {
                // Clears the current spawns and loads a new file
                this.ofdLoadFile.Title = "Load Spawn File";
                if( this.ofdLoadFile.ShowDialog( this ) == DialogResult.OK )
                {
                    // Update the screen
                    this.Text = "Spawn Editor - " + this.ofdLoadFile.FileName;
                    this.stbMainLabel.Text = string.Format( "Loading {0}...", this.ofdLoadFile.FileName );
                    this.tvwSpawnPoints.Nodes.Clear();
                    this.Refresh();
                    this.LoadSpawnFile( this.ofdLoadFile.FileName, WorldMap.Internal );
                }
            }
            finally
            {
                this.stbMainLabel.Text = "Finished loading spawn file.";
            }
        }

        private void btnMergeSpawn_Click(object sender, System.EventArgs e)
        {
            try
            {
                // Keeps the current spawns and loads another file
                this.ofdLoadFile.Title = "Merge Spawn File";
                if( this.ofdLoadFile.ShowDialog( this ) == DialogResult.OK )
                {
                    // Update the screen
                    this.Text = "Spawn Editor - " + this.ofdLoadFile.FileName;
                    this.stbMainLabel.Text = string.Format( "Merging {0}...", this.ofdLoadFile.FileName );
                    this.Refresh();
                    this.LoadSpawnFile( this.ofdLoadFile.FileName, WorldMap.Internal );
                }
            }
            finally
            {
                this.stbMainLabel.Text = "Finished merging spawn file.";
            }
        }

        private void btnSaveSpawn_Click(object sender, System.EventArgs e)
        {
            try
            {
                // Saves the current list of spawns out to a file
                if( this.sfdSaveFile.ShowDialog( this ) == DialogResult.OK )
                {
                    // Update the screen
                    this.Text = "Spawn Editor - " + this.sfdSaveFile.FileName;
                    this.stbMainLabel.Text = string.Format( "Saving {0}...", this.ofdLoadFile.FileName );
                    this.Refresh();
                    this.SaveSpawnFile( this.sfdSaveFile.FileName );
                }
            }
            finally
            {
                this.stbMainLabel.Text = "Finished saving spawn file.";
            }
        }

        private void btnResetTypes_Click(object sender, System.EventArgs e)
        {
            this.clbRunUOTypes.ClearSelected();
            for( int x = 0; x < this.clbRunUOTypes.Items.Count; x++ )
                this.clbRunUOTypes.SetItemChecked( x, false );
        }

        private void btnUpdateSpawn_Click(object sender, System.EventArgs e)
        {
            // Get the selected node from the tree view
            TreeNode SelectedNode = this.tvwSpawnPoints.SelectedNode;

            SpawnPointNode SelectedSpawnNode = SelectedNode as SpawnPointNode;
            SpawnObjectNode SelectedObjectNode = SelectedNode as SpawnObjectNode;

            // Check if the selected node is a object node
            if( SelectedObjectNode != null )
                SelectedSpawnNode = SelectedObjectNode.Parent as SpawnPointNode;

            // Make sure the spawn node is valid
            if( SelectedSpawnNode != null )
            {
                // Confirm the action with the user
                //if( MessageBox.Show( this, "Are you sure you want to update spawn [" + spn.Spawn.SpawnName + "] with the new settings?", "Update Spawn", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Update the spawn point
                    this.SetSpawn( SelectedSpawnNode, true );
                }
            }

            this.RefreshSpawnPoints();
        }

        private void SetSpawn( SpawnPointNode SpawnNode, bool IsUpdate )
        {
            // Set the spawn point
            // Make sure there is a name for the spawner
            this.txtName.Text = this.txtName.Text.Trim();

            // Check that the name length is valid
            if( this.txtName.Text.Length == 0 )
            {
                MessageBox.Show( this, "You must specify a name for the spawner!", "Spawn Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return;
            }

            SpawnNode.Spawn.SpawnName = this.txtName.Text;
            SpawnNode.Spawn.SpawnHomeRangeIsRelative = this.chkHomeRangeIsRelative.Checked;
            SpawnNode.Spawn.SpawnHomeRange = (int)this.spnHomeRange.Value;
            SpawnNode.Spawn.SpawnIsGroup = this.chkGroup.Checked;
            SpawnNode.Spawn.SpawnIsRunning = this.chkRunning.Checked;
            SpawnNode.Spawn.SpawnMaxCount = (int)this.spnMaxCount.Value;
            SpawnNode.Spawn.SpawnMaxDelay = (int)this.spnMaxDelay.Value;
            SpawnNode.Spawn.SpawnMinDelay = (int)this.spnMinDelay.Value;
            SpawnNode.Spawn.SpawnTeam = (int)this.spnTeam.Value;

            // Check each item in the type list against the current object list
            // The list can only be added to, not removed.  Removing of spawn
            // objects must be done in the spawn list by right-clicking on an item
            // and deleting it.
            foreach( string s in this.clbRunUOTypes.CheckedItems )
            {
                bool FoundObject = false;

                // Check if the item exists in the spawn list
                foreach( SpawnObject so in SpawnNode.Spawn.SpawnObjects )
                {
                    if( s.ToUpper() == so.TypeName.ToUpper() )
                    {
                        FoundObject = true;
                        break;
                    }
                }

                // If the object was not found, add it
                if( FoundObject == false )
                    SpawnNode.Spawn.SpawnObjects.Add( new SpawnObject( s, 1 ) );
            }

            // Refresh the node
            SpawnNode.UpdateNode();
        }

        private void btnDeleteSpawn_Click(object sender, System.EventArgs e)
        {
            this.mniDeleteSpawn_Click( sender, e );
        }

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

        private void btnConfigure_Click(object sender, System.EventArgs e)
        {
            this._CfgDialog.ShowDialog();
        }

        private void mncSpawns_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if( this.mncSpawns.SourceControl == this.tvwSpawnPoints )
            {
                // Hide all menu items
                foreach( ToolStripItem mi in this.mncSpawns.Items )
                    mi.Visible = false;

                // Get the selected spawn point OR spawn object
                if( this.tvwSpawnPoints.SelectedNode is SpawnPointNode )
                {
                    this.mniDeleteSpawn.Visible = true;
                }
                else if( this.tvwSpawnPoints.SelectedNode is SpawnObjectNode )
                {
                    this.mniSetSpawnAmount.Visible = true;
                    this.mniDeleteSpawn.Visible = true;
                }

                if( this.tvwSpawnPoints.Nodes.Count > 0 )
                    this.mniDeleteAllSpawns.Visible = true;
            }
        }

        private void btnRestoreSpawnDefaults_Click(object sender, System.EventArgs e)
        {
            this.LoadDefaultSpawnValues();
        }

        private void tvwSpawnPoints_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Get the node at the selected point
            TreeNode SelectedNode = this.tvwSpawnPoints.GetNodeAt( e.X, e.Y );

            // Check if the node is valid
            if( SelectedNode != null )
            {
                // Force a refresh of the tree
                this.tvwSpawnPoints.Refresh();

                SpawnPointNode SelectedSpawnNode = SelectedNode as SpawnPointNode;
                SpawnObjectNode SelectedObjectNode = SelectedNode as SpawnObjectNode;

                // Check if the selected node is a object node
                if( SelectedObjectNode != null )
                    SelectedSpawnNode = (SpawnPointNode)SelectedObjectNode.Parent;

                // Make sure the object is valid
                if( SelectedSpawnNode != null )
                {
                    // Get the spawn point for the node
                    SpawnPoint SelectedSpawn = SelectedSpawnNode.Spawn;

                    // Clear the currently selected spawn point
                    foreach( SpawnPointNode spn in this.tvwSpawnPoints.Nodes )
                        spn.Spawn.IsSelected = false;

                    // Set the current spawn point to be selected
                    SelectedSpawn.IsSelected = true;

                    // Send the go command if possible
                    this.SendGoCommand( SelectedSpawn );

                    // Check if the current map is the proper one
                    if( SelectedSpawn.Map != (WorldMap)this.cbxMap.SelectedItem )
                        this.cbxMap.SelectedItem = SelectedSpawn.Map;

                    // Centre the map on the centre point of the spawner
                    this.axUOMap.SetCenter( SelectedSpawn.CentreX, SelectedSpawn.CentreY );

                    // Set the properties of the spawner in the right window
                    this.txtName.Text = SelectedSpawn.SpawnName;
                    this.spnHomeRange.Value = SelectedSpawn.SpawnHomeRange;
                    this.spnMaxCount.Value = SelectedSpawn.SpawnMaxCount;
                    this.spnMinDelay.Value = SelectedSpawn.SpawnMinDelay;
                    this.spnMaxDelay.Value = SelectedSpawn.SpawnMaxDelay;
                    this.spnTeam.Value = SelectedSpawn.SpawnTeam;
                    this.chkGroup.Checked = SelectedSpawn.SpawnIsGroup;
                    this.chkRunning.Checked = SelectedSpawn.SpawnIsRunning;
                    this.chkHomeRangeIsRelative.Checked = SelectedSpawn.SpawnHomeRangeIsRelative;
                  
                    // Refresh the spawn points
                    this.RefreshSpawnPoints();
                }

                if( e.Button == MouseButtons.Right )
                {
                    // Right mouse button is clicked
                    
                    // Select the node that was right clicked
                    this.tvwSpawnPoints.SelectedNode = SelectedNode;

                    // Show the context menu
                    this.mncSpawns.Show( this.tvwSpawnPoints, new Point( e.X, e.Y ) );
                }

                // Set the selected spawn types
                this.SetSelectedSpawnTypes();
            }
        }

        private void mniDeleteAllSpawns_Click(object sender, System.EventArgs e)
        {
            // Get the selected node from the tree view
            TreeNode SelectedNode = this.tvwSpawnPoints.SelectedNode;

            if( SelectedNode is SpawnObjectNode )
            {
                SpawnObjectNode ObjectNode = (SpawnObjectNode)SelectedNode;
                SpawnPointNode SpawnNode = (SpawnPointNode)SelectedNode.Parent;
            
                // Confirm the action with the user
                if( MessageBox.Show( this, "Are you sure you want to delete all objects from spawn [" + SpawnNode.Spawn.SpawnName + "]?", "Delete All Spawn Objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Remove ALL of the spawn objects from this spawn
                    SpawnNode.Nodes.Clear();
                }
            }
            else
            {
                // Confirm the action with the user
                if( MessageBox.Show( this, "Are you sure you want to delete ALL spawns from the list?", "Delete All Spawns", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Remove all of the spawns from the list
                    this.tvwSpawnPoints.Nodes.Clear();
                }
            }

            // Set the selected types
            this.SetSelectedSpawnTypes();

            this.RefreshSpawnPoints();
        }

        private void mniDeleteSpawn_Click(object sender, System.EventArgs e)
        {
            // Get the selected node from the tree view
            TreeNode SelectedNode = this.tvwSpawnPoints.SelectedNode;

            // What are we to delete, spawn or spawn object
            if( SelectedNode is SpawnPointNode )
            {
                SpawnPointNode SpawnNode = (SpawnPointNode)SelectedNode;
            
                // Confirm the action with the user
                if( MessageBox.Show( this, "Are you sure you want to delete spawn [" + SpawnNode.Spawn.SpawnName + "] from the list?", "Delete Spawn", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Remove all of the spawns from the list
                    SpawnNode.Remove();
                }
            }
            else if( SelectedNode is SpawnObjectNode )
            {
                SpawnObjectNode ObjectNode = (SpawnObjectNode)SelectedNode;
            
                // Confirm the action with the user
                if( MessageBox.Show( this, "Are you sure you want to delete object [" + ObjectNode.SpawnObject.TypeName + "] from the spawn?", "Delete Spawn Object", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Remove the spawn object from the spawn
                    SpawnPointNode SpawnNode = (SpawnPointNode)ObjectNode.Parent;
                    SpawnNode.Spawn.SpawnObjects.Remove( ObjectNode.SpawnObject );
                    ObjectNode.Remove();
                }
            }

            // Set the selected types
            this.SetSelectedSpawnTypes();

            this.RefreshSpawnPoints();
        }

        private void mniSetSpawnAmount_Click(object sender, System.EventArgs e)
        {
            // Get the selected node
            SpawnObjectNode ObjectNode = this.tvwSpawnPoints.SelectedNode as SpawnObjectNode;

            // Make sure the item is valid
            if( ObjectNode != null )
            {
                Amount SpawnAmount = new Amount( ObjectNode.SpawnObject.TypeName, ObjectNode.SpawnObject.Count );

                // Make sure the user clicked OK in the dialog before updating the amount
                if( SpawnAmount.ShowDialog( this ) == DialogResult.OK )
                {
                    ObjectNode.SpawnObject.Count = SpawnAmount.SpawnAmount;
                    ObjectNode.UpdateNode();
                }
            }
        }

        private void chkShowSpawns_CheckedChanged(object sender, System.EventArgs e)
        {
            this.RefreshSpawnPoints();
        }

        private void btnMove_Click(object sender, System.EventArgs e)
        {
            SpawnPointNode SelectedSpawnNode = this.tvwSpawnPoints.SelectedNode as SpawnPointNode;
            SpawnObjectNode SelectedObjectNode = this.tvwSpawnPoints.SelectedNode as SpawnObjectNode;

            // Check if the selected node is a object node
            if( SelectedObjectNode != null )
                SelectedSpawnNode = (SpawnPointNode)SelectedObjectNode.Parent;

            if( SelectedSpawnNode != null )
            {
                Area area = new Area( SelectedSpawnNode.Spawn, this );
                area.ShowDialog( this );
            }
        }

        private void cbxMap_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // Remove the selection window
            this._SelectionWindow = null;

            // Refresh the status bar to show the current map
            this.stbMainLabel.Text = this.cbxMap.SelectedItem.ToString() + " Map Selected";
            this.stbMain.Refresh();

            switch( (WorldMap)this.cbxMap.SelectedItem )
            {
                case WorldMap.Trammel:
                    this.axUOMap.MapFile = (short)WorldMap.Trammel;
                    this.axUOMap.SetCenter( 3072, 2048 );
                    this.axUOMap.xCenter = 3072;                    this.axUOMap.yCenter = 2048;                    break;

                case WorldMap.Felucca:
                    this.axUOMap.MapFile = (short)WorldMap.Trammel; // Same viewable map as Trammel
                    this.axUOMap.SetCenter( 3072, 2048 );                    this.axUOMap.xCenter = 3072;                    this.axUOMap.yCenter = 2048;                    break;

                case WorldMap.Ilshenar:
                    this.axUOMap.MapFile = (short)WorldMap.Ilshenar;
                    this.axUOMap.SetCenter( 1150, 800 );                    this.axUOMap.xCenter = 1150;                    this.axUOMap.yCenter = 800;                    break;

                case WorldMap.Malas:
                    this.axUOMap.MapFile = (short)WorldMap.Malas;
                    this.axUOMap.SetCenter( 1280, 1024 );                    this.axUOMap.xCenter = 1280;                    this.axUOMap.yCenter = 1024;                    break;
            }

            // Refresh the spawn points for the current map
            this.RefreshSpawnPoints();
        }

        [DllImport("User32.dll",EntryPoint="SendMessageA")] 
        public extern static int SendMessage(int _WindowHandler, int _WM_USER, int _data, int _id);

        [DllImport("User32.dll",EntryPoint="FindWindowA")] 
        public extern static int FindWindow(string _ClassName, string _WindowName);
        
        [DllImport("User32.dll",EntryPoint="SetForegroundWindow")] 
        public extern static bool SetForegroundWindow(int hWnd);

        public void SendGoCommand( SpawnPoint Spawn )
        {
            // Only send the go command if selected by the user
            if( this.chkSyncUO.Checked == true )
            {
                // Get the clients handle
                int UoClientHandle = FindWindow( this._CfgDialog.CfgUoClientWindowValue, null );

                if( UoClientHandle > 0) 
                {
                    // Create the go command
                    string GoCommand = string.Empty;
                
                    // Check if the go command should use the 2 argument form
                    if( Spawn.CentreZ == short.MinValue )
                        GoCommand = string.Format( "{0}SpawnEditorGo {1} {2} {3}", this._CfgDialog.CfgRunUoCmdPrefix, Spawn.Map, Spawn.CentreX, Spawn.CentreY );
                    this.stbMainLabel.Text = string.Format( "Loading {0} into {1}...", this.ofdLoadFile.FileName, ForceMap.ToString() );
                this.stbMainLabel.Text = "Finished loading spawn file.";

                    this.stbMainLabel.Text = string.Format( "Merging {0} into {1}...", this.ofdLoadFile.FileName, ForceMap.ToString() );
                this.stbMainLabel.Text = "Finished merging spawn file.";

                    // Send the command
                    for( int x = 0; x < GoCommand.Length; x++ )
                    {
                        SendMessage( UoClientHandle, 256, 69, 1 );
                        SendMessage( UoClientHandle, 258, GoCommand[x], 1 );
                        SendMessage( UoClientHandle, 257, 69, 1 );
                    }

                    // Send the carriage return to execute the command
                    SendMessage( UoClientHandle, 258, 13, 0 );
                }
                else
                {
                    MessageBox.Show( string.Format( "{0} could not be found. Make sure the client is started and that the 'Client Window' option in Setup is correct.", this._CfgDialog.CfgUoClientWindowValue + " Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning ) );
                }
            }
        }

        private void mniForceLoad_Click(object sender, System.EventArgs e)
        {
            try
            {
                WorldMap ForceMap = (WorldMap)this.cbxMap.SelectedItem;

                // Forceably loads the spawn file selected into the current map
                this.ofdLoadFile.Title = "Force Load Spawn File Into " + ForceMap.ToString();
                if( this.ofdLoadFile.ShowDialog( this ) == DialogResult.OK )
                {
                    this.Refresh();
                    // Update the status bar
                    this.stbMain.Text = string.Format( "Loading {0} into {1}...", this.ofdLoadFile.FileName, ForceMap.ToString() );
                    this.tvwSpawnPoints.Nodes.Clear();
                    this.LoadSpawnFile( this.ofdLoadFile.FileName, ForceMap );
                }
            }
            finally
            {
                this.stbMain.Text = "Finished loading spawn file.";
            }
        }

        private void mniForceMerge_Click(object sender, System.EventArgs e)
        {
            try
            {
                WorldMap ForceMap = (WorldMap)this.cbxMap.SelectedItem;

                // Keeps the current spawns and loads another file
                this.ofdLoadFile.Title = "Merge Spawn File Into " + ForceMap.ToString();
                if( this.ofdLoadFile.ShowDialog( this ) == DialogResult.OK )
                {
                    this.Refresh();
                    // Update the status bar
                    this.stbMain.Text = string.Format( "Merging {0} into {1}...", this.ofdLoadFile.FileName, ForceMap.ToString() );
                    this.LoadSpawnFile( this.ofdLoadFile.FileName, WorldMap.Internal );
                }
            }
            finally
            {
                this.stbMain.Text = "Finished merging spawn file.";
            }
        }

        public class SelectionWindow
        {
            public int Index = -1;
            public short X;
            public short Y;
            public short Width;
            public short Height;

            public Rectangle Bounds
            {
                get{ return new Rectangle( this.X, this.Y, this.Width, this.Height ); }
            }

            public bool IsWithinWindow( short MapX, short MapY )
            {
                // Check to see if the map coordinates provided
                // fall with the current selection window boundaries
                return new Rectangle( this.X, this.Y, this.Width, this.Height ).Contains( MapX, MapY );
            }
        }

    }

    public class SpawnObject
    {
        public string TypeName;
        public int Count;

        public SpawnObject( string Name, int MaxCount )
        {
            this.TypeName = Name;
            this.Count = MaxCount;
        }

        public override string ToString()
        {
            return this.TypeName + "=" + this.Count;
        }
    }

    public class SpawnPointAreaComparer : IComparer
    {
        public int Compare( object A, object B )
        {
            if( ( A is SpawnPointNode ) && ( B is SpawnPointNode ) )
            {
                SpawnPoint SpA = ((SpawnPointNode)A).Spawn;
                SpawnPoint SpB = ((SpawnPointNode)B).Spawn;

                // Return the size difference between area A and area B
                return ( SpA.Area - SpB.Area );
            }

            return 0;
        }
    }

    public class SpawnPoint
    {
        public short CentreX;
        public short CentreY;
        public short CentreZ;
        public short Range;
        public int Index;
        private Rectangle _Bounds;
        public bool IsSelected;
        public string XmlFileName;
        public string SpawnName;
        private int _SpawnHomeRange;
        public bool SpawnHomeRangeIsRelative;
        public int SpawnMaxCount;
        public int SpawnMinDelay;
        public int SpawnMaxDelay;
        public int SpawnTeam;
        public bool SpawnIsGroup;
        public bool SpawnIsRunning;
        public WorldMap Map;
        public Guid UnqiueId;
        public ArrayList SpawnObjects = new ArrayList();

        public SpawnPoint( Guid unqiueId, WorldMap Map, short MapX, short MapY, short MapWidth, short MapHeight )
        {
            this.UnqiueId = unqiueId;
            this.Map = Map;
            this.Index = -1;
            this.IsSelected = true;

            // Set the bounds of this spawn point
            this.Bounds = new Rectangle( MapX, MapY, MapWidth, MapHeight );
            this.SpawnName = "Spawn Point " + this.Bounds.ToString();
        }

        public SpawnPoint( Guid uniqueId, WorldMap Map, Rectangle SpawnBounds )
        {
            this.UnqiueId = uniqueId;
            this.Map = Map;
            this.Index = -1;
            this.IsSelected = true;

            // Set the bounds of this spawn point
            this.Bounds = SpawnBounds;
            this.SpawnName = "Spawn Point " + this.Bounds.ToString();
        }

        public SpawnPoint( DataRow SpawnRow )
        {
            // Try load the GUID (might not work so create a new GUID)
            Guid uniqueId = Guid.NewGuid();
            try{ uniqueId = new Guid( (string)SpawnRow["UniqueId"] ); }
            catch{}

            // Try load the map (might not work so default to the currently selected map)
            WorldMap map = WorldMap.Trammel;
            try{ map = (WorldMap)Enum.Parse( typeof(WorldMap), (string)SpawnRow["Map"], true ); }
            catch{}

            // Try to get the home range relative flag
            bool HomeRangeIsRelative = false;
            try{ HomeRangeIsRelative = bool.Parse( (string)SpawnRow["IsHomeRangeRelative"] ); }
            catch{}

            int x = int.Parse( (string)SpawnRow["X"] );
            int y = int.Parse( (string)SpawnRow["Y"] );
            int w = int.Parse( (string)SpawnRow["Width"] );
            int h = int.Parse( (string)SpawnRow["Height"] );

            this.UnqiueId = uniqueId;
            this.Map = map;
            this._Bounds = new Rectangle( x, y, w, h );
            this.SpawnName = (string)SpawnRow["Name"];
            this.CentreX = short.Parse( (string)SpawnRow["CentreX"] );
            this.CentreY = short.Parse( (string)SpawnRow["CentreY"] );
            this.CentreZ = short.Parse( (string)SpawnRow["CentreZ"] );
            this._SpawnHomeRange = int.Parse( (string)SpawnRow["Range"] );
            this.SpawnMaxCount = int.Parse( (string)SpawnRow["MaxCount"] );
            this.SpawnMinDelay = int.Parse( (string)SpawnRow["MinDelay"] );
            this.SpawnMaxDelay = int.Parse( (string)SpawnRow["MaxDelay"] );
            this.SpawnTeam = int.Parse( (string)SpawnRow["Team"] );
            this.SpawnIsGroup = bool.Parse( (string)SpawnRow["IsGroup"] );
            this.SpawnIsRunning = bool.Parse( (string)SpawnRow["IsRunning"] );
            this.SpawnHomeRangeIsRelative = HomeRangeIsRelative;
            this.LoadSpawnObjectsFromString( (string)SpawnRow["Objects"] );
            this.IsSelected = false;
        }

        public bool IsSameArea( short MapX, short MapY, short Range )
        {
            // Check to see if the map coordinates provided
            // fall with the current spawn points boundaries
            Rectangle ProposedSpawnPoint = new Rectangle( MapX - Range, MapY - Range, Range*2, Range*2 );
            return this.Bounds.IntersectsWith( ProposedSpawnPoint );
        }

        public bool IsSameArea( short MapX, short MapY )
        {
            // Check to see if the map coordinates provided
            // fall with the current spawn points boundaries
            return this.Bounds.Contains( MapX, MapY );
        }

        public Rectangle Bounds
        {
            get{ return this._Bounds; }
            set
            {
                this._Bounds = value;
                this.CentreX = (short)(value.X + ( value.Width / 2 ));
                this.CentreY = (short)(value.Y + ( value.Height / 2 ));
            }        
        }

        public int SpawnHomeRange
        {
            get{ return this._SpawnHomeRange; }
            set{ this._SpawnHomeRange = value; }
            /*{
                // Check if the relative home range is checked or not
                if( this.SpawnHomeRangeIsRelative == true )
                {
                    // Relative home range is checked, so don't do anything
                    // special, just set the value
                    this._SpawnHomeRange = value;
                }
                else
                {
                    // The home range is not relative, it is absolute
                    // so we need to recalculate the bounds of the spawner.
                    // Get the delta change between the current value and the
                    // new value.
                    int Delta = value - this._SpawnHomeRange;

                    // The Delta value is the change in the size of the spawners
                    // home range.  The X, Y, Width & Height should be 
                    // altered to a percentage of the new home range
                    if( Delta < 0 )
                    {
                        // The home range was decreased, so increase the X & Y
                        // and decrease the Width and Height
                        int NewX = this._Bounds.X + ( Math.Abs(Delta) / 2 );
                        int NewY = this._Bounds.Y + ( Math.Abs(Delta) / 2 );
                        int NewWidth = this._Bounds.Width - ( Delta / 2 );
                        int NewHeight = this._Bounds.Height - ( Delta / 2 );

                        // Set the new bounds
                        this._Bounds = new Rectangle( NewX, NewY, NewWidth, NewHeight );
                    }
                    else if( Delta > 0 )
                    {
                        // The home range was increased, so decrease the X & Y
                        // and increase the Width and Height
                        int NewX = this._Bounds.X - ( Math.Abs(Delta) / 2 );
                        int NewY = this._Bounds.Y - ( Math.Abs(Delta) / 2 );
                        int NewWidth = this._Bounds.Width + ( Delta / 2 );
                        int NewHeight = this._Bounds.Height + ( Delta / 2 );

                        // Set the new bounds
                        this._Bounds = new Rectangle( NewX, NewY, NewWidth, NewHeight );
                    }
                    else
                    {
                        // No change so don't do anything
                    }
                }
            }*/
        }

        public int Area
        {
            get{ return ( this.Bounds.Width * this.Bounds.Height ); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( this.SpawnName );
            sb.Append( Environment.NewLine );
            sb.Append( "==============================" );
            sb.Append( Environment.NewLine );
            sb.Append( this.Bounds.ToString() );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Home Range: {0}", this.SpawnHomeRange );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Maximum: {0}", this.SpawnMaxCount );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Delay: {0}m - {1}m", this.SpawnMinDelay, this.SpawnMaxDelay );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Team: {0}", this.SpawnTeam );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Grouped [{0}]", ( this.SpawnIsGroup == true ? "True" : "False" ) );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Running [{0}]", ( this.SpawnIsRunning == true ? "True" : "False" ) );
            sb.Append( Environment.NewLine );
            sb.AppendFormat( "Relative Home Range [{0}]", ( this.SpawnHomeRangeIsRelative == true ? "True" : "False" ) );
            sb.Append( Environment.NewLine );
            sb.Append( "==============================" );

            // List all spawn objects
            for( int x = 0; x < this.SpawnObjects.Count; x++ )
            {
                SpawnObject so = this.SpawnObjects[x] as SpawnObject;

                if( so != null )
                {
                    sb.Append( Environment.NewLine );
                    sb.AppendFormat( "{0} [Max:{1}]", so.TypeName, so.Count );
                }
            }
            return sb.ToString();
        }

        public string GetSerializedObjectList()
        {
            StringBuilder sb = new StringBuilder();

            foreach( SpawnObject so in this.SpawnObjects )
            {
                if( sb.Length > 0 )
                    sb.Append( ':' ); // ':' Separates multiple object types

                sb.AppendFormat( "{0}={1}", so.TypeName, so.Count ); // '=' separates object name from maximum amount
            }

            return sb.ToString();
        }

        public void LoadSpawnObjectsFromString( string SerializedObjectList )
        {
            // Clear the spawn object list
            this.SpawnObjects.Clear();

            if( SerializedObjectList.Length > 0 )
            {
                // Split the string based on the object separator first ':'
                string[] SpawnObjectList = SerializedObjectList.Split( ':' );

                // Parse each item in the array
                foreach( string s in SpawnObjectList )
                {
                    // Split the single spawn object item by the max count '='
                    string[] SpawnObjectDetails = s.Split( '=' );

                    // Should be two entries
                    if( SpawnObjectDetails.Length == 2 )
                    {
                        // Validate the information

                        // Make sure the spawn object name part has a valid length
                        if( SpawnObjectDetails[0].Length > 0 )
                        {
                            // Make sure the max count part has a valid length
                            if( SpawnObjectDetails[1].Length > 0 )
                            {
                                int MaxCount = 1;

                                try
                                {
                                    MaxCount = int.Parse( SpawnObjectDetails[1] );
                                }
                                catch( System.Exception )
                                { // Something went wrong, leave the default amount }
                                }

                                // Create the spawn object and store it in the array list
                                SpawnObject so = new SpawnObject( SpawnObjectDetails[0], MaxCount );
                                this.SpawnObjects.Add( so );
                            }
                        }
                    }
                }
            }
        }
    }

    public class SpawnPointNode : TreeNode
    {
        private SpawnPoint _Spawn;

        public SpawnPointNode( SpawnPoint Spawn )
        {
            this._Spawn = Spawn;
            this.UpdateNode();
        }

        public void UpdateNode()
        {
            this.Text = this._Spawn.SpawnName;

            // Add all of the spawn objects to this node
            this.Nodes.Clear();
            foreach( SpawnObject so in this._Spawn.SpawnObjects )
                this.Nodes.Add( new SpawnObjectNode( so ) );
        }

        public SpawnPoint Spawn
        {
            get{ return this._Spawn; }
        }
    }
    
    public class SpawnObjectNode : TreeNode
    {
        private SpawnObject _Object;

        public SpawnObjectNode( SpawnObject SpawnObject )
        {
            this._Object = SpawnObject;
            this.UpdateNode();
        }

        public void UpdateNode()
        {
            this.Text = this._Object.ToString();
        }

        public SpawnObject SpawnObject
        {
            get{ return this._Object; }
        }
    }
}
