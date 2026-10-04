// -----------------------------------------------------------------------
// <copyright file="PhotonUtilitiesUnity.cs" company="Exit Games GmbH">
// Photon Realtime API - Copyright (C) 2022 Exit Games GmbH
// </copyright>
// <summary>
//   Unity Editor Utils for Photon
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER
#define SUPPORTED_UNITY
#endif


#if SUPPORTED_UNITY && UNITY_EDITOR

namespace Photon.Realtime
{
    using System;
    using System.Linq;

    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;

    [InitializeOnLoad]
    public static class PhotonUtilitiesUnity
    {
        static PhotonUtilitiesUnity()
        {
            ApplyDefinesRealtimeV4();     // to be used in Realtime v4 (a.k.a. LoadBalancing)
            //ApplyDefinesRealtimeV5();       // to be used in Realtime v5
        }

        private static void ApplyDefinesRealtimeV4()
        {
            #if !PHOTON_REALTIME_4
            AddScriptingDefineSymbolToAllBuildTargetGroups("PHOTON_REALTIME_4");
            #endif
        }

        private static void ApplyDefinesRealtimeV5()
        {
            #if !PHOTON_REALTIME_5_OR_NEWER
            AddScriptingDefineSymbolToAllBuildTargetGroups("PHOTON_REALTIME_5_OR_NEWER");
            #endif
        }


        /// <summary>Adds a given scripting define symbol to all build target groups.</summary>
        /// <param name="defineSymbol">Define symbol.</param>
        public static void AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol)
        {
            var defineSymbols = GetCurrentDefines()
                               .Split(';')
                               .Select(d => d.Trim())
                               .ToList();

            if (!defineSymbols.Contains(defineSymbol))
            {
                defineSymbols.Add(defineSymbol);

                try
                {
                    SetCurrentDefines(string.Join(";", defineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.Log("Could not set Photon " + defineSymbol + " defines for current build target. Caught: " + e);
                }
            }
        }


        private static string GetCurrentDefines()
        {
            #if UNITY_SERVER
            var defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Server);
            #else
            var group   = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
            #endif

            return defines;
        }

        private static void SetCurrentDefines(string defines)
        {
            #if UNITY_SERVER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Server, defines);
            #else
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), defines);
            #endif
        }
    }
}
#endif