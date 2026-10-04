// ----------------------------------------------------------------------------
// <copyright file="ConnectionHandler.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   If the game logic does not call Service() for whatever reason, this keeps the connection.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_2017_4_OR_NEWER
#define SUPPORTED_UNITY
#endif


namespace Photon.Realtime
{
    using System.Text;
    using System.Collections.Generic;


    /// <summary>
    /// The SystemConnectionSummary (SBS) is useful to analyze low level connection issues in Unity. This requires a ConnectionHandler in the scene.
    /// </summary>
    /// <remarks>
    /// A LoadBalancingClient automatically creates a SystemConnectionSummary on these disconnect causes:
    /// DisconnectCause.ExceptionOnConnect, DisconnectCause.Exception, DisconnectCause.ServerTimeout and DisconnectCause.ClientTimeout.
    ///
    /// The SBS can then be turned into an integer (ToInt()) or string to debug the situation or use in analytics.
    /// Both, ToString and ToInt summarize the network-relevant conditions of the client at and before the connection fail, including the PhotonPeer.SocketErrorCode.
    ///
    /// Important: To correctly create the SBS instance, a ConnectionHandler component must be present and enabled in the
    /// Unity scene hierarchy. In best case, keep the ConnectionHandler on a GameObject which is flagged as
    /// DontDestroyOnLoad.
    /// </remarks>
    public class SystemConnectionSummary
    {
        // SystemConditionSummary v0  has 32 bits:
        // Version bits (4 bits)
        // UDP, TCP, WS, WSS (WebRTC potentially) (3 bits)
        // 1 bit empty
        //
        // AppQuits
        // AppPause
        // AppPauseRecent
        // AppOutOfFocus
        //
        // AppOutOfFocusRecent
        // NetworkReachability (Unity value)
        // ErrorCodeFits (ErrorCode > short.Max would be a problem)
        // WinSock (true) or BSD (false) Socket Error Codes
        //
        // -> 12 of upper 16 bits are used
        //
        // lower 2 bytes are the Socket Error Code (capped to 0xFFFF)


        /// <summary>Version of the SystemConnectionSummary type.</summary>
        public readonly byte Version = 0;

        /// <summary>Which protocol is used. Refer to ConnectionProtocol.</summary>
        public byte UsedProtocol;

        /// <summary>True if the Unity app is closing / shut down.</summary>
        public bool AppQuits;

        /// <summary>True if the Unity app is paused.</summary>
        public bool AppPause;
        /// <summary>True if the Unity app was paused recently (past 5 sec).</summary>
        public bool AppPauseRecent;

        /// <summary>True if the Unity app is out of focus / minimized.</summary>
        public bool AppOutOfFocus;
        /// <summary>True if the Unity app was out of focus / minimized recently (past 5 sec).</summary>
        public bool AppOutOfFocusRecent;

        /// <summary>True if the Unity engine tells us the network is reachable.</summary>
        public bool NetworkReachable;

        /// <summary>True if the Socket-level error code fits into the usual byte "budget".</summary>
        public bool ErrorCodeFits;
        /// <summary>True if the Socket-level error code is WinSock based.</summary>
        public bool ErrorCodeWinSock;

        /// <summary>Socket-level error code (if any is available).</summary>
        public int SocketErrorCode;

        private static readonly string[] ProtocolIdToName = { "UDP", "TCP", "2(N/A)", "3(N/A)", "WS", "WSS", "6(N/A)", "7WebRTC" };

        internal class SCSBitPos
        {
            /// <summary>28 and up. 4 bits.</summary>
            internal const int Version = 28;
            /// <summary>25 and up. 3 bits.</summary>
            internal const int UsedProtocol = 25;
            /// <summary>Position of an empty bit.</summary>
            internal const int EmptyBit = 24;
            /// <summary>App Quits was called bit.</summary>
            internal const int AppQuits = 23;
            /// <summary>App Pause was called bit.</summary>
            internal const int AppPause = 22;
            /// <summary>App Quits was called recently bit.</summary>
            internal const int AppPauseRecent = 21;
            /// <summary>App not in focus bit.</summary>
            internal const int AppOutOfFocus = 20;
            /// <summary>App not in focus recently bit.</summary>
            internal const int AppOutOfFocusRecent = 19;
            /// <summary>Unity signals network is reachable bit.</summary>
            internal const int NetworkReachable = 18;
            /// <summary>ErrorCode is small enough to fit bit.</summary>
            internal const int ErrorCodeFits = 17;
            /// <summary>Error code is of WinSock type bit.</summary>
            internal const int ErrorCodeWinSock = 16;
        }

