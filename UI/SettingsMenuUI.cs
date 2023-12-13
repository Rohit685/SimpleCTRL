using Rage;
using Rage.Attributes;
using RAGENativeUI;
using SimpleCTRL.Handlers;
using System.Drawing;

namespace SimpleCTRL.UI
{
    internal static class SettingsMenuUI
    {
        #region Fields
        private static MenuPool MenuPool;
        private static UIMenu mainMenu;
        #endregion

        internal static void Initialize()
        {
            MenuPool = new MenuPool();

            mainMenu = new UIMenu("", "MAIN MENU");

            if (ConfigHandler.DisableMenuMouse)
            {
                mainMenu.MouseControlsEnabled = false;
                mainMenu.AllowCameraMovement = true;
            }

            mainMenu.SetBannerType(new RAGENativeUI.Elements.Sprite("simplemenu", "SimpleCTRLBanner", Point.Empty, Size.Empty));

            MenuPool.Add(mainMenu);
        }

        internal static void ProcessMenus()
        {
            MenuPool.ProcessMenus();

        }

        [ConsoleCommand]
        private static void Command_SettingsMenu()
        { 
            mainMenu.Visible = true;
        }
    }
}
