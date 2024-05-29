using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Extensions;
using SimpleCTRL.Handlers;

namespace SimpleCTRL.Components
{
    internal static class GameWorld
    {
		internal static void CreateDepartmentPumps()
		{
			if (ConfigHandler.FuelSystem == true)
			{
				while (true)
				{
					Model model = new Model("prop_gas_pump_old2");
					N.RequestModel(model);

					if (N.HasModelLoaded(model))
					{
						foreach (GasPump pump in Globals.DepartmentPumps)
						{
							if (pump != null && Game.LocalPlayer.Character != null)
							{
								unsafe
								{
									Vector3 position = Game.LocalPlayer.Character.Position;
									if (position.DistanceToSquared(pump.Position) <= 50000f && !N.DoesObjectOfTypeExistAtCoords(pump.Position.X, pump.Position.Y, pump.Position.Z, 2f, Globals.DepartmentPumpObjectHash))
									{
										position = pump.Position;
										N.RequestCollisionAtCoord(pump.Position.X, pump.Position.Y, 1000f);
										float resultArg = N.GetGroundZFor3DCoord(pump.Position.X, pump.Position.Y, 1000f, false);
										pump.Position.Z = resultArg;
										Rage.Object obj = new Rage.Object(model.Hash, pump.Position);
										Globals.DepartmentPumpObjects.Add(obj);
										NativeFunction.CallByHash<int>(0x8524A8B0171D5E07, obj, 0.0f, 0.0f, Common.API.Math.DirectionToRotation(Common.API.Math.HeadingToDirection(pump.Rotation), 0f).Z, 1);
										if (EntityExtensions.Exists(obj))
										{
											obj.IsInvincible = true;
											obj.IsPositionFrozen = true;
											obj.IsExplosionProof = true;
											obj.IsFireProof = true;
											obj.IsCollisionProof = true;
										}
									}
								}
							}
							GameFiber.Wait(250);
						}
						GameFiber.Wait(250);
					}
					GameFiber.Yield();
				}
			}
		}
	}
}
