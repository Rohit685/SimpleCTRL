namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    public class VehicleControlUI
    {
        #region Fields & Properties
        private Canvas uiCanvas;
        private RectangleWidget backgroundPanel;
        private SelectableButton engineToggleButton;
        public readonly List<SelectableButton> controlButtons = new();

        public bool IsInteractive
        {
            get => uiCanvas.IsInteractive;
            set => uiCanvas.IsInteractive = value;
        }
        #endregion

        #region Initialization
        public void Initialize()
        {
            uiCanvas = new Canvas();
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "SimpleCTRL");
            uiCanvas.Load(Path.Combine(basePath, "textures"), Path.Combine(basePath, "canvas.xml"));

            SetupUI(screenWidth: 1920);
        }

        private void SetupUI(int screenWidth)
        {
            const int buttonWidth = 72, buttonHeight = 54, spacingX = 15, rowSpacingY = 24;
            const int topRowCount = 8, bottomRowCount = 7, bottomRowSlots = 8;

            int topRowWidth = topRowCount * buttonWidth + (topRowCount - 1) * spacingX;
            int bottomRowWidth = bottomRowSlots * buttonWidth + (bottomRowSlots - 1) * spacingX;
            int gridWidth = Math.Max(topRowWidth, bottomRowWidth);

            int totalWidth = buttonWidth + spacingX + gridWidth;
            int startX = (screenWidth - totalWidth) / 2;
            int startY = 920;
            int totalHeight = buttonHeight * 2 + rowSpacingY;

            CreateEngineToggleButton(buttonWidth, buttonHeight, startX, startY, totalHeight);
            CreateControlButtons(topRowCount, bottomRowCount, buttonWidth, buttonHeight);
            AttachObservers();
            PositionControlButtons(topRowCount, bottomRowCount, buttonWidth, buttonHeight, spacingX, rowSpacingY, startX, startY);
            UpdateBackgroundPanel(startX, startY, buttonWidth, totalWidth, totalHeight);
        }
        #endregion

        #region UI Components
        private void CreateEngineToggleButton(int width, int height, int startX, int startY, int totalHeight)
        {
            engineToggleButton = new SelectableButton(
                id: "engineToggle",
                parentCanvas: uiCanvas,
                position: new Point(startX, startY + (totalHeight - height) / 2),
                size: new Size(width, height),
                iconPath: "panel/empty.png"
            );
        }

        private void CreateControlButtons(int topCount, int bottomCount, int width, int height)
        {
            controlButtons.Clear();

            string[] topIcons = { "left-indicator", "hazards", "right-indicator", "empty", "hood", "door-front", "empty", "empty" };
            string[] bottomIcons = { "cruise-control", "headlight-low", "interior-light", "trunk", "door-front", "empty", "empty" };

            void AddButtons(int count, string[] icons)
            {
                for (int i = 0; i < count; i++)
                {
                    string iconName = i < icons.Length ? icons[i] : "empty";
                    controlButtons.Add(new SelectableButton(
                        id: iconName,
                        parentCanvas: uiCanvas,
                        position: new Point(100, 100),
                        size: new Size(width, height),
                        iconPath: $"panel/{iconName}.png"
                    ));
                }
            }

            AddButtons(topCount, topIcons);
            AddButtons(bottomCount, bottomIcons);
        }

        private void AttachObservers()
        {
            // TODO: Add observer logic
        }
        #endregion

        #region Layout
        private void PositionControlButtons(int topCount, int bottomCount, int width, int height, int spacingX, int rowSpacingY, int startX, int startY)
        {
            int baseX = startX + width + spacingX;

            for (int i = 0; i < topCount; i++)
                controlButtons[i].SetPosition(new Point(baseX + i * (width + spacingX), startY));

            for (int i = 0; i < bottomCount; i++)
            {
                int slotIndex = i < 3 ? i : i + 1;
                int x = baseX + slotIndex * (width + spacingX);
                int y = startY + height + rowSpacingY;
                controlButtons[topCount + i].SetPosition(new Point(x, y));
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
                    BackgroundColor = Color.FromArgb(128, 0, 0, 0)
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

        #region Input
        public void HandleClick()
        {
            var cursorPos = uiCanvas.Cursor.Position;

            if (engineToggleButton.Bounds.Contains(cursorPos))
                engineToggleButton.Toggle();

            foreach (var btn in controlButtons)
                if (btn.Bounds.Contains(cursorPos))
                    btn.Toggle();
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