using LingoEngine.Core;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace LingoEngine.Tools
{
    public class CsvImporter
    {


        public record CsvRow
        {
            public int Number { get; set; }
            public LingoMemberType Type { get; set; }
            public string Name { get; set; }
            public LingoPoint RegPoint { get; set; }
            public string FileName { get; set; }

            public CsvRow(int number, LingoMemberType type, string name, LingoPoint registration, string fileName)
            {
                Number = number;
                Type = type;
                Name = name;
                this.RegPoint = registration;
                FileName = fileName;
            }
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public void ImportInCastFromCsvFile(ILingoCast cast, string filePath, bool skipFirstLine = true)
        {
            var rootFolder = Path.GetDirectoryName(Path.GetRelativePath(Environment.CurrentDirectory, filePath))??"";
            var csv = ImportCsvCastFile(filePath, skipFirstLine);
            foreach (var row in csv)
            {
                var fn = row.FileName;
                if (string.IsNullOrWhiteSpace(row.FileName)) {
                    var ext = ".png";
                    switch (row.Type)
                    {
                        case LingoMemberType.Text:
                            ext = ".txt";
                            break; 
                        case LingoMemberType.Field:
                            ext = ".txt";
                            break;
                        case LingoMemberType.Sound:
                            ext = ".wav";
                            break;
                    }
                    var name = row.Name.Length <1?"":"_"+ row.Name;
                    fn = row.Number + name + ext;
                }
                var fileName = Path.Combine(rootFolder, fn);
                var newMember = cast.Add(row.Type,row.Number, row.Name, fileName, row.RegPoint);
                if (newMember is  ILingoMemberTextBaseInteral textbased)
                    textbased.LoadFile();
            }
        }
        public IReadOnlyCollection<CsvRow> ImportCsvCastFile(string filePath, bool skipFirstLine = true)
        {
            var returnData = new List<CsvRow>();
            var csv = Import(filePath, skipFirstLine);
            foreach (var row in csv)
            {
                var number = Convert.ToInt32(row[0]);
                var type = row[1];
                var name = row[2];
                var registration = row[3].TrimStart('(').TrimEnd(')').Split(',').Select(int.Parse).ToArray();
                var fileName = row[4];
                var type1 = LingoMemberType.Unknown;
                Enum.TryParse(type,true, out type1);
                returnData.Add(new CsvRow(number, type1, name, (registration[0], registration[1]), fileName));
            }
            return returnData;
        }

        public List<string[]> Import(string filePath, bool skipFirstLine = true)
        {
            var rows = new List<string[]>();
            var hasSkipped = false;
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (skipFirstLine && !hasSkipped)
                {
                    hasSkipped = true;
                    continue;
                }

                rows.Add(ParseCsvLine(line));
            }

            return rows;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++; // Skip next quote
                        }
                        else
                            inQuotes = false;
                    }
                    else
                        sb.Append(c);
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                        inQuotes = true;
                    else
                        sb.Append(c);
                }
            }

            result.Add(sb.ToString()); // Add last field
            return result.ToArray();
        }
    }

}
