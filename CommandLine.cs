using System.IO.Compression;
using System.IO;
using System;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenScratch {
    public static class CommandLine {

        public static int Main(string[] args) {
            return Parser.Default.ParseArguments<AssembleOptions, DisassembleOptions>(args)
            .MapResult((AssembleOptions opts) => Assemble(opts), (DisassembleOptions opts) => Disassemble(opts), errs => 1);
        }

        public static int Assemble(AssembleOptions options) {
            new OpenScratchProject(options.InputDir).Assemble(options.OutputSB3);
            return 0;
        }

        public static int Disassemble(DisassembleOptions options) {

            if (!File.Exists(options.InputSB3)) {
                Console.Error.WriteLine($"{options.InputSB3}: No such file.");
                return 1;
            }

            string tempExtractDirectory = Utils.CreateTemporaryDirectory();

            try {
                if (Directory.Exists(options.OutputDir) && Directory.GetFiles(options.OutputDir).Length != 0) {
                    Console.Error.WriteLine($"Output directory {options.OutputDir} not empty!");
                    return 1;
                }
                Directory.CreateDirectory(tempExtractDirectory);

                try {
                    ZipFile.ExtractToDirectory(options.InputSB3, tempExtractDirectory);
                } catch (Exception e) {
                    Console.Error.WriteLine($"Error while extracting SB3 {options.InputSB3}. Make sure this is a valid scratch project. {e}");
                    return 1;
                }

                string projectJsonLocation = Path.Combine(tempExtractDirectory, "project.json");

                if (!File.Exists(projectJsonLocation)) {
                    Console.Error.WriteLine($"Error while parsing SB3 {options.InputSB3}. Make sure this is a valid scratch project. No project.json found!");
                    return 1;
                }

                string osProjectJsonLocation = Path.Combine(options.OutputDir, Utils.OpenScratchProjectJsonName);

                try {
                    dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(projectJsonLocation));
                    dynamic osProjectJson = new JObject();

                    osProjectJson["meta-semver"] = projectJson["meta"]["semver"];
                    osProjectJson["meta-vm"] = projectJson["meta"]["vm"];
                    osProjectJson["extensions"] = projectJson["extensions"];
                    osProjectJson["format-version"] = "1.0";

                    JArray spriteNames = new JArray();
                    string spritesFolder = Path.Combine(options.OutputDir, Utils.SpritesFolderName);

                    foreach (dynamic sprite in projectJson["targets"]) {
                        string spriteName = sprite["name"];
                        spriteNames.Add(spriteName);
                        string spriteFolder = Path.Combine(spritesFolder, spriteName);
                        Directory.CreateDirectory(spriteFolder);

                        string spriteFile = Path.Combine(spriteFolder, spriteName + ".json");
                        File.WriteAllText(spriteFile, JsonConvert.SerializeObject(sprite, Formatting.Indented));

                        string costumesFolder = Path.Combine(spriteFolder, Utils.SpriteCostumesFolderName);
                        Directory.CreateDirectory(costumesFolder);
                        foreach (dynamic costume in sprite["costumes"]) {
                            File.Copy(Path.Combine(tempExtractDirectory, (string)costume["md5ext"]), Path.Combine(costumesFolder, ((string)costume["name"]) + "." + ((string)costume["dataFormat"])));
                        }

                        string soundsFolder = Path.Combine(spriteFolder, Utils.SpriteSoundsFolderName);
                        Directory.CreateDirectory(soundsFolder);
                        foreach (dynamic sound in sprite["sounds"]) {
                            File.Copy(Path.Combine(tempExtractDirectory, (string)sound["md5ext"]), Path.Combine(soundsFolder, ((string)sound["name"]) + "." + ((string)sound["dataFormat"])));
                        }
                    }

                    string monitorsFile = Path.Combine(options.OutputDir, Utils.OpenScratchMonitorsJsonName);
                    File.WriteAllText(monitorsFile, JsonConvert.SerializeObject(projectJson["monitors"], Formatting.Indented));

                    osProjectJson["sprites"] = spriteNames;
                    File.WriteAllText(osProjectJsonLocation, JsonConvert.SerializeObject(osProjectJson, Formatting.Indented));
                } catch (Exception e) {
                    Console.Error.WriteLine($"Error while parsing SB3 project.json from {options.InputSB3}. Make sure this is a valid scratch project. {e}");
                    return 1;
                }

            } catch (Exception e) {
                Console.Error.WriteLine($"Unexpected error while parsing SB3 {options.InputSB3}. {e}");
                return 1;
            } finally {
                if (Directory.Exists(tempExtractDirectory)) {
                    foreach (string file in Directory.EnumerateFiles(tempExtractDirectory)) {
                        File.Delete(file);
                    }
                    Directory.Delete(tempExtractDirectory);
                }
            }
            return 0;
        }

        [Verb("assemble", HelpText = "Assembles a scratch project")]
        public class AssembleOptions {
            [Value(0, MetaName = "Project Path", HelpText = "", Required = true)]
            public string InputDir { get; set; }

            [Value(1, MetaName = "Output SB3", HelpText = "", Required = true)]
            public string OutputSB3 { get; set; }
        }

        [Verb("disassemble", HelpText = "Disassembles a scratch project.")]
        public class DisassembleOptions {

            [Value(0, MetaName = "Input SB3", HelpText = "", Required = true)]
            public string InputSB3 { get; set; }

            [Value(1, MetaName = "Output Project Path", Required = false, Default = "")]
            public string OutputDir { get; set; }

        }
    }
}
