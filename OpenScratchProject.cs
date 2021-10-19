using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace OpenScratch {

    public class OpenScratchProject {
        public readonly string ProjectPath;
        public string MetaSemver, MetaVm;

        public dynamic JsonExtensions;
        public dynamic JsonMonitors;

        public List<OpenScratchSprite> Sprites;
        public OpenScratchSprite Stage;

        public OpenScratchProject(string path) {
            ProjectPath = path;
            dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(path, Utils.OpenScratchProjectJsonName)));
            JsonMonitors = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(path, Utils.OpenScratchMonitorsJsonName)));
            JsonExtensions = projectJson["extensions"];
            MetaSemver = (string)projectJson["meta-semver"];
            MetaVm = (string)projectJson["meta-vm"];

            string spritesFolder = Path.Join(path, Utils.SpritesFolderName);
            Sprites = new List<OpenScratchSprite>();
            foreach (dynamic spriteNameDyn in projectJson["sprites"]) {
                string spriteName = (string)spriteNameDyn;
                string spriteFolder = Path.Join(spritesFolder, spriteName);
                OpenScratchSprite sprite = new OpenScratchSprite(JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(spriteFolder, spriteName + ".json"))));
                if (sprite.IsStage) Stage = sprite;
                Sprites.Add(sprite);
            }
        }

        public void Assemble(string output) {
            string tempDirectory = Utils.CreateTemporaryDirectory();

            try {
                dynamic projectJson = new JObject();
                dynamic metaJson = new JObject();

                metaJson["semver"] = MetaSemver;
                metaJson["vm"] = MetaVm;
                metaJson["agent"] = Utils.MetaAgent;

                projectJson["meta"] = metaJson;
                projectJson["extensions"] = JsonExtensions;
                projectJson["monitors"] = JsonMonitors;

                JArray targetsJson = new JArray();

                foreach (OpenScratchSprite sprite in Sprites) {
                    targetsJson.Add(sprite.Json);
                    foreach (OpenScratchAsset asset in sprite.Costumes) {
                        string assetPath = Path.Combine(ProjectPath, Utils.SpritesFolderName, sprite.Name, Utils.SpriteCostumesFolderName, asset.FileName);
                        string outPath = Path.Combine(tempDirectory, asset.Md5ext);
                        if (!File.Exists(outPath))
                            File.Copy(assetPath, outPath);
                    }

                    foreach (OpenScratchAsset asset in sprite.Sounds) {
                        string assetPath = Path.Combine(ProjectPath, Utils.SpritesFolderName, sprite.Name, Utils.SpriteSoundsFolderName, asset.FileName);
                        string outPath = Path.Combine(tempDirectory, asset.Md5ext);
                        if (!File.Exists(outPath))
                            File.Copy(assetPath, outPath);
                    }
                }
                projectJson["targets"] = targetsJson;

                File.WriteAllText(Path.Combine(tempDirectory, "project.json"), JsonConvert.SerializeObject(projectJson));

                ZipFile.CreateFromDirectory(tempDirectory, output);
            } finally {
                if (Directory.Exists(tempDirectory)) {
                    foreach (string f in Directory.EnumerateFiles(tempDirectory))
                        File.Delete(f);
                    Directory.Delete(tempDirectory);
                }
            }
        }
    }

    public class OpenScratchSprite {
        public List<OpenScratchAsset> Costumes, Sounds;
        public string Name;
        public dynamic Json;
        public bool IsStage;

        public OpenScratchSprite(dynamic json) {
            Json = json;
            Name = json["name"];
            IsStage = (bool)json["isStage"];
            Costumes = new List<OpenScratchAsset>();
            foreach (dynamic costume in json["costumes"])
                Costumes.Add(new OpenScratchAsset(costume));
            Sounds = new List<OpenScratchAsset>();
            foreach (dynamic sound in json["sounds"])
                Sounds.Add(new OpenScratchAsset(sound));
        }
    }

    public class OpenScratchAsset {
        public dynamic Json;
        public string Md5ext, FileName;

        public OpenScratchAsset(dynamic json) {
            Json = json;
            Md5ext = json["md5ext"];
            FileName = json["name"] + "." + json["dataFormat"];
        }
    }
}