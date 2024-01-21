using Common.Native;
using RAGENativeUI.TinyTween;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Drawing;
using SimpleCTRL.Handlers;
using SimpleCTRL.Utils;
using Rage.Native;

namespace SimpleCTRL.UI
{
	public class CustomUI
	{
		#region Fields
		public static Scaleform buttons = new Scaleform();

		public static float fuelBarWidth = GetBarWidth();

		public static float fuelBarHeight = 6f;

		public static PointF basePosition = new PointF(0f, 584f);

		public static PointF fuelBarBackdropPosition = basePosition;

		public static PointF fuelBarBackPosition = new PointF(fuelBarBackdropPosition.X, fuelBarBackdropPosition.Y + 3f);

		public static PointF fuelBarPosition = fuelBarBackPosition;

		public static SizeF fuelBarBackdropSize = new SizeF(fuelBarWidth, 12f);

		public static SizeF fuelBarBackSize = new SizeF(fuelBarWidth, fuelBarHeight);

		public static SizeF fuelBarSize = fuelBarBackSize;

		public static Color fuelBarBackdropColour = Color.FromArgb(100, 0, 0, 0);

		public static Color fuelBarBackColour = Color.FromArgb(50, 255, 179, 0);

		public static Color fuelBarColourNormal = Color.FromArgb(150, 255, 179, 0);

		public static Color fuelBarColourWarning = Color.FromArgb(255, 255, 245, 220);

		public static Color fuelBarElectricColourNormal = Color.FromArgb(255, 12, 110, 201);

		public static Color fuelBarElectricColourWarning = Color.FromArgb(255, 187, 231, 237);

		public static Tween<float> fuelBarColorTween = new FloatTween();

		public static bool fuelBarAnimationDir = true;

		public static Common.Elements.Rectangle fuelBarBackdrop = new Common.Elements.Rectangle(fuelBarBackdropPosition, fuelBarBackdropSize, fuelBarBackdropColour);

		public static Common.Elements.Rectangle fuelBarBack = new Common.Elements.Rectangle(fuelBarBackPosition, fuelBarBackSize, fuelBarBackColour);

		public static Common.Elements.Rectangle fuelBar = new Common.Elements.Rectangle(fuelBarPosition, fuelBarSize, fuelBarColourNormal);

		public static PointF Position
		{
			set
			{
				fuelBarBackdrop.Position = value;
				fuelBarBack.Position = new PointF(value.X, value.Y + 3f);
				fuelBar.Position = fuelBarBack.Position;
			}
		}

		private static string EngineKeyFormat { get; set; } = Extensions.FormatKeyBinding(ConfigHandler.EngineToggleKey);
		private static string EngineButtonFormat { get; set; } = Extensions.FormatKeyBinding(ConfigHandler.EngineControllerButton);
		private static string RefuelKeyFormat { get; set; } = Extensions.FormatKeyBinding(ConfigHandler.RefuelKey);
		private static string RefuelButtonFormat { get; set; } = Extensions.FormatKeyBinding(ConfigHandler.RefuelControllerButton);

		private static bool IsUsingController => !NativeFunction.Natives.xA571D46727E2B718<bool>(2);
		#endregion

		public static void RenderBar(float currentFuelLevel, float maxFuelLevel, bool isElectric)
		{
			float fuelLevelPercentage = currentFuelLevel / maxFuelLevel * 100f;
			PointF safeZone = GetSafezoneBounds();
			Position = new PointF(basePosition.X + safeZone.X, basePosition.Y - safeZone.Y);
			fuelBar.SizeF = new SizeF(fuelBarWidth / 100f * fuelLevelPercentage, fuelBarHeight);
			if (maxFuelLevel > 0f && fuelLevelPercentage < 15f)
			{
				if (fuelBarColorTween.State == TweenState.Stopped)
				{
					fuelBarAnimationDir = !fuelBarAnimationDir;
					fuelBarColorTween.Start(fuelBarAnimationDir ? 100f : 255f, fuelBarAnimationDir ? 255f : 100f, 0.5f, ScaleFuncs.QuarticEaseOut);
				}
				// fuelBarColorTween.Update(NativeFunction.CallByHash<float>(0x15C40837039FFAF7));
				fuelBarColorTween.Update(N.GetFrameTime());
				fuelBar.Color = Color.FromArgb((int)Math.Floor(fuelBarColorTween.CurrentValue), isElectric ? fuelBarElectricColourWarning : fuelBarColourWarning);
			}
			else
			{
				fuelBar.Color = (isElectric ? fuelBarElectricColourNormal : fuelBarColourNormal);
				if (fuelBarColorTween.State != TweenState.Stopped)
				{
					fuelBarColorTween.Stop(StopBehavior.ForceComplete);
				}
			}
			fuelBarBackdrop.Draw();
			fuelBarBack.Draw();
			fuelBar.Draw();
		}

		#region Utilities
		public static PointF GetSafezoneBounds()
		{
			float t = N.GetSafeZoneSize();
			float w = 1280f;
			float h = 720f;
			return new PointF((int)Math.Round((w - w * t) / 2f + 1f), (int)Math.Round((h - h * t) / 2f - 2f));
		}

		public static float GetBarWidth()
		{
			double aspect = N.GetAspectRatio(false);
			if (aspect <= 1.3333333730697632)
			{
				if (aspect == 1.25)
				{
					return 255f;
				}
				if (aspect == 1.3333333730697632)
				{
					return 240f;
				}
			}
			else
			{
				if (aspect == 1.5)
				{
					return 212f;
				}
				if (aspect == 1.600000023841858)
				{
					return 200f;
				}
				if (aspect == 1.6666666269302368)
				{
					return 191f;
				}
			}
			return 180f;
		}
		#endregion

		public static void InstructToggleEngine()
        {
			buttons.Load("instructional_buttons");
			buttons.CallFunction("CLEAR_ALL");
			buttons.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
			buttons.CallFunction("CREATE_CONTAINER");
			if (IsUsingController)
            {
				buttons.CallFunction("SET_DATA_SLOT", 0, EngineButtonFormat, "Toggle engine");
			} 
			else
            {
				buttons.CallFunction("SET_DATA_SLOT", 0, EngineKeyFormat, "Toggle engine");
			}
			buttons.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
		}

		public static void InstructRefuel()
		{
			buttons.Load("instructional_buttons");
			buttons.CallFunction("CLEAR_ALL");
			buttons.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
			buttons.CallFunction("CREATE_CONTAINER");
			if (IsUsingController)
            {
				buttons.CallFunction("SET_DATA_SLOT", 0, RefuelButtonFormat, "Refuel");
			}
			else
            {
				buttons.CallFunction("SET_DATA_SLOT", 0, RefuelKeyFormat, "Refuel");
			}
			buttons.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
		}

		public static void InstructFullOrEmpty(string fuel)
        {
			buttons.Load("instructional_buttons");
			buttons.CallFunction("CLEAR_ALL");
			buttons.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
			buttons.CallFunction("CREATE_CONTAINER");
			// buttons.CallFunction("SET_DATA_SLOT", 0, NativeFunction.CallByHash(0x0499D7B09FC9B407, typeof(string), 2, 0, 0), fuel);
			buttons.CallFunction("SET_DATA_SLOT", 0, N.GetControlInstructionalButtonsString(2, 0, false), fuel);
			buttons.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
		}

		// possibly change in future but fine for now should use instructfull or empty
		public static void HideRefuel()
        {
			buttons.CallFunction("CLEAR_ALL");
		}

		public static void RenderInstructions()
        {
			if (!N.IsHudHidden())
            {
				buttons.Render2D();
            }
		}
	}
}