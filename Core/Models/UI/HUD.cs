using Common.Native;
using Common.UI.Elements;
using Rectangle = Common.UI.Elements.Rectangle;
using RAGENativeUI.Elements;
using System;
using System.Drawing;
using SimpleCTRL.TinyTween;
using SimpleCTRL.Handlers;
using SimpleCTRL.Utils;
using Rage.Native;
using Rage;

namespace SimpleCTRL.Core.Models.UI
{
    public class HUD
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

		public static Rectangle fuelBarBackdrop = new Rectangle(fuelBarBackdropPosition, fuelBarBackdropSize, fuelBarBackdropColour);

		public static Rectangle fuelBarBack = new Rectangle(fuelBarBackPosition, fuelBarBackSize, fuelBarBackColour);

		public static Rectangle fuelBar = new Rectangle(fuelBarPosition, fuelBarSize, fuelBarColourNormal);

		public static PointF Position
		{
			set
			{
				fuelBarBackdrop.Position = value;
				fuelBarBack.Position = new PointF(value.X, value.Y + 3f);
				fuelBar.Position = fuelBarBack.Position;
			}
		}

		private static string EngineKeyFormat { get; set; } = ConversionAndFormattingHelper.FormatKeyBinding(ConfigHandler.EngineToggleKey);
		private static string EngineButtonFormat { get; set; } = ConversionAndFormattingHelper.FormatKeyBinding(ConfigHandler.EngineControllerButton);
		private static string RefuelKeyFormat { get; set; } = ConversionAndFormattingHelper.FormatKeyBinding(ConfigHandler.RefuelKey);
		private static string RefuelButtonFormat { get; set; } = ConversionAndFormattingHelper.FormatKeyBinding(ConfigHandler.RefuelControllerButton);

		private static bool IsUsingController => !NativeFunction.Natives.xA571D46727E2B718<bool>(2);
		#endregion

		public static void RenderBar(float currentFuelLevel, float maxFuelLevel, bool isElectric)
		{
			float fuelLevelPercentage = currentFuelLevel / maxFuelLevel * 100f;
			PointF safeZone = GetSafezoneBounds();

			bool bigMap = false; // code later

			if (bigMap)
			{
				Position = new PointF(basePosition.X + safeZone.X, basePosition.Y - safeZone.Y - 180f);
			}
			else
			{
				Position = new PointF(basePosition.X + safeZone.X, basePosition.Y - safeZone.Y);
			}

			fuelBar.SizeF = new SizeF(fuelBarWidth / 100f * fuelLevelPercentage, fuelBarHeight);
			if (maxFuelLevel > 0f && fuelLevelPercentage < 15f)
			{
				if (fuelBarColorTween.State == TweenState.Stopped)
				{
					fuelBarAnimationDir = !fuelBarAnimationDir;
					fuelBarColorTween.Start(fuelBarAnimationDir ? 100f : 255f, fuelBarAnimationDir ? 255f : 100f, 0.5f, ScaleFuncs.QuarticEaseOut);
				}
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

		/// <summary>
		/// Returns resolution specified bar width
		/// </summary>
		/// <returns></returns>
		private static float GetBarWidth()
		{
			float width;
			double aspect = NativeFunction.CallByHash<float>(0xF1307EF624A80D87);
			bool bigMap = false; // code later

			switch (aspect)
			{
				case (float)1.5: // 3:2
					width = bigMap ? 336f : 212f;
					break;
				case (float)1.33333337306976: // 4:3
					width = bigMap ? 378f : 240f;
					break;
				case (float)1.66666662693024: // 5:3
					width = bigMap ? 302f : 191f;
					break;
				case (float)1.25: // 5:4
					width = bigMap ? 405f : 255f;
					break;
				case (float)1.60000002384186: // 16:10
					width = bigMap ? 316f : 200f;
					break;
				default:
					width = bigMap ? 285f : 180f; // 16:9
					break;
			}
			return width;
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
			buttons.CallFunction("SET_DATA_SLOT", 0, N.GetControlInstructionalButtonsString(2, 0, false), fuel);
			buttons.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
		}

		public static void InstructManualRefuel()
		{
			buttons.Load("instructional_buttons");
			buttons.CallFunction("CLEAR_ALL");
			buttons.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
			buttons.CallFunction("CREATE_CONTAINER");
			buttons.CallFunction("SET_DATA_SLOT", 0, N.GetControlInstructionalButtonsString(2, 24, false), "Refuel");
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
