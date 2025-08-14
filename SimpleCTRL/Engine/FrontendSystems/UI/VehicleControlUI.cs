using System.Linq;

namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    public class VehicleControlUI
    {
        #region Fields & Properties
        private Canvas uiCanvas;
        private RectangleWidget backgroundPanel;
        private SelectableButton engineToggleButton;
        private readonly List<SelectableButton> controlButtons = new();

        private List<VehicleButtonSlot> topRowSlots = new();
        private List<VehicleButtonSlot> bottomRowSlots = new();

        private List<string> topSlotOrder = new();
        private List<string> bottomSlotOrder = new();

        public int VehicleDoorCount { get; private set; } = 0;

        public bool IsInteractive
        {
            get => uiCanvas.IsInteractive;
            set => uiCanvas.IsInteractive = value;
        }

        private const int ButtonWidth = 72;
        private const int ButtonHeight = 54;
        private const int SpacingX = 15;
        private const int RowSpacingY = 24;
        private const int EngineSpacing = 10;
        private const int ScreenWidth = 1920;
        private const int StartY = 920;
        #endregion

        #region Initialization
        public void Initialize()
        {
            uiCanvas = new Canvas();
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "SimpleCTRL");
            uiCanvas.Load(Path.Combine(basePath, "textures"), Path.Combine(basePath, "canvas.xml"));

            CreateEngineToggleButton();
            RefreshSlots();
        }

        private void CreateEngineToggleButton()
        {
            engineToggleButton = new SelectableButton(
                "engineToggle",
                uiCanvas,
                new Point(0, 0),
                new Size(ButtonWidth, ButtonHeight),
                "panel/empty.png"
            );
        }
        #endregion

        #region Dynamic & Ordered Slots
        public void SetSlotOrder(List<string> topOrder, List<string> bottomOrder)
        {
            topSlotOrder = topOrder;
            bottomSlotOrder = bottomOrder;
            RefreshSlots();
        }

        public void UpdateDynamicSlots(int vehicleDoorCount)
        {
            if (VehicleDoorCount == vehicleDoorCount) return;
            VehicleDoorCount = vehicleDoorCount;
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            // Remove old dynamic slots
            topRowSlots.RemoveAll(s => s.IsDynamic);
            bottomRowSlots.RemoveAll(s => s.IsDynamic);

            // Add dynamic slots up to VehicleDoorCount
            for (int i = 1; i <= VehicleDoorCount; i++)
            {
                AddDynamicSlotIfOrdered($"door_{i}", topSlotOrder, topRowSlots);
                AddDynamicSlotIfOrdered($"window_{i}", topSlotOrder, topRowSlots);
                AddDynamicSlotIfOrdered($"seat_{i}", bottomSlotOrder, bottomRowSlots);
            }

            // Rebuild top/bottom rows in order while filtering by VehicleDoorCount
            topRowSlots = topSlotOrder
                .Select(id => topRowSlots.FirstOrDefault(s => s.Id == id) ?? new VehicleButtonSlot(id))
                .Where(s => IsSlotAllowed(s.Id))
                .ToList();

            bottomRowSlots = bottomSlotOrder
                .Select(id => bottomRowSlots.FirstOrDefault(s => s.Id == id) ?? new VehicleButtonSlot(id))
                .Where(s => IsSlotAllowed(s.Id))
                .ToList();

            // Recreate buttons
            CreateControlButtons();
            PositionControlButtons();
            UpdateBackgroundPanel();
        }

        // Returns true if a slot should be shown for the current vehicle
        private bool IsSlotAllowed(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            if (id.StartsWith("door_") || id.StartsWith("window_") || id.StartsWith("seat_"))
                return GetSlotIndex(id) <= VehicleDoorCount;

            return true; // always show other types (hazards, lights, hood, etc.)
        }

        // Parses "door_1" or "window_2" -> 1, 2, etc.
        private int GetSlotIndex(string id)
        {
            var parts = id.Split('_');
            if (parts.Length < 2) return 0;
            return int.TryParse(parts[1], out int index) ? index : 0;
        }

        private void AddDynamicSlotIfOrdered(string id, List<string> orderList, List<VehicleButtonSlot> slotList)
        {
            if (orderList.Contains(id) && !slotList.Any(s => s.Id == id))
                slotList.Add(new VehicleButtonSlot(id, true));
        }

        private void CreateControlButtons()
        {
            controlButtons.Clear();

            void AddRow(List<VehicleButtonSlot> slots)
            {
                foreach (var slot in slots.Where(s => s.Icon != "empty"))
                {
                    var btn = new SelectableButton(
                        slot.Id,
                        uiCanvas,
                        new Point(0, 0),
                        new Size(ButtonWidth, ButtonHeight),
                        $"panel/{slot.Icon}.png"
                    );

                    btn.AddObserver(new VehicleControlObserver());
                    controlButtons.Add(btn);
                }
            }

            AddRow(topRowSlots);
            AddRow(bottomRowSlots);
        }
        #endregion

        #region Layout
        private void PositionControlButtons()
        {
            int topCount = topRowSlots.Count(s => s.Icon != "empty");
            int bottomCount = bottomRowSlots.Count(s => s.Icon != "empty");

            int topWidth = topCount * (ButtonWidth + SpacingX) - SpacingX;
            int bottomWidth = bottomCount * (ButtonWidth + SpacingX) - SpacingX;

            int panelLeft = (ScreenWidth - (ButtonWidth + EngineSpacing + Math.Max(topWidth, bottomWidth))) / 2
                            + ButtonWidth + EngineSpacing;

            int index = 0;
            foreach (var slot in topRowSlots.Where(s => s.Icon != "empty"))
            {
                int x = panelLeft + index * (ButtonWidth + SpacingX);
                int y = StartY;
                controlButtons[index].SetPosition(new Point(x, y));
                index++;
            }

            int bottomIndex = 0;
            foreach (var slot in bottomRowSlots.Where(s => s.Icon != "empty"))
            {
                int x = panelLeft + bottomIndex * (ButtonWidth + SpacingX);
                int y = StartY + ButtonHeight + RowSpacingY;
                controlButtons[index + bottomIndex].SetPosition(new Point(x, y));
                bottomIndex++;
            }
        }

        private void UpdateBackgroundPanel()
        {
            int gridWidth = Math.Max(topRowSlots.Count(s => s.Icon != "empty"),
                                     bottomRowSlots.Count(s => s.Icon != "empty"))
                                     * (ButtonWidth + SpacingX) - SpacingX;
            int gridHeight = ButtonHeight * 2 + RowSpacingY;

            int panelWidth = ButtonWidth + EngineSpacing + gridWidth;
            int panelHeight = gridHeight;

            int panelLeft = (ScreenWidth - panelWidth) / 2;
            int panelTop = StartY;

            if (backgroundPanel == null)
            {
                backgroundPanel = new RectangleWidget(panelWidth + 20, panelHeight + 20)
                {
                    Parent = uiCanvas,
                    BackgroundColor = Color.FromArgb(128, 0, 0, 0)
                };
            }
            else
            {
                backgroundPanel.Resize(new Size(panelWidth + 20, panelHeight + 20));
            }

            backgroundPanel.MoveTo(new Point(panelLeft - 10, panelTop - 10));
            UpdateEngineTogglePosition();
        }

        private void UpdateEngineTogglePosition()
        {
            int gridWidth = Math.Max(topRowSlots.Count(s => s.Icon != "empty"),
                                     bottomRowSlots.Count(s => s.Icon != "empty"))
                                     * (ButtonWidth + SpacingX) - SpacingX;
            int gridHeight = ButtonHeight * 2 + RowSpacingY;

            int panelWidth = ButtonWidth + EngineSpacing + gridWidth;
            int panelHeight = gridHeight;

            int panelLeft = (ScreenWidth - panelWidth) / 2;
            int panelTop = StartY;

            engineToggleButton.SetPosition(new Point(panelLeft, panelTop + (panelHeight - ButtonHeight) / 2));
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

    public class VehicleButtonSlot
    {
        public string Id { get; set; }
        public string Icon { get; set; }
        public bool IsDynamic { get; set; }

        public VehicleButtonSlot(string id, bool isDynamic = false)
        {
            Id = id;
            Icon = id;
            IsDynamic = isDynamic;

            if (id.StartsWith("seat_"))
                Icon = "seat";
        }
    }
}
