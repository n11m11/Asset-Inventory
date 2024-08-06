using Spectre.Console;

namespace Asset_Inventory;

internal class IntroBanner
{
    public Thread thread = new(Draw);
    public bool Joined = true;
    public void Start()
    {
        thread.Start();
        Joined = false;
    }
    public void Join()
    {
        if (!Joined)
        {
            Joined = true;
            thread.Join();
#if DEBUG
            Console.WriteLine("DEBUG: Calling thread joined banner thread.");
#endif
        }
    }


    static void Draw()
    {
        Thread.Sleep(1000);
        var text1 = new FigletText("Asset").Centered();
        var text2 = new FigletText("Inventory").Centered();
        var rows = new Rows(text1, text2).Expand();
        AnsiConsole.Live(rows)
            .Start(ctxLive =>
            {
                for (int i = 0; i <= 13; i++)
                {
                    if (i != 0) Thread.Sleep(41);
                    text1.Color(Color.Black.Blend(Color.DodgerBlue1, (int.Clamp(i,0,10) + 3) / 13f));
                    text2.Color(Color.Black.Blend(Color.DeepSkyBlue1, (int.Clamp(i-3,0,10) + 3) / 13f));
                    ctxLive.Refresh();
                }
            });
#if DEBUG
        Console.WriteLine("DEBUG: Banner done.");
#endif
    }

}