using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BatchVerify
{
    public static class Serializer
    {
        private static string fileName = "PreviousResults.xml";

        public static string ScanDataFilepath
        {
            get
            {
                return Directory.GetCurrentDirectory() + @"\" + fileName;
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

        public static List<App> DeSerializeAppList()
        {
            List<App> appList;

            var mySerializer = new XmlSerializer(typeof(List<App>));

            using (var fs = new FileStream(ScanDataFilepath, FileMode.Open))
            {
                appList = (List<App>)mySerializer.Deserialize(fs);
            }

            return appList;
        }
    }
}