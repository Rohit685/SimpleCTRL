using Rage;
using RAGENativeUI;

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

            SettingsMenu = new UIMenu("SimpleCTRL", "SimpleCTRL");

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
