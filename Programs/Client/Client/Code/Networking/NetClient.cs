using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CarCRUD.Networking
{
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
        public event ClientDisconnected OnClientDisconnectedEvent;
        /// <summary>
        /// Raised when a file transport finished or has been canceled.
        /// </summary>
        public event FileTransportResulted OnFileTransportResultedEvent;

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
            OnClientDisconnectedEvent += DisconnectedEventHandle;
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
            tcpClient?.Close();
            tcpClient?.Dispose();
            netStream?.Dispose();
            listener?.Stop();
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

            if (OnClientDisconnectedEvent != null)
                OnClientDisconnectedEvent.Invoke(this);
        }

        private async Task<bool> Send(byte[] data)
        {
            try
            {
                int header = data.Length;       //Create the header for the message                    
                List<byte> finalData = BitConverter.GetBytes(header).ToList();
                List<byte> dataList = data.ToList();
                finalData.AddRange(dataList);

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

            if (OnClientDisconnectedEvent != null)
                OnClientDisconnectedEvent.Invoke(this);
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

            if (result) return;

            //Receive returned false => client is disconnected
            if (OnClientDisconnectedEvent != null)
                OnClientDisconnectedEvent.Invoke(this);
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

                    //If nothing was read due to disconnected sockets
                    if (header <= 0) return false;

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

        }

        private void MessageReceivedEventHandle(object _sender, ref byte[] _data)
        {

        }

        private void DisconnectedEventHandle(object _sender)
        {
            StopClient();
        }

        private void FileTransportResultedHandle(object _sender, bool _result)
        {
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

    public class ConnectionResultEventArgs
    {
        public TcpClient client { get; private set; }
        public string ip { get; set; }
        public Result result { get; set; }

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