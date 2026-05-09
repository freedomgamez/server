using DSO.DataImport.Processors;

namespace DSO.DataImport
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: DSO.DataImport <connection-string> [xmls-base-dir]");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  connection-string  MariaDB connection string");
                Console.Error.WriteLine("  xmls-base-dir      Directory containing the XMLs/ folder (default: cwd)");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Example:");
                Console.Error.WriteLine("  DSO.DataImport \"Server=localhost;Port=3306;Database=dso;Uid=dso;Pwd=...;\"");
                return 2;
            }

            var cs = args[0];
            var baseDir = Path.Combine(
                args.Length > 1 ? args[1] : Directory.GetCurrentDirectory(),
                "XMLs");

            if (!Directory.Exists(baseDir))
            {
                Console.Error.WriteLine($"XMLs directory not found: {baseDir}");
                return 2;
            }

            try
            {
                Run("EvoInfo", baseDir, EvoInfoImportProcessor.Import, cs);
                Run("DigimonBaseInfo", baseDir, DigimonBaseInfoImportProcessor.Import, cs);
                Run("ItemInfo", baseDir, ItemInfoImportProcessor.Import, cs);

                // Other importers are available in this project but disabled by default
                // (matching the original WinForms tool's commented-out flow). To enable,
                // add Run() calls here:
                //   - SealInfoImportProcessor.Import           "SealInfo"
                //   - MonsterBaseInfoImportProcessor.Import    "MonsterBaseInfo"
                //   - HatchInfoImportProcessor.Import          "HatchInfo"
                //   - CloneInfoImportProcessor.Import          "CloneInfo"
                //   - CloneValueImportProcessor.Import         "CloneValues"
                //   - MapInfoImportProcessor.Import            "Maps"
                //   - NpcInfoImportProcessor.Import            "NPCs"
                //   - FruitInfoImportProcessor.Import          "Fruits"
                //   - MonsterSkillInfoImportProcessor.Import   "MonsterSkill"
                // The remaining importers (Quest, SkillCode, Skill, DigimonSkill, TamerSkill,
                // Buff, MonthlyEvent, Achievement, Summon, Portal, NpcColiseum, EvolutionArmor,
                // ArenaDailyItemRewards, AccessoryStatus) live as private static methods inside
                // Form1.cs; bump them to internal and add Run() calls here to enable.

                Console.WriteLine("Done.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return 1;
            }
        }

        private static void Run(string subdir, string baseDir, Action<string, string[]> importer, string cs)
        {
            var dir = Path.Combine(baseDir, subdir);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"[skip] {subdir}: not found ({dir})");
                return;
            }
            var files = Directory.GetFiles(dir, "*.xml");
            Console.WriteLine($"[run]  {subdir}: {files.Length} file(s)");
            importer(cs, files);
            Console.WriteLine($"[ok]   {subdir}");
        }
    }
}
