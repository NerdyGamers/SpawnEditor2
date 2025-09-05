using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpawnEditor
{
        public class ManagedMap : Panel
        {
        private short _zoomLevel;
        private short _mapFile;
        private short _centerX;
        private short _centerY;
        private string _clientPath;
        private ArrayList _drawRects = new ArrayList();
        private FileStream _mapStream;
        private byte[] _radarColors;
        private FileStream _staticIndexStream;
        private FileStream _staticStream;
        private Hashtable _staticBlocks = new Hashtable();
        private static readonly int[] _mapWidth = new int[] { 6144, 6144, 2304, 2560, 1448, 1280 };
        private static readonly int[] _mapHeight = new int[] { 4096, 4096, 1600, 2048, 1448, 4096 };

        public short ZoomLevel
        {
            get { return _zoomLevel; }
            set { _zoomLevel = value; Invalidate(); }
        }

        public bool DrawStatics { get; set; }

        public short MapFile
        {
            get { return _mapFile; }
            set { _mapFile = value; OpenMapFiles(); Invalidate(); }
        }

        public void SetClientPath(string path)
        {
            _clientPath = path;
            OpenMapFiles();
            Invalidate();
        }

        public void SetCenter(short x, short y)
        {
            _centerX = x;
            _centerY = y;
            Invalidate();
        }

        private float ZoomFactor
        {
            get
            {
                float f = 1;
                short i = 0;
                for( i = 0; i < _zoomLevel; i++ )
                {
                    f = f * 2;
                }
                return f;
            }
        }

        public short CtrlToMapX(short x)
        {
            float zoom = ZoomFactor;
            short startX = (short)( _centerX - ( this.Width / ( 2 * zoom ) ) );
            return (short)( startX + ( x / zoom ) );
        }

        public short CtrlToMapY(short y)
        {
            float zoom = ZoomFactor;
            short startY = (short)( _centerY - ( this.Height / ( 2 * zoom ) ) );
            return (short)( startY + ( y / zoom ) );
        }

        public short GetMapHeight(short x, short y)
        {
            int id;
            sbyte h;
            if( ReadTile( x, y, out id, out h ) )
            {
                return h;
            }
            return 0;
        }

        public int AddDrawRect(short x, short y, short width, short height, short layer, int color)
        {
            DrawRect rect = new DrawRect();
            rect.X = x;
            rect.Y = y;
            rect.Width = width;
            rect.Height = height;
            rect.Layer = layer;
            rect.Color = color;
            _drawRects.Add( rect );
            Invalidate();
            return _drawRects.Count - 1;
        }

        public void RemoveDrawRectAt(int index)
        {
            if( ( index >= 0 ) && ( index < _drawRects.Count ) )
            {
                _drawRects.RemoveAt( index );
                Invalidate();
            }
        }

        public void RemoveDrawRects()
        {
            _drawRects.Clear();
            Invalidate();
        }

        public void RemoveDrawObjects()
        {
            RemoveDrawRects();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint( e );

            Graphics g = e.Graphics;
            g.Clear( Color.Black );

            float zoom = ZoomFactor;
            int widthTiles = (int)( this.Width / zoom ) + 1;
            int heightTiles = (int)( this.Height / zoom ) + 1;
            int startX = _centerX - (int)( widthTiles / 2 );
            int startY = _centerY - (int)( heightTiles / 2 );

            int[] pixels = new int[ widthTiles * heightTiles ];
            int x = 0;
            while( x < widthTiles )
            {
                int y = 0;
                while( y < heightTiles )
                {
                    int mapX = startX + x;
                    int mapY = startY + y;
                    Color c = GetMapColor( mapX, mapY );
                    pixels[ y * widthTiles + x ] = c.ToArgb();
                    y++;
                }
                x++;
            }

            Bitmap bmp = new Bitmap( widthTiles, heightTiles, PixelFormat.Format32bppArgb );
            BitmapData data = bmp.LockBits( new Rectangle( 0, 0, widthTiles, heightTiles ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
            Marshal.Copy( pixels, 0, data.Scan0, pixels.Length );
            bmp.UnlockBits( data );

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage( bmp, new Rectangle( 0, 0, this.Width, this.Height ) );
            bmp.Dispose();

            int i = 0;
            for( i = 0; i < _drawRects.Count; i++ )
            {
                DrawRect dr = (DrawRect)_drawRects[i];
                Rectangle rect = new Rectangle( (int)( ( dr.X - startX ) * zoom ), (int)( ( dr.Y - startY ) * zoom ), (int)( dr.Width * zoom ), (int)( dr.Height * zoom ) );
                int color = dr.Color | unchecked( (int)0xFF000000 );
                using( Pen pen = new Pen( Color.FromArgb( color ), 1 ) )
                {
                    g.DrawRectangle( pen, rect );
                }
            }
        }

        private void OpenMapFiles()
        {
            if( _mapStream != null )
            {
                _mapStream.Close();
                _mapStream = null;
            }
            if( _staticIndexStream != null )
            {
                _staticIndexStream.Close();
                _staticIndexStream = null;
            }
            if( _staticStream != null )
            {
                _staticStream.Close();
                _staticStream = null;
            }
            _staticBlocks.Clear();
            if( _clientPath == null )
            {
                return;
            }
            string mapPath = Path.Combine( _clientPath, "map" + _mapFile + ".mul" );
            if( File.Exists( mapPath ) )
            {
                _mapStream = new FileStream( mapPath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
            string radarPath = Path.Combine( _clientPath, "radarcol.mul" );
            if( File.Exists( radarPath ) )
            {
                _radarColors = File.ReadAllBytes( radarPath );
            }
            string idxPath = Path.Combine( _clientPath, "staidx" + _mapFile + ".mul" );
            if( File.Exists( idxPath ) )
            {
                _staticIndexStream = new FileStream( idxPath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
            string staticsPath = Path.Combine( _clientPath, "statics" + _mapFile + ".mul" );
            if( File.Exists( staticsPath ) )
            {
                _staticStream = new FileStream( staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
        }

        private bool ReadTile( int x, int y, out int id, out sbyte height )
        {
            id = 0;
            height = 0;
            if( _mapStream == null )
            {
                return false;
            }
            if( x < 0 || y < 0 )
            {
                return false;
            }
            int width = _mapWidth[_mapFile];
            int heightMap = _mapHeight[_mapFile];
            if( x >= width || y >= heightMap )
            {
                return false;
            }
            int blockWidth = width / 8;
            int blockX = x / 8;
            int blockY = y / 8;
            long offset = ( ( blockY * blockWidth ) + blockX ) * 196 + 4;
            offset = offset + ( ( y % 8 ) * 8 + ( x % 8 ) ) * 3;
            _mapStream.Seek( offset, SeekOrigin.Begin );
            int b0 = _mapStream.ReadByte();
            int b1 = _mapStream.ReadByte();
            int b2 = _mapStream.ReadByte();
            if( b2 == -1 )
            {
                return false;
            }
            id = b0 | ( b1 << 8 );
            height = (sbyte)b2;
            return true;
        }

        private int ReadInt32( FileStream s )
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            if( b3 == -1 )
            {
                return -1;
            }
            return b0 | ( b1 << 8 ) | ( b2 << 16 ) | ( b3 << 24 );
        }

        private ArrayList GetStaticBlock( int blockX, int blockY )
        {
            int key = blockY * ( _mapWidth[_mapFile] / 8 ) + blockX;
            ArrayList list = (ArrayList)_staticBlocks[key];
            if( list != null )
            {
                return list;
            }
            list = new ArrayList();
            if( ( _staticIndexStream == null ) || ( _staticStream == null ) )
            {
                _staticBlocks[key] = list;
                return list;
            }
            long offset = key * 12;
            _staticIndexStream.Seek( offset, SeekOrigin.Begin );
            int lookup = ReadInt32( _staticIndexStream );
            int length = ReadInt32( _staticIndexStream );
            ReadInt32( _staticIndexStream );
            if( ( lookup < 0 ) || ( length <= 0 ) )
            {
                _staticBlocks[key] = list;
                return list;
            }
            _staticStream.Seek( lookup, SeekOrigin.Begin );
            int count = length / 7;
            int i = 0;
            for( i = 0; i < count; i++ )
            {
                int id0 = _staticStream.ReadByte();
                int id1 = _staticStream.ReadByte();
                int x = _staticStream.ReadByte();
                int y = _staticStream.ReadByte();
                int z = _staticStream.ReadByte();
                _staticStream.ReadByte();
                _staticStream.ReadByte();
                if( id1 == -1 )
                {
                    break;
                }
                StaticTile st = new StaticTile();
                st.ID = id0 | ( id1 << 8 );
                st.X = (sbyte)x;
                st.Y = (sbyte)y;
                st.Z = (sbyte)z;
                list.Add( st );
            }
            _staticBlocks[key] = list;
            return list;
        }

        private bool GetStaticColor( int x, int y, out Color color )
        {
            color = Color.Black;
            if( !DrawStatics )
            {
                return false;
            }
            if( _radarColors == null )
            {
                return false;
            }
            ArrayList list = GetStaticBlock( x / 8, y / 8 );
            if( list == null || list.Count == 0 )
            {
                return false;
            }
            int tileX = x % 8;
            int tileY = y % 8;
            int topZ = -128;
            int topID = -1;
            int i = 0;
            for( i = 0; i < list.Count; i++ )
            {
                StaticTile st = (StaticTile)list[i];
                if( ( st.X == tileX ) && ( st.Y == tileY ) && ( st.Z >= topZ ) )
                {
                    topZ = st.Z;
                    topID = st.ID;
                }
            }
            if( topID == -1 )
            {
                return false;
            }
            int index = ( topID + 0x4000 ) * 2;
            if( index + 1 >= _radarColors.Length )
            {
                return false;
            }
            int c = _radarColors[index] | ( _radarColors[index + 1] << 8 );
            int r = ( c >> 10 ) & 0x1F;
            int g = ( c >> 5 ) & 0x1F;
            int b = c & 0x1F;
            r = ( r << 3 ) | ( r >> 2 );
            g = ( g << 3 ) | ( g >> 2 );
            b = ( b << 3 ) | ( b >> 2 );
            color = Color.FromArgb( r, g, b );
            return true;
        }

        public void VerifyStaticTiles()
        {
            if( ( _staticIndexStream == null ) || ( _staticStream == null ) || ( _radarColors == null ) )
            {
                return;
            }
            int blocksX = _mapWidth[_mapFile] / 8;
            int blocksY = _mapHeight[_mapFile] / 8;
            int bx = 0;
            while( bx < blocksX )
            {
                int by = 0;
                while( by < blocksY )
                {
                    ArrayList list = GetStaticBlock( bx, by );
                    int i = 0;
                    for( i = 0; i < list.Count; i++ )
                    {
                        StaticTile st = (StaticTile)list[i];
                        Color c;
                        if( !GetStaticColor( bx * 8 + st.X, by * 8 + st.Y, out c ) )
                        {
                            Console.WriteLine( "Missing static color at {0},{1}", bx * 8 + st.X, by * 8 + st.Y );
                        }
                    }
                    by++;
                }
                bx++;
            }
        }

        private Color GetMapColor( int x, int y )
        {
            int id;
            sbyte h;
            if( !ReadTile( x, y, out id, out h ) )
            {
                return Color.Black;
            }
            Color sc;
            if( GetStaticColor( x, y, out sc ) )
            {
                return sc;
            }
            if( _radarColors == null )
            {
                return Color.Black;
            }
            int index = id * 2;
            if( index + 1 >= _radarColors.Length )
            {
                return Color.Magenta;
            }
            int c = _radarColors[index] | ( _radarColors[index + 1] << 8 );
            int r = ( c >> 10 ) & 0x1F;
            int g = ( c >> 5 ) & 0x1F;
            int b = c & 0x1F;
            r = ( r << 3 ) | ( r >> 2 );
            g = ( g << 3 ) | ( g >> 2 );
            b = ( b << 3 ) | ( b >> 2 );
            return Color.FromArgb( r, g, b );
        }

        private class StaticTile
        {
            public int ID;
            public sbyte X;
            public sbyte Y;
            public sbyte Z;
        }

        private class DrawRect
        {
            public short X;
            public short Y;
            public short Width;
            public short Height;
            public short Layer;
            public int Color;
        }
        }
}
