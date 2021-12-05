using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CarCRUD.Networking
{
    /// <summary>
    /// A class created to manage NetClients in groups.
    /// </summary>
    public class NetClientController
    {
        //General
        public string id = string.Empty;
        public int port { get; private set; }
        public int maxBroadcastSize { get; set; }
        public int timeOut = 5;

        //Client Handling
        /// <summary>
        /// Automatically removes disconnected clients upon disconnection.
        /// </summary>
        public bool autoRemove = false;
        private int ncID = -1;
        private int maxClientCount = 0;
        private bool acceptClients = false;
        private TcpListener listener;

        //File Handling
        private int fhID = -1;

        //Library
        public List<NetClient> clients = new List<NetClient>();
        public List<FileHandler> fileHandlers = new List<FileHandler>();

        //Delegates
        public delegate void ClientConnectedHandler(NetClient _client);
        public delegate void ClientAcceptStop(object _object);

        //Events
        public event ClientAcceptStop OnClientAcceptStopped;

        public NetClientController(string _id, int _port, int _maxBroadcastSize = 65535, int _maxClientCount = -1)
        {
            id = _id;
            maxBroadcastSize = _maxBroadcastSize;
            maxClientCount = _maxClientCount;
            port = _port;
        }

        #region Accept Clients
        public async void AcceptClientsAsync(ClientConnectedHandler clientConnectedCallback = null)
        {
            if (clients.Count == maxClientCount)
                return;

            listener = new TcpListener(IPAddress.Any, port);
            acceptClients = true;

            await Task.Run(() => AcceptClients(clientConnectedCallback));

            acceptClients = false;

            if (OnClientAcceptStopped != null)
                OnClientAcceptStopped.Invoke(this);
        }

        private async Task<bool> AcceptClients(ClientConnectedHandler clientConnectedCallback = null)
        {
            try
            {
                listener.Start();
                while (acceptClients)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    string ip = ((IPEndPoint)(tcpClient.Client.RemoteEndPoint)).Address.ToString();

                    NetClient nClient = new NetClient(null, port, this, null, timeOut);
                    if (!AttachClient(nClient)) continue;
                    nClient.Establish(tcpClient);

                    if (clientConnectedCallback != null)
                        clientConnectedCallback(nClient);

                    //maxClientCount is -1 by default => never stops listening
                    if (clients.Count == maxClientCount)
                        return true;
                }
                return true;
            }
            catch { return false; }
        }

        public void StopAcceptingClients()
        {
            acceptClients = false;
        }
        #endregion

        #region Client Handle
        public bool CreateClient(int _port, string _name = null, int _clientTimeOut = 5)
        {
            if (clients.Count == maxClientCount)
                return false;

            NetClient netClient = new NetClient(null, _port, this, _name, _clientTimeOut);

            return netClient.created ? AttachClient(netClient) : false;
        }

        /// <summary>
        /// Attaches the client to this NetClientController. The ID of the client will get changed!
        /// </summary>
        /// <param name="_client"></param>
        /// <returns></returns>
        public bool AttachClient(NetClient _client)
        {
            if (_client == null || clients.Exists(c => c.ipAddress == _client.ipAddress) || clients.Count == maxClientCount)
                return false;

            _client.ncc?.RemoveClient(_client);
            _client.ncc = this;
            _client.id = Guid.NewGuid().ToString();
            clients.Add(_client);

            if (autoRemove)
                _client.OnClientDisconnected += RemoveClientOnDisconnect;

            return true;
        }

        public NetClient GetClient(string _ip)
        {
            if (string.IsNullOrEmpty(_ip) || !clients.Exists(c => c.ipAddress == _ip))
                return null;

            return clients.First(c => c.ipAddress == _ip);
        }

        public bool RemoveClient(NetClient _client)
        {
            if (_client == null || clients.Exists(c => c.id == _client.id))
                return false;

            //Release client from every FileHandler
            _client.ReleaseFileHandler();
            clients.Remove(_client);
            _client.ncc = null;

            return true;
        }
        #endregion

        #region File Handle
        public bool CreateFileHandler(string _path, FileMode _mode)
        {
            //Path is null or there is a FileHandler for that file already
            if (string.IsNullOrEmpty(_path) || fileHandlers.Exists(c => c.path == _path))
                return false;

            FileHandler newFH = new FileHandler(++fhID, _path, true, _mode, this);
            if (newFH.created) fileHandlers.Add(newFH);

            return newFH.created;
        }

        public bool RemoveFileHandler(FileHandler _handler)
        {
            //If _handler is null or does not exists
            if (_handler == null || fileHandlers.Exists(c => c.id == _handler.id))
                return false;

            fileHandlers.Remove(_handler);
            return true;
        }

        /// <summary>
        /// Gets the desired FileHandler instance based on <paramref name="_path"/>.
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        public FileHandler GetFileHandler(string _path)
        {
            if (string.IsNullOrEmpty(_path))
                return null;

            try { return fileHandlers.First(c => c.path == _path); }
            catch { return null; }
        }
        #endregion

        #region Messaging
        /// <summary>
        /// Send a broadcast data byte array that does not exceeds the maxBroadcastLength value. (Zero by default.)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        /// <returns>Returns the result of the operation. (bool)</returns>
        public bool SendBroadcast(byte[] data, NetClient sender = null)
        {
            //Broadcast byte limit exceeded?
            if (data.Length > maxBroadcastSize)
                return false;

            clients.ForEach(c =>
            {
                if (c.id != sender?.id)
                    c.SendAsync(data);
            });

            return true;
        }
        #endregion

        #region Events
        public void RemoveClientOnDisconnect(object _object)
        {
            if (_object == null)
                return;

            NetClient client;
            try { client = _object as NetClient; } catch { return; }

            RemoveClient(client);
        }
        #endregion
    }
}
