using RawCanvasUI;
using RawCanvasUI.Widgets;

namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    public class VehicleControlUI
    {
        #region Fields and Properties
        private Canvas uiCanvas;
        private RectangleWidget backgroundPanel;
        private ToggledButton engineToggleButton;
        private List<ToggledButton> controlButtons = new();

        public bool IsInteractive
        {
            get => uiCanvas.IsInteractive;
            set => uiCanvas.IsInteractive = value;
        }
        #endregion

        #region Initialization and Setup
        public void Initialize()
        {
            uiCanvas = new Canvas();

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "SimpleCTRL");
            uiCanvas.Load(Path.Combine(basePath, "textures"), Path.Combine(basePath, "canvas.xml"));

            const int screenWidth = 1920;
            SetupUI(screenWidth);
        }

        private void SetupUI(int screenWidth)
        {
            const int buttonWidth = 72;
            const int buttonHeight = 54;
            const int spacingX = 15;
            const int rowSpacingY = 24;

            const int topRowCount = 8;
            const int bottomRowCount = 7;
            const int bottomRowSlots = 8;

            int topRowWidth = topRowCount * buttonWidth + (topRowCount - 1) * spacingX;
            int bottomRowWidth = bottomRowSlots * buttonWidth + (bottomRowSlots - 1) * spacingX;
            int gridWidth = Math.Max(topRowWidth, bottomRowWidth);

            int totalWidth = buttonWidth + spacingX + gridWidth;
            int startX = (screenWidth - totalWidth) / 2;
            int startY = 920;
            int totalHeight = buttonHeight * 2 + rowSpacingY;

            CreateEngineToggleButton(buttonWidth, buttonHeight, startX, startY, totalHeight);
            CreateControlButtons(topRowCount, bottomRowCount, buttonWidth, buttonHeight);
            PositionControlButtons(topRowCount, bottomRowCount, buttonWidth, buttonHeight, spacingX, rowSpacingY, startX, startY);
            UpdateBackgroundPanel(startX, startY, buttonWidth, totalWidth, totalHeight);
        }
        #endregion

        #region UI Components Creation
        private void CreateEngineToggleButton(int width, int height, int startX, int startY, int totalHeight)
        {
            engineToggleButton = new ToggledButton(
                id: "engineToggle",
                width,
                height,
                activeTextureName: "panel/buttons/on/hazards.png",
                inactiveTextureName: "panel/buttons/off/hazards.png",
                text: ""
            )
            {
                Parent = uiCanvas
            };

            int x = startX;
            int y = startY + (totalHeight - height) / 2;
            engineToggleButton.MoveTo(new Point(x, y));
        }

        private void CreateControlButtons(int topCount, int bottomCount, int width, int height)
        {
            controlButtons.Clear();

            string[] activeTopIcons = {
                "panel/buttons/on/left_indicator.png",
                "panel/buttons/on/hazards.png",
                "panel/buttons/on/right_indicator.png"
            };
            string[] inactiveTopIcons = {
                "panel/buttons/off/left_indicator.png",
                "panel/buttons/off/hazards.png",
                "panel/buttons/off/right_indicator.png"
            };

            ToggledButton CreateButton(string id, string activeTex, string inactiveTex) =>
                new(id, width, height, activeTex, inactiveTex, "") { Parent = uiCanvas };

            // Top row buttons
            for (int i = 0; i < topCount; i++)
            {
                bool useDefaultIcon = i >= 3;
                string activeTex = useDefaultIcon ? "panel/buttons/on/hazards.png" : activeTopIcons[i];
                string inactiveTex = useDefaultIcon ? "panel/buttons/off/hazards.png" : inactiveTopIcons[i];
                controlButtons.Add(CreateButton($"buttonTop{i + 1}", activeTex, inactiveTex));
            }

            // Bottom row buttons
            for (int i = 0; i < bottomCount; i++)
            {
                controlButtons.Add(CreateButton($"buttonBottom{i + 1}",
                    "panel/buttons/on/hazards.png",
                    "panel/buttons/off/hazards.png"));
            }
        }
        #endregion

        #region UI Layout
        private void PositionControlButtons(int topCount, int bottomCount, int width, int height, int spacingX, int rowSpacingY, int startX, int startY)
        {
            int baseX = startX + width + spacingX;

            // Top row
            for (int i = 0; i < topCount; i++)
            {
                int x = baseX + i * (width + spacingX);
                int y = startY;
                controlButtons[i].MoveTo(new Point(x, y));
            }

            // Bottom row (skip one slot after 3rd button)
            for (int i = 0; i < bottomCount; i++)
            {
                int slotIndex = i < 3 ? i : i + 1;
                int x = baseX + slotIndex * (width + spacingX);
                int y = startY + height + rowSpacingY;
                controlButtons[topCount + i].MoveTo(new Point(x, y));
            }
        }

        private void UpdateBackgroundPanel(int startX, int startY, int buttonWidth, int totalWidth, int totalHeight)
        {
            const int padding = 10;

            int bgX = startX - padding;
            int bgY = startY - padding;
            int bgWidth = totalWidth + padding * 2;
            int bgHeight = totalHeight + padding * 2;

            if (backgroundPanel == null)
            {
                backgroundPanel = new RectangleWidget(bgWidth, bgHeight)
                {
                    Parent = uiCanvas,
                    BackgroundColor = Color.FromArgb(204, 42, 42, 44)
                };
            }
            else
            {
                backgroundPanel.Resize(new Size(bgWidth, bgHeight));
            }

            backgroundPanel.MoveTo(new Point(bgX, bgY));
            backgroundPanel.UpdateBounds();
        }
        #endregion

        #region Input Handling
        public void HandleClick()
        {
            var cursorPos = uiCanvas.Cursor.Position;

            if (engineToggleButton.Bounds.Contains(cursorPos))
                engineToggleButton.Click(uiCanvas.Cursor);

            foreach (var btn in controlButtons)
            {
                if (btn.Bounds.Contains(cursorPos))
                    btn.Click(uiCanvas.Cursor);
            }
        }
        #endregion

        #region Rendering
        public void Draw(Rage.Graphics graphics)
        {
            backgroundPanel?.Draw(graphics);
            engineToggleButton?.Draw(graphics);

            foreach (var btn in controlButtons)
                btn?.Draw(graphics);

            uiCanvas.Cursor.Draw(graphics);
        }
        #endregion
    }
}