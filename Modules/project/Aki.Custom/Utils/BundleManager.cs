﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Custom.Models;
using Newtonsoft.Json.Linq;

namespace Aki.Custom.Utils
{
    public static class BundleManager
    {
        public const string CachePath = "user/cache/bundles/";
        public static Dictionary<string, BundleInfo> Bundles { get; private set; }

        static BundleManager()
        {
            Bundles = new Dictionary<string, BundleInfo>();

            if (VFS.Exists(CachePath))
            {
                VFS.DeleteDirectory(CachePath);
            }
        }

        public static void GetBundles()
        {
            var json = RequestHandler.GetJson("/singleplayer/bundles");
            var jArray = JArray.Parse(json);

            foreach (var jObj in jArray)
            {
                var key = jObj["key"].ToString();
                var path = jObj["path"].ToString();
                var bundle = new BundleInfo(key, path, jObj["dependencyKeys"].ToObject<string[]>());

                if (path.Contains("http"))
                {
                    var filepath = CachePath + Regex.Split(path, "bundle/", RegexOptions.IgnoreCase)[1];
                    var data = RequestHandler.GetData(path, true);
                    VFS.WriteFile(filepath, data);
                    bundle.Path = filepath;
                }

                Bundles.Add(key, bundle);
            }

            VFS.WriteTextFile(CachePath + "bundles.json", Json.Serialize<Dictionary<string, BundleInfo>>(Bundles));
        }
    }
}
