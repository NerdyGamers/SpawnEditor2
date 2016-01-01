using System;
using System.Data;
using System.IO;
using System.Collections;
using Server;
using Server.Items;
using Server.Network;
using Server.Gumps;

namespace Server.Mobiles
{
    public class XmlSpawner : Item
    {
        private const int ShowBoundsItemId = 14089; // 14089 Fire Column // 3555 Campfire // 8708 Skull Pole
        private const string SpawnDataSetName = "Spawns";
        private const string SpawnTablePointName = "Points";
        private const int SpawnFitSize = 16; // Normal wall/door height for a mobile is 20 to walk through

        private const int BaseItemId = 0x1F1C;// Purple Magic Crystal
        private string m_Name = string.Empty;
        private string m_UniqueId = string.Empty;
        private bool m_PlayerCreated = false;
        private bool m_HomeRangeIsRelative = false;
        private int m_Team;
        private int m_HomeRange;
        private int m_Count;
        private TimeSpan m_MinDelay;
        private TimeSpan m_MaxDelay;
        private ArrayList m_SpawnObjects = new ArrayList(); // List of objects to spawn
        private DateTime m_End;
        private InternalTimer2 m_Timer;
        private bool m_Running;
        private bool m_Group;
        private int m_X;
        private int m_Y;
        private int m_Width;
        private int m_Height;
        private WayPoint m_WayPoint;
        private static WarnTimer2 m_WarnTimer;
		
