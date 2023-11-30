using Rage;
using SimpleCTRL.UI;
using SimpleCTRL.Utils;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SimpleCTRL.Handlers
{
    class TextureHandler
    {
        internal static int fileCount = 0;

        public static void GetCustomSprites()
        {
            string path = @"Plugins\SimpleCTRL\interface";
            CustomUI customUI = new CustomUI();
            customUI.Width = FobHandler.sizeX;
            customUI.Height = FobHandler.sizeY;
            if (File.Exists(path + @"\custom.xml"))
            {
                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(CustomUI));
                    StreamReader streamReader = new StreamReader(path + @"\custom.xml");
                    customUI = (CustomUI)mySerializer.Deserialize(streamReader);
                    streamReader.Close();
                    customUI.Height = customUI.SHeight.ToInt32();
                    customUI.Width = customUI.SWidth.ToInt32();
                }
                catch (Exception ex)
                {
                    Logging.Error($"An exception occurred: {ex.Message}", "TextureHandler");
                }
            }
            if (Directory.Exists(ConfigHandler.TexturePath))
            {
                foreach (string file in Directory.EnumerateFiles(ConfigHandler.TexturePath, "*.png"))
                {
                    switch (Path.GetFileNameWithoutExtension(file))
                    {
                        case "background":
                            FobHandler.Background = new Sprite(Game.CreateTextureFromFile(file), new System.Drawing.Point(1920 - customUI.Width, 1080 - customUI.Height), new System.Drawing.Size(customUI.Width, customUI.Height));
                            break;
                    }

                    fileCount++;
                }

                Logging.Info($"loaded {fileCount} textures", "TextureHandler");
            }
        }
    }
}
