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
    // This state machine or whatever...sorta doesn't actually fucking do anything except for controlling exiting.
    // Q: "Okay, UnderSet, why haven't you yanked it out then?"
    // A: This is an example of what I call "I know I should remove it but dear GOD if there's side effects when I do that-"
    switch (currentState)
    {
        case runStates.Menu:
            string fileName = string.IsNullOrEmpty(editor.fileName) ? "No file" : editor.fileName;
            Console.Clear(); // yes, we clear the console every frame; yes this flickers a metric fuckton; no I don't have any (good) way to deal with this
                             // (without being a pain in the ass to do that is; doubly pain in the ass in Editor.cs)
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"MomoHTML Tools v{version}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" | Compiled {buildDate}{Environment.NewLine}");
            Console.ForegroundColor = ConsoleColor.Yellow;
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

            ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Fun fact: this pauses execution until you press something

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                // If we're at 0, wrap to bottom (7); else decrement by 1
                // Bottom is 7 because enums start at 1 but length starts counting from 1, so a 8 elem enum would have its final one at 7
                // Technically we're not using a proper enum for this one, but it's close enough for this explanation
                selMenuInd = (selMenuInd == 0) ? options.Length - 1 : selMenuInd - 1;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                // If we're at 7 (max), wrap to top (0); else increment by 1
                selMenuInd = (selMenuInd == options.Length - 1) ? 0 : selMenuInd + 1;
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                switch (selMenuInd)
                {
                    // See this file around L7 (string[] options) for what these options are respectively (reminder: this starts indexing from 0)
                    case 0: // Open Editor (translation: close the menu)
                        editor.Engine();
                        break;
                    case 1:
                        editor = new Editor(); // Instantiate (?) a new editor instance, discarding old one; this clears everything
                                               // idk I'm still bit of a C# newbie myself, but this works well enough for New File
                        selMenuInd = 0; // for buttons such as Open File/New File, jump to Open Editor after it finished
                        break;
                    case 2:
                        editor.Load();
                        selMenuInd = 0;
                        break;
                    case 4:
                        // Most apps I know make Save trigger Save As instead if there's no current active file (or whatever it's supposed to be called)
                        // So here's MomoHTMLEditor doing the same (as it probably should tbh)
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
                        Console.ResetColor(); // Reset colors on exit so we don't fuck over the user's console
                                              // (applies mostly if this was run from, say, CMD or bash instead of being doubleclicked)
                        break;
                }
            }
            break;
        default:
            editor.Engine();
            break;
    }
}