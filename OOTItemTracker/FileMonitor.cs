using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using static OOTItemTracker.Program;

namespace OOTItemTracker
{
    /// <summary>
    /// Automatically monitors all the files in a directory, determines which files are the most recent, and reload them when changes are detected.
    /// </summary>
    public class FileMonitor : IDisposable
    {
        //filepath
        public readonly string directory;

        //tracker variables
        private long timeLastAccessed = -1;
        public DateTime LastAccessTime
        {
            get
            {
                return new DateTime(timeLastAccessed);
            }
        }
        private string activeFile;
        public string ActiveFile
        {
            get
            {
                return activeFile;
            }
        }
        private ushort naviCounter = 0;

        //save file
        private OOTSaveFile ootSave;
        public OOTSaveFile SaveFile
        {
            get
            {
                lock(saveFileLock)
                {
                    return ootSave;
                }
            }
        }

        //logic control
        private Thread thread;
        private object saveFileLock = new object();

        /// <summary>
        /// Create a file monitor to automatically monitor the requested file for changes
        /// </summary>
        /// <param name="filepath"></param>
        public FileMonitor(string directory)
        {
            this.directory = directory;
            thread = new Thread(() => MonitorLoop());
            thread.Start();
        }

        /// <summary>
        /// Begin execution of the monitor loop
        /// </summary>
        private void MonitorLoop()
        {
            while(true)
            {
                var file = GetLastFile();
                if(file.Item1 > timeLastAccessed)
                {
                    ushort naviCounter = GetNaviCounter(file.Item2);
                    if(this.naviCounter != naviCounter)
                    {
                        ootSave = OpenSave(file.Item2, file.Item1, naviCounter);
                    }
                }
                Thread.Sleep(50);
            }
        }

        private Tuple<long, string> GetLastFile()
        {
            long lastAccessTime = -1;
            string output = "";
            foreach(var file in Directory.GetFiles(directory, "*.srm"))
            {
                long time = Math.Max(File.GetLastAccessTime(file).Ticks, File.GetLastWriteTime(file).Ticks);
                if(time > lastAccessTime)
                {
                    lastAccessTime = time;
                    output = file;
                }
            }
            return new Tuple<long, string>(lastAccessTime, output);
        }

        private OOTSaveFile OpenSave(string filepath, long timeLastAccessed, ushort naviCounter)
        {
            byte[] arr;
            OOTSaveFile output;

            lock(saveFileLock)
            {
                try
                {
                    using(BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open)))
                    {
                        br.BaseStream.Position = Program.SAVE_FILE_HEAD;
                        arr = br.ReadBytes(Program.SAVE_FILE_SIZE);
                    }
                    output = new OOTSaveFile(arr);
                    this.naviCounter = naviCounter;
                    this.timeLastAccessed = timeLastAccessed;
                    this.activeFile = filepath;
                    return output;
                }
                catch(Exception e)
                {
                    if(e is ThreadAbortException)
                    {
                        throw e;
                    }
                    return ootSave;
                }
            }
        }

        private ushort GetNaviCounter(string filepath)
        {
            using(BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open)))
            {
                try
                {
                    br.BaseStream.Position += Program.SAVE_FILE_HEAD;
                    br.BaseStream.Position += NAVI_ADDRESS;
                    return br.ReadUInt16();
                }
                catch(Exception e)
                {
                    if(e is ThreadAbortException)
                    {
                        throw e;
                    }
                    return 0;
                }
            }
        }

        public void Dispose()
        {
            thread.Abort();
        }
    }
}