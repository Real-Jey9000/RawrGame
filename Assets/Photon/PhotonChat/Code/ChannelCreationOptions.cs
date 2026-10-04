// ----------------------------------------------------------------------------------------------------------------------
// <summary>The Photon Chat Api enables clients to connect to a chat server and communicate with other clients.</summary>
// <remarks>ChannelCreationOptions is a parameter used when subscribing to a public channel for the first time.</remarks>
// <copyright company="Exit Games GmbH">Photon Chat Api - Copyright (C) 2018 Exit Games GmbH</copyright>
// ----------------------------------------------------------------------------------------------------------------------


#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif


namespace Photon.Chat
{
    #if SUPPORTED_UNITY
    using UnityEngine;
    #endif

    public class ChannelCreationOptions
    {
        /// <summary>Default values of channel creation options.</summary>
        public static ChannelCreationOptions Default = new ChannelCreationOptions();
        /// <summary>Whether or not the channel to be created will allow client to keep a list of users.</summary>
        public bool PublishSubscribers { get; set; }
        /// <summary>Limit of the number of users subscribed to the channel to be created.</summary>
        public int MaxSubscribers { get; set; }

        #if SUPPORTED_UNITY
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            Default = new ChannelCreationOptions();
        }
        #endif

        #if CHAT_EXTENDED
        public System.Collections.Generic.Dictionary<string, object> CustomProperties { get; set; }
        #endif
    }
}
