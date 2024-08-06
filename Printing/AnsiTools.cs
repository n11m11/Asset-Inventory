using System.Runtime.InteropServices;

namespace AssetInventory.Printing;

internal class AnsiTools
{
	// not VT100
	public const string AlternateScreenBufferOn = "\u001b[?1049h";
	public const string AlternateScreenBufferOff = "\u001b[?1049l";
	public static bool TrySetTitle(string s)
	{
		try
		{
			Console.Title = s;
		}
		catch
		{
			return false;
		}
		return true;
	}
	/// <summary>
	/// The windows terminal cmd.exe does not take ANSI escape codes by default.
	/// </summary>
	/// <returns>Whether the terminal on stdout will understand ANSI escape codes.</returns>
	public static bool HasVTEnabledStdout()
	{
		if (Console.IsOutputRedirected) // ???
			return false;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Microsoft.Win32.SafeHandles.SafeFileHandle hOut = Windows.Win32.PInvoke.GetStdHandle_SafeHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE);
			Windows.Win32.System.Console.CONSOLE_MODE modeOut;
			Windows.Win32.PInvoke.GetConsoleMode(hOut, out modeOut);
			return modeOut.HasFlag(Windows.Win32.System.Console.CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
		}
		else
		{
			return true; // 99% accurate
		}
	}
	public static bool EnableVT()
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return true;
		Microsoft.Win32.SafeHandles.SafeFileHandle hOut = Windows.Win32.PInvoke.GetStdHandle_SafeHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE);
		Windows.Win32.System.Console.CONSOLE_MODE modeOut;
		if (!Windows.Win32.PInvoke.GetConsoleMode(hOut, out modeOut))
		{
#if DEBUG
			Console.WriteLine("DEBUG: GetConsoleMode() failed on Windows. Using MinTTY?");
#endif
			return !Console.IsOutputRedirected;
		}
		if (!modeOut.HasFlag(Windows.Win32.System.Console.CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING))
		{
#if DEBUG
			Console.WriteLine("DEBUG: Enabling VT processing on stdout.");
#endif
			modeOut |= Windows.Win32.System.Console.CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			return Windows.Win32.PInvoke.SetConsoleMode(hOut, modeOut);
		}
		else
		{
#if DEBUG
			Console.WriteLine("DEBUG: Not enabling VT processing on stdout because it's already enabled.");
#endif
			return true;
		}
	}
}