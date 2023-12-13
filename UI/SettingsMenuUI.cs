using Rage;
using RAGENativeUI;
using System.Drawing;

namespace SimpleCTRL.UI
{
    internal static class SettingsMenuUI
    {
        #region Fields
        private static MenuPool MenuPool;
        private static UIMenu SettingsMenu;
        #endregion

        internal static void Initialize()
        {
            MenuPool = new MenuPool();

            SettingsMenu = new UIMenu("", "Main Menu");

            SettingsMenu.SetBannerType(new RAGENativeUI.Elements.Sprite("simplemenu", "SimpleCTRLBanner", Point.Empty, Size.Empty));

            MenuPool.Add(SettingsMenu);
        }

        internal static void ProcessMenus()
        {
            MenuPool.ProcessMenus();

            if (Game.IsKeyDown(System.Windows.Forms.Keys.F8))
            {
                SettingsMenu.Visible = true;
            }
        }
    }
}
