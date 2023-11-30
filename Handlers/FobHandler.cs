using Rage;
using SimpleCTRL.UI;
using SimpleCTRL.Utils;

namespace SimpleCTRL.Handlers
{
    internal static class FobHandler
    {
        internal static int sizeX = 403;
        internal static int sizeY = 619;

        internal static bool IsUIOn = false;

        // Background (printed first)
        internal static Sprite Background = new Sprite(Properties.Resources.background, new System.Drawing.Point(1920 - sizeX, 1080 - sizeY), new System.Drawing.Size(sizeX, sizeY));

        internal static void Start()
        {
            sizeX = 403;
            sizeY = 619;
            TextureHandler.GetCustomSprites();
            Game.RawFrameRender += RawFrameRender;
        }

        static void RawFrameRender(object sender, GraphicsEventArgs e)
        {
            if (IsUIOn && UIHelper.IsUIAbleToDisplay)
            {
                Background.Draw(e.Graphics);
            }
        }
    }
}
