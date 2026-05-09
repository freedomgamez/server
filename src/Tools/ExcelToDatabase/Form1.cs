using MySqlConnector;

namespace ExcelToDatabase
{
    // Form1 was originally a WinForms class. The UI has been stripped so this tool
    // can run as a console app on Linux; the class is preserved as a namespace
    // anchor for the nested DTOs in Object.cs.
    public partial class Form1
    {
        internal static void ExtractSkillCodeInfo(string cs, string csvFile)
        {
            var insertSkillCode = @"INSERT INTO Asset_SkillCode (`SkillCode`, `Comment`)
                                    SELECT @SkillCode, @Comment FROM dual
                                    WHERE NOT EXISTS (SELECT 1 FROM Asset_SkillCode WHERE SkillCode = @SkillCode);
                                    SELECT LAST_INSERT_ID();";

            var insertSkillCodeApply = @"INSERT INTO Asset_SkillCodeApply
                                            (`Type`, `Attribute`, `Value`, `AdditionalValue`, `SkillCodeAssetId`)
                                         VALUES
                                            (@Type, @Attribute, @Value, @AdditionalValue, @SkillCodeAssetId);";

            using var conn = new MySqlConnection(cs);
            conn.Open();

            var skip = 2;
            using var reader = new StreamReader(csvFile);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line?.Split('\t');
                if (values == null) continue;
                if (skip > 0) { skip--; continue; }

                var skillCode = new SkillCodeDTO
                {
                    SkillCode = Convert.ToInt64(values[1]),
                    Comment = Convert.ToString(values[5]) ?? string.Empty,
                    SkillApply1 = Convert.ToInt32(values[6]),
                    SkillApply1Attribute = Convert.ToInt32(values[8]),
                    SkillApply1Value = Convert.ToInt32(values[10]),
                    SkillApply1ExtraValue = Convert.ToInt32(string.IsNullOrEmpty(values[11]) ? "0" : values[11]),
                    SkillApply2 = Convert.ToInt32(values[14]),
                    SkillApply2Attribute = Convert.ToInt32(values[16]),
                    SkillApply2Value = Convert.ToInt32(values[18]),
                    SkillApply2ExtraValue = Convert.ToInt32(values[19]),
                    SkillApply3 = Convert.ToInt32(values[22]),
                    SkillApply3Attribute = Convert.ToInt32(values[24]),
                    SkillApply3Value = Convert.ToInt32(values[26]),
                    SkillApply3ExtraValue = Convert.ToInt32(values[27]),
                };

                using var command = new MySqlCommand(insertSkillCode, conn);
                command.Parameters.AddWithValue("SkillCode", skillCode.SkillCode);
                command.Parameters.AddWithValue("Comment", skillCode.Comment);
                var skillCodeId = Convert.ToInt64(command.ExecuteScalar());
                if (skillCodeId <= 0) continue;

                InsertApply(conn, insertSkillCodeApply, skillCode.SkillApply1, skillCode.SkillApply1Attribute, skillCode.SkillApply1Value, skillCode.SkillApply1ExtraValue, skillCodeId);
                InsertApply(conn, insertSkillCodeApply, skillCode.SkillApply2, skillCode.SkillApply2Attribute, skillCode.SkillApply2Value, skillCode.SkillApply2ExtraValue, skillCodeId);
                InsertApply(conn, insertSkillCodeApply, skillCode.SkillApply3, skillCode.SkillApply3Attribute, skillCode.SkillApply3Value, skillCode.SkillApply3ExtraValue, skillCodeId);
            }
        }

        private static void InsertApply(MySqlConnection conn, string sql, int type, int attr, int value, int additional, long skillCodeId)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("Type", type);
            cmd.Parameters.AddWithValue("Attribute", attr);
            cmd.Parameters.AddWithValue("Value", value);
            cmd.Parameters.AddWithValue("AdditionalValue", additional);
            cmd.Parameters.AddWithValue("SkillCodeAssetId", skillCodeId);
            cmd.ExecuteNonQuery();
        }
    }
}
