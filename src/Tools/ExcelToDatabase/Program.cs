using ExcelToDatabase;

namespace DSO.ExcelImporter
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: DSO.ExcelImporter <connection-string> <tsv-file>");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Reads a TSV (tab-separated) skill-code dump and imports rows into");
                Console.Error.WriteLine("Asset_SkillCode and Asset_SkillCodeApply, skipping the first two header rows.");
                return 2;
            }

            var cs = args[0];
            var csvFile = args[1];

            if (!File.Exists(csvFile))
            {
                Console.Error.WriteLine($"TSV file not found: {csvFile}");
                return 2;
            }

            try
            {
                Console.WriteLine($"Importing from {csvFile} ...");
                Form1.ExtractSkillCodeInfo(cs, csvFile);
                Console.WriteLine("Done.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return 1;
            }
        }
    }
}
