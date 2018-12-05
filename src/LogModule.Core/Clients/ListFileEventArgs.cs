using System;

namespace LogModule.Clients
{
    public class ListFileEventArgs : EventArgs
    {
        public ListFileEventArgs(string[] fileList)
        {
            FileList = fileList;
        }

        public string[] FileList { get; internal set; }
    }
}
