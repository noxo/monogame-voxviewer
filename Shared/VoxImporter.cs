using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;

using Microsoft.Xna.Framework;

namespace XnaVoxViewer
{
    class VoxImporter
    {

        private bool timelapse;

        public delegate void BlockConstructedEventHandler(Tuple<Vector3, int> blockData);
        public event BlockConstructedEventHandler BlockConstructed;

        public VoxImporter(bool timelapse)
        {
            this.timelapse = timelapse;
        }

        private static uint[] voxColors = new uint[] {
		    0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
		    0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
		    0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
		    0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
		    0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
		    0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
		    0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
		    0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
		    0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
		    0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
		    0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
		    0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
		    0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
		    0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
		    0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
		    0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
	    };

        private struct MagicaVoxelData
        {
            public byte x;
            public byte y;
            public byte z;
            public int color;

            public MagicaVoxelData(Stream stream, bool subsample)
            {
                x = (byte)(subsample ? stream.ReadByte() / 2 : stream.ReadByte());
                y = (byte)(subsample ? stream.ReadByte() / 2 : stream.ReadByte());
                z = (byte)(subsample ? stream.ReadByte() / 2 : stream.ReadByte());
                color = stream.ReadByte() & 0xff;
            }
        }

        private static int ReadInt32(Stream stream)
        {
            // needs little endian to big endian conversion
            int b1 = stream.ReadByte() & 0xff;
            int b2 = stream.ReadByte() & 0xff;
            int b3 = stream.ReadByte() & 0xff;
            int b4 = stream.ReadByte() & 0xff;
            int res = (b4 << 24) + (b3 << 16) + (b2 << 8) + (b1 << 0);
            //int res = (b1 << 24) + (b2 << 16) + (b3 << 8) + (b4 << 0);
            return res;
        }

        private static char[] ReadChars(Stream stream, int len)
        {

            char[] buffer = new char[len];

            for (int i = 0; i < len; i++)
            {
                buffer[i] = (char)stream.ReadByte();
            }

            return buffer;

        }

        public async Task ReadMagica(Stream stream)
        {

            int[] colors = null;
            MagicaVoxelData[] voxelData = null;

            String magic = new String(ReadChars(stream, 4));
            int version = ReadInt32(stream);

            // a MagicaVoxel .vox file starts with a 'magic' 4 character 'VOX ' identifier
            if (magic.CompareTo("VOX ") == 0)
            {
                int sizex = 0, sizey = 0, sizez = 0;
                bool subsample = false;

                while (stream.Position < stream.Length)
                {
                    // each chunk has an ID, size and child chunks
                    char[] chunkId = ReadChars(stream, 4);
                    int chunkSize = ReadInt32(stream);
                    int childChunks = ReadInt32(stream);
                    String chunkName = new String(chunkId);

                    // there are only 2 chunks we only care about, and they are SIZE and XYZI
                    if (chunkName.CompareTo("SIZE") == 0)
                    {
                        sizex = ReadInt32(stream);
                        sizey = ReadInt32(stream);
                        sizez = ReadInt32(stream);

                        if (sizex > 32 || sizey > 32)
                            subsample = true;


                        stream.Seek(chunkSize - 4 * 3, SeekOrigin.Current);
                    }
                    else if (chunkName.CompareTo("XYZI") == 0)
                    {
                        // XYZI contains n voxels
                        int numVoxels = ReadInt32(stream);
                        int div = (subsample ? 2 : 1);

                        // each voxel has x, y, z and color index values
                        voxelData = new MagicaVoxelData[numVoxels];
                        for (int i = 0; i < voxelData.Length; i++)
                            voxelData[i] = new MagicaVoxelData(stream, subsample);
                    }
                    else if (chunkName.CompareTo("RGBA") == 0)
                    {
                        colors = new int[256];

                        for (int i = 0; i < 256; i++)
                        {

                            int r = stream.ReadByte() & 0xff;
                            int g = stream.ReadByte() & 0xff;
                            int b = stream.ReadByte() & 0xff;
                            int a = stream.ReadByte() & 0xff;

                            colors[i] = (a & 0xff) << 24 | (r & 0xff) << 16 | (g & 0xff) << 8 | (b & 0xff);

                        }
                    }
                    else stream.Seek(chunkSize, SeekOrigin.Current);   // read any excess bytes
                }

                if (voxelData.Length == 0)
                    return;

                // now push the voxel data into our voxel chunk structure
                for (int i = 0; i < voxelData.Length; i++)
                {
                    // do not store this voxel if it lies out of range of the voxel chunk (32x128x32)
                    if (voxelData[i].x > 31 || voxelData[i].y > 31 || voxelData[i].z > 127)
                        continue;

                    int color = 0;

                    if (colors == null) // use default palette
                    {
                        // WTF hardcoded palette is ABGR, doing ABGR => ARGB
                        color = (int)voxColors[voxelData[i].color - 1];
                        int r = color & 0xff;
                        int g = (color >> 8) & 0xff;
                        int b = (color >> 16) & 0xff;
                        int a = (color >> 28) & 0xff;
                        color = a << 28 | r << 16 | g << 8 | b;
                    }
                    else // use palette from file
                    {
                        color = colors[voxelData[i].color - 1];

                    }

                    if (timelapse)
                        await Task.Delay(2);

                    if (BlockConstructed != null)
                    {
                        Vector3 position = new Vector3(voxelData[i].x, voxelData[i].y, voxelData[i].z);
                        BlockConstructed(new Tuple<Vector3, int>(position, color));
                    }
                }
            }
        }

    }
}