        /// <summary>Brief error description per Windows socket error code.</summary>
        static readonly Dictionary<int, string> UdpSocketErrors = new Dictionary<int, string>
                                                                  {
                                                                      { 10004,  "WSAEINTR - interrupted (temp)" },
                                                                      { 10009,  "WSAEBADF - bad file descriptor (fatal)" },
                                                                      { 10013,  "WSAEACCES - blocked by filter or missing SO_BROADCAST (fatal)" },
                                                                      { 10014,  "WSAEFAULT - invalid buffer pointer (fatal)" },
                                                                      { 10022,  "WSAEINVAL - socket not bound or invalid argument (fatal)" },
                                                                      { 10035,  "WSAEWOULDBLOCK - buffer full or no data yet (temp)" },
                                                                      { 10036,  "WSAEINPROGRESS - operation in progress (temp)" },
                                                                      { 10038,  "WSAENOTSOCK - socket handle invalid (fatal)" },
                                                                      { 10039,  "WSAEDESTADDRREQ - destination address required (fatal)" },
                                                                      { 10040,  "WSAEMSGSIZE - send: datagram too large / receive: datagram truncated (fatal)" },
                                                                      { 10049,  "WSAEADDRNOTAVAIL - cannot assign requested address (fatal)" },
                                                                      { 10050,  "WSAENETDOWN - network subsystem failed (fatal)" },
                                                                      { 10051,  "WSAENETUNREACH - network unreachable (fatal)" },
                                                                      { 10054,  "WSAECONNRESET - ICMP port unreachable from remote (fatal)" },
                                                                      { 10055,  "WSAENOBUFS - buffer exhaustion (temp)" },
                                                                      { 10057,  "WSAENOTCONN - socket not connected (fatal)" },
                                                                      { 10058,  "WSAESHUTDOWN - cannot send after socket shutdown (fatal)" },
                                                                      { 10064,  "WSAEHOSTDOWN - host is down (fatal)" },
                                                                      { 10065,  "WSAEHOSTUNREACH - host unreachable, no route (fatal)" },
                                                                  };

        /// <summary>
        /// Creates a SystemConnectionSummary for an incident of a local LoadBalancingClient. This gets used automatically by the LoadBalancingClient!
        /// </summary>
        /// <remarks>
        /// If the LoadBalancingClient.SystemConnectionSummary is non-null after a connection-loss, you can call .ToInt() and send this to analytics or log it.
        ///
        /// </remarks>
        /// <param name="client"></param>
        public SystemConnectionSummary(LoadBalancingClient client)
        {
            if (client != null)
            {
                // protocol = 3 bits! potentially adding WebRTC.
                this.UsedProtocol = (byte)((int)client.LoadBalancingPeer.UsedProtocol & 7);
                this.SocketErrorCode = (int)client.LoadBalancingPeer.SocketErrorCode;
            }

            this.AppQuits = ConnectionHandler.AppQuits;
            this.AppPause = ConnectionHandler.AppPause;
            this.AppPauseRecent = ConnectionHandler.AppPauseRecent;
            this.AppOutOfFocus = ConnectionHandler.AppOutOfFocus;

            this.AppOutOfFocusRecent = ConnectionHandler.AppOutOfFocusRecent;
            this.NetworkReachable = ConnectionHandler.IsNetworkReachableUnity();

            this.ErrorCodeFits = this.SocketErrorCode >= 0 && this.SocketErrorCode <= ushort.MaxValue; // socket error code fits in 4 bytes
            this.ErrorCodeWinSock = true;
        }

        /// <summary>
        /// Creates a SystemConnectionSummary instance from an int (reversing ToInt()). This can then be turned into a string again.
        /// </summary>
        /// <param name="summary">An int, as provided by ToInt(). No error checks yet.</param>
        public SystemConnectionSummary(int summary)
        {
            this.Version = GetBits(ref summary, SCSBitPos.Version, 0xF);
            this.UsedProtocol = GetBits(ref summary, SCSBitPos.UsedProtocol, 0x7);
            // 1 empty bit

            this.AppQuits = GetBit(ref summary, SCSBitPos.AppQuits);
            this.AppPause = GetBit(ref summary, SCSBitPos.AppPause);
            this.AppPauseRecent = GetBit(ref summary, SCSBitPos.AppPauseRecent);
            this.AppOutOfFocus = GetBit(ref summary, SCSBitPos.AppOutOfFocus);

            this.AppOutOfFocusRecent = GetBit(ref summary, SCSBitPos.AppOutOfFocusRecent);
            this.NetworkReachable = GetBit(ref summary, SCSBitPos.NetworkReachable);
            this.ErrorCodeFits = GetBit(ref summary, SCSBitPos.ErrorCodeFits);
            this.ErrorCodeWinSock = GetBit(ref summary, SCSBitPos.ErrorCodeWinSock);

            this.SocketErrorCode = summary & 0xFFFF;
        }

