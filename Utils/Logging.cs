using Rage;
using System;

namespace SimpleCTRL.Utils
{
    internal class Logging
    {
		public static void Debug(string message, string caller, Exception ex = null)
		{
			Log(LoggingLevel.DEBUG, message, caller, ex);
		}

		public static void Error(string message, string caller, Exception ex = null)
		{
			Log(LoggingLevel.ERROR, message, caller, ex);
		}

		public static void Info(string message, string caller, Exception ex = null)
		{
			Log(LoggingLevel.INFO, message, caller, ex);
		}

		public static void Warning(string message, string caller, Exception ex = null)
		{
			Log(LoggingLevel.WARNING, message, caller, ex);
		}


		public static void Log(LoggingLevel level, string message, string caller, Exception ex = null)
		{
			// if ((int)level >= ConfigHandler.LoggingLevel)
			// {
				Game.LogTrivial("SimpleCTRL: [" + caller + "] " + message);
				if (ex != null)
				{
					Error(ex.Message, caller);
					Debug(ex.StackTrace, caller);
				}
			// }
		}
	}
}