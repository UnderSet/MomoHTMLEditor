// Exporter to final HTML format. This so far is not a reversible process. (I might write a reverse converter one day. Sounds like a fun project.)
// Possibly the other cornerstone of this whole project aside from the editor. ("MomoHTMLEditor" is a bit of a misleading name now that I think about it...)
//
// I've decided to document this file specifically more extensively than I do the other ones as I think this may be useful to others as well.
//
// Disclaimer: This class is likely not usable standalone and may require significant reworking when used outside of this tool. Reference Program.cs for how I use it here.
//
// Disclaimer: Profile images are currently not working as of me writing this. It's coming at a later date, once I also figure out how to let you configure custom profile
//   images and override defaults.
//
// Huuuuge thanks to ManicMinic on AO3 for their MomoTalk workskin template: https://archiveofourown.org/works/64821349
//   (which this uses a modified version of as a regular CSS sheet on one of my GitHub repositories; source:
//     https://github.com/UnderSet/ogrerefresh/blob/main/momotalk.css
//    and uses the HTML structure in output as well, see below for my usage of it and in HTML files exported by this for how it is in practice)
// If it wasn't for them, I wouldn't have thought of trying to make a tool to assist making stuff with it probably wouldn't even have started this whole thing.
// ManicMinic, if you're reading this, thanks for making the template and letting people use and modify it. (And let me know if you're not okay with all this if that's
//   unfortunately the case. I'll remove this if you want me to.)
// 
// Additional disclaimer: The way this tool was made, it is *HEAVILY* opinionated to my usage and influenced by the rest of this tool's design.
//   What that means: There's no support for image messages (even though ManicMinic's template did include it and my stylesheet by extension retains support for it) and
//   choices, and this tool does *not* use the MomoTalk box/wrapper in its output (again, even though the original template and my stylesheet by extension still supports
//   it).
//
// Regardless, whoever's reading this, I hope you find the tool - or at least this code - useful in some way.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MomoHTMLEditor {
    internal class Exporter {
        public void Export(List<Message> List, string fileName) {
            #pragma warning disable CS8604 // embedded resource - never null...unless something goes catastrophically wrong ofc
            string assembly = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("profiles.json")).ReadToEnd();
            #pragma warning restore CS8604
            Dictionary<string, string> profileList = JsonSerializer.Deserialize(assembly, AppJsonContext.Default.DictionaryStringString)!;

            string? overridesstr = null;

            try {
                overridesstr = File.ReadAllText("profiles.json");
            }
            catch (UnauthorizedAccessException) {
                Console.WriteLine("Permission denied: Cannot read profile image overrides.");
            }
            catch (System.Text.Json.JsonException ex) {
                Console.WriteLine($"JSON conversion (profile image overrides) failed: {ex.Message}");
            }
            catch (IOException ex) {
                Console.WriteLine($"I/O exception while loading profile image overrides: {ex.Message}");
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred loading profile image overrides: {ex.Message}");
            }

            Dictionary<string, string>? profileListOverrides = !string.IsNullOrEmpty(overridesstr)
                ? JsonSerializer.Deserialize(overridesstr, AppJsonContext.Default.DictionaryStringString)
                : new();

            string fileExportName = $"{fileName}.html"; // For example, if our input file name is "case1.json" in the case of use with this tool, we'll get output named
                                                        //   "case1.json.html" - I at the end decided against trying to replace the file extension as this tool was *not*
                                                        //   designed with a hard extension and whatever the user typed as their filename is saved as-is.
                                                        // (and yes, that does also mean you can save with no extension entirely)
            MessageType lastMessageType = MessageType.Received; // lastMessageType MUST be initialized with SOME value or things break; I thought this was a safe default
            string lastSender = "";

            // Disk I/O is prone to errors and we write after every message, so that's why I use a try/catch here
            // There's quite a lot of things that can go wrong even in a language like C#...
            try {
                File.WriteAllText(fileExportName, string.Empty); // ALWAYS wipe file before begin export or BAD SHIT HAPPENS.

                using StreamWriter writer = new StreamWriter(fileExportName, true);
                // Feel free to edit this link with a link to your hosted version of the stylesheet or remove this line outright if you're using ManicMinic's
                //   workskin as-is (or with minor edits to modify margin or something) on an AO3 fic
                // Note if you're using my stylesheet: It sets Open Sans as the font to be more accurate to ingame, so you may want to - if you have the choice,
                //   at least - embed the font from Google Fonts or something.
                writer.WriteLine("<link rel=\"stylesheet\" href=\"https://underset.github.io/MomoHTMLEditorAssets/css/momotalk.css\" crossorigin=\"anonymous\">");

                foreach (Message m in List) {
                    string line = ""; // VS will bitch at me if I don't set it to explicit empty string

                    // Switch what we're going to write depending on current message's type
                    switch (m.Type) {
                        case MessageType.Received:
                            // Sender is ALWAYS saved for ALL message types so we also check message type here as well
                            // Technically I could have dealt with it by only saving sender type if it's a received message but...hindsight is 20/20 and I don't
                            // wanna break any backwards compatibility (this tool had some use in private)
                            var parts = m.Sender.Split("|", 2);
                            string sender = parts[0];
                            string profile = parts.Length > 1 ? parts[1] : parts[0];
                            #pragma warning disable CS8620 // "Argument cannot be used for parameter due to differences in the nullability of reference types."
                                                           // I have...not the slightest fucking clue what that is, but this works so...
                            #pragma warning disable CS8604 // good ol' "Possible null reference argument." (99% sure it can't happen once we got here though)
                            string image = profileListOverrides.GetValueOrDefault(profile, null)
                                        ?? profileList.GetValueOrDefault(profile, "");
                            #pragma warning restore CS8604
                            #pragma warning restore CS8620


                            if (lastMessageType == MessageType.Received && lastSender == m.Sender) {
                                line = $"<div class=\"msg received notail\">" +
                                    "<p class=\"bubble\"" + (string.IsNullOrEmpty(m.Styling) ? "" : $" style=\"{m.Styling}\"") + $">{m.Text}</p></div>";
                            }
                            else {
                                line = $"<div class=\"msg received\">" +
                                    $"<p><img class=\"avatar\" src=\"{image}\"></p>" +
                                    $"<p><span class=\"speaker\">{sender}</span></p>" +
                                    "<p class=\"bubble\"" + (string.IsNullOrEmpty(m.Styling) ? "" : $" style=\"{m.Styling}\"") + $">{m.Text}</p></div>";
                            }

                            // There was probably a better way to do it than this, but the way this foreach loop works, this was all I could come up with atm
                            lastSender = sender;
                            lastMessageType = MessageType.Received;
                            break;
                        case MessageType.Sent:
                            line = $"<div class=\"msg sent\"><p class=\"bubble\""
                                + (string.IsNullOrEmpty(m.Styling) ? "" : $" style=\"{m.Styling}\"")
                                + $">{m.Text}</p></div>";
                            lastMessageType = MessageType.Sent;
                            break;
                        // This is only supported when using my stylesheet specifically for now, unfortunately.
                        // These are the "neutral" gray system bubbles (hence being internally called System like this) you sometimes see for, say, noting what time
                        //   it is or to narrate in MomoTalk works (though use like that is rare from what I know).
                        case MessageType.System:
                            line = $"<div class=\"msg system\"><p class=\"bubble\""
                                + (string.IsNullOrEmpty(m.Styling) ? "" : $" style=\"{m.Styling}\"")
                                + $">{m.Text}</p></div>";
                            lastMessageType = MessageType.Sent;
                            break;
                    }

                    // Finally, write (append) it to the file
                    writer.WriteLine(line);
                }

                Console.WriteLine("Export complete.");
            }
            catch (UnauthorizedAccessException) {
                Console.WriteLine("Permission denied: Cannot write here.");
            }
            catch (IOException ex) {
                Console.WriteLine($"I/O exception: {ex.Message}");
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to return.");
            // Console.ReadKey delays execution until you press a key - this fortunately doesn't cause it to "hang" per se
            Console.ReadKey(true);
        }
    }
}