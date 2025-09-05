using System;
using System.Windows.Forms;

namespace SpawnEditor
{
	public class ManagedMap : Panel
	{
        private short _zoomLevel;
        private short _mapFile;

        public short ZoomLevel
        {
            get { return _zoomLevel; }
            set { _zoomLevel = value; }
        }

        public bool DrawStatics { get; set; }

        public short MapFile
        {
            get { return _mapFile; }
            set { _mapFile = value; }
        }

        public void SetClientPath(string path)
        {
        }

        public void SetCenter(short x, short y)
        {
        }

        public short CtrlToMapX(short x)
        {
            return x;
        }

        public short CtrlToMapY(short y)
        {
            return y;
        }

        public short GetMapHeight(short x, short y)
        {
            return 0;
        }

        public int AddDrawRect(short x, short y, short width, short height, short layer, int color)
        {
            return -1;
        }

        public void RemoveDrawRectAt(int index)
        {
        }

        public void RemoveDrawRects()
        {
        }

        public void RemoveDrawObjects()
        {
        }
	}
}
