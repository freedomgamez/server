using MySqlConnector;
using System.Xml.Serialization;
using static DataImporterTool.Form1;

namespace DSO.DataImport.Processors
{
    internal static class EvoInfoImportProcessor
    {
        internal static void Import(string cs, string[] files)
        {
            var newEvoInfoList = new DigimonEvoList();

            foreach (var file in files)
            {
                var xmlDocument = File.ReadAllText(file);

                var serializer = new XmlSerializer(typeof(DigimonEvoList));

                using (TextReader reader = new StringReader(xmlDocument))
                {
                    var newList = (DigimonEvoList)serializer.Deserialize(reader);

                    newEvoInfoList.DigimonEvos.AddRange(newList.DigimonEvos);
                }
            }

            var evoQuery = $@"  INSERT INTO Asset_Evolution (`Type`, `EvolutionRank`) SELECT @Type, @EvolutionRank FROM dual WHERE NOT EXISTS (SELECT 1 FROM Asset_Evolution WHERE Type = @Type); SELECT LAST_INSERT_ID();";

            var lineQuery = $@" INSERT INTO Asset_EvolutionLine
                                    (EvolutionId, `Type`, UnlockItemSection, UnlockItemSectionAmount, SlotLevel,UnlockLevel, UnlockQuestId)
                                VALUES
                                    (@EvolutionId, @Type, @UnlockItemSection, @UnlockItemSectionAmount,@SlotLevel, @UnlockLevel, @UnlockQuestId);

                                SELECT LAST_INSERT_ID();";

            var stageQuery = $@"INSERT INTO Asset_EvolutionStage
                                    (`Type`
                                    ,`Value`
                                    ,`EvolutionLineId`)
                                VALUES
                                    (@Type
                                    ,@Value
                                    ,@EvolutionLineId)";

            var conn = new MySqlConnection(cs);

            conn.Open();

            foreach (var evo in newEvoInfoList.DigimonEvos)
            {
                var command = new MySqlCommand(evoQuery, conn);
                command.Parameters.AddWithValue("Type", evo.Type);
                command.Parameters.AddWithValue("EvolutionRank", evo.EvolutionRank);

                var evoId = Convert.ToInt64(command.ExecuteScalar());

                if (evoId == 0)
                    continue;

                foreach (var evoLine in evo.EvolutionLines)
                {
                    var lineCommand = new MySqlCommand(lineQuery, conn);
                    lineCommand.Parameters.AddWithValue("EvolutionId", evoId);
                    lineCommand.Parameters.AddWithValue("Type", evoLine.Type);
                    lineCommand.Parameters.AddWithValue("UnlockItemSection", evoLine.UnlockItemSection);
                    lineCommand.Parameters.AddWithValue("UnlockItemSectionAmount", evoLine.UnlockItemSectionAmount);
                    lineCommand.Parameters.AddWithValue("UnlockLevel", evoLine.UnlockLevel);
                    lineCommand.Parameters.AddWithValue("SlotLevel", evoLine.SlotLevel);
                    lineCommand.Parameters.AddWithValue("UnlockQuestId", evoLine.UnlockQuestId);

                    var lineId = Convert.ToInt64(lineCommand.ExecuteScalar());

                    foreach (var stage in evoLine.EvolutionStages)
                    {
                        var stageCommand = new MySqlCommand(stageQuery, conn);
                        stageCommand.Parameters.AddWithValue("Type", stage.Type);
                        stageCommand.Parameters.AddWithValue("Value", stage.Value);
                        stageCommand.Parameters.AddWithValue("EvolutionLineId", lineId);

                        stageCommand.ExecuteNonQuery();
                    }
                }
            }

            conn.Close();
        }
    }
}