
namespace AssetInventory.Printing
{
	internal class ConsolePrompt
	{
		public static void Pause()
		{
			ConsolePrompt.String("Press enter to continue");
		}
		public static string String(string prompt)
		{
			Console.Write(prompt);
			return (string)Console.ReadLine();
		}
		public static decimal Decimal(string prompt, string initialText = "", Func<decimal, bool>? validate = null)
		{
			Func<string, bool> validate2 =
				validate == null ?
				input => decimal.TryParse(input, out _) :
				input => decimal.TryParse(input, out decimal d) && validate(d);
			void onSubmit(string input) => Console.WriteLine(prompt + input);
			void onChange(string input) => Console.Write($"\r{prompt}\x1b[1;{(validate2(input) ? 33 : 31)}m{input}\x1b[0m\x1b[K");
			return decimal.Parse(SimplerGnuReadLine.ReadLine(initialText, onChange, onSubmit, validate2));
		}
		public static bool IsPositiveCurrency(decimal d) => d * 100m % 1m == 0 && d > 0m;

		//public static DateOnly Date(string prompt, DateOnly? initialDate = null, Func<DateOnly,bool> validate=null)
		//{
		//    bool TryParse(string s,out DateOnly d)
		//    {
		//        switch(s)
		//        {
		//            case "now":
		//                d = DateOnly.FromDateTime(DateTime.Now);
		//                return true;
		//            default:
		//                return DateOnly.TryParse(s, out d);
		//        }
		//    }
		//    string initialValue = initialDate == null ? "now" : initialDate.ToString()!;
		//    Func<string, bool> validate2 =
		//        validate == null ?
		//        input => TryParse(input, out _) :
		//        input => TryParse(input, out DateOnly d) && validate(d);
		//    Action<string> onSubmit = input => Console.WriteLine(prompt + input);
		//    Action<string> onChange = input => Console.Write($"\r{prompt}\x1b[1;{(validate2(input) ? 33 : 31)}m{input}\x1b[0m\x1b[K");
		//    return DateOnly.Parse(SimplerGnuReadLine.ReadLine(initialValue, onChange, onSubmit, validate2));
		//}
		public static DateOnly DatePicker(DateOnly date = new())
		{
			if (date.Year <= 1) date = DateOnly.FromDateTime(DateTime.Today);
			const int CalendarWidgetHeight = 6 + 1 + 1;
			const int CalendarWidgetWidth = 7 * 3 - 1;
			//Console.Write(VT100.Save);
			int X, Y;
			int startOfMonth = (int)date.AddDays(1 - date.Day).DayOfWeek;
			(X, Y) = ((int)date.DayOfWeek, (date.Day + startOfMonth - 1) / 7 + 1);
			bool firstRun = true;
			do
			{
				int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
				startOfMonth = (int)date.AddDays(1 - date.Day).DayOfWeek;
				if (Y != 0)
					(X, Y) = ((int)date.DayOfWeek, (date.Day + startOfMonth - 1) / 7 + 1);
				if (!firstRun)
				{
					Console.CursorLeft = 0;
					Console.CursorTop = int.Max(Console.CursorTop - CalendarWidgetHeight + 1, 0);
				}
				firstRun = false;
				if (Y == 0)
					Console.WriteLine("\u001b[7m" + $" {date:M} {date.Year.ToString().PadLeft(4, '0')}".PadRight(CalendarWidgetWidth) + "\u001b[0m\x1b[K");
				else
					Console.WriteLine($" {date:M} {date.Year.ToString().PadLeft(4, '0')}".PadRight(CalendarWidgetWidth));
				foreach (DayOfWeek weekday in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
					Console.Write(weekday.ToString("F").Substring(0, 2) + ((int)weekday < 6 ? " " : ""));
				for (int i = 0, j = 0; i < 7 * 6; i++, j = i - startOfMonth + 1)
				{
					string sgr1 = i == Y * 7 - 7 + X ? "\x1b[7m" : "";
					string sgr2 = i == Y * 7 - 7 + X ? "\x1b[0m" : "";
					Console.Write((i % 7 == 0 ? "\n" : " ") + sgr1 + (j < 1 || j > daysInMonth ? "  " : j.ToString().PadLeft(2)) + sgr2);
				}

				ConsoleKeyInfo key = Console.ReadKey();
				switch (key.Key)
				{
					case ConsoleKey.UpArrow:
						if (Y == 0) { }
						else if (Y == 1) Y = 0;
						else date = date.AddDays(-7);
						break;
					case ConsoleKey.DownArrow:
						if (Y == 0)
						{
							Y = 1;
							date = date.AddDays(int.Clamp(X - startOfMonth, 1, 7) - date.Day);
						}
						else date = date.AddDays(7);
						break;
					case ConsoleKey.LeftArrow:
						if (Y == 0) date = date.AddMonths(-1);
						else date = date.AddDays(-1);
						break;
					case ConsoleKey.RightArrow:
						if (Y == 0) date = date.AddMonths(1);
						else date = date.AddDays(1);
						break;
					case ConsoleKey.Enter:
						return date;
				}
			} while (true);
		}
	}
}