        public static void Initialize()
        {
            Server.Commands.Register( "XmlSpawnerShowAll", AccessLevel.Administrator, new CommandEventHandler( ShowSpawnPoints_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerHideAll", AccessLevel.Administrator, new CommandEventHandler( HideSpawnPoints_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerWipe", AccessLevel.Administrator, new CommandEventHandler( Wipe_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerWipeAll", AccessLevel.Administrator, new CommandEventHandler( WipeAll_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerLoad", AccessLevel.Administrator, new CommandEventHandler( Load_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerSave", AccessLevel.Administrator, new CommandEventHandler( Save_OnCommand ) );
            Server.Commands.Register( "XmlSpawnerSaveAll", AccessLevel.Administrator, new CommandEventHandler( SaveAll_OnCommand ) );
        }
        
        [Usage( "XmlSpawnerShowAll" )]
        [Description( "Makes all XmlSpawner objects visible and movable and also changes the item id to a blue ships mast for easy identification." )]
        public static void ShowSpawnPoints_OnCommand( CommandEventArgs e )
        {
            foreach( Item item in World.Items.Values )
            {
                if( item is XmlSpawner )
                {
                    item.Visible = true;    // Make the spawn item visible to everyone
                    item.Movable = true;    // Make the spawn item movable
                    item.Hue = 88;          // Bright blue colour so its easy to spot
                    item.ItemID = 0x3E57;   // Ship Mast (Very tall, easy to see if beneath other objects)
                }
            }
        }

        [Usage( "XmlSpawnerHideAll" )]
        [Description( "Makes all XmlSpawner objects invisible and unmovable returns the object id to the default." )]
        public static void HideSpawnPoints_OnCommand( CommandEventArgs e )
        {
            foreach( Item item in World.Items.Values )
            {
                if( item is XmlSpawner )
                {
                    item.Visible = false;
                    item.Movable = false;
                    item.Hue = 0;
                    item.ItemID = BaseItemId;
                }
            }
        }

        [Usage( "XmlSpawnerWipe [SpawnerPrefixFilter]" )]
        [Description( "Removes all XmlSpawner objects from the current map." )]
        public static void Wipe_OnCommand( CommandEventArgs e )
        {
            WipeSpawners( e, false );
        }

        [Usage( "XmlSpawnerWipeAll [SpawnerPrefixFilter]" )]
        [Description( "Removes all XmlSpawner objects from the entire world." )]
        public static void WipeAll_OnCommand( CommandEventArgs e )
        {
            WipeSpawners( e, true );
        }

        [Usage( "XmlSpawnerLoad <SpawnFile> [SpawnerPrefixFilter]" )]
        [Description( "Loads XmlSpawner objects into the proper map as defined in the file supplied." )]
        public static void Load_OnCommand( CommandEventArgs e )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                if( e.Arguments.Length >= 1 )
                {
                    // Check if the file exists
                    if( System.IO.File.Exists( e.Arguments[0].ToString() ) == true )
                    {
                        int TotalCount = 0;
                        int TrammelCount = 0;
                        int FeluccaCount = 0;
                        int IlshenarCount = 0;
                        int MalasCount = 0;

                        // Spawner load criteria (if any)
                        string SpawnerPrefix = string.Empty;

                        // Check if there is an argument provided (load criteria)
                        if( e.Arguments.Length > 1 )
                            SpawnerPrefix = e.Arguments[1];

                        e.Mobile.SendMessage( string.Format( "Loading {0} objects{1} from file {2}.", "XmlSpawner", ( SpawnerPrefix.Length > 0 ? " beginning with " + SpawnerPrefix : string.Empty ), e.Arguments[0].ToString() ) );

                        // Create the data set
                        DataSet ds = new DataSet( SpawnDataSetName );

                        // Read in the file
                        ds.ReadXml( e.Arguments[0].ToString() );

                        // Check that at least a single table was loaded
                        if( ds.Tables.Count > 0 )
                        {
                            // Add each spawn point to the current map
                            foreach( DataRow dr in ds.Tables[SpawnTablePointName].Rows )
                            {
                                // Each row makes up a single spawner
                                string SpawnName = (string)dr["Name"];

                                // Check if there is any spawner name criteria specified on the load
                                if( ( SpawnerPrefix.Length == 0 ) || ( SpawnName.StartsWith( SpawnerPrefix ) == true ) )
                                {
                                    // Try load the GUID (might not work so create a new GUID)
                                    Guid SpawnId = Guid.NewGuid();
                                    try{ SpawnId = new Guid( (string)dr["UniqueId"] ); }
                                    catch{}

                                    // Get the map (default to the mobiles map)
                                    Map SpawnMap = e.Mobile.Map;
                                    string XmlMapName = e.Mobile.Map.Name;

                                    // Try to get the "map" field, but in case it doesn't exist, catch and discard the exception
                                    try{ XmlMapName = (string)dr["Map"]; }
                                    catch{}

                                    // Convert the xml map value to a real map object
                                    if( string.Compare( XmlMapName, Map.Trammel.Name, true ) == 0 )
                                    {
                                        SpawnMap = Map.Trammel;
                                        TrammelCount++;
                                    }
                                    else if( string.Compare( XmlMapName, Map.Felucca.Name, true ) == 0 )
                                    {
                                        SpawnMap = Map.Felucca;
                                        FeluccaCount++;
                                    }
                                    else if( string.Compare( XmlMapName, Map.Ilshenar.Name, true ) == 0 )
                                    {
                                        SpawnMap = Map.Ilshenar;
                                        IlshenarCount++;
                                    }
                                    else if( string.Compare( XmlMapName, Map.Malas.Name, true ) == 0 )
                                    {   
                                        SpawnMap = Map.Malas;
                                        MalasCount++;
                                    }

                                    // Try load the IsRelativeHomeRange (default to true)
                                    bool SpawnIsRelativeHomeRange = true;
                                    try{ SpawnIsRelativeHomeRange = bool.Parse( (string)dr["IsHomeRangeRelative"] ); }
                                    catch{}

                                    int SpawnX = int.Parse( (string)dr["X"] );
                                    int SpawnY = int.Parse( (string)dr["Y"] );
                                    int SpawnWidth = int.Parse( (string)dr["Width"] );
                                    int SpawnHeight = int.Parse( (string)dr["Height"] );
                                    int SpawnCentreX = int.Parse( (string)dr["CentreX"] );
                                    int SpawnCentreY = int.Parse( (string)dr["CentreY"] );
                                    int SpawnCentreZ = int.Parse( (string)dr["CentreZ"] );
                                    int SpawnHomeRange = int.Parse( (string)dr["Range"] );
                                    int SpawnMaxCount = int.Parse( (string)dr["MaxCount"] );
                                    TimeSpan SpawnMinDelay = TimeSpan.FromMinutes( int.Parse( (string)dr["MinDelay"] ) );
                                    TimeSpan SpawnMaxDelay = TimeSpan.FromMinutes( int.Parse( (string)dr["MaxDelay"] ) );
                                    int SpawnTeam = int.Parse( (string)dr["Team"] );
                                    bool SpawnIsGroup = bool.Parse( (string)dr["IsGroup"] );
                                    bool SpawnIsRunning = bool.Parse( (string)dr["IsRunning"] );
                                    SpawnObject[] Spawns = SpawnObject.LoadSpawnObjectsFromString( (string)dr["Objects"] );
                
                                    // Check if this spawner already exists
                                    XmlSpawner OldSpawner = null;
                                    foreach( Item i in World.Items.Values )
                                    {
                                        if( i is XmlSpawner )
                                        {
                                            XmlSpawner CheckXmlSpawner = (XmlSpawner)i;
                                        
                                            // Check if the spawners GUID is the same as the one being loaded
                                            // and that the spawners map is the same as the one being loaded
                                            if( ( CheckXmlSpawner.UniqueId == SpawnId.ToString() ) &&
                                                ( CheckXmlSpawner.Map == SpawnMap ) )
                                            {
                                                OldSpawner = (XmlSpawner)i;
                                                break;
                                            }
                                        }
                                    }

                                    // Delete the old spawner if it exists
                                    if( OldSpawner != null )
                                        OldSpawner.Delete();

                                    // Create the new spawner
                                    XmlSpawner TheSpawn = new XmlSpawner( SpawnId, SpawnX, SpawnY, SpawnWidth, SpawnHeight, SpawnName, SpawnMaxCount, SpawnMinDelay, SpawnMaxDelay, SpawnTeam, SpawnHomeRange, SpawnIsRelativeHomeRange, Spawns );
                                    TheSpawn.Group = SpawnIsGroup;

                                    // Try to find a valid Z height if required (SpawnCentreZ = short.MinValue)
                                    int NewZ = 0;

                                    // Check if the 
                                    if( SpawnCentreZ == short.MinValue )
                                    {
                                        NewZ = SpawnMap.GetAverageZ( SpawnCentreX, SpawnCentreY );

                                        if( SpawnMap.CanFit( SpawnCentreX, SpawnCentreY, NewZ, SpawnFitSize ) == false )
                                        { 
                                            for( int x = 1; x <= 39; x++ ) 
                                            { 
                                                if( SpawnMap.CanFit( SpawnCentreX, SpawnCentreY, NewZ + x, SpawnFitSize ) ) 
                                                { 
                                                    NewZ += x; 
                                                    break; 
                                                }
                                            } 
                                        }
                                    }
                                    else
                                    {
                                        // This spawn point already has a defined Z location, so use it
                                        NewZ = SpawnCentreZ;
                                    }

                                    // Place the spawner into the world
                                    TheSpawn.MoveToWorld( new Point3D( SpawnCentreX, SpawnCentreY, NewZ ), SpawnMap );

                                    // Send a message to the client that the spawner is created
                                    e.Mobile.SendMessage( 188, "Created spawner '{0}' in {1} at {2}", TheSpawn.Name, TheSpawn.Map.Name, TheSpawn.Location.ToString() );

                                    // Do a total respawn
                                    TheSpawn.Respawn();

                                    // Increment the count
                                    TotalCount++;
                                }
                            }
                        }

                        e.Mobile.SendMessage( "{0} spawner(s) were created from file {1} [Trammel={2}, Felucca={3}, Ilshenar={4}, Malas={5}].", TotalCount, e.Arguments[0].ToString(), TrammelCount, FeluccaCount, IlshenarCount, MalasCount );
                    }
                    else
                        e.Mobile.SendMessage( "File {0} does not exist.", e.Arguments[0].ToString() );
                }
                else
                    e.Mobile.SendMessage( "Usage:  {0} <SpawnFile>", e.Command );
            }
            else
                e.Mobile.SendMessage( "You do not have rights to perform this command." );
        }

        [Usage( "XmlSpawnerSave <SpawnFile> [SpawnerPrefixFilter]" )]
        [Description( "Saves all XmlSpawner objects from the current map into the file supplied." )]
        public static void Save_OnCommand( CommandEventArgs e )
        {
            SaveSpawns( e, false );
        }

        [Usage( "XmlSpawnerSaveAll <SpawnFile> [SpawnerPrefixFilter]" )]
        [Description( "Saves ALL XmlSpawner objects from the entire world into the file supplied." )]
        public static void SaveAll_OnCommand( CommandEventArgs e )
        {
            SaveSpawns( e, true );
        }

        private static void SaveSpawns( CommandEventArgs e, bool SaveAllMaps )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                if( e.Arguments.Length >= 1 )
                {
                    int TotalCount = 0;
                    int TrammelCount = 0;
                    int FeluccaCount = 0;
                    int IlshenarCount = 0;
                    int MalasCount = 0;

                    // Spawner save criteria (if any)
                    string SpawnerPrefix = string.Empty;

                    // Check if there is an argument provided (save criteria)
                    if( e.Arguments.Length > 1 )
                        SpawnerPrefix = e.Arguments[1];

                    if( SaveAllMaps == true )
                        e.Mobile.SendMessage( string.Format( "Saving {0} objects{1} to file {2} from {3}.", "XmlSpawner", ( SpawnerPrefix.Length > 0 ? " beginning with " + SpawnerPrefix : string.Empty ), e.Arguments[0].ToString(), e.Mobile.Map ) );
                    else
                        e.Mobile.SendMessage( string.Format( "Saving {0} obejcts{1} to file {2} from the entire world.", "XmlSpawner", ( SpawnerPrefix.Length > 0 ? " beginning with " + SpawnerPrefix : string.Empty ), e.Arguments[0].ToString() ) );
                    
                    // Create the data set
                    DataSet ds = new DataSet( SpawnDataSetName );

                    // Load the data set up
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
                    foreach( Item i in World.Items.Values )
                    {
                        if( ( ( SaveAllMaps == true ) || ( i.Map == e.Mobile.Map ) ) &&
                            ( i is XmlSpawner ) && ( i.Deleted == false ) )
                        {
                            // Check if there is a save condition
                            if( ( SpawnerPrefix.Length == 0 ) || ( i.Name.StartsWith( SpawnerPrefix ) ) )
                            {
                                // Cast the item to a spawner
                                XmlSpawner sp = (XmlSpawner)i;

                                // Send a message to the client that the spawner is being saved
                                e.Mobile.SendMessage( 68, "Saving spawner '{0}' in {1} at {2}", sp.Name, sp.Map.Name, sp.Location.ToString() );

                                // Create a new data row
                                DataRow dr = ds.Tables[SpawnTablePointName].NewRow();

                                // Populate the data
                                dr["Name"] = (string)sp.m_Name;

                                // Set the unqiue id
                                dr["UniqueId"] = (string)sp.m_UniqueId;

                                // Get the map name
                                dr["Map"] = (string)sp.Map.Name;

                                // Convert the xml map value to a real map object
                                if( string.Compare( sp.Map.Name, Map.Trammel.Name, true ) == 0 )
                                    TrammelCount++;
                                else if( string.Compare( sp.Map.Name, Map.Felucca.Name, true ) == 0 )
                                    FeluccaCount++;
                                else if( string.Compare( sp.Map.Name, Map.Ilshenar.Name, true ) == 0 )
                                    IlshenarCount++;
                                else if( string.Compare( sp.Map.Name, Map.Malas.Name, true ) == 0 )
                                    MalasCount++;

                                dr["X"] = (int)sp.m_X;
                                dr["Y"] = (int)sp.m_Y;
                                dr["Width"] = (int)sp.m_Width;
                                dr["Height"] = (int)sp.m_Height;
                                dr["CentreX"] = (int)sp.Location.X;
                                dr["CentreY"] = (int)sp.Location.Y;
                                dr["CentreZ"] = (int)sp.Location.Z;
                                dr["Range"] = (int)sp.m_HomeRange;
                                dr["MaxCount"] = (int)sp.m_Count;
                                dr["MinDelay"] = (int)sp.m_MinDelay.TotalMinutes;
                                dr["MaxDelay"] = (int)sp.m_MaxDelay.TotalMinutes;
                                dr["Team"] = (int)sp.m_Team;
                                dr["IsGroup"] = (bool)sp.m_Group;
                                dr["IsRunning"] = (bool)sp.m_Running;
                                dr["IsHomeRangeRelative"] = (bool)sp.m_HomeRangeIsRelative;
                                dr["Objects"] = (string)sp.GetSerializedObjectList();

                                // Add the row the the table
                                ds.Tables[SpawnTablePointName].Rows.Add( dr );

                                // Increment the count
                                TotalCount++;
                            }
                        }
                    }

                    // Write out the file
                    ds.WriteXml( e.Arguments[0].ToString() );

                    // Indicate how many spawners were written
                    if( SaveAllMaps == true )
                        e.Mobile.SendMessage( "{0} spawner(s) were saved to file {1} [Trammel={2}, Felucca={3}, Ilshenar={4}, Malas={5}].", TotalCount, e.Arguments[0].ToString(), TrammelCount, FeluccaCount, IlshenarCount, MalasCount );
                    else
                        e.Mobile.SendMessage( "{0} spawner(s) from {1} were saved to file {2} .", TotalCount, e.Mobile.Map, e.Arguments[0].ToString() );
                }
                else
                    e.Mobile.SendMessage( "Usage:  {0} <SpawnFile>", e.Command );
            }
            else
                e.Mobile.SendMessage( "You do not have rights to perform this command." );
        }
        
