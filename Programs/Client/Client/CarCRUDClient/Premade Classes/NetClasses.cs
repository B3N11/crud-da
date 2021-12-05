using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CarCRUD
{
    namespace Networking
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

        /// <summary>
        /// A class representing a connection with a remote endpoint. Can be used for sending, receiving data and handling connection.
        /// </summary>
        public class NetClient
        {
            #region Properties
            //General
            public string id = string.Empty;
            public string ipAddress { get; private set; }
            public int port = 1989;
            public int timeOut = 5;
            public string name = string.Empty;
            public IPEndPoint endPoint;

            public bool created { get; private set; }
            public bool connected = false;
            public bool binded { get; private set; }
            public bool receiveEnabled = true;

            public NetClientController ncc;
            public FileHandler fileHandler;

            //Events
            public delegate void ConnectionTry(object sender, ConnectionResultEventArgs result);
            public delegate void MessageReceived(object sender, ref byte[] data);
            public delegate void ClientDisconnected(object sender);
            public delegate void FileTransportResulted(object sender, bool result);

            /// <summary>
            /// Raised when connection try succeded or failed.
            /// </summary>
            public event ConnectionTry OnConnectionResultedEvent;       //Raised when TcpClient successfully connects or timeouts
            /// <summary>
            /// Raised when a message is received trough TCP stream.
            /// </summary>
            public event MessageReceived OnMessageReceivedEvent;    //Raised when a message is read out from NetworkStream
            /// <summary>
            /// Raised when client is disconnected.
            /// </summary>
            public event ClientDisconnected OnClientDisconnected;
            /// <summary>
            /// Raised when a file transport finished or has been canceled.
            /// </summary>
            public event FileTransportResulted OnFileTransportResultedEvent;
            /// <summary>
            /// Raised when a UDP message arrived.
            /// </summary>
            public event MessageReceived OnMessageReceivedUDPEvent;

            //Network General
            private int bufferSize = 65535;
            private TcpListener listener;
            private TcpClient tcpClient;
            private NetworkStream netStream;

            //File Transport        
            public bool fileTransportInProgress { get; private set; }
            public int streamReadAmount = 0;
            public int maxStreamBytes = 0;
            public double fileTransportedPercentage = 0;
            private CancellationTokenSource fileCancelTokenSource;

            //Constructor
            public NetClient(string _id, int _port, NetClientController _ncc = null, string _name = null, int _timeOut = 5)
            {
                id = _id;
                port = _port;

                ncc = _ncc;
                name = _name;
                timeOut = _timeOut;

                tcpClient = new TcpClient();

                OnConnectionResultedEvent += ConnectionEventHandle;
                OnMessageReceivedEvent += MessageReceivedEventHandle;
                OnMessageReceivedUDPEvent += MessageReceivedEventHandle;
                OnClientDisconnected += DisconnectedEventHandle;
                OnFileTransportResultedEvent += FileTransportResultedHandle;

                created = true;
            }
            #endregion

            #region Connection Handle
            /// <summary>
            /// Sets the reusability of the instance's tcp socket local endpoint. It is recommended to call this method befor establishing a connection.
            /// </summary>
            /// <param name="_reusable"></param>
            public void SetAddressReusability(bool _reusable)
            {
                tcpClient?.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseUnicastPort, _reusable);
                tcpClient?.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, _reusable);
            }

            /// <summary>
            /// Set the TcpClient of the NetClient. Only send connected TcpClients! Raises ConnectionResultEvent.
            /// </summary>
            /// <param name="_tcpClient"></param>
            public void Establish(TcpClient _tcpClient)
            {
                tcpClient = _tcpClient;
                tcpClient.ReceiveBufferSize = bufferSize;
                tcpClient.SendBufferSize = bufferSize;

                endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                ipAddress = endPoint.Address.ToString();
                netStream = tcpClient.GetStream();

                connected = tcpClient.Connected;
                receiveEnabled = true;

                listener?.Stop();
                ReceiveAsync();

                //Raise connection event
                if (OnConnectionResultedEvent != null)
                    OnConnectionResultedEvent.Invoke(this, new ConnectionResultEventArgs(Result.Success, ipAddress, tcpClient));
            }

            /// <summary>
            /// Tries to connect to a server via TCP. Raises ConnectionResultEvent. Setting the _bindSocket to true will make the socket unbindable.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="_port"></param>
            public async void ConnectAsync(string _ip = null, bool _bindSocket = false)
            {
                string ip = ValidateIPAddresses(_ip);
                if (ip == null)
                    return;

                tcpClient = new TcpClient();
                SetAddressReusability(_bindSocket);

                bool result = await Connect(ip, _bindSocket);

                if (result)
                {
                    Establish(tcpClient);
                    return;
                }

                //If failed
                if (tcpClient != null)
                    tcpClient.Close();
                if (OnConnectionResultedEvent != null)
                    OnConnectionResultedEvent.Invoke(this, new ConnectionResultEventArgs(Result.Fail, ip));
            }

            private async Task<bool> Connect(string _ip, bool _bindSocket = false)
            {
                IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, port);
                if (_bindSocket) tcpClient.Client.Bind(_endPoint);

                CancellationTokenSource cts = new CancellationTokenSource();
                CountdownAsync(timeOut * 1000, timeOut, cts);

                while (!cts.Token.IsCancellationRequested)
                {
                    try { await tcpClient?.ConnectAsync(_ip, port); } catch { }
                    if (tcpClient.Connected) return true;
                }
                return false;
            }

            /// <summary>
            /// Closes connection for the TCP Client of this NetClient.
            /// </summary>
            public void Disconnect()
            {
                if (!connected)
                    return;

                try { tcpClient.Close(); } catch { }
            }

            /// <summary>
            /// Starts pinging an IP address for opening a port and waiting for answer. Raises ConnectionResultEvent.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="_port"></param>
            public async void ListenForConnectionAsync()
            {
                if (connected) return;

                bool result = await Task.Run(() => ListenForConnection());

                //If succeeds
                if (result)
                {
                    Establish(tcpClient);
                    return;
                }

                //If fails
                try
                {
                    listener?.Stop();
                    tcpClient?.Close();
                }
                catch { }

                if (OnConnectionResultedEvent != null)
                    OnConnectionResultedEvent.Invoke(this, new ConnectionResultEventArgs(Result.Fail, null));
            }

            private async Task<bool> ListenForConnection()
            {
                try { listener = new TcpListener(IPAddress.Any, port); }
                catch { return false; }
                listener.Start();

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Token.Register(() => listener.Stop());
                CountdownAsync(timeOut * 1000, timeOut, cts);

                try { tcpClient = await listener.AcceptTcpClientAsync(); } catch { }
                if (tcpClient.Connected) return true;
                return false;
            }

            public void StopClient()
            {
                connected = false;
                receiveEnabled = false;
                netStream?.Dispose();
                listener?.Stop();
                tcpClient?.Dispose();
            }
            #endregion

            #region Messaging

            #region TCP
            /// <summary>
            /// Send an array of bytes as data on TCP stream.
            /// </summary>
            /// <param name="data"></param>
            public async void SendAsync(byte[] data)
            {
                if (!connected || fileTransportInProgress)
                    return;

                bool result = await Send(data);

                if (result)
                    return;

                if (OnClientDisconnected != null)
                    OnClientDisconnected.Invoke(this);
            }

            private async Task<bool> Send(byte[] data)
            {
                try
                {
                    int header = data.Length;       //Create the header for the message                    
                    List<byte> finalData = BitConverter.GetBytes(header).ToList();
                    finalData.Concat(data.ToList());

                    netStream.Write(finalData.ToArray(), 0, finalData.Count);        //Write into stream
                    return true;
                }
                catch { return false; }
            }

            /// <summary>
            /// Sends data without header. The data buffer will be written to the NetworkStream as it is.
            /// </summary>
            public async void SendRawAsync(byte[] data)
            {
                if (data == null || data.Length == 0 || tcpClient == null || !connected)
                    return;

                bool result = await SendRaw(data);

                if (result) return;

                if (OnClientDisconnected != null)
                    OnClientDisconnected.Invoke(this);
            }

            private async Task<bool> SendRaw(byte[] data)
            {
                if (data == null || data.Length == 0)
                    return false;

                try { netStream.Write(data, 0, data.Length); }
                catch { return false; }
                return true;
            }

            /// <summary>
            /// Get the received data from TCP stream.
            /// </summary>
            public async void ReceiveAsync()
            {
                if (!connected || fileTransportInProgress)
                    return;

                receiveEnabled = true;
                bool result = await Receive();

                if (result)
                    return;

                //Receive returned false => client is disconnected
                if (OnClientDisconnected != null)
                    OnClientDisconnected.Invoke(this);
            }

            /// <summary>
            /// Receives data.
            /// </summary>
            /// <returns></returns>
            private async Task<bool> Receive()
            {
                try
                {
                    while (receiveEnabled)
                    {
                        await Task.Delay(10);

                        if (fileTransportInProgress)
                            continue;

                        //Get header
                        byte[] headerData = new byte[4];
                        await netStream.ReadAsync(headerData, 0, 4);
                        int header = BitConverter.ToInt32(headerData, 0);

                        //Get data
                        byte[] data = new byte[header];
                        int readBytes = await netStream.ReadAsync(data, 0, data.Length);

                        //Raise event
                        if (OnMessageReceivedEvent != null)
                            OnMessageReceivedEvent.Invoke(this, ref data);
                    }

                    return true;
                }
                catch { return false; }
            }
            #endregion

            #endregion

            #region File Transporting
            /// <summary>
            /// Instantly starts the file receiving process and it lasts as long as it is not resulted or ForceStopFileTransport is not called.
            /// </summary>
            /// <param name="_maxBytesToRead"></param>
            public async void ReceiveFileAsync(int _maxBytesToRead)
            {
                if (fileHandler == null || _maxBytesToRead == 0 || !connected)
                    return;

                maxStreamBytes = _maxBytesToRead;
                fileTransportInProgress = true;

                bool result = await Task.Run(ReceiveFile);

                if (OnFileTransportResultedEvent != null)
                    OnFileTransportResultedEvent.Invoke(this, result);
            }

            private async Task<bool> ReceiveFile()
            {
                try
                {
                    fileCancelTokenSource = new CancellationTokenSource();
                    netStream.CopyToAsync(fileHandler.stream, maxStreamBytes);
                    await Task.Run(GetFilePercentage);

                    return true;
                }
                catch { return false; }
            }

            /// <summary>
            /// Instantly starts the file sending process and it lasts as long as it is not resulted or ForceStopFileTransport is not called.
            /// </summary>
            /// <param name="_maxBytesToWrite"></param>
            public async void SendFileAsync(int _maxBytesToWrite)
            {
                if (fileHandler == null || _maxBytesToWrite == 0 || !connected)
                    return;

                maxStreamBytes = _maxBytesToWrite;
                fileTransportInProgress = true;

                bool result = await Task.Factory.StartNew(SendFile).Result;

                if (OnFileTransportResultedEvent != null)
                    OnFileTransportResultedEvent.Invoke(this, result);
            }

            private async Task<bool> SendFile()
            {
                try
                {
                    fileCancelTokenSource = new CancellationTokenSource();
                    Task.Run(GetFilePercentage);
                    await fileHandler.stream.CopyToAsync(netStream, maxStreamBytes, fileCancelTokenSource.Token);

                    return true;
                }
                catch { return false; }
            }

            private async Task<bool> GetFilePercentage()
            {
                while (fileTransportedPercentage < 100 && !fileCancelTokenSource.Token.IsCancellationRequested)
                {
                    fileTransportedPercentage = ((double)fileHandler.stream.Position * 100 / fileHandler.size);
                    await Task.Delay(10);
                }

                if (fileCancelTokenSource.Token.CanBeCanceled)
                    fileCancelTokenSource.Cancel();
                return true;
            }

            /// <summary>
            /// Instantly stops a file transport operation.
            /// </summary>
            public void ForceStopFileTransport()
            {
                if (fileCancelTokenSource == null || !fileCancelTokenSource.Token.CanBeCanceled)
                    return;

                fileCancelTokenSource.Cancel();
            }
            #endregion

            #region File Handle
            public void AttachToFileHandler(FileHandler _fileHandler)
            {
                //Release previos
                ReleaseFileHandler();

                _fileHandler.SetClient(this);
            }

            public void ReleaseFileHandler()
            {
                fileHandler?.Release(this);
            }
            #endregion

            #region Own Event Handlers
            private void ConnectionEventHandle(object _sender, ConnectionResultEventArgs _result)
            {
                OnConnectionResultedEvent += ConnectionEventHandle;
            }

            private void MessageReceivedEventHandle(object _sender, ref byte[] _data)
            {
                OnMessageReceivedEvent += MessageReceivedEventHandle;
            }

            private void DisconnectedEventHandle(object _sender)
            {
                OnClientDisconnected += DisconnectedEventHandle;

                StopClient();
            }

            private void FileTransportResultedHandle(object _sender, bool _result)
            {
                OnFileTransportResultedEvent += FileTransportResultedHandle;

                fileHandler?.Release(this);

                maxStreamBytes = 0;
                streamReadAmount = 0;
                fileTransportedPercentage = 0;
                fileTransportInProgress = false;
            }
            #endregion

            #region Other Methods
            private string ValidateIPAddresses(string _ip)
            {
                if (ipAddress == null && _ip == null)
                    return null;

                if (_ip == null)
                    return ipAddress;

                return _ip;
            }

            private async void CountdownAsync(int _milliseconds, int _iterations, CancellationTokenSource _cts)
            {
                bool result = await Task.Run(() => Countdown(_milliseconds, _iterations, _cts));

                if (!result) return;

                if (_cts.Token.CanBeCanceled)
                    _cts.Cancel();
            }

            private async Task<bool> Countdown(int _milliseconds, int _iterations, CancellationTokenSource _cts)
            {
                int timePerIteration = _milliseconds / _iterations;
                int tick = _iterations;

                while (!_cts.Token.IsCancellationRequested && tick > 0)
                {
                    tick--;
                    await Task.Delay(timePerIteration);
                }

                if (_cts.Token.IsCancellationRequested) return false;
                return true;
            }
            #endregion
        }

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

        public class ConnectionResultEventArgs
        {
            public TcpClient client { get; }
            public string ip;
            public Result result;

            public ConnectionResultEventArgs(Result _result, string _ip, TcpClient _client = null)
            {
                result = _result;
                ip = _ip;
                client = _client;
            }
        }

        public enum Result
        {
            Success,
            Fail
        }
    }
}