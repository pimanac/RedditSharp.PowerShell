﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using RedditSharp.Things;

namespace RedditSharp.PowerShell
{
    internal static class Session
    {
        ///<summary>Adds an AssemblyResolve handler to redirect all attempts to load a specific assembly name to the specified version.</summary>
        private static void RedirectAssembly(string shortName, Version targetVersion, string publicKeyToken)
        {
            ResolveEventHandler handler = null;

            handler = (sender, args) => {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName)
                    return null;

                Debug.WriteLine("Redirecting assembly load of " + args.Name
                              + ",\tloaded by " + (args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName));

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;

                return Assembly.Load(requestedAssembly);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }
      //  private static bool done = false;

        public static Reddit Reddit;
        public static IWebAgent WebAgent;
        public static AuthenticatedUser AuthenticatedUser;

        public static IDictionary<string, Thing> Cache;


        public static void Start()
        {

            //  if (!done)
            //        RedirectAssembly("Newtonsoft.Json",new Version(9,0,0), "30ad4fe6b2a6aeed");
            Cache = new Dictionary<string, Thing>();
        }

        public static T GetCacheItem<T>(string name) where T : Thing
        {
            return Cache[name] as T;
        }

        public static void AddCacheItem(string key, Thing value)
        {
            Cache.Remove(key);
            Cache.Add(key, value);
        }
    }
}
