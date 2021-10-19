using System;
using System.IO;

namespace OpenScratch {

    public static class Utils {

        public const string SpritesFolderName = "sprites";
        public const string OpenScratchProjectJsonName = "os-project.json";
        public const string OpenScratchMonitorsJsonName = "os-monitors.json";
        public const string SpriteCostumesFolderName = "costumes";
        public const string SpriteSoundsFolderName = "sounds";
        public const string MetaAgent = "OpenScratch/1.0";

        public static readonly Random Random = new Random();

        public static string CreateTemporaryDirectory() {
            string tempExtractDirectory = Path.Combine(Path.GetTempPath(), "openscratchtemp" + Random.Next());
            Directory.CreateDirectory(tempExtractDirectory);
            return tempExtractDirectory;
        }

    }
}