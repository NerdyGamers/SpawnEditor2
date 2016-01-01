using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Scripts.Commands
{
    public class SpawnEditorCommands
    {
        // Spawner world gem object id
        private const int BaseItemId = 0x1F13;

        // DataSet and DataTable names used by the Spawn Editor
        private const string SpawnDataSetName = "Spawns";
        private const string SpawnTablePointName = "Points";

        public static void Initialize()
        {
            Server.Commands.Register( "SpawnShowAll", AccessLevel.Administrator, new CommandEventHandler( ShowSpawnPoints_OnCommand ) );
            Server.Commands.Register( "SpawnHideAll", AccessLevel.Administrator, new CommandEventHandler( HideSpawnPoints_OnCommand ) );
            Server.Commands.Register( "SpawnWipe", AccessLevel.Administrator, new CommandEventHandler( Wipe_OnCommand ) );
            Server.Commands.Register( "SpawnSave", AccessLevel.Administrator, new CommandEventHandler( Save_OnCommand ) );
            Server.Commands.Register( "SpawnLoad", AccessLevel.Administrator, new CommandEventHandler( Load_OnCommand ) );
            Server.Commands.Register( "SpawnEditorGo", AccessLevel.Administrator, new CommandEventHandler( SpawnEditorGo_OnCommand ) );
        }

        [Usage( "SpawnEditorGo <map> | <map> <x> <y> [z]" )]
        [Description( "Go command used with spawn editor, takes the name of the map as the first parameter." )]
        private static void SpawnEditorGo_OnCommand( CommandEventArgs e )
        {
            Mobile from = e.Mobile;

            // Make sure a map name was given at least
            if( e.Length >= 1 )
            {
                // Get the map
                Map NewMap = null;
                string MapName = e.Arguments[0];

                // Convert the xml map value to a real map object
                if( string.Compare( MapName, Map.Trammel.Name, true ) == 0 )
                    NewMap = Map.Trammel;
                else if( string.Compare( MapName, Map.Felucca.Name, true ) == 0 )
                    NewMap = Map.Felucca;
                else if( string.Compare( MapName, Map.Ilshenar.Name, true ) == 0 )
                    NewMap = Map.Ilshenar;
                else if( string.Compare( MapName, Map.Malas.Name, true ) == 0 )
                    NewMap = Map.Malas;
                else
                {
                    from.SendMessage( "Map '{0}' does not exist!", MapName );
                    return;
                }

                // Now that the map has been determined, continue
                // Check if the request is to simply change maps
                if( e.Length == 1 )
                {
                    // Map Change ONLY
                    from.Map = NewMap;
                }
                else if( e.Length == 3 )
                {
                    // Map & X Y ONLY
                    if( NewMap != null )
                    {
                        int x = e.GetInt32( 1 );
                        int y = e.GetInt32( 2 );
                        int z = NewMap.GetAverageZ( x, y );
                        from.Map = NewMap;
                        from.Location = new Point3D( x, y, z );
                    }
                }
                else if( e.Length == 4 )
                {
                    // Map & X Y Z
                    from.Map = NewMap;
                    from.Location = new Point3D( e.GetInt32( 1 ), e.GetInt32( 2 ), e.GetInt32( 3 ) );
                }
                else
                {
                    from.SendMessage( "Format: SpawnEditorGo <map> | <map> <x> <y> [z]" );
                }
            }
        }

        [Usage( "SpawnWipe" )]
        [Description( "Wipes ALL spawners from the current map." )]
        public static void Wipe_OnCommand( CommandEventArgs e )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                e.Mobile.SendMessage( string.Format( "Removing ALL {0} objects from {1} .", "Spawner", e.Mobile.Map ) );

                // Delete spawner's in the world based on the mobiles current map
                int Count = 0;
                ArrayList ToDelete = new ArrayList();
                foreach( Item i in World.Items.Values )
                {
                    if( ( i is Spawner ) && ( i.Map == e.Mobile.Map ) && ( i.Deleted == false ) )
                    {
                        ToDelete.Add( i );
                        Count++;
                    }
                }

                // Delete the items in the array list
                foreach( Item i in ToDelete )
                    i.Delete();

                e.Mobile.SendMessage( string.Format( "Removed {0} {1} objects from {2}.", Count, "Spawner", e.Mobile.Map ) );
            }
            else
                e.Mobile.SendMessage( "You do not have rights to perform this command." );
        }

        [Usage( "SpawnSave [FileName]" )]
        [Description( "Saves all spawner information for the current map to the file specified to be used with the Spawn Editor." )]
        public static void Save_OnCommand( CommandEventArgs e )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                if( e.Arguments.Length == 1 )
                {
                    int Count = 0;

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
                    ds.Tables[SpawnTablePointName].Columns.Add( "Objects" );

                    // Add each spawn point to the new table
                    foreach( Item i in World.Items.Values )
                    {
                        if( ( i.Map == e.Mobile.Map ) && ( i is Spawner ) && ( i.Deleted == false ) )
                        {
                            // Cast the item to a spawner
                            Spawner spawner = (Spawner)i;

                            // Create a new data row
                            DataRow dr = ds.Tables[SpawnTablePointName].NewRow();

                            // Populate the data
                            dr["Name"] = (string)( spawner.Name.Length == 0 ? ( spawner.GetType().Name + Count ) : ( spawner.Name + Count ) );

                            // Create a unique ID
                            dr["UniqueId"] = Guid.NewGuid().ToString();

                            // Get the map name
                            dr["Map"] = (string)spawner.Map.Name;

                            // Calculate the top left based on the centre and home range
                            dr["X"] = (int)( spawner.Location.X - ( spawner.HomeRange / 2 ) );
                            dr["Y"] = (int)( spawner.Location.Y - ( spawner.HomeRange / 2 ) );
                            dr["Width"] = (int)spawner.HomeRange;
                            dr["Height"] = (int)spawner.HomeRange;
                            dr["CentreX"] = (int)spawner.Location.X;
                            dr["CentreY"] = (int)spawner.Location.Y;
                            dr["CentreZ"] = (int)spawner.Location.Z;
                            dr["Range"] = (int)spawner.HomeRange;
                            dr["MaxCount"] = (int)spawner.Count;
                            dr["MinDelay"] = (int)spawner.MinDelay.TotalMinutes;
                            dr["MaxDelay"] = (int)spawner.MaxDelay.TotalMinutes;
                            dr["Team"] = (int)spawner.Team;
                            dr["IsGroup"] = (bool)spawner.Group;
                            dr["IsRunning"] = (bool)spawner.Running;

                            // Create the spawn object list
                            // Make a copy of the original creatures name list
                            ArrayList SpawnObjs = new ArrayList( spawner.CreaturesName );

                            // Sort the list
                            SpawnObjs.Sort();

                            // Create the new version of the spawn object list
                            StringBuilder SpawnObjectList = new StringBuilder();
                            for( int x = 0; x < SpawnObjs.Count; x++ )
                            {
                                string SpawnType = SpawnObjs[x].ToString();
                                int SpawnCount = 0;

                                for( int y = x; y < SpawnObjs.Count; y++ )
                                {
                                    if( SpawnType.ToUpper() == SpawnObjs[y].ToString().ToUpper() )
                                        SpawnCount++;
                                    else
                                        break;
                                }

                                // Increment t by the SpawnCount
                                if( SpawnCount > 0 )
                                    x += ( SpawnCount - 1 );

                                // Add the spawn object to the spawn object list
                                // Check if the object separator needs to be included
                                if( SpawnObjectList.Length > 0 )
                                    SpawnObjectList.Append( ":" );

                                SpawnObjectList.AppendFormat( "{0}={1}", SpawnType, SpawnCount );
                            }

                            // Set the spawn object list
                            dr["Objects"] = SpawnObjectList.ToString();

                            // Add the row the the table
                            ds.Tables[SpawnTablePointName].Rows.Add( dr );

                            // Increment the count
                            Count++;
                        }
                    }

                    // Write out the file
                    ds.WriteXml( e.Arguments[0].ToString() );

                    // Indicate how many spawners were written
                    e.Mobile.SendMessage( "{0} spawner(s) in {1} were saved to file {2}.", Count, e.Mobile.Map, e.Arguments[0].ToString() );
                }
                else
                    e.Mobile.SendMessage( "Usage:  {0} <SpawnFile>", e.Command );
            }
            else
                e.Mobile.SendMessage( "You do not have rights to perform this command." );
        }

        [Usage( "SpawnLoad [FileName]" )]
        [Description( "Load all spawner information from the file created using the Spawn Editor." )]
        public static void Load_OnCommand( CommandEventArgs e )
        {
            if( e.Mobile.AccessLevel == AccessLevel.Administrator )
            {
                if( e.Arguments.Length == 1 )
                {
                    // Check if the file exists
                    if( File.Exists( e.Arguments[0].ToString() ) == true )
                    {
                        int TotalCount = 0;
                        int TrammelCount = 0;
                        int FeluccaCount = 0;
                        int IlshenarCount = 0;
                        int MalasCount = 0;

                        e.Mobile.SendMessage( string.Format( "Loading {0} file {1}.", "Spawner", e.Arguments[0].ToString() ) );

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
                                
                                // Try load the GUID (might not work so create a new GUID)
                                // Don't use it in the normal spawner, but the XmlSpawner does
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
                                string SpawnObjects = (string)dr["Objects"];

                                // Create the creatures name array from the spawn object list
                                ArrayList CreatureNames = new ArrayList();

                                // Check if there are any names in the list
                                if( SpawnObjects.Length > 0 )
                                {
                                    // Split the string based on the object separator first ':'
                                    string[] SpawnObjectList = SpawnObjects.Split( ':' );

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

                                                    // Add the required number of creature names to the list of creatures
                                                    for( int x = 0; x < MaxCount; x++ )
                                                        CreatureNames.Add( SpawnObjectDetails[0].ToString() );
                                                }
                                            }
                                        }
                                    }
                                }
                
                                // Create the spawn
                                Spawner TheSpawn = new Spawner( SpawnMaxCount, SpawnMinDelay, SpawnMaxDelay, SpawnTeam, SpawnHomeRange, CreatureNames );

                                // Try to find a valid Z height
                                int NewZ = SpawnMap.GetAverageZ( SpawnCentreX, SpawnCentreY );
                                
                                if( SpawnMap.CanFit( SpawnCentreX, SpawnCentreY, NewZ, 16 ) == false )
                                { 
                                    for( int i = 1; i <= 20; ++i ) 
                                    { 
                                        if( SpawnMap.CanFit( SpawnCentreX, SpawnCentreY, NewZ + i, 16 ) ) 
                                        { 
                                            NewZ += i; 
                                            break; 
                                        } 
                                        else if ( SpawnMap.CanFit( SpawnCentreX, SpawnCentreY, NewZ - i, 16 ) ) 
                                        { 
                                            NewZ -= i; 
                                            break; 
                                        } 
                                    } 
                                } 

                                // Place the spawner into the world
                                TheSpawn.MoveToWorld( new Point3D( SpawnCentreX, SpawnCentreY, NewZ ), SpawnMap );

                                // Do a total respawn
                                TheSpawn.Respawn();

                                // Increment the count
                                TotalCount++;
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

        [Usage( "SpawnShowAll" )]
        [Description( "Shows all spawners by making them visible and changing their object id to that of a ships mast for easy visiblity." )]
        public static void ShowSpawnPoints_OnCommand( CommandEventArgs e )
        {
            foreach( Item item in World.Items.Values )
            {
                if( item is Spawner )
                {
                    item.Visible = true;    // Make the spawn item visible to everyone
                    item.Movable = true;    // Make the spawn item movable
                    item.Hue = 88;          // Bright blue colour so its easy to spot
                    item.ItemID = 0x3E57;   // Ship Mast (Very tall, easy to see if beneath other objects)
                }
            }
        }

        [Usage( "SpawnHideAll" )]
        [Description( "Hides all spawners by making them invisible and reverting them back to their normal object id." )]
        public static void HideSpawnPoints_OnCommand( CommandEventArgs e )
        {
            foreach( Item item in World.Items.Values )
            {
                if( item is Spawner )
                {
                    item.Visible = false;
                    item.Movable = false;
                    item.Hue = 0;
                    item.ItemID = BaseItemId;
                }
            }
        }
    }
}