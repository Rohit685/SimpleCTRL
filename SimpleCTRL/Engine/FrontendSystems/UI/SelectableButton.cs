namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    public class SelectableButton : IObservable
    {
        #region Fields & Properties
        public string Id { get; }
        public Point Position { get; private set; }
        public Size Size { get; }
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                UpdateVisualState();
            }
        }

        private readonly List<IObserver> observers = new();
        private readonly Canvas canvas;
        private readonly RectangleWidget backgroundTexture;
        private readonly TextureWidget iconTexture;
        private readonly RectangleWidget underlineBar;
        private bool isActive;
        #endregion

        #region Constructor
        public SelectableButton(string id, Canvas parentCanvas, Point position, Size size, string iconPath)
        {
            Id = id;
            canvas = parentCanvas;
            Position = position;
            Size = size;

            backgroundTexture = new RectangleWidget(size.Width, size.Height)
            {
                Parent = canvas,
                BackgroundColor = Color.FromArgb(33, 33, 33)
            };

            int iconW = (int)(size.Width * 0.8);
            int iconH = (int)(size.Height * 0.8);
            iconTexture = new TextureWidget(iconPath, iconW, iconH) { Parent = canvas };

            underlineBar = new RectangleWidget(size.Width, 4)
            {
                Parent = canvas,
                BackgroundColor = Color.FromArgb(127, 127, 127)
            };

            SetPosition(position);
        }
        #endregion

        #region Methods
        public void SetPosition(Point pos)
        {
            Position = pos;
            backgroundTexture.MoveTo(pos);

            int iconX = pos.X + (Size.Width - iconTexture.Width) / 2;
            int iconY = pos.Y + (Size.Height - iconTexture.Height) / 2 - 4;
            iconTexture.MoveTo(new Point(iconX, iconY));

            int underlineWidth = (int)(Size.Width * 0.25);
            underlineBar.Resize(new Size(underlineWidth, underlineBar.Height));
            underlineBar.MoveTo(new Point(pos.X + (Size.Width - underlineWidth) / 2, pos.Y + Size.Height - underlineBar.Height - 8));
        }

        public void Toggle()
        {
            IsActive = !IsActive;
            NotifyObservers();
        }

        private void UpdateVisualState()
        {
            underlineBar.BackgroundColor = IsActive
                ? Color.FromArgb(50, 205, 50)
                : Color.FromArgb(127, 127, 127);
        }

        public void Draw(Rage.Graphics graphics)
        {
            backgroundTexture.Draw(graphics);
            iconTexture.Draw(graphics);
            underlineBar.Draw(graphics);
        }

        public void AddObserver(IObserver observer) => observers.Add(observer);
        public void RemoveObserver(IObserver observer) => observers.Remove(observer);
        public void NotifyObservers() => observers.ForEach(o => o.OnUpdated(this));
        public Rectangle Bounds => new(Position, Size);
        #endregion
    }
}