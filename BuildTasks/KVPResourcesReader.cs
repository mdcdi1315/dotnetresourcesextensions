﻿
using System;
using System.Collections;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions.BuildTasks
{
    internal class KVPResourcesReader : System.Resources.IResourceReader , IStreamOwnerBase
    {
        private System.IO.Stream stream;
        private System.Boolean strmown;
        private Dictionary<System.String, String> res;

        private KVPResourcesReader() 
        {
            res = null;
            strmown = false;
            stream = null;
        }

        public KVPResourcesReader(System.String path) : this()
        {
            stream = new System.IO.FileStream(path , System.IO.FileMode.Open);
            strmown = true;
        }

        public KVPResourcesReader(System.IO.Stream stream) : this()
        {
            this.stream = stream;
        }

        public System.Boolean IsStreamOwner { get => strmown; set => strmown = value; }

        private void FetchData()
        {
            stream.Position = 0;
            System.Int32 rb , idx;
            System.String data = System.String.Empty;
            res = new();
            System.Boolean cond = true;
            while (cond && (rb = stream.ReadByte()) > -1)
            {
                switch (rb) {
                    case 10:
                    case 13:
                        data = MsIniStringsEncoder.Decode(data);
                        idx = data.IndexOf('=');
                        if (idx == -1) {
                            data = System.String.Empty;
                            System.Diagnostics.Debug.WriteLine("KVP parser cannot find the \'=\' sign required for splitting the resources from values.");
                            continue;
                        }
                        res.Add(data.Remove(idx), data.Substring(idx + 1));
                        break;
                    case 35:
                        if (System.String.IsNullOrEmpty(data)) { 
                            // In such case we will skip up to next line.
                            while ((rb = stream.ReadByte()) > -1)
                            {
                                if (rb == 10 || rb == 13) { break; }
                            }
                            // And then FetchData will continue to parse...
                        }
                        break;
                    default:
                        data += (System.Char)rb;
                        break;
                }
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            res?.Clear();
            FetchData();
            return res.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Close()
        {
            if (strmown) { stream?.Close(); }
        }

        public void Dispose()
        {
            if (strmown) { stream?.Dispose(); }
        }
    }
}
