using MySqlConnector;
using System.Xml.Serialization;
using static DataImporterTool.Form1;

namespace DSO.DataImport.Processors
{
    internal static class MonsterBaseInfoImportProcessor
    {
        internal static void Import(string cs, string[] files)
        {
            var newDataList = new MonsterBaseInfoList();

            foreach (var file in files)
            {
                var xmlDocument = File.ReadAllText(file);

                var serializer = new XmlSerializer(typeof(MonsterBaseInfoList));

                using (TextReader reader = new StringReader(xmlDocument))
                {
                    var newList = (MonsterBaseInfoList)serializer.Deserialize(reader);

                    newDataList.MonsterBaseList.AddRange(newList.MonsterBaseList);
                }
            }

            var mainQuery = $@" INSERT INTO Asset_MonsterBaseInfo
                                    (`Type`, Model, Name, `Level`, ViewRange, HuntRange, ReactionType,
                                    `Attribute`, `Element`, Family1, Family2, Family3, ASValue, ARValue,
                                    ATValue, BLValue, CTValue, DEValue, DSValue, EVValue, HPValue, HTValue,
                                    MSValue, WSValue, Class)
                                SELECT @Type, @Model, @Name, @Level, @ViewRange, @HuntRange, @ReactionType,
                                       @Attribute, @Element, @Family1, @Family2, @Family3, @ASValue, @ARValue,
                                       @ATValue, @BLValue, @CTValue, @DEValue, @DSValue, @EVValue, @HPValue, @HTValue,
                                       @MSValue, @WSValue, @Class
                                FROM dual
                                WHERE NOT EXISTS (SELECT 1 FROM Asset_DigimonBaseInfo WHERE Type = @Type);";

            var subQuery = $@"  UPDATE Asset_MonsterBaseInfo mbi
                                INNER JOIN Asset_DigimonBaseInfo dbi
                                    ON dbi.Type = mbi.Type OR dbi.Type = mbi.Model
                                SET mbi.Attribute = dbi.Attribute,
                                    mbi.Element = dbi.Element,
                                    mbi.Family1 = dbi.Family1,
                                    mbi.Family2 = dbi.Family2,
                                    mbi.Family3 = dbi.Family3;";

            var conn = new MySqlConnection(cs);

            conn.Open();
            foreach (var digimonBase in newDataList.MonsterBaseList)
            {
                var command = new MySqlCommand(mainQuery, conn);
                command.Parameters.AddWithValue("Type", digimonBase.Type);
                command.Parameters.AddWithValue("Model", digimonBase.Model);
                command.Parameters.AddWithValue("Name", digimonBase.Name);
                command.Parameters.AddWithValue("Level", digimonBase.Level);
                command.Parameters.AddWithValue("ViewRange", digimonBase.ViewRange);
                command.Parameters.AddWithValue("HuntRange", digimonBase.HuntRange);
                command.Parameters.AddWithValue("ReactionType", digimonBase.ReactionType);
                command.Parameters.AddWithValue("Attribute", digimonBase.Attribute);
                command.Parameters.AddWithValue("Element", digimonBase.Element);
                command.Parameters.AddWithValue("Family1", digimonBase.Family1);
                command.Parameters.AddWithValue("Family2", digimonBase.Family2);
                command.Parameters.AddWithValue("Family3", digimonBase.Family3);
                command.Parameters.AddWithValue("ASValue", digimonBase.ASValue);
                command.Parameters.AddWithValue("ARValue", digimonBase.ARValue);
                command.Parameters.AddWithValue("ATValue", digimonBase.ATValue);
                command.Parameters.AddWithValue("BLValue", digimonBase.BLValue);
                command.Parameters.AddWithValue("CTValue", digimonBase.CTValue);
                command.Parameters.AddWithValue("DEValue", digimonBase.DEValue);
                command.Parameters.AddWithValue("DSValue", digimonBase.DSValue);
                command.Parameters.AddWithValue("EVValue", digimonBase.EVValue);
                command.Parameters.AddWithValue("HPValue", digimonBase.HPValue);
                command.Parameters.AddWithValue("HTValue", digimonBase.HTValue);
                command.Parameters.AddWithValue("MSValue", digimonBase.MSValue);
                command.Parameters.AddWithValue("WSValue", digimonBase.WSValue);
                command.Parameters.AddWithValue("Class", digimonBase.Class);
                command.ExecuteNonQuery();
            }
            conn.Close();

            var subConn = new MySqlConnection(cs);
            subConn.Open();
            var subCommand = new MySqlCommand(subQuery, subConn);
            subCommand.ExecuteNonQuery();
            subConn.Close();
        }
    }
}