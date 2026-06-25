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
        public string? fileName; // handle this being NULL...OR ELSE
        private List<Message> MessagesBuffer = new List<Message>();
        private string senderBuffer = "";
        private string messageBuffer = "";
        private bool activeSenderBuffer = false;
        private int MessagesIndex = 0;
        private int MessageCursorPos = 0;
        private int SenderCursorPos = 0;
        private MessageType MsgType = MessageType.Received;
        private string activeBuffer
        {
            get => activeSenderBuffer ? senderBuffer : messageBuffer;
            set
            {
                if (activeSenderBuffer) senderBuffer = value;
                else messageBuffer = value;
            }
        } // little method to easily get the actual active buffer and not duplicate lots of code
          // (see MomoHTMLEditor.Editor.Engine; if not for this that'd be double the input codebase at best)
        private int TextCursorPos {
            get => activeSenderBuffer ? SenderCursorPos : MessageCursorPos;
            set {
                if (activeSenderBuffer) SenderCursorPos = value;
                else MessageCursorPos = value;
            }
        } // little method to easily get the actual active buffer and not duplicate lots of code

        public void Engine()
        {
            while (true)
            {
                int width = Console.WindowWidth;
                int freeLines = Console.WindowHeight - 3;
                int peekLines = 0;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[ESC] Menu [ENTER] Save Current Message [DEL] Delete Current Message [UP/DOWN] Change Selected Message");
                Console.WriteLine("[TAB] Sender/Message [ALT]+[LEFT/RIGHT] Change Message Type [ALT]+[UP/DOWN] Move Message");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Index {MessagesIndex + 1} | " + (string.IsNullOrEmpty(fileName) ? "No File" : fileName));

                // Make ABSOLUTE, BLOODY SURE these are within valid values before attempting anything
                // (lesson learnt in blood, and yes, I distrust my own code this much)
                SenderCursorPos = Math.Clamp(SenderCursorPos, 0, senderBuffer.Length);
                MessageCursorPos = Math.Clamp(MessageCursorPos, 0, messageBuffer.Length);

                // probably don't need four spans here and could get away with 2 and just doing [..SenderCursorPos] inline (for example) but I'm mentally done
                ReadOnlySpan<char> senderSpanLeft = senderBuffer.AsSpan()[..SenderCursorPos];
                ReadOnlySpan<char> senderSpanRight = senderBuffer.AsSpan()[SenderCursorPos..];
                ReadOnlySpan<char> messageSpanLeft = messageBuffer.AsSpan()[..MessageCursorPos];
                ReadOnlySpan<char> messageSpanRight = messageBuffer.AsSpan()[MessageCursorPos..];

                // does a switch work here?
                if (MsgType == MessageType.Received) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    
                    // "Why are you concatenating with + instead of inside the string.Concat?"
                    // If you've got a better way to workaround the compiler error here, be my guest!
                    string dispSendBuf = (MessagesIndex + 1) + string.Concat(" >R |", senderSpanLeft, (activeSenderBuffer ? "_" : ""), senderSpanRight);
                    string dispMesgBuf = (MessagesIndex + 1) + string.Concat(" >R |", messageSpanLeft, (activeSenderBuffer ? "" : "_"), messageSpanRight);
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispSendBuf.Length / (decimal)width);
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                    Console.WriteLine(dispSendBuf);
                    Console.WriteLine(dispMesgBuf);
                }
                else if (MsgType == MessageType.Sent ) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    string dispMesgBuf = (MessagesIndex + 1) + string.Concat("  S<|", messageSpanLeft, (activeSenderBuffer ? "" : "_"), messageSpanRight);
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                    Console.WriteLine(dispMesgBuf);
                }
                else if (MsgType == MessageType.System) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string dispMesgBuf = (MessagesIndex + 1) + string.Concat(" -N-|", messageSpanLeft, (activeSenderBuffer ? "" : "_"), messageSpanRight);
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                    Console.WriteLine(dispMesgBuf);
                }

                while (freeLines > 1) {
                    peekLines = peekLines + 1;
                    if (MessagesIndex + peekLines < MessagesBuffer.Count) {
                        var nextmsg = MessagesBuffer[MessagesIndex + peekLines];
                        if (nextmsg.Type == MessageType.Received) {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            string dispSendBuf = ">R |" + nextmsg.Sender;
                            string dispMesgBuf = ">  |" + nextmsg.Text;
                            freeLines = freeLines - (int)Math.Ceiling((decimal)dispSendBuf.Length / (decimal)width);
                            freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                            Console.WriteLine(dispSendBuf);
                            Console.WriteLine(dispMesgBuf);
                        }
                        else if (nextmsg.Type == MessageType.Sent) {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            string dispMesgBuf = " S<|" + nextmsg.Text;
                            freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                            Console.WriteLine(dispMesgBuf);
                        }
                        else if (nextmsg.Type == MessageType.System) {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            string dispMesgBuf = "-N-|" + nextmsg.Text;
                            freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                            Console.WriteLine(dispMesgBuf);
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
                        // we need to do THIS or the program will softlock
                        // (while freeLines > 1) - 1 is for the cursor line since we use Console.WriteLine
                    }
                }

                Console.ResetColor();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter) {
                    if (MessagesIndex < MessagesBuffer.Count) {
                        MessagesBuffer[MessagesIndex].Sender = senderBuffer;
                        MessagesBuffer[MessagesIndex].Text = messageBuffer;
                        MessagesBuffer[MessagesIndex].Type = MsgType;
                    }
                    else {
                        MessagesBuffer.Add(new Message {
                            Sender = senderBuffer,
                            Text = messageBuffer,
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
                        activeSenderBuffer = !activeSenderBuffer;
                    }
                    else {
                        activeSenderBuffer = false;
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
                            MsgType = MessagesBuffer[MessagesIndex].Type;
                        }
                        else {
                            messageBuffer = "";
                        }
                        if (MsgType != MessageType.Received) { activeSenderBuffer = false; }
                        MessageCursorPos = messageBuffer.Length; // Reset cursor position every time we switch
                        SenderCursorPos = senderBuffer.Length;   // I. Know. What. I'm. Doing.
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.RightArrow) {
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt)) {
                        // I know what you're thinking. This bit is insane too.
                        // But holy FUCK if I don't already wanna fucking lose it.
                        if (keyInfo.Key == ConsoleKey.LeftArrow) {
                            MsgType = (MessageType)((MsgType == 0) ? 2 : (int)MsgType - 1);
                        }
                        else if (keyInfo.Key == ConsoleKey.RightArrow) {
                            MsgType = (MessageType)(((int)MsgType == 2) ? 0 : MsgType + 1);
                        }
                        if (MsgType != MessageType.Received) { activeSenderBuffer = false; }
                    }
                    else {
                        // INSANELY dangerous trick for cursor pos manipulation
                        // You would not believe my face when I first did this shit
                        TextCursorPos = Math.Clamp(TextCursorPos + (keyInfo.Key == ConsoleKey.LeftArrow ? -1 : 1), 0, activeBuffer.Length);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace) {
                    // TODO: needs a rework after cursor feature add
                    TextCursorPos = Math.Clamp(TextCursorPos, 0, activeBuffer.Length);
                    ReadOnlySpan<char> bufferLeft = activeBuffer.AsSpan()[..TextCursorPos];
                    int delNumber = 1;
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        bufferLeft = bufferLeft.TrimEnd();
                        int lastSpace = bufferLeft.LastIndexOf(" ");
                        delNumber = bufferLeft.Length - (lastSpace < 0 ? 0 : lastSpace);
                    }
                    if (bufferLeft.Length >= delNumber) {
                        // By some Christmas miracle, this DISASTER works, and I don't wanna think about it
                        activeBuffer = string.Concat(bufferLeft[..(TextCursorPos - delNumber)], activeBuffer.AsSpan()[TextCursorPos..]);
                        Console.WriteLine(bufferLeft);
                        Console.WriteLine(string.Concat(bufferLeft[..(TextCursorPos - delNumber)], "!"));
                    }
                    TextCursorPos = TextCursorPos - delNumber; // correct cursor position after deleting
                }
                else if (char.IsControl(keyInfo.KeyChar)) {
                    // quite literally do NOTHING if it's a control character and is NOT covered by the cases above
                }
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

        //private static (ReadOnlySpan<char> Left, ReadOnlySpan<char> Right) SplitBuffer(string text, int cursorPos) {
        //    ReadOnlySpan<char> span = text.AsSpan();
        //    return (span[..cursorPos], span[cursorPos..]);
        //}
    }
    public class Message
    {
        public required string Sender { get; set; }
        public required string Text { get; set; }
        public MessageType Type { get; set; }
    }

    [JsonSerializable(typeof(Message))]
    [JsonSerializable(typeof(List<Message>))]
    public partial class AppJsonContext : JsonSerializerContext {
    }
}