using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;

namespace AgileMultiIdeogramFileTester
{
    class gVCFReader
    {
        bool isgzed = false;
        string fileName = "";
        System.IO.StreamReader fs = null;
        System.IO.Stream gfs = null;
        System.IO.Compression.GZipStream gfz = null;
        byte[] buffer = null;
        int bufferPlace = 0;
        int bufferSize = 0;

        public gVCFReader(string fileName)
        {
            if (fileName.ToLower().Substring(fileName.Length - 3).Equals(".gz") == true)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(fileName);
                gfs = fi.OpenRead();
                gfz = new GZipStream(gfs, CompressionMode.Decompress);
                buffer = new byte[4096];
                bufferSize = gfz.Read(buffer, 0, buffer.Length);
            }
            else
            { fs = new System.IO.StreamReader(fileName); }
        }

        public int Peek()
        {
            try
            {
                if (fs != null)
                { return fs.Peek(); }
                else
                {
                    if (bufferPlace == buffer.Length)
                    {
                        bufferSize = gfz.Read(buffer, 0, buffer.Length);
                        bufferPlace = 0;
                        
                    }

                    if (bufferSize == 0) { return 0; }
                    else { return (int)buffer[bufferPlace]; }
                }
            }
            catch (Exception ex) { }
            return 0;
        }

        public string ReadLine()
        {
            try
            {
                if (fs != null)
                { return fs.ReadLine(); }
                else
                {
                    StringBuilder line = new StringBuilder();
                    while (bufferSize > 0)
                    {

                        if (bufferSize  == bufferPlace)
                        {
                            bufferSize = gfz.Read(buffer, 0, buffer.Length);
                            bufferPlace = -1;
                        }
                        else if (buffer[bufferPlace] == (byte)10)
                        {
                            bufferPlace++;
                            return line.ToString();
                        }                         
                        else if (buffer[bufferPlace] == (byte)13)
                        { bufferPlace++; }
                        else { line.Append((char)buffer[bufferPlace]); }
                        bufferPlace++;
                    }
                    return line.ToString();
                }
            }
            catch (Exception ex)
            { }
            return "";
        }

        public void Close()
        {
            if (fs != null)
            { fs.Close(); }
            else
            {
                gfz.Close();
                gfs.Close();
            }
        }

    }
}
