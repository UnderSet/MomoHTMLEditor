using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace MomoHTMLEditor
{
    public enum MessageType { Received, Sent, System }
    public enum Inputs { Cursor, Index, Enter }
    public class Editor
    {
        // A bunch of variables used throughout the program.
        // DO NOT TOUCH THESE UNLESS YOU WANT TO BREAK THINGS. I HAVE WARNED YOU.
        public string? fileName;
        public List<Message> MessagesBuffer = new List<Message>();
        private string senderBuffer = "";
        private string messageBuffer = "";
        private string stylingBuffer = "";
        private int activeBufferType = 0;
        private int MessagesIndex = 0;
        private int MessageCursorPos = 0;
        private int SenderCursorPos = 0;
        private int StylingCursorPos = 0;
        private MessageType MsgType = MessageType.Received;

        // little trick to easily get the actual active buffer and not duplicate lots of code
        // (see MomoHTMLEditor.Editor.Engine; if not for this that'd be double the input codebase at best)
        private string activeBuffer
        {
            //get => activeBufferType ? senderBuffer : messageBuffer;
            get {
                switch (activeBufferType) {
                    case 0:
                        return senderBuffer;
                    case 1:
                        return messageBuffer;
                    case 2:
                        return stylingBuffer;
                    default: // this shouldn't ever be outside of 0-2, but if I don't do this the tools won't STFU
                        return "";
                }
            }
            set
            {
                //if (activeBufferType) senderBuffer = value;
                //else messageBuffer = value;
                switch (activeBufferType) {
                    case 0:
                        senderBuffer = value;
                        break;
                    case 1:
                        messageBuffer = value;
                        break;
                    case 2:
                        stylingBuffer = value;
                        break;
                }
            }
        }

        // same as the thing in activeBuffer, but apply that logic to getting the right cursor position instead
        // (and yes, there's always two cursors being kept track of lmao)
        private int TextCursorPos {
            //get => activeBufferType ? SenderCursorPos : MessageCursorPos;
            //set {
            //    if (activeBufferType) SenderCursorPos = value;
            //    else MessageCursorPos = value;
            //}
            get {
                switch (activeBufferType) {
                    case 0:
                        return SenderCursorPos;
                    case 1:
                        return MessageCursorPos;
                    case 2:
                        return StylingCursorPos;
                    default: // this shouldn't ever be outside of 0-2, but if I don't do this the tools won't STFU
                        return 0;
                }
            }
            set {
                switch (activeBufferType) {
                    case 0:
                        SenderCursorPos = value;
                        break;
                    case 1:
                        MessageCursorPos = value;
                        break;
                    case 2:
                        StylingCursorPos = value;
                        break;
                }
            }
        }

        public void Engine()
        {
            while (true)
            {
                int width = Console.WindowWidth;
                int freeLines = Console.WindowHeight - 4; // This calculates the remaining free lines in the console, which is used for the next message
                                                          // previews (to not unnecessarily show all of them all the time and overflow the screen)
                int peekLines = 0;
                int dispLines = 0; // variable used and reused elsewhere

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                // yeah, the inputs are hardcoded...
                Console.WriteLine("[ESC] Menu [CTRL]+[ENTER] Insert Message [ENTER] Save Message [DEL] Delete Message [UP/DOWN] Change Selected Message");
                Console.WriteLine("[TAB] Sender/Message [ALT]+[LEFT/RIGHT] Change Message Type [ALT]+[UP/DOWN] Move Message");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Index {MessagesIndex + 1} | " + (string.IsNullOrEmpty(fileName) ? "No File" : fileName));
                // ternaries don't work in variable substitution so we use concat here; not the greatest idea, but it works so

                // Make ABSOLUTE, BLOODY SURE these are within valid values before attempting anything
                // (lesson learnt in blood, and yes, I distrust my own code this much)
                // "Wouldn't you lose a bit of performance?" Yeah I did think of that, but probably doesn't affect performance THAT much
                // (and besides I'd rather not crash, lose the user's data and- yeah...)
                SenderCursorPos = Math.Clamp(SenderCursorPos, 0, senderBuffer.Length);
                MessageCursorPos = Math.Clamp(MessageCursorPos, 0, messageBuffer.Length);
                StylingCursorPos = Math.Clamp(StylingCursorPos, 0, stylingBuffer.Length);

                // probably don't need four spans here and could get away with 2 and just doing [..SenderCursorPos] inline (for example) but I'm mentally done
                ReadOnlySpan<char> senderSpanLeft = senderBuffer.AsSpan()[..SenderCursorPos];
                ReadOnlySpan<char> senderSpanRight = senderBuffer.AsSpan()[SenderCursorPos..];
                ReadOnlySpan<char> messageSpanLeft = messageBuffer.AsSpan()[..MessageCursorPos];
                ReadOnlySpan<char> messageSpanRight = messageBuffer.AsSpan()[MessageCursorPos..];
                ReadOnlySpan<char> stylingSpanLeft = stylingBuffer.AsSpan()[..StylingCursorPos];
                ReadOnlySpan<char> stylingSpanRight = stylingBuffer.AsSpan()[StylingCursorPos..];

                // does a switch work here?
                // "Wait, UnderSet, couldn't you just use some clever tricks and only write the message once?" Yeah, maybe?
                // "Wait, why string.Concat (mostly) here and not normal concatenation?" Because we need to concatenate span values.
                // "Underscore as your cursor looks confusing..." I know... I'm aware of it...
                if (MsgType == MessageType.Received) {
                    // string.Concat has a max of 4 arguments when using spans before it freaks the crap out
                    string dispSendBuf1 = (MessagesIndex + 1) + " >R  ";
                    string dispMesgBuf1 = (MessagesIndex + 1) + string.Concat("     ", stylingSpanLeft);
                    dispLines = (int)Math.Ceiling(
                                    ((decimal)dispSendBuf1.Length
                                     + (decimal)senderBuffer.Length
                                     + (activeBufferType == 0 ? 2 : 1))
                                    / (decimal)width) 
                              + (int)Math.Ceiling(
                                    ((decimal)dispMesgBuf1.Length
                                     + (decimal)senderSpanRight.Length
                                     + (decimal)messageBuffer.Length
                                     + (activeBufferType == 0 ? 1 : 2))
                                    / (decimal)width);
                    freeLines = freeLines - dispLines;

                    // If anyone has a less insane way to do multicolored lines, lemme know
                    Console.ForegroundColor = (activeBufferType == 0 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(dispSendBuf1);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("|");
                    Console.ForegroundColor = (activeBufferType == 0 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(senderSpanLeft);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write((activeBufferType == 0 ? "_" : ""));
                    Console.ForegroundColor = (activeBufferType == 0 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(senderSpanRight);
                    Console.Write(Environment.NewLine);

                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(dispMesgBuf1);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write((activeBufferType == 2 ? "_" : ""));
                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(stylingSpanRight);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("|");
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(messageSpanLeft);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write((activeBufferType == 1 ? "_" : ""));
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Green : ConsoleColor.Gray);
                    Console.Write(messageSpanRight);
                    Console.Write(Environment.NewLine);
                }
                else if (MsgType == MessageType.Sent) {
                    string dispMesgBuf2 = (MessagesIndex + 1) + string.Concat("  S< ", stylingSpanLeft);
                    freeLines = freeLines - (int)Math.Ceiling(
                        ((decimal)dispMesgBuf2.Length 
                         + 2
                         + (decimal)stylingSpanRight.Length
                         + messageBuffer.Length)
                        / (decimal)width);

                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Cyan : ConsoleColor.Gray);
                    Console.Write(dispMesgBuf2);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(activeBufferType == 2 ? "_" : "");
                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Cyan : ConsoleColor.Gray);
                    Console.Write(stylingSpanRight);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("|");
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Cyan : ConsoleColor.Gray);
                    Console.Write(messageSpanLeft);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write((activeBufferType == 1 ? "_" : ""));
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Cyan : ConsoleColor.Gray);
                    Console.Write(messageSpanRight);
                    Console.Write(Environment.NewLine);
                }
                else if (MsgType == MessageType.System) {
                    //string dispMesgBuf = (MessagesIndex + 1) + string.Concat(" -N- |", messageSpanLeft, (activeBufferType == 0 ? "" : "_"), messageSpanRight);
                    string dispMesgBuf2 = (MessagesIndex + 1) + string.Concat(" -N- ", stylingSpanLeft);
                    freeLines = freeLines - (int)Math.Ceiling(
                        ((decimal)dispMesgBuf2.Length
                         + 2
                         + (decimal)stylingSpanRight.Length
                         + messageBuffer.Length)
                        / (decimal)width);
                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Magenta : ConsoleColor.Gray);
                    Console.Write(dispMesgBuf2);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(activeBufferType == 2 ? "_" : "");
                    Console.ForegroundColor = (activeBufferType == 2 ? ConsoleColor.Magenta : ConsoleColor.Gray);
                    Console.Write(stylingSpanRight);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("|");
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Magenta : ConsoleColor.Gray);
                    Console.Write(messageSpanLeft);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write((activeBufferType == 1 ? "_" : ""));
                    Console.ForegroundColor = (activeBufferType == 1 ? ConsoleColor.Magenta : ConsoleColor.Gray);
                    Console.Write(messageSpanRight);
                    Console.Write(Environment.NewLine);
                }

                // Next line previews. Unless you know what you're doing, don't touch this shit.
                // This is a gigantic house of cards where changing a single thing has weird consequences.
                while (freeLines > 1) {
                    peekLines = peekLines + 1;
                    if (MessagesIndex + peekLines < MessagesBuffer.Count) {
                        var nextmsg = MessagesBuffer[MessagesIndex + peekLines];
                        if (nextmsg.Type == MessageType.Received) {
                            //string dispSendHeadBuf = ">R  " + (nextmsg.Styling ?? "");
                            string dispMesgBuf = $"{nextmsg.Styling ?? ""}|{nextmsg.Text}";
                            dispLines = (int)Math.Ceiling(((decimal)nextmsg.Sender.Length + 5) / (decimal)width)
                                + (int)Math.Ceiling(((decimal)dispMesgBuf.Length + 4) / (decimal)width);
                            if (freeLines >= dispLines) {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write(">R  ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write($"|{nextmsg.Sender}");
                                //Console.Write("|");
                                //Console.Write(nextmsg.Sender);
                                Console.Write(Environment.NewLine);
                                Console.Write("    ");
                                Console.Write(dispMesgBuf);
                                Console.Write(Environment.NewLine);
                                freeLines = freeLines - dispLines;
                            }
                            else {
                                freeLines = 0;
                            }
                        }
                        else if (nextmsg.Type == MessageType.Sent) {
                            string dispMesgBuf = (nextmsg.Styling ?? "") + "|" + nextmsg.Text;
                            dispLines = (int)Math.Ceiling(((decimal)dispMesgBuf.Length + 4) / (decimal)width);
                            if (freeLines >= dispLines) {
                                //Console.WriteLine(dispMesgBuf);
                                //freeLines = freeLines - dispLines;
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write(" S< ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write(dispMesgBuf);
                                Console.Write(Environment.NewLine);
                                freeLines = freeLines - dispLines;
                            }
                            else {
                                freeLines = 0;
                            }
                        }
                        else if (nextmsg.Type == MessageType.System) {
                            string dispMesgBuf = (nextmsg.Styling ?? "") + "|" + nextmsg.Text;
                            dispLines = (int)Math.Ceiling(((decimal)dispMesgBuf.Length + 4) / (decimal)width);
                            if (freeLines >= dispLines) {
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.Write("-N- ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write(dispMesgBuf);
                                Console.Write(Environment.NewLine);
                                freeLines = freeLines - dispLines;
                            }
                            else {
                                freeLines = 0;
                            }
                        }
                    }
                    else if (MessagesIndex + peekLines == MessagesBuffer.Count) {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("[Move Down to Create New Message...]");
                        freeLines = 0;
                    }
                    else {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("[Creating New Message]");
                        freeLines = 0;
                    }
                }

                Console.ResetColor();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter) {
                    // Insert a message in between when you press Ctrl+Enter specifically
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        MessagesBuffer.Insert(MessagesIndex, new Message {
                            Sender = senderBuffer,
                            Text = "",
                            Styling = stylingBuffer,
                            Type = MsgType
                        });
                        MessagesIndex += 1;
                        messageBuffer = "";
                    }
                    // Else if you just press Enter regularly...
                    else if (MessagesIndex < MessagesBuffer.Count) {
                        // ...if you're selecting an existing message, simply save it
                        MessagesBuffer[MessagesIndex].Sender = senderBuffer;
                        MessagesBuffer[MessagesIndex].Text = messageBuffer;
                        MessagesBuffer[MessagesIndex].Styling = stylingBuffer;
                        MessagesBuffer[MessagesIndex].Type = MsgType;
                    }
                    else {
                        // ...but if you're creating an existing message, this is where we "commit" it to the list
                        // And yes, that's why we ONLY increment the index in this case (Ctrl+Enter notwithstanding)
                        MessagesBuffer.Add(new Message {
                            Sender = senderBuffer,
                            Text = messageBuffer,
                            Styling = stylingBuffer,
                            Type = MsgType });
                        MessagesIndex += 1;
                        messageBuffer = "";
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete && MessagesIndex < MessagesBuffer.Count) {
                    MessagesBuffer.RemoveAt(MessagesIndex);
                    CorrectPointer();
                }
                else if (keyInfo.Key == ConsoleKey.Escape) {
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Tab) {
                    if (MsgType == MessageType.Received) {
                        activeBufferType = (activeBufferType + 1) % 3;
                    }
                    else {
                        // "What is this fucking ternary?" is a REALLY damn good question.
                        // There's probably a better way to prevent accidentally changing to Sender buffer but allow Styling buffer when in Sent/System message
                        //   modes, but eh, whatever.
                        activeBufferType = activeBufferType == 2 ? 1 : 2;
                    }
                    //TextCursorPos = 0;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow) {
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt)) { 
                        int TargetIndex = MessagesIndex + (keyInfo.Key == ConsoleKey.UpArrow ? -1 : 1);
                        if (0 <= TargetIndex && TargetIndex < MessagesBuffer.Count) {
                            Message MessageHolder = MessagesBuffer[TargetIndex];
                            MessagesBuffer[TargetIndex] = MessagesBuffer[MessagesIndex];
                            MessagesBuffer[MessagesIndex] = MessageHolder;
                            MessagesIndex = TargetIndex;
                        }
                    }
                    else {
                        MessagesIndex += (keyInfo.Key == ConsoleKey.UpArrow ? -1 : 1);
                        CorrectPointer();
                        if (MessagesIndex < MessagesBuffer.Count) {
                            senderBuffer = MessagesBuffer[MessagesIndex].Sender;
                            messageBuffer = MessagesBuffer[MessagesIndex].Text;
                            stylingBuffer = MessagesBuffer[MessagesIndex].Styling ?? "";
                            MsgType = MessagesBuffer[MessagesIndex].Type;
                        }
                        else {
                            messageBuffer = "";
                            stylingBuffer = "";
                        }
                        if (MsgType != MessageType.Received) { activeBufferType = 1; }
                        MessageCursorPos = messageBuffer.Length; // Reset cursor position every time we switch
                        SenderCursorPos = senderBuffer.Length;   // I. Know. What. I'm. Doing.
                        StylingCursorPos = stylingBuffer.Length;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.RightArrow) {
                    // Alt+(Arrow L/R) changes message type...
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt)) {
                        // I know what you're thinking. This bit is insane too.
                        // But holy FUCK if I don't already wanna fucking lose it.
                        if (keyInfo.Key == ConsoleKey.LeftArrow) {
                            MsgType = (MessageType)((MsgType == 0) ? 2 : (int)MsgType - 1);
                        }
                        else if (keyInfo.Key == ConsoleKey.RightArrow) {
                            MsgType = (MessageType)(((int)MsgType == 2) ? 0 : MsgType + 1);
                        }
                        if (MsgType != MessageType.Received) { activeBufferType = 1; }
                    }
                    // ..else manipulate cursor position; this is done very terribly though
                    else {
                        TextCursorPos = Math.Clamp(TextCursorPos + (keyInfo.Key == ConsoleKey.LeftArrow ? -1 : 1), 0, activeBuffer.Length);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace) {
                    // Deleting stuff. Supports regular Backspace and Ctrl+Backspace...
                    TextCursorPos = Math.Clamp(TextCursorPos, 0, activeBuffer.Length);
                    ReadOnlySpan<char> bufferLeft = activeBuffer.AsSpan()[..TextCursorPos];
                    int delNumber = 1;
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        // FIXME: ...except (AFAIK) Ctrl+Backspace currently has an edge case (underscore as cursor here):
                        // If you attempt to use it on something like "cats _are nice pets", Ctrl+Backspace will instead delete it to "c_are nice pets"
                        // I've tried various things to fix it but they either didn't work or had weird side effects so this will have to do
                        bufferLeft = bufferLeft.TrimEnd();
                        int lastSpace = bufferLeft.LastIndexOf(" ");
                        delNumber = bufferLeft.Length - (lastSpace < 0 ? 0 : lastSpace);
                    }
                    if (bufferLeft.Length >= delNumber) {
                        // By some Christmas miracle, this DISASTER works, and I don't wanna think about it
                        activeBuffer = string.Concat(bufferLeft[..(TextCursorPos - delNumber)], activeBuffer.AsSpan()[TextCursorPos..]);

                        // Debugging stuff - breakpoint the if end or TextCursorPos decrement right below and uncomment these lines if you would like
                        // to see the values of bufferLeft directly after performing a delete
                        //Console.WriteLine(bufferLeft);
                        //Console.WriteLine(string.Concat(bufferLeft[..(TextCursorPos - delNumber)], "!"));
                    }
                    TextCursorPos = TextCursorPos - Math.Min(delNumber, TextCursorPos); // correct cursor position after deleting
                }
                else if (char.IsControl(keyInfo.KeyChar)) {
                    // quite literally do NOTHING if it's a control character and is NOT covered by the cases above
                }
                // After checking it's not a control character or controlling input 
                else {
                    activeBuffer = activeBuffer.Insert(TextCursorPos, keyInfo.KeyChar.ToString());
                    TextCursorPos += 1;
                }
            }
        }

        internal void SaveAs() {
            Console.WriteLine($"{Environment.NewLine}Type a filename or file path (leave empty to cancel):");
            string TempFileName = Console.ReadLine() ?? "";

            if (TempFileName != "") {
                try {
                    string MessagesJSON = JsonSerializer.Serialize(MessagesBuffer, AppJsonContext.Default.ListMessage);
                    File.WriteAllText(TempFileName, MessagesJSON);

                    Console.WriteLine("File saved successfully.");
                    fileName = TempFileName;
                }
                catch (UnauthorizedAccessException) {
                    Console.WriteLine("Permission denied: Cannot write here.");
                }
                catch (System.Text.Json.JsonException ex) {
                    Console.WriteLine($"JSON conversion failed: {ex.Message}");
                }
                catch (IOException ex) {
                    Console.WriteLine($"I/O exception: {ex.Message}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            else {
                Console.WriteLine("Saving canceled.");
            }

            Console.WriteLine("Press any key to continue.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        }

        // You might have noticed this is similar to SaveAs() for the most part.
        // Look, I'm not making SaveAs() do double duty, okay? See Program.CS L79-93 (as of time of comment).
        internal void Save() {
            Console.WriteLine($"{Environment.NewLine}");
            try {
                string MessagesJSON = JsonSerializer.Serialize(MessagesBuffer, AppJsonContext.Default.ListMessage);
                // fileName*!* suppresses compiler warning; this is dealt with in logic by calling SaveAs() instead if
                // there's no fileName (active file name)
                File.WriteAllText(fileName!, MessagesJSON);

                Console.WriteLine("File saved successfully.");
            }
            catch (UnauthorizedAccessException) {
                Console.WriteLine("Permission denied: Cannot write here.");
            }
            catch (System.Text.Json.JsonException ex) {
                Console.WriteLine($"JSON conversion failed: {ex.Message}");
            }
            catch (IOException ex) {
                Console.WriteLine($"I/O exception: {ex.Message}");
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to continue.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        }

        internal void Load() {
            Console.WriteLine($"{Environment.NewLine}Type a filename or file path: (leave empty to cancel)");
            string TempFileName = Console.ReadLine() ?? "";

            if (TempFileName != "" && !File.Exists(TempFileName)) {
                Console.WriteLine("File does not exist. Double check your entered path/filename.");
            }
            else if (TempFileName != "") {
                try {
                    string MessagesJSON = File.ReadAllText(TempFileName);
                    #pragma warning disable CS8601 // Possible null reference assignment.
                                                   // This part will work fine even if ReadAllText returns an empty file.
                                                   // There's already a check in place right above for if file doesn't exist.
                    MessagesBuffer = JsonSerializer.Deserialize(MessagesJSON, AppJsonContext.Default.ListMessage);
                    #pragma warning restore CS8601

                    Console.WriteLine("File loaded successfully.");
                    fileName = TempFileName;
                }
                catch (UnauthorizedAccessException) {
                    Console.WriteLine("Permission denied: Cannot read here.");
                }
                catch (System.Text.Json.JsonException ex) {
                    Console.WriteLine($"JSON conversion failed: {ex.Message}");
                }
                catch (IOException ex) {
                    Console.WriteLine($"I/O exception: {ex.Message}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            else {
                Console.WriteLine("File load canceled.");
            }

            Console.WriteLine("Press any key to continue.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

        }

        private void CorrectPointer() {
            MessagesIndex = Math.Clamp(MessagesIndex, 0, MessagesBuffer.Count);
        }

        // Looking back, I'm not sure what the hell this is...
        //private static (ReadOnlySpan<char> Left, ReadOnlySpan<char> Right) SplitBuffer(string text, int cursorPos) {
        //    ReadOnlySpan<char> span = text.AsSpan();
        //    return (span[..cursorPos], span[cursorPos..]);
        //}
    }

    // Data structure used for the individual message elements in the MessageBuffer List.
    // Serializes into JSON as:
    // {
    //     "Sender": (message sender name),
    //     "Text": (message text),
    //     "Type": 1
    // },
    // Also, Sender is ALWAYS saved in the editor's output JSON even if type is Received/System (aka Neutral), however is ignored when converting.

    public class Message
    {
        public required string Sender { get; set; }
        // Converter (?) treats sender name special.
        // Specifically, the format for non-matching sender name and profile image is: (Sender name)|(Profile image)
        // I've seen a few MomoTalk works here and there have a need for that, and that was a way of getting it working that I personally think is both
        //   simple to parse and easy to understand at a glance, so I'm keeping that format all the way from the original.
        public required string Text { get; set; }
        public MessageType Type { get; set; } // 0 is Received, 1 is Sent, 2 is System/Neutral (think the middle aligned gray bubbles)
        public string? Styling { get; set; }
    }

    // Required to get JSON serialization working when AOT compiling, which I want to make possible and is how I prefer to build/deploy this outside of
    // debugging.
    // (AOT compilation does not require a .NET runtime, which significantly reduces barrier to entry (much as something like this even has one).)
    // And yeah, I'm aware that .NET has singlefile builds with runtime bundled in (self-contained is the proper term).
    // Feel free to build this that way! I might reconsider that approach myself.
    [JsonSerializable(typeof(Message))]
    [JsonSerializable(typeof(List<Message>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class AppJsonContext : JsonSerializerContext {
    }
}