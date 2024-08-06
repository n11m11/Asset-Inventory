using Microsoft.Identity.Client;

namespace AssetInventory.Printing;

internal class SimplerGnuReadLine(string Prompt = " > ")
{
	public string Prompt = Prompt;
	public virtual void OnChange(string s) =>
		Console.Write("\r"+Prompt+s+"\x1b[K");
	public virtual void OnSubmit(string s) =>
		Console.WriteLine("\r" + Prompt + s + "\x1b[K");
	bool Validate(string s) => true;
	public string ReadLine(string InitialValue, bool NoQuickBackspace = false, bool AutoBasicReadLineOnEmpty = true) =>
		ReadLine(InitialValue, OnChange, OnSubmit, Validate, NoQuickBackspace, AutoBasicReadLineOnEmpty);

	/// <summary>
	/// Glorified Console.ReadLine() with callbacks, allowing vast customizability.
	/// </summary>
	/// <param name="InitialValue"></param>
	/// <param name="OnChange">This is called to redraw (i.e. print) the current line.</param>
	/// <param name="OnSubmit">This is called right before returning to do a final redraw (i.e. print) of the line read.</param>
	/// <param name="Validate">Called when a newline is read. Should not print anything.</param>
	/// <param name="NoQuickBackspace">Disable clearing the whole line on backspace.</param>
	/// <param name="AutoBasicReadLineOnEmpty">Switch to Console.ReadLine() when the line buffer is empty.</param>
	/// <returns>The state of the buffer after Validate() returns true.</returns>
	public static string ReadLine(string InitialValue, Action<string> OnChange, Action<string> OnSubmit, Func<string, bool> Validate, bool NoQuickBackspace = false, bool AutoBasicReadLineOnEmpty = false)
	{
		string LineBuffer = InitialValue;
		bool running = true;
		while (running)
		{
			OnChange(LineBuffer);
			if (AutoBasicReadLineOnEmpty && LineBuffer.Length == 0)
			{
				LineBuffer = Console.ReadLine();
				break;
			}
			ConsoleKeyInfo key = Console.ReadKey();
			switch (key.KeyChar)
			{
				case '\0':
				case '\t':
				case '\x1b':
					break;
				case '\b':
				case '\u007f':
					if(NoQuickBackspace)
						LineBuffer = LineBuffer.Substring(0, LineBuffer.Length-1);
					else
						LineBuffer = "";
					break;
				case '\r':
				case '\n':
					running = !Validate(LineBuffer);
					break;
				default:
					LineBuffer += key.KeyChar;
					break;
			}
		}
		OnSubmit(LineBuffer);
		return LineBuffer;
	}
}