        private static void WipeSpawners( CommandEventArgs e, bool WipeAll )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                // Spawner delete criteria (if any)
                string SpawnerPrefix = string.Empty;

                // Check if there is an argument provided (delete criteria)
                if( e.Arguments.Length > 0 )
                    SpawnerPrefix = e.Arguments[0];

                if( WipeAll == true )
                    e.Mobile.SendMessage( "Removing ALL XmlSpawner objects from the world{0}.", ( SpawnerPrefix.Length > 0 ? " beginning with " + SpawnerPrefix : string.Empty ) );
                else
                    e.Mobile.SendMessage( "Removing ALL XmlSpawner objects from {0}{1}.", e.Mobile.Map , ( SpawnerPrefix.Length > 0 ? " beginning with " + SpawnerPrefix : string.Empty ) );

                // Delete Xml spawner's in the world based on the mobiles current map
                int Count = 0;
                ArrayList ToDelete = new ArrayList();
                foreach( Item i in World.Items.Values )
                {
                    if( ( i is XmlSpawner ) && ( WipeAll == true || i.Map == e.Mobile.Map ) && ( i.Deleted == false ) )
                    {
                        // Check if there is a delete condition
                        if( ( SpawnerPrefix.Length == 0 ) || ( i.Name.StartsWith( SpawnerPrefix ) ) )
                        {
                            // Send a message to the client that the spawner is being deleted
                            e.Mobile.SendMessage( 33, "Removing spawner '{0}' in {1} at {2}", i.Name, i.Map.Name, i.Location.ToString() );

                            ToDelete.Add( i );
                            Count++;
                        }
                    }
                }

                // Delete the items in the array list
                foreach( Item i in ToDelete )
                    i.Delete();

