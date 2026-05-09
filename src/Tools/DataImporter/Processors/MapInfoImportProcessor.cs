using MySqlConnector;
using System.Xml.Serialization;
using static DataImporterTool.Form1;

namespace DSO.DataImport.Processors
{
    internal static class MapInfoImportProcessor
    {
        internal static void Import(string cs, string[] files)
        {
            var newDataList = new MapInfoList();

            foreach (var file in files)
            {
                var xmlDocument = File.ReadAllText(file);

                var serializer = new XmlSerializer(typeof(MapInfoList));

                using (TextReader reader = new StringReader(xmlDocument))
                {
                    var newList = (MapInfoList)serializer.Deserialize(reader);

                    newDataList.MapList.AddRange(newList.MapList);
                }
            }

            var mainQuery = $@" INSERT INTO Asset_Map (MapId, Name, RegionIndex) SELECT @MapId, @Name, @RegionIndex FROM dual WHERE NOT EXISTS (SELECT 1 FROM Asset_Map WHERE MapId = @MapId);";

            var conn = new MySqlConnection(cs);
            conn.Open();
            foreach (var map in newDataList.MapList)
            {
                
                var command = new MySqlCommand(mainQuery, conn);
                command.Parameters.AddWithValue("MapId", map.MapId);
                command.Parameters.AddWithValue("Name", map.Name);
                command.Parameters.AddWithValue("RegionIndex", map.Region);

                command.ExecuteNonQuery();
            }
            conn.Close();
        }
    }
}