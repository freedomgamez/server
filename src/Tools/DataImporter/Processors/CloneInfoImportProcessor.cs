using MySqlConnector;
using System.Xml.Serialization;
using static DataImporterTool.Form1;

namespace DSO.DataImport.Processors
{
    internal static class CloneInfoImportProcessor
    {
        internal static void Import(string cs, string[] files)
        {
            var newDataList = new CloneInfoList();

            foreach (var file in files)
            {
                var xmlDocument = File.ReadAllText(file);

                var serializer = new XmlSerializer(typeof(CloneInfoList));

                using (TextReader reader = new StringReader(xmlDocument))
                {
                    var newList = (CloneInfoList)serializer.Deserialize(reader);

                    newDataList.CloneList.AddRange(newList.CloneList);
                }
            }

            var mainQuery = $@" INSERT INTO Asset_Clone (ItemSection, MinLevel, MaxLevel, Bits) SELECT @ItemSection, @MinLevel, @MaxLevel, @Bits FROM dual WHERE NOT EXISTS (SELECT 1 FROM Asset_Clone WHERE ItemSection = @ItemSection and MinLevel = @MinLevel);";

            var conn = new MySqlConnection(cs);
            conn.Open();
            foreach (var clone in newDataList.CloneList)
            {
                
                var command = new MySqlCommand(mainQuery, conn);
                command.Parameters.AddWithValue("ItemSection", clone.ItemSection);
                command.Parameters.AddWithValue("MinLevel", clone.MinLevel);
                command.Parameters.AddWithValue("MaxLevel", clone.MaxLevel);
                command.Parameters.AddWithValue("Bits", clone.Bits);

                command.ExecuteNonQuery();
            }
            conn.Close();
        }
    }
}