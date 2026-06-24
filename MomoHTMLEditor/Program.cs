using MomoHTMLEditor;

Console.Title = "MomoHTML Tool";

int selMenuInd = 0;
string[] options = { "Open Editor", "New File", "Open File", "Convert Current File", "Save File", "Save File As", "Settings", "Exit" };

runStates currentState = runStates.Menu;

Editor editor = new Editor(); //cursed C# things; also, see Editor.cs for the ACTUAL editing code

while (currentState != runStates.Exit)
{
    switch (currentState)
    {
        case runStates.Menu:
            Console.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selMenuInd)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"> {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"  {options[i]}");
                }
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selMenuInd = (selMenuInd == 0) ? options.Length - 1 : selMenuInd - 1;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selMenuInd = (selMenuInd == options.Length - 1) ? 0 : selMenuInd + 1;
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                switch (selMenuInd)
                {
                    case 0: // Open Editor (translation: close the menu)
                        editor.Engine();
                        break;
                    case 7: // Exit
                        currentState = runStates.Exit;
                        break;
                }
            }
            break;
        default:
            editor.Engine();
            break;
    }
}

Console.WriteLine("i'm barely fucking putting the menu together chill the hell out");
Console.ReadKey(true); // quite literally just stall so we can see our shit happen