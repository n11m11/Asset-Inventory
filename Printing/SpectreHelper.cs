using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AssetInventory.Printing;
using Spectre.Console;

namespace AssetInventory.Printing
{
	internal static class SpectreHelper
	{
		/// <summary>
		/// Example: SpectreHelper.Loading().Start()
		/// </summary>
		/// <returns></returns>
		public static Status Loading() => AnsiConsole.Status()
							.Spinner(Spinner.Known.SimpleDots)
							.SpinnerStyle(Style.Parse("blue"));

		/// <summary>
		/// Prints an error and optionally resets the terminal when an exception is thrown.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="resetConsoleOnError"></param>
		public static void SpectreTryCatchAll<T>(Action action, bool resetConsoleOnError = false) where T : Exception
		{
			try
			{
				action();
			}
			catch (T ex)
			{
				if (resetConsoleOnError)
				{
					Console.ResetColor();
					Console.Clear(); // clears using current colors.
				}
				Console.WriteLine("There was an error generating content with Spectre.");
				try
				{
					AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
				}
				catch
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.Data);
				}
			}
		}
		public static Regex? PromptRegex(bool allowCancel = true)
		{

			Regex? compile(string s)
			{
				try
				{
					return new(s, RegexOptions.Compiled);
				}
				catch
				{
					return null;
				}
			}
			if (allowCancel)
				Console.WriteLine("Leave blank to cancel");
			return compile(
							AnsiConsole.Prompt(
								new TextPrompt<string>("Regex pattern: ")
								{
									Validator = s => compile(s) is not null ?
										ValidationResult.Success() :
										ValidationResult.Error("Invalid regex"),

								})
							);
		}
	}
}

internal class ActionOrWriteException(Action action)
{
	Action action = () =>
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
			Console.WriteLine();
			ConsolePrompt.Pause();
		}
	};

	public static explicit operator ActionOrWriteException(Action action)
	{
		return new(action);
	}
	public static implicit operator Action(ActionOrWriteException action)
	{
		return action.action;
	}
}