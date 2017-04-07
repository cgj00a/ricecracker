using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ricecracker
{
    public class RiceCracker
    {
        private Dictionary<string, string> Config = new Dictionary<string, string>();
        private Dictionary<string, string> RegexDictionary = new Dictionary<string, string>();

        public RiceCracker(string[] args)
        {
            ParseArguments(args);

            var rootdir = Config["dir"];
            var extensions = Config["ext"].Split('|');
            var inputRegexFile = Config["i"];

            if (File.Exists(inputRegexFile))
            {
                var splitter = "";
                foreach (var line in File.ReadAllLines(inputRegexFile))
                {
                    if (line.Length == 0 || line[0] == '#')
                        continue;
                    else if (line.Substring(0, "$splitter=".Length) == "$splitter=")
                    {
                        splitter = line.Substring("$splitter=".Length, line.Length - "$splitter=".Length);
                        continue;
                    }
                    var reEnd = line.IndexOf(splitter);
                    if (reEnd == -1)
                        continue;
                    var re = line.Substring(0, reEnd - 1);
                    var replaceStart = reEnd + splitter.Length;
                    var replace = line.Substring(replaceStart + 1, line.Length - replaceStart - 1);

                    RegexDictionary.Add(re, replace);
                }
            }

            if (Directory.Exists(rootdir))
            {
                var dir = rootdir;
                foreach (var ext in extensions)
                {
                    var ext2 = ext.Substring(0, 2) == "*." ? ext : "*." + ext;
                    foreach (var file in Directory.GetFiles(rootdir, ext2, SearchOption.AllDirectories))
                    {
                        bool changed = false;
                        var fileContents = File.ReadAllLines(file);
                        for (var i = 0; i < fileContents.Length; ++i)
                        {
                            changed = true;
                            var line = fileContents[i];
                            // auto replace tabs with 4 spaces
                            // todo: handle this properly...
                            fileContents[i] = fileContents[i].Replace("	", "    ");

                            foreach (var regex in RegexDictionary)
                            {
                                if (Regex.IsMatch(fileContents[i], regex.Key))
                                {
                                    fileContents[i] = Regex.Replace(fileContents[i], regex.Key, regex.Value, RegexOptions.CultureInvariant);
                                }
                            }
                        }
                        if (changed)
                        {
                            File.WriteAllLines(file, fileContents);
                        }
                        Debugger.Break();
                    }
                }
            }
        }

        private void ParseArguments(string[] args)
        {
            for(var i = 0; i + 1 < args.Length; i += 2)
            {
                var arg = args[i].ToLower().TrimStart('-');
                var val = args[i + 1];

                if (arg == "dir")
                {
                    Config[arg] = Path.GetFullPath(val);
                }
                else if (arg == "i")
                {
                    Config[arg] = Path.GetFullPath(val);
                }
                else if (arg == "ext")
                {
                    Config[arg] = val;
                }
            }
        }
    }
}