                if( WipeAll == true )
                    e.Mobile.SendMessage( "Removed {0} XmlSpawner objects from the world.", Count );
                else
                    e.Mobile.SendMessage( "Removed {0} XmlSpawner objects from {1}.", Count, e.Mobile.Map );
            }
            else
                e.Mobile.SendMessage( "You do not have rights to perform this command." );
        }

        public SpawnObject[] SpawnObjects
        {
            get { return (SpawnObject[])this.m_SpawnObjects.ToArray( typeof(SpawnObject) ); }
            set
            {
                if( ( value != null ) && ( value.Length > 0 ) )
                {
                    foreach( SpawnObject so in value )
                    {
                        bool AlreadyInList = false;

                        // Check if the new array has an existing spawn object
                        foreach( SpawnObject TheSpawn in this.m_SpawnObjects )
                        {
                            if( TheSpawn.TypeName.ToUpper() == so.TypeName.ToUpper() )
                            {
                                AlreadyInList = true;
                                break;
                            }
                        }

                        // Does this item need to be added
                        if( AlreadyInList == false )
                        {
                            // This is a new spawn object so add it to the array (deep copy)
                            this.m_SpawnObjects.Add( new SpawnObject( so.TypeName, so.Count ) );
                        }
                    }

                    if( this.SpawnObjects.Length < 1 )
                        Stop();
                    else
                        Start();

                    InvalidateProperties();
                }
            }
        }
		
        #region Command Properties
        [CommandProperty( AccessLevel.GameMaster )]
        public Point3D X1_Y1
        {
            get{ return new Point3D( this.m_X, this.m_Y, this.Z ); }
            set
            {
                int OriginalX2 = this.m_X + this.m_Width;
                int OriginalY2 = this.m_Y + this.m_Height;

                this.m_X = value.X;
                this.m_Y = value.Y;
                this.m_Width = OriginalX2 - this.m_X;
                this.m_Height = OriginalY2 - this.m_Y;

                if( this.m_HomeRangeIsRelative == false )
                {
                    int NewHomeRange = ( this.m_Width > this.m_Height ? this.m_Height : this.m_Width );
                    this.m_HomeRange = ( NewHomeRange > 0 ? NewHomeRange : 1 );
                }

                // Stop the spawner if the width or height is less than 1
                if( ( this.m_Width < 1 ) || ( this.m_Height < 1 ) )
                    this.Running = false;
            
                InvalidateProperties();

                // Check if the spawner is showing its bounds
                if( this.ShowBounds == true )
                {
                    this.ShowBounds = false;
                    this.ShowBounds = true;
                }
            }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public bool ShowBounds
        {
            get{ return ( this.m_ShowBoundsItems.Count > 0 ); }
            set
            {
                if( ( value == true ) && ( this.ShowBounds == false ) )
                {
                    // Boundary lines
                    int ValidX1 = this.m_X;
                    int ValidX2 = this.m_X + this.m_Width;
                    int ValidY1 = this.m_Y;
                    int ValidY2 = this.m_Y + this.m_Height;

                    for( int x = 0; x <= this.m_Width; x++ )
                    {
                        int NewX = this.m_X + x;
                        for( int y = 0; y <= this.m_Height; y++ )
                        {
                            int NewY = this.m_Y + y;

                            if( NewX == ValidX1 || NewX == ValidX2 || NewX == ValidY1 || NewX == ValidY2 || NewY == ValidX1 || NewY == ValidX2 || NewY == ValidY1 || NewY == ValidY2 )
                            {
                                // Add an object to show the spawn area
                                Static s = new Static( ShowBoundsItemId );
                                s.MoveToWorld( new Point3D( NewX, NewY, this.Z ), this.Map );
                                this.m_ShowBoundsItems.Add( s );
                            }
                        }
                    }
                }
                
                if( value == false )
                {
                    // Remove all of the items from the array
                    foreach( Static s in this.m_ShowBoundsItems )
                        s.Delete();

                    this.m_ShowBoundsItems.Clear();
                }
            }
        }
        private ArrayList m_ShowBoundsItems = new ArrayList();

        [CommandProperty( AccessLevel.GameMaster )]
        public Point3D X2_Y2
        {
            get{ return new Point3D( ( this.m_X + this.m_Width ), ( this.m_Y + this.m_Height ), this.Z ); }
            set
            {
                int OriginalX2 = this.m_X + this.m_Width;
                int OriginalY2 = this.m_Y + this.m_Height;

                this.m_Width = value.X - this.m_X;
                this.m_Height = value.Y - this.m_Y;

                if( this.m_HomeRangeIsRelative == false )
                {
                    int NewHomeRange = ( this.m_Width > this.m_Height ? this.m_Height : this.m_Width );
                    this.m_HomeRange = ( NewHomeRange > 0 ? NewHomeRange : 0 );
                }

                // Stop the spawner if the width or height is less than 1
                if( ( this.m_Width < 1 ) || ( this.m_Height < 1 ) )
                    this.Running = false;

                InvalidateProperties();
                            
                // Check if the spawner is showing its bounds
                if( this.ShowBounds == true )
                {
                    this.ShowBounds = false;
                    this.ShowBounds = true;
                }
            }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public override string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; InvalidateProperties(); }
        }

        //[CommandProperty( AccessLevel.GameMaster )]
        public string UniqueId
        {
            get { return this.m_UniqueId; }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public int MaxCount
        {
            get { return m_Count; }
            set { m_Count = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public int CurrentCount
        {
            get { return this.TotalSpawnedObjects; }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public WayPoint WayPoint
        {
            get{ return m_WayPoint; }
            set{ m_WayPoint = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public bool Running
        {
            get { return m_Running; }
            set
            {
                // Don't start the spawner unless the height and width are valid
                if( ( value == true ) && ( this.m_Width > 0 ) && ( this.m_Height > 0 ) )
                    Start();
                else
                    Stop();

                InvalidateProperties();
            }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public int HomeRange
        {
            get { return m_HomeRange; }
            set { m_HomeRange = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public bool HomeRangeIsRelative
        {
            get { return m_HomeRangeIsRelative; }
            set { m_HomeRangeIsRelative = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public int Team
        {
            get { return m_Team; }
            set { m_Team = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public TimeSpan MinDelay
        {
            get { return m_MinDelay; }
            set { m_MinDelay = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public TimeSpan MaxDelay
        {
            get { return m_MaxDelay; }
            set { m_MaxDelay = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public TimeSpan NextSpawn
        {
            get
            {
                if ( m_Running )
                    return m_End - DateTime.Now;
                else
                    return TimeSpan.FromSeconds( 0 );
            }
            set
            {
                Start();
                DoTimer( value );
                InvalidateProperties();
            }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public bool Group
        {
            get { return m_Group; }
            set { m_Group = value; InvalidateProperties(); }
        }
        #endregion

        [Constructable]
        public XmlSpawner() : base( BaseItemId )
        {
            this.m_PlayerCreated = true;
            this.m_UniqueId = Guid.NewGuid().ToString();
            this.InitSpawn( 0, 0, 0, 0, string.Empty, 0, TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 10 ), 0, 5, true, new SpawnObject[0] );
        }

        public XmlSpawner( Guid UniqueId, int X, int Y, int Width, int Height, string Name, int amount, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, bool IsRelativeHomeRange, SpawnObject[] SpawnObjects ) : base( BaseItemId )
        {
            this.m_UniqueId = UniqueId.ToString();
            this.InitSpawn( X, Y, Width, Height, Name, amount, minDelay, maxDelay, team, homeRange, IsRelativeHomeRange, SpawnObjects );
        }
        
        public override bool Decays 
        { 
            get{ return false; } 
        } 

        public void InitSpawn( int X, int Y, int Width, int Height, string Name, int amount, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, bool IsRelativeHomeRange, SpawnObject[] ObjectsToSpawn )
        {
            Visible = false;
            Movable = false;
            m_X = X;
            m_Y = Y;
            m_Width = Width;
            m_Height = Height;
            m_Running = true;
            m_Group = false;
            m_Name = "Spawner";
            m_MinDelay = minDelay;
            m_MaxDelay = maxDelay;
            m_Count = amount;
            m_Team = team;
            m_HomeRange = homeRange;
            m_HomeRangeIsRelative = IsRelativeHomeRange;

            if( ( Name != null ) && ( Name.Length > 0 ) )
                this.m_Name = Name;

            // Create the array of spawned objects
            this.m_SpawnObjects = new ArrayList();

            // Assign the list of objects to spawn
            this.SpawnObjects = ObjectsToSpawn;

            // Kick off the process
            DoTimer( TimeSpan.FromSeconds( 1 ) );
        }

        public XmlSpawner( Serial serial ) : base( serial )
        {
        }

        public override void OnDoubleClick( Mobile from )
        {
            XmlSpawnerGump g = new XmlSpawnerGump( this );
            from.SendGump( g );
        }

        public override void GetProperties( ObjectPropertyList list )
        {
            base.GetProperties( list );

            if( this.m_Running )
            {
                list.Add( 1060742 ); // active

                list.Add( 1060656, this.m_Count.ToString() ); // amount to make: ~1_val~
                list.Add( 1061169, this.m_HomeRange.ToString() ); // range ~1_val~

                list.Add( 1060658, "group\t{0}", this.m_Group ); // ~1_val~: ~2_val~
                list.Add( 1060659, "team\t{0}", this.m_Team ); // ~1_val~: ~2_val~
                list.Add( 1060660, "speed\t{0} to {1}", this.m_MinDelay, this.m_MaxDelay ); // ~1_val~: ~2_val~

                for( int i = 0; i < 3 && i < this.m_SpawnObjects.Count; ++i )
                    list.Add( 1060661 + i, "{0}\t{1}", ((SpawnObject)this.m_SpawnObjects[i]).TypeName, ((SpawnObject)this.m_SpawnObjects[i]).SpawnedObjects.Count );
            }
            else
            {
                list.Add( 1060743 ); // inactive
            }
        }

        public override void OnSingleClick( Mobile from )
        {
            LabelTo( from, "XmlSpawner" );
            LabelTo( from, this.Name + ( this.m_Running == true ? " [On]" : " [Off]" ) );
        }

        public void Start()
        {
            if( this.m_Running == false )
            {
                if( this.m_SpawnObjects.Count > 0 )
                {
                    this.m_Running = true;
                    this.DoTimer();
                }
            }
        }

        public void Stop()
        {
            if( this.m_Running == true )
            {
                this.m_Timer.Stop();
                this.m_Running = false;
            }
        }

        public void Defrag()
        {
            bool removed = false;

            foreach( SpawnObject so in this.m_SpawnObjects )
            {
                for( int x = 0; x < so.SpawnedObjects.Count; x++ )
                {
                    object o = so.SpawnedObjects[x];

                    if( o is Item )
                    {
                        Item item = (Item)o;

                        // Check if the items has been deleted or
                        // if something else now owns the item (picked it up for example)
                        if( item.Deleted || item.Parent != null )
                        {
                            so.SpawnedObjects.RemoveAt( x );
                            x--;
                            removed = true;
                        }
                    }
                    else if( o is Mobile )
                    {
                        Mobile m = (Mobile)o;

                        if( m.Deleted )
                        {
                            // Remove the delete mobile from the list
                            so.SpawnedObjects.RemoveAt( x );
                            x--;
                            removed = true;
                        }
                        else if( m is BaseCreature )
                        {
                            // Check if the creature has been tamed
                            // and if it is, remove it from the list of spawns
                            if( ((BaseCreature)m).Controled || ((BaseCreature)m).IsStabled )
                            {
                                so.SpawnedObjects.RemoveAt( x );
                                x--;
                                removed = true;
                            }
                        }
                    }
                    else
                    {
                        // Don't know what this is, so remove it
                        so.SpawnedObjects.RemoveAt( x );
                        x--;
                        removed = true;
                    }
                }
            }

            // Check if anything has been removed
            if( removed == true )
                InvalidateProperties();
        }

        public int TotalSpawnedObjects
        {
            get
            {
                int Count = 0;
                foreach( SpawnObject so in this.m_SpawnObjects )
                    Count += so.SpawnedObjects.Count;

                return Count;
            }
        }

        public int TotalSpawnObjectCount
        {
            get
            {
                int Count = 0;
                foreach( SpawnObject so in this.m_SpawnObjects )
                    Count += so.Count;

                return Count;
            }
        }

        public void OnTick()
        {
            this.Defrag();
            this.DoTimer();

            if( this.m_Group == true )
            {
                if( this.TotalSpawnedObjects <= 0 )
                    Respawn();
                else
                    return;
            }
            else
                Spawn();
        }
		
        public void Respawn()
        {
            // Delete all currently spawned objects
            this.RemoveSpawnObjects();

            // Respawn all objects up to the spawners current maximum allowed
            for( int x = 0; x < this.m_Count; x++ )
                this.Spawn();
        }
		
        public void Spawn()
        {
            if( this.m_SpawnObjects.Count > 0 )
            {
                // Try 10 times to pick a random spawn
                for( int x = 0; x < 10; x++ )
                {
                    // Pick a random object to spawn
                    int SpawnIndex = Utility.Random( this.m_SpawnObjects.Count );

                    // Check if the spawned item has reached its max count
                    if( ((SpawnObject)this.m_SpawnObjects[SpawnIndex]).SpawnedObjects.Count < ((SpawnObject)this.m_SpawnObjects[SpawnIndex]).Count  )
                    {
                        // Found a valid spawn object
                        if( this.Spawn( SpawnIndex ) == true )
                            return;
                    }
                }
            }
        }
		
        public void Spawn( string SpawnObjectName )
        {
            for( int i = 0; i < this.m_SpawnObjects.Count; i++ )
            {
                if( ((SpawnObject)this.m_SpawnObjects[i]).TypeName.ToUpper() == SpawnObjectName.ToUpper() )
                {
                    this.Spawn( i );
                    break;
                }
            }
        }

        public bool Spawn( int index )
        {
            Map map = this.Map;

            // Make sure everything is ok to spawn an object
            if( ( map == null ) ||
                ( map == Map.Internal ) ||
                ( this.m_SpawnObjects.Count == 0 ) ||
                ( index < 0 ) ||
                ( index >= this.m_SpawnObjects.Count ) )
                return false;

            // Remove any spawns that don't belong to the spawner any more.
            this.Defrag();

            // Get the spawn object at the required index
            SpawnObject TheSpawn = this.m_SpawnObjects[index] as SpawnObject;

            // Check if the object retrieve is a valid SpawnObject
            if( TheSpawn != null )
            {
                int CurrentCreatureMax = TheSpawn.Count;
                int CurrentCreatureCount = TheSpawn.SpawnedObjects.Count;

                // Check that the current object to be spawned has not reached its maximum allowed
                // and make sure that the maximum spawner count has not been exceeded as well
                if( ( CurrentCreatureCount >= CurrentCreatureMax ) ||
                    ( this.TotalSpawnedObjects >= this.m_Count ) )
                    return false;

                Type type = SpawnerType.GetType( TheSpawn.TypeName );

                if( type != null )
                {
                    try
                    {
                        object o = Activator.CreateInstance( type );

                        if( o is Mobile )
                        {
                            Mobile m = (Mobile)o;

                            TheSpawn.SpawnedObjects.Add( m );
                            InvalidateProperties();

                            m.Map = map;

                            if( ( m is BaseVendor || m is Banker || m is Healer ) &&
                                ( this.Map.CanFit( this.Location, SpawnFitSize, true, false ) == true ) )
                                m.Location = this.Location;
                            else
                                m.Location = this.GetSpawnPosition();

                            if( m is BaseCreature )
                            {
                                BaseCreature c = (BaseCreature)m;
                                c.RangeHome = m_HomeRange;
                                c.CurrentWayPoint = m_WayPoint;

                                if ( m_Team > 0 )
                                    c.Team = m_Team;

                                // Check if this spawner uses absolute (from spawnER location)
                                // or relative (from spawnED location) as the mobiles home point
                                if( this.m_HomeRangeIsRelative == true )
                                    c.Home = m.Location; // Mobiles spawned location is the home point
                                else
                                    c.Home = this.Location; // Spawners location is the home point
                            }

                            return true;
                        }
                        else if( o is Item )
                        {
                            Item item = (Item)o;

                            TheSpawn.SpawnedObjects.Add( item );
                            InvalidateProperties();

                            item.MoveToWorld( this.GetSpawnPosition(), map );
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        public Point3D GetSpawnPosition()
        {
            Map map = Map;

            if( map == null )
                return Location;

            // Try 20 times to find a Spawnable location.
            for( int i = 0; i < 20; i++ )
            {
                // Take the top left position of the map and
                // pick a random distance based on the width and
                // height of the spawn area.
                int x = this.m_X + Utility.Random( this.m_Width );
                int y = this.m_Y + Utility.Random( this.m_Height);
                int z = Map.GetAverageZ( x, y );

                if( Map.CanFit( x, y , this.Z, SpawnFitSize ) )
                    return new Point3D( x, y, this.Z );
                else if( Map.CanFit( x, y, z, SpawnFitSize ) )
                    return new Point3D( x, y, z );
            }

            return this.Location;
        }

        public void DoTimer()
        {
            if ( !m_Running )
                return;

            int minSeconds = (int)m_MinDelay.TotalSeconds;
            int maxSeconds = (int)m_MaxDelay.TotalSeconds;

            TimeSpan delay = TimeSpan.FromSeconds( Utility.RandomMinMax( minSeconds, maxSeconds ) );
            DoTimer( delay );
        }

        public void DoTimer( TimeSpan delay )
        {
            if ( !m_Running )
                return;

            m_End = DateTime.Now + delay;

            if ( m_Timer != null )
                m_Timer.Stop();

            m_Timer = new InternalTimer2( this, delay );
            m_Timer.Start();
        }

        public int GetCreatureMax( int index )
        {
            this.Defrag();
            return ((SpawnObject)this.m_SpawnObjects[index]).Count;
        }

        public void RemoveSpawnObjects()
        {
            this.Defrag();

            foreach( SpawnObject so in this.m_SpawnObjects )
            {
                for( int i = 0; i < so.SpawnedObjects.Count; ++i )
                {
                    object o = so.SpawnedObjects[i];

                    if ( o is Item )
                        ((Item)o).Delete();
                    else if ( o is Mobile )
                        ((Mobile)o).Delete();
                }
            }

            // Defrag again
            this.Defrag();

            InvalidateProperties();
        }
		
        public void AddSpawnObject( string SpawnObjectName )
        {
            this.Defrag();

            // Find the spawn object and increment its count by one
            foreach( SpawnObject so in this.m_SpawnObjects )
            {
                if( so.TypeName.ToUpper() == SpawnObjectName.ToUpper() )
                {
                    // Add one to the total count
                    this.m_Count++;

                    // Increment the max count for the current creature
                    so.Count++;
                    this.Spawn( so.TypeName );
                }
            }

            InvalidateProperties();
        }

        public void DeleteSpawnObject( string SpawnObjectName )
        {
            bool WasRunning = this.m_Running;

            try
            {
                // Stop spawning for a moment
                this.Stop();
            
                // Clean up any spawns marked as deleted
                this.Defrag();

                // Keep a reference to the spawn object
                SpawnObject TheSpawn = null;

                // Find the spawn object and increment its count by one
                foreach( SpawnObject so in this.m_SpawnObjects )
                {
                    if( so.TypeName.ToUpper() == SpawnObjectName.ToUpper() )
                    {
                        // Set the spawn
                        TheSpawn = so;

                        break;
                    }
                }

                // Was the spawn object found
                if( TheSpawn != null )
                {
                    // Subtract one to the total count
                    this.m_Count--;

                    // Make sure the count does not go negative
                    if( this.m_Count < 0 )
                        this.m_Count = 0;

                    // Decrement the max count for the current creature
                    TheSpawn.Count--;

                    // Make sure the spawn count does not go negative
                    if( TheSpawn.Count < 0 )
                        TheSpawn.Count = 0;

                    // Remove any spawns over the count
                    while( TheSpawn.SpawnedObjects.Count > TheSpawn.Count )
                    {
                        object o = TheSpawn.SpawnedObjects[0];
                        
                        // Delete the object
                        if( o is Item )
                            ((Item)o).Delete();
                        else if ( o is Mobile )
                            ((Mobile)o).Delete();

                        TheSpawn.SpawnedObjects.RemoveAt(0);
                    }

                    // Check if the spawn object should be removed
                    if( TheSpawn.Count < 1 )
                        this.m_SpawnObjects.Remove( TheSpawn );
                }

                InvalidateProperties();
            }
            finally
            {
                if( WasRunning )
                    this.Start();
            }
        }

        public void BringToHome()
        {
            this.Defrag();

            foreach( SpawnObject so in this.m_SpawnObjects )
            {
                for( int i = 0; i < so.SpawnedObjects.Count; ++i )
                {
                    object o = so.SpawnedObjects[i];

                    if ( o is Mobile )
                    {
                        Mobile m = (Mobile)o;

                        m.Map = Map;
                        m.Location = new Point3D( Location );
                    }
                    else if ( o is Item )
                    {
                        Item item = (Item)o;

                        item.MoveToWorld( Location, Map );
                    }
                }
            }
        }

        public override void OnDelete()
        {
            base.OnDelete();

            if( this.ShowBounds == true )
                this.ShowBounds = false;

            this.RemoveSpawnObjects();
            
            if( this.m_Timer != null )
                this.m_Timer.Stop();
        }
		
        public override void OnLocationChange( Point3D oldLocation )
        {
            if( this.m_HomeRangeIsRelative == true )
            {
                // Keep the original dimensions the same (Width, Height),
                // just recalculate the new top left corner
                this.m_X = this.X - ( this.m_Width / 2 );
                this.m_Y = this.Y - ( this.m_Height / 2 );
                InvalidateProperties();
            }
            else
            {
                // Set the new top left corner based on the spawn objects
                // new location and the home range.
                this.m_X = this.X - ( this.m_HomeRange / 2 );
                this.m_Y = this.Y - ( this.m_HomeRange / 2 );
                this.m_Width = this.m_HomeRange;
                this.m_Height = this.m_HomeRange;
                InvalidateProperties();
            }

            // Check if the spawner is showing its bounds
            if( this.ShowBounds == true )
            {
                this.ShowBounds = false;
                this.ShowBounds = true;
            }
        }

        public override void Serialize( GenericWriter writer )
        {
            base.Serialize( writer );

            writer.Write( (int) 1 ); // version

            // Version 1
            writer.Write( this.m_UniqueId );
            writer.Write( this.m_HomeRangeIsRelative );

            // Version 0
            writer.Write( this.m_Name );
            writer.Write( this.m_X );
            writer.Write( this.m_Y );
            writer.Write( this.m_Width );
            writer.Write( this.m_Height );
            writer.Write( this.m_WayPoint );
            writer.Write( this.m_Group );
            writer.Write( this.m_MinDelay );
            writer.Write( this.m_MaxDelay );
            writer.Write( this.m_Count );
            writer.Write( this.m_Team );
            writer.Write( this.m_HomeRange );
            writer.Write( this.m_Running );
			
            if( this.m_Running )
                writer.Write( this.m_End - DateTime.Now );

            // Write the spawn object list
            writer.Write( this.SpawnObjects.Length );
            for( int i = 0; i < this.SpawnObjects.Length; ++i )
            {
                // Write the type and maximum count
                writer.Write( (string)this.SpawnObjects[i].TypeName );
                writer.Write( (int)this.SpawnObjects[i].Count );

                // Write the spawned object information
                writer.Write( this.SpawnObjects[i].SpawnedObjects.Count );
                for( int x = 0; x < this.SpawnObjects[i].SpawnedObjects.Count; ++x )
                {
                    object o = this.SpawnObjects[i].SpawnedObjects[x];

                    if ( o is Item )
                        writer.Write( (Item)o );
                    else if ( o is Mobile )
                        writer.Write( (Mobile)o );
                    else
                        writer.Write( Serial.MinusOne );
                }
            }
        }

        public override void Deserialize( GenericReader reader )
        {
            base.Deserialize( reader );

            int version = reader.ReadInt();
            switch ( version )
            {
                case 1:
                {
                    this.m_UniqueId = reader.ReadString();
                    this.m_HomeRangeIsRelative = reader.ReadBool();
                    goto case 0;
                }
                case 0:
                {
                    this.m_Name = reader.ReadString();
                    this.m_X = reader.ReadInt();
                    this.m_Y = reader.ReadInt();
                    this.m_Width = reader.ReadInt();
                    this.m_Height = reader.ReadInt();
                    this.m_WayPoint = reader.ReadItem() as WayPoint;
                    this.m_Group = reader.ReadBool();
                    this.m_MinDelay = reader.ReadTimeSpan();
                    this.m_MaxDelay = reader.ReadTimeSpan();
                    this.m_Count = reader.ReadInt();
                    this.m_Team = reader.ReadInt();
                    this.m_HomeRange = reader.ReadInt();
                    this.m_Running = reader.ReadBool();

                    if( this.m_Running == true )
                    {
                        TimeSpan delay = reader.ReadTimeSpan();
                        this.DoTimer( delay );
                    }
					
                    // Read in the size of the spawn object list
                    int SpawnListSize = reader.ReadInt();
                    this.m_SpawnObjects = new ArrayList( SpawnListSize );
                    for( int i = 0; i < SpawnListSize; ++i )
                    {
                        string TypeName = reader.ReadString();
                        int TypeMaxCount = reader.ReadInt();
                        
                        SpawnObject TheSpawnObject = new SpawnObject( TypeName, TypeMaxCount );

                        this.m_SpawnObjects.Add( TheSpawnObject );

                        if ( SpawnerType.GetType( TypeName ) == null )
                        {
                            if ( m_WarnTimer == null )
                                m_WarnTimer = new WarnTimer2();

                            m_WarnTimer.Add( Location, Map, TypeName );
                        }

                        // Read in the number of spawns already
                        int SpawnedCount = reader.ReadInt();

                        TheSpawnObject.SpawnedObjects = new ArrayList( SpawnedCount );

                        for( int x = 0; x < SpawnedCount; ++x )
                        {
                            IEntity e = World.FindEntity( reader.ReadInt() );

                            if( e != null )
                                TheSpawnObject.SpawnedObjects.Add( e );
                        }
                    }

                    break;
                }
            }
        }

        
        internal string GetSerializedObjectList()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach( SpawnObject so in this.SpawnObjects )
            {
                if( sb.Length > 0 )
                    sb.Append( ':' ); // ':' Separates multiple object types

                sb.AppendFormat( "{0}={1}", so.TypeName, so.Count ); // '=' separates object name from maximum amount
            }

            return sb.ToString();
        }

        public class SpawnObject
        {
            public string TypeName;
            public int Count;
            public ArrayList SpawnedObjects;

            public SpawnObject( string Name, int MaxAmount )
            {
                this.TypeName = Name;
                this.Count = MaxAmount;
                this.SpawnedObjects = new ArrayList();
            }

            internal static SpawnObject[] LoadSpawnObjectsFromString( string ObjectList )
            {
                // Clear the spawn object list
                ArrayList NewSpawnObjects = new ArrayList();

                if( ObjectList.Length > 0 )
                {
                    // Split the string based on the object separator first ':'
                    string[] SpawnObjectList = ObjectList.Split( ':' );

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
                                    NewSpawnObjects.Add( so );
                                }
                            }
                        }
                    }
                }

                return (SpawnObject[])NewSpawnObjects.ToArray( typeof(SpawnObject) );
            }
        }

        private class InternalTimer2 : Timer
        {
            private XmlSpawner m_Spawner;

            public InternalTimer2( XmlSpawner spawner, TimeSpan delay ) : base( delay )
            {
                Priority = TimerPriority.OneSecond;
                m_Spawner = spawner;
            }

            protected override void OnTick()
            {
                if ( m_Spawner != null )
                    if ( !m_Spawner.Deleted )
                        m_Spawner.OnTick();
            }
        }

        private class WarnTimer2 : Timer
        {
            private ArrayList m_List;

            private class WarnEntry2
            {
                public Point3D m_Point;
                public Map m_Map;
                public string m_Name;

                public WarnEntry2( Point3D p, Map map, string name )
                {
                    m_Point = p;
                    m_Map = map;
                    m_Name = name;
                }
            }

            public WarnTimer2() : base( TimeSpan.FromSeconds( 1.0 ) )
            {
                m_List = new ArrayList();
                Start();
            }

            public void Add( Point3D p, Map map, string name )
            {
                m_List.Add( new WarnEntry2( p, map, name ) );
            }

            protected override void OnTick()
            {
                try
                {
                    Console.WriteLine( "Warning: {0} bad spawns detected, logged: 'badspawn.log'", m_List.Count );

                    using ( StreamWriter op = new StreamWriter( "badspawn.log", true ) )
                    {
                        op.WriteLine( "# Bad spawns : {0}", DateTime.Now );
                        op.WriteLine( "# Format: X Y Z F Name" );
                        op.WriteLine();

                        foreach ( WarnEntry2 e in m_List )
                            op.WriteLine( "{0}\t{1}\t{2}\t{3}\t{4}", e.m_Point.X, e.m_Point.Y, e.m_Point.Z, e.m_Map, e.m_Name );

                        op.WriteLine();
                        op.WriteLine();
                    }
                }
                catch
                {
                }
            }
        }
    }

    public class XmlSpawnerGump : Gump
    {
        private XmlSpawner m_Spawner;

        public XmlSpawnerGump( XmlSpawner spawner ) : base( 0, 0 )
        {
            m_Spawner = spawner;

            AddPage( 0 );

            AddBackground( 0, 0, 320, /*371*/474, 5054 );
            AddAlphaRegion( 0, 0, 320, /*371*/474 );

            AddImageTiled( 3, 5, 224, 23, 0x52 );
            AddImageTiled( 4, 6, 222, 21, 0xBBC );
            AddTextEntry( 6, 5, 219, 21, 50, 999, spawner.Name );

            AddButton( 5, /*347*/450, 0xFB1, 0xFB3, 0, GumpButtonType.Reply, 0 );
            AddLabel( 38, /*347*/450, 0x384, "Close" );

            AddButton( 5, /*325*/428, 0xFB7, 0xFB9, 1, GumpButtonType.Reply, 0 );
            AddLabel( 38, /*325*/428, 0x384, "Okay" );

            AddButton( 80, /*325*/428, 0xFB4, 0xFB6, 2, GumpButtonType.Reply, 0 );
            AddLabel( 113, /*325*/428, 0x384, "Bring to Home" );

            AddButton( 80, /*347*/450, 0xFA8, 0xFAA, 3, GumpButtonType.Reply, 0 );
            AddLabel( 113, /*347*/450, 0x384, "Total Respawn" );

            // Props button
            AddButton( 231, /*325*/428, 0xFAB, 0xFAD, 9999, GumpButtonType.Reply, 0 );
            AddLabel( 264, /*325*/428, 0x384, "Props" );

            // Add Current / Max count labels
            AddLabel( 231, 9, 68, "Count" );
            AddLabel( 272, 9, 33, "Max" );

            for ( int i = 0;  i < 18; i++ )
            {
                AddButton( 5, ( 22 * i ) + 34, 0x15E0, 0x15E4, 4 + (i * 2), GumpButtonType.Reply, 0 );
                AddButton( 20, ( 22 * i ) + 34, 0x15E2, 0x15E6, 5 + (i * 2), GumpButtonType.Reply, 0 );

                AddImageTiled( 38, ( 22 * i ) + 30, 189, 23, 0x52 );
                AddImageTiled( 39, ( 22 * i ) + 31, 187, 21, 0xBBC );
            
                string str = "";

                if ( i < this.m_Spawner.SpawnObjects.Length )
                {
                    str = (string)this.m_Spawner.SpawnObjects[i].TypeName;
                    int count = this.m_Spawner.SpawnObjects[i].SpawnedObjects.Count;
                    int max = this.m_Spawner.SpawnObjects[i].Count;

                    // Add current count
                    AddImageTiled( 231, ( 22 * i ) + 30, 40, 23, 0x52 );
                    AddImageTiled( 232, ( 22 * i ) + 31, 37, 21, 0xBBC );
                    AddLabel( 233, ( 22 * i ) + 30, 68, count.ToString() );

                    // Add maximum count
                    AddImageTiled( 272, ( 22 * i ) + 30, 40, 23, 0x52 );
                    AddImageTiled( 273, ( 22 * i ) + 31, 37, 21, 0xBBC );
                    AddLabel( 275, ( 22 * i ) + 30, 33, max.ToString() );
                }

                AddTextEntry( 42, ( 22 * i ) + 31, 184, 21, 0, i, str );
            }
        }

        public XmlSpawner.SpawnObject[] CreateArray( RelayInfo info, Mobile from )
        {
            ArrayList SpawnObjects = new ArrayList();

            for ( int i = 0;  i < 13; i++ )
            {
                TextRelay te = info.GetTextEntry( i );

                if ( te != null )
                {
                    string str = te.Text;

                    if ( str.Length > 0 )
                    {
                        str = str.Trim();

                        Type type = SpawnerType.GetType( str );

                        if ( type != null )
                            SpawnObjects.Add( new XmlSpawner.SpawnObject( str, 0 ) );
                        else
                            from.SendMessage( "{0} is not a valid type name.", str );
                    }
                }
            }

            return (XmlSpawner.SpawnObject[])SpawnObjects.ToArray( typeof(XmlSpawner.SpawnObject) );
        }
	
        public override void OnResponse( NetState state, RelayInfo info )
        {
            if( this.m_Spawner.Deleted )
                return;

            // Get the current name
            this.m_Spawner.Name = info.GetTextEntry( 999 ).Text;

            // Update the creature list
            this.m_Spawner.SpawnObjects = CreateArray( info, state.Mobile );

            switch ( info.ButtonID )
            {
                case 0: // Close
                {
                    return;
                }
                case 1: // Okay
                {
                    break;
                }
                case 2: // Bring everything home
                {
                    m_Spawner.BringToHome();
                    break;
                }
                case 3: // Complete respawn
                {
                    m_Spawner.Respawn();
                    break;
                }
                case 9999:
                {
                    // Show the props window for the spawner
                    state.Mobile.SendGump( new PropertiesGump( state.Mobile, m_Spawner ) );
                    return;
                }
                default:
                {
                    int buttonID = info.ButtonID - 4;
                    int index = buttonID / 2;
                    int type = buttonID % 2;

                    TextRelay entry = info.GetTextEntry( index );

                    if ( entry != null && entry.Text.Length > 0 )
                    {
                        if ( type == 0 ) // Add creature
                            m_Spawner.AddSpawnObject( entry.Text );
                        else // Remove creatures
                            m_Spawner.DeleteSpawnObject( entry.Text );
                    }

                    break;
                }
            }

            // Create a new gump
            m_Spawner.OnDoubleClick( state.Mobile );
        }
    }
}
