using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using BatchVerify.Containers;
using System;

namespace BatchVerify.Data
{
    public static class Serializer
    {
        private static string GetFileName()
        {
            return string.Format("PreviousResults_{0}.xml", DateTime.Now.ToString("yyyy-MM-ddTHHmmss"));
        }

        public static string ScanDataFilepath
        {
            get
            {
                return Directory.GetCurrentDirectory() + @"\" + GetFileName();
            }
        }

        public static void SerializeAppList(List<App> apps)
        {
            var mySerializer = new XmlSerializer(typeof(List<App>));

            using (var sw = new StreamWriter(ScanDataFilepath))
            {
                mySerializer.Serialize(sw, apps);
                sw.Close();
            }
        }
                
        public static List<App> DeSerializeAppList(string filePath)
        {
            List<App> appList;

            var mySerializer = new XmlSerializer(typeof(List<App>));

            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                appList = (List<App>)mySerializer.Deserialize(fs);
            }

            return appList;
        }
    }
}