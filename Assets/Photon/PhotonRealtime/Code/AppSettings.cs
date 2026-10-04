// -----------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>Settings for Photon application(s) and the server to connect to.</summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_2017_4_OR_NEWER
#define SUPPORTED_UNITY
#endif

namespace Photon.Realtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ExitGames.Client.Photon;

    #if SUPPORTED_UNITY || NETFX_CORE
    using Hashtable = ExitGames.Client.Photon.Hashtable;
    using SupportClass = ExitGames.Client.Photon.SupportClass;
    #endif


    /// <summary>
    /// Settings for Photon application(s) and the server to connect to.
    /// </summary>
    /// <remarks>
    /// This is Serializable for Unity, so it can be included in ScriptableObject instances.
    /// </remarks>
    #if !NETFX_CORE || SUPPORTED_UNITY
    [Serializable]
    #endif
    public class AppSettings
    {
        /// <summary>AppId for Realtime or PUN.</summary>
        public string AppIdRealtime;

        /// <summary>AppId for Photon Fusion.</summary>
        public string AppIdFusion;

        /// <summary>AppId for Photon Chat.</summary>
        public string AppIdChat;

        /// <summary>AppId for Photon Voice.</summary>
        public string AppIdVoice;

        /// <summary>AppId for Photon Video.</summary>
        /// <remarks>Photon Video is a separate SDK and AppId-type. Components that work for Voice and Video alike can use AppIdVoiceOrVideo.</remarks>
        public string AppIdVideo;

        /// <summary>Gets either the AppIdVoice or the AppIdVideo for convenient use in either SDK.</summary>
        /// <remarks>If the Video SDK is enabled, AppIdVideo is preferred. If that is empty, AppIdVoice is returned.</remarks>
        public string AppIdVoiceOrVideo
        {
            #if PHOTON_VOICE_VIDEO_ENABLE
            get { return string.IsNullOrEmpty(this.AppIdVideo) ? this.AppIdVoice : this.AppIdVideo; }
            #else
            get { return string.IsNullOrEmpty(this.AppIdVoice) ? this.AppIdVideo : this.AppIdVoice; }
            #endif
        }

        /// <summary>The AppVersion can be used to identify builds and will split the AppId distinct "Virtual AppIds" (important for matchmaking).</summary>
        public string AppVersion;


        /// <summary>If false, the app will attempt to connect to a Master Server (which is obsolete but sometimes still necessary).</summary>
        /// <remarks>if true, Server points to a NameServer (or is null, using the default), else it points to a MasterServer.</remarks>
        public bool UseNameServer = true;

        /// <summary>Can be set to any of the Photon Cloud's region names to directly connect to that region.</summary>
        /// <remarks>if this IsNullOrEmpty() AND UseNameServer == true, use BestRegion. else, use a server</remarks>
        public string FixedRegion;

        /// <summary>Set to a previous BestRegionSummary value before connecting.</summary>
        /// <remarks>
        /// This is a value used when the client connects to the "Best Region".<br/>
        /// If this is null or empty, all regions gets pinged. Providing a previous summary on connect,
        /// speeds up best region selection and makes the previously selected region "sticky".<br/>
        ///
        /// Unity clients should store the BestRegionSummary in the PlayerPrefs.
        /// You can store the new result by implementing <see cref="IConnectionCallbacks.OnConnectedToMaster"/>.
        /// If <see cref="LoadBalancingClient.SummaryToCache"/> is not null, store this string.
        /// To avoid storing the value multiple times, you could set SummaryToCache to null.
        /// </remarks>
        #if SUPPORTED_UNITY
        [NonSerialized]
        #endif
        public string BestRegionSummaryFromStorage;

        /// <summary>The address (hostname or IP) of the server to connect to.</summary>
        public string Server;

        /// <summary>If not null, this sets the port of the first Photon server to connect to (that will "forward" the client as needed).</summary>
        public int Port;

        /// <summary>The address (hostname or IP and port) of the proxy server.</summary>
        public string ProxyServer;

        /// <summary>The network level protocol to use.</summary>
        public ConnectionProtocol Protocol = ConnectionProtocol.Udp;

        /// <summary>Enables a fallback to another protocol in case a connect to the Name Server fails.</summary>
        /// <remarks>See: LoadBalancingClient.EnableProtocolFallback.</remarks>
        public bool EnableProtocolFallback = true;

        /// <summary>Defines how authentication is done. On each system, once or once via a WSS connection (safe).</summary>
        public AuthModeOption AuthMode = AuthModeOption.Auth;

        /// <summary>If true, the client will request the list of currently available lobbies.</summary>
        public bool EnableLobbyStatistics;

        /// <summary>Log level for the network lib.</summary>
        public DebugLevel NetworkLogging = DebugLevel.ERROR;

        /// <summary>If true, the Server field contains a Master Server address (if any address at all).</summary>
        public bool IsMasterServerAddress
        {
            get { return !this.UseNameServer; }
        }

        /// <summary>If true, the client should fetch the region list from the Name Server and find the one with best ping.</summary>
        /// <remarks>See "Best Region" in the online docs.</remarks>
        public bool IsBestRegion
        {
            get { return this.UseNameServer && string.IsNullOrEmpty(this.FixedRegion); }
        }

        /// <summary>If true, the default nameserver address for the Photon Cloud should be used.</summary>
        public bool IsDefaultNameServer
        {
            get { return this.UseNameServer && string.IsNullOrEmpty(this.Server); }
        }

        /// <summary>If true, the default ports for a protocol will be used.</summary>
        public bool IsDefaultPort
        {
            get { return this.Port <= 0; }
        }

        /// <summary>ToString but with more details.</summary>
        public string ToStringFull()
        {
            var sb = new StringBuilder();

            sb.Append("AppId ");

            var appIds = new List<string>();

            AppendAppIdIfNotEmpty(appIds, "Realtime/PUN", AppIdRealtime);
            AppendAppIdIfNotEmpty(appIds, "Fusion", AppIdFusion);
            AppendAppIdIfNotEmpty(appIds, "Chat", AppIdChat);
            AppendAppIdIfNotEmpty(appIds, "Voice", AppIdVoice);
            AppendAppIdIfNotEmpty(appIds, "Video", AppIdVideo);

            sb.Append(string.Join(", ", appIds.ToArray()));

            sb.Append($", NameServer: {UseNameServer}");
            sb.Append($", Region: {FixedRegion}");
            sb.Append($", AppVersion: {AppVersion}");
            sb.Append($", Server: {Server}");
            sb.Append($", Port: {Port}");
            sb.Append($", Proxy: {ProxyServer}");
            sb.Append($", AuthMode: {AuthMode}");
            sb.Append($", Protocol: {Protocol}");
            sb.Append($", Enable Protocol Fallback: {EnableProtocolFallback}");
            sb.Append($", Lobby Statistics: {EnableLobbyStatistics}");
            sb.Append($", Network Logging: {NetworkLogging}");

            return sb.ToString();

            void AppendAppIdIfNotEmpty(List<string> list, string label, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add($"{label}: {HideAppId(value)}");
                }
            }
        }

        /// <summary>Checks if a string is a Guid by attempting to create one.</summary>
        /// <param name="val">The potential guid to check.</param>
        /// <returns>True if new Guid(val) did not fail.</returns>
        public static bool IsAppId(string val)
        {
            try
            {
                new Guid(val);
            }
            catch
            {
                return false;
            }

            return true;
        }


        private string HideAppId(string appId)
        {
            return string.IsNullOrEmpty(appId) || appId.Length < 8
                       ? appId
                       : string.Concat(appId.Substring(0, 8), "***");
        }

        /// <summary>Copies values of this instance to the target.</summary>
        /// <param name="target">Target instance.</param>
        /// <returns>The target.</returns>
        public AppSettings CopyTo(AppSettings d)
        {
            d.AppIdRealtime = this.AppIdRealtime;
            d.AppIdFusion = this.AppIdFusion;
            d.AppIdChat = this.AppIdChat;
            d.AppIdVoice = this.AppIdVoice;
            d.AppIdVideo = this.AppIdVideo;
            d.AppVersion = this.AppVersion;
            d.UseNameServer = this.UseNameServer;
            d.FixedRegion = this.FixedRegion;
            d.BestRegionSummaryFromStorage = this.BestRegionSummaryFromStorage;
            d.Server = this.Server;
            d.Port = this.Port;
            d.ProxyServer = this.ProxyServer;
            d.Protocol = this.Protocol;
            d.AuthMode = this.AuthMode;
            d.EnableLobbyStatistics = this.EnableLobbyStatistics;
            d.NetworkLogging = this.NetworkLogging;
            d.EnableProtocolFallback = this.EnableProtocolFallback;
            return d;
        }
    }
}
