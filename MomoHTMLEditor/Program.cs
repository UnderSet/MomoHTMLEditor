using MomoHTMLEditor;
using System.Reflection;

Console.Title = "MomoHTML Tool";

int selMenuInd = 0;
string[] options = { "Open Editor", "New File", "Open File", "Convert Current File", "Save File", "Save File As", "Settings", "Exit" };
var buildDate = Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "BuildDate")?.Value;
var version = Assembly.GetExecutingAssembly()?.GetName().Version;

runStates currentState = runStates.Menu;

Editor editor = new Editor(); //cursed C# things; also, see Editor.cs for the ACTUAL editing code

while (currentState != runStates.Exit)
{
    switch (currentState)
    {
        case runStates.Menu:
            string fileName = string.IsNullOrEmpty(editor.fileName) ? "No file" : editor.fileName;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"MomoHTML Tools v{version}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" | Compiled {buildDate}{Environment.NewLine}");
            Console.ResetColor();
            Console.WriteLine($"Current file: {fileName}");
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selMenuInd)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"> {options[i]}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White; // this "fixes" a bizarre edge case where colors are screwed
                                                                  // (in hindsight should have done this in the beginning)
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
                    case 1:
                        editor = new Editor();
                        selMenuInd = 0; // for buttons such as Open File/New File, jump to Open Editor after it finished
                        break;
                    case 2:
                        editor.Load();
                        selMenuInd = 0;
                        break;
                    case 4:
                        if (!string.IsNullOrEmpty(editor.fileName)) {
                            editor.Save();
                        }
                        else {
                            editor.SaveAs();
                        }
                        selMenuInd = 0;
                        break;
                    case 5:
                        editor.SaveAs();
                        selMenuInd = 0;
                        break;
                    case 7: // Exit
                        currentState = runStates.Exit;
                        Console.ResetColor();
                        break;
                }
            }
            break;
        default:
            editor.Engine();
            break;
    }
}