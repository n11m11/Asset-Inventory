using System.Runtime.CompilerServices;
using Spectre.Console;

namespace AssetInventory.Printing;

/// <summary>
/// <![CDATA[
/// Used as List<OrderedChoice>.
/// ]]>
/// </summary>
/// <param name="Id">Choice ID.</param>
/// <param name="Shortname">Keyword used for searching the list of choices.</param>
/// <param name="Longname">Description for this choice. Also used for search.</param>
internal record struct OrderedChoice(int Id, string Shortname, string Longname)
{
	public Action? Callback = null;
	public object? Data = null;
	public bool Hidden = false;
	public bool Unselectable = false;

	public OrderedChoice(int id, string shortname, string longname, Action? callback) : this(id, shortname, longname)
	{
		Callback = callback;
	}

	/// <summary>
	/// Creates a new unselectable OrderedChoice.
	/// </summary>
	public OrderedChoice() : this(-1, "", "")
	{
		Unselectable = true;
	}
}

/// <summary>
/// This tuple is passed to the formatter of an OrderedChoiceList instance to assist TUI table formatting.
/// </summary>
/// <param name="Id">Max width of column.</param>
/// <param name="Shortname">Max width of column.</param>
/// <param name="Longname">Max width of column.</param>
internal record struct OrderedChoiceLengths(int Id, int Shortname, int Longname);

internal class OrderedChoiceList : List<OrderedChoice>
{
	public bool GapTitle { get; set; } = true;
	public bool GapCallback { get; set; } = true;
	public bool AllowEmpty { get; set; } = false;
	public Func<OrderedChoice, OrderedChoiceLengths, string> PrintFormatter { get; set; } =
		(x, l) => $"  {x.Shortname.PadLeft(l.Shortname)} {x.Longname}" + int.MaxValue;
	public Func<OrderedChoice, OrderedChoiceLengths, FormattableString> MarkupFormatter { get; set; } =
		//(x, l) => $"  [bold]{x.Shortname.PadLeft(l.Shortname)}[/] [blue]{x.Longname}[/]";
		(x, l) =>
	FormattableStringFactory.Create($"  [bold]{{0}}[/] [blue]{x.Longname}[/]", x.Shortname.PadLeft(l.Shortname));

	protected static int MaxValidFuzzy = FuzzyString.LengthSensitiveFuzzy("femurs", "edited gollum");

	static OrderedChoice UnknownChoice { get; set; } = new(-1, "???", "???");

	public OrderedChoiceLengths GetLengths()
	{
		var q = this.Where(x => !x.Hidden);
		return new(
			q.Select(x => x.Id.ToString().Length).Max(),
			q.Select(x => x.Shortname.ToString().Length).Max(),
			q.Select(x => x.Longname.ToString().Length).Max()
		);
	}

	public OrderedChoice? Match(string s)
		=>
		this.Where(oc => !oc.Unselectable && !(oc.Shortname.Length == 0 ^ s.Length == 0))
		.Cast<OrderedChoice?>()
		.MinBy(oc =>
			oc!.Value.Id.ToString() == s ? 0 : FuzzyString.Fuzzy(s, oc.Value.Shortname)
			);




	protected OrderedChoice? ValidMatchFromInput(string input)
	{
		OrderedChoice? match = Match(input);
		if (match != null && FuzzyString.LengthSensitiveFuzzy(((OrderedChoice)match).Shortname, input) >= MaxValidFuzzy)
			match = UnknownChoice;
		return match;
	}

	protected OrderedChoice MatchFromInput(string input)
	{
		return Match(input) ?? UnknownChoice;
	}

	protected void onChange(string input)
	{
		int minLengthOfInputField = 10;
		if (AllowEmpty || input.Length > 0)
		{
			OrderedChoice match = ValidMatchFromInput(input) ?? UnknownChoice;
			int temp = (input.Length + minLengthOfInputField + 1) / 2;
			Console.Write($"\r >> {input.PadLeft(temp).PadRight(minLengthOfInputField)} {match.Longname}\x1b[K");
			Console.CursorLeft = int.Clamp($" >> {input.PadLeft(temp)}".Length, 0, Console.BufferWidth - 1);
		}
		else
		{
			Console.Write($"\r >>   Enter a choice...\x1b[K");
		}
	}

	protected void onSubmit(string input)
	{
		OrderedChoice match = MatchFromInput(input);
		Console.WriteLine($"\r >>   {match.Longname}\x1b[K");
	}

	protected bool validate(string input)
	{
		return AllowEmpty && input.Length == 0 || input.Length > 0 && ValidMatchFromInput(input) != null;
	}

	/// <summary>
	/// ReadLine but returns OrderedChoice using FuzzyString.
	/// </summary>
	/// <param name="promptTitle">A Spectre.Console markup string if parameter noMarkup is false.</param>
	/// <param name="noMarkup">Use PrintFormatter instead of MarkupFormatter.</param>
	/// <returns>The chosen list element.</returns>
	public OrderedChoice Prompt(string promptTitle = "Choose an action:", bool noMarkup = false)
	{
		// Choose an action:
		//   (1) add asset
		//   (2) list
		//   (3) remove asset
		//   (4) edit asset
		//  >>   Enter a choice...
		OrderedChoiceLengths l = GetLengths();
		if (noMarkup)
		{
			AnsiConsole.MarkupLine(promptTitle);
			foreach (OrderedChoice v in this)
				if (!v.Hidden)
					Console.WriteLine(PrintFormatter(v, l));
		}
		else
		{
			Console.WriteLine(promptTitle);
			foreach (OrderedChoice v in this)
				if (!v.Hidden)
					AnsiConsole.MarkupLineInterpolated(MarkupFormatter(v, l));
		}
		if (GapTitle)
			Console.WriteLine();
		string input = SimplerGnuReadLine.ReadLine("", onChange, onSubmit, validate);
		OrderedChoice match = MatchFromInput(input);
		if (match.Callback is Action callback)
		{
			if (GapCallback)
				Console.WriteLine();
			callback();
		}
		else if (match.Data is Action callback_legacy) // support older functions passing callback as data
		{
#if DEBUG
			Console.WriteLine("DEBUG: WARNING: legacy callback used in Prompt()");
#endif
			callback_legacy();
		}

		return match;
	}
}