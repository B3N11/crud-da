using System.IO;
using System.Collections.Generic;

namespace CarCRUD.Networking
{
    public class FileHandler
    {
        public int id;
        public string name;
        public string path;
        public long size;
        public bool created = false;
        private bool restriceted = false;       //FileHandler can be accessed by one client at a time
        private bool autoDispose = false;

        public NetClientController ncc;

        public FileStream stream;
        public List<NetClient> clients = new List<NetClient>();

        public FileHandler(int _id, string _path, bool _autoDispose, FileMode _mode, NetClientController _ncc = null, bool _restricted = false)
        {
            try { FileInfo file = new FileInfo(_path); }
            catch { return; }

            id = _id;
            path = _path;
            ncc = _ncc;
            restriceted = _restricted;
            autoDispose = _autoDispose;
            SetupFile(_mode);
            created = true;
        }

        #region Setup
        private void SetupFile(FileMode _mode)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                name = file.Name;
                size = file.Length;
                stream = new FileStream(path, _mode);
            }
            catch { }
        }
        #endregion

        #region General
        public bool Close(bool forced = false)
        {
            //Closing is not allowed as long as clients are using this FileHandler
            if ((clients.Count != 0 && !forced) || (clients.Count == 0 && !autoDispose && !forced))
                return false;

            if (forced) clients.ForEach(c => c.ReleaseFileHandler());

            stream?.Dispose();
            clients = null;
            ncc?.RemoveFileHandler(this);

            return true;
        }
        #endregion

        #region Client Handle
        /// <summary>
        /// Binds a NetClient to this FileHandler.
        /// </summary>
        /// <param name="_client"></param>
        /// <returns>Returns the result of the operation. (bool)</returns>
        public bool SetClient(NetClient _client)
        {
            //If _client is null or already exists in list => return
            if (_client == null || clients.Exists(c => c.id == _client.id))
                return false;

            if (restriceted && clients.Count > 0)
                return false;

            _client.fileHandler = this;
            clients.Add(_client);
            return true;
        }

        /// <summary>
        /// An instance of a NetClient releases this FileHandler.
        /// </summary>
        /// <returns>Returns the result of the operation. (bool)</returns>
        public bool Release(NetClient _client)
        {
            if (_client == null || !clients.Exists(c => c.id == _client.id))
                return false;

            _client.fileHandler = null;
            clients.Remove(_client);

            //Tries to close this FileHandler
            Close();
            return true;
        }
        #endregion
    }
}