        /// <summary>
        /// Turns the SystemConnectionSummary into an integer, which can be be used for analytics purposes. It contains a lot of info and can be used to instantiate a new SystemConnectionSummary.
        /// </summary>
        /// <returns>Compact representation of the context for a disconnect issue.</returns>
        public int ToInt()
        {
            int result = 0;
            SetBits(ref result, this.Version, SCSBitPos.Version);
            SetBits(ref result, this.UsedProtocol, SCSBitPos.UsedProtocol);
            // 1 empty bit

            SetBit(ref result, this.AppQuits, SCSBitPos.AppQuits);
            SetBit(ref result, this.AppPause, SCSBitPos.AppPause);
            SetBit(ref result, this.AppPauseRecent, SCSBitPos.AppPauseRecent);
            SetBit(ref result, this.AppOutOfFocus, SCSBitPos.AppOutOfFocus);

            SetBit(ref result, this.AppOutOfFocusRecent, SCSBitPos.AppOutOfFocusRecent);
            SetBit(ref result, this.NetworkReachable, SCSBitPos.NetworkReachable);
            SetBit(ref result, this.ErrorCodeFits, SCSBitPos.ErrorCodeFits);
            SetBit(ref result, this.ErrorCodeWinSock, SCSBitPos.ErrorCodeWinSock);


            // insert socket error code as lower 2 bytes
            int socketErrorCode = this.SocketErrorCode & 0xFFFF;
            result |= socketErrorCode;

            return result;
        }

        /// <summary>
        /// A readable debug log string of the context for network problems.
        /// </summary>
        /// <returns>SystemConnectionSummary as readable string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string transportProtocol = ProtocolIdToName[this.UsedProtocol];
            string annotation = "";
            UdpSocketErrors.TryGetValue(this.SocketErrorCode, out annotation);


            sb.Append($"SCS v{this.Version} {transportProtocol} SocketError: {this.SocketErrorCode} ");
            if (!string.IsNullOrEmpty(annotation)) sb.Append($"[{annotation}] ");

            if (this.AppQuits) sb.Append("AppQuits ");
            if (this.AppPause) sb.Append("AppPause ");
            if (!this.AppPause && this.AppPauseRecent) sb.Append("AppPauseRecent ");
            if (this.AppOutOfFocus) sb.Append("AppOutOfFocus ");
            if (!this.AppOutOfFocus && this.AppOutOfFocusRecent) sb.Append("AppOutOfFocusRecent ");
            if (!this.NetworkReachable) sb.Append("NetworkUnreachable ");
            if (!this.ErrorCodeFits) sb.Append("ErrorCodeRangeExceeded ");

            if (!this.ErrorCodeWinSock) sb.Append("BSDSock");

            string result = sb.ToString();
            return result;
        }

        /// <summary>Extracts the recorded error code from a SystemConnectionSummary binary representation.</summary>
        /// <param name="summary">Pass the value of SystemConnectionSummary.ToInt().</param>
        /// <returns>Error code from a SCS summary (int) or -1 if the version can't be read.</returns>
        public static int GetErrorCode(int summary)
        {
            byte version = GetBits(ref summary, SCSBitPos.Version, 0xF);
            if (version == 0)
            {
                // in SCS v0, the error code is put into the last 2 bytes of an int
                return summary & 0xFFFF;
            }

            return -1;
        }

        /// <summary>Gets a specific bit out of the value at the given position.</summary>
        internal static bool GetBit(ref int value, int bitpos)
        {
            int result = (value >> bitpos) & 1;
            return result != 0;
        }

        /// <summary>Gets bitvals out of the value at the given position.</summary>
        internal static byte GetBits(ref int value, int bitpos, byte mask)
        {
            int result = (value >> bitpos) & mask;
            return (byte)result;
        }

        /// <summary>Applies bitval to bitpos (no matter value's initial bit value).</summary>
        internal static void SetBit(ref int value, bool bitval, int bitpos)
        {
            if (bitval)
            {
                value |= 1 << bitpos;
            }
            else
            {
                value &= ~(1 << bitpos);
            }
        }

        /// <summary>Applies bitvals via OR operation (expects bits in value to be 0 initially).</summary>
        internal static void SetBits(ref int value, byte bitvals, int bitpos)
        {
            value |= bitvals << bitpos;
        }
    }
}