using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        private MessageType MsgType = MessageType.Received;
        private string activeBuffer
        {
            get => activeSenderBuffer ? senderBuffer : messageBuffer;
            set
            {
                if (activeSenderBuffer) senderBuffer = value;
                else messageBuffer = value;
            }
        }
        public void Engine()
        {
            while (true)
            {
                int width = Console.WindowWidth;
                int freeLines = Console.WindowHeight - 3;
                int peekLines = 0;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[ESC] Menu [TAB] Sender/Message [ALT]+[UP/DOWN] Move Message [ALT]+[LEFT/RIGHT] Change Message Type");
                Console.WriteLine("Type normally to enter text");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Index {MessagesIndex + 1} | " + (string.IsNullOrEmpty(fileName) ? "No File" : fileName));

                // does a switch work here?
                if (MsgType == MessageType.Received) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string dispSendBuf = (MessagesIndex + 1) + " >R |" + senderBuffer + (activeSenderBuffer ? "_" : "");
                    string dispMesgBuf = (MessagesIndex + 1) + " >  |" + messageBuffer + (activeSenderBuffer ? "" : "_");
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispSendBuf.Length / (decimal)width);
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                    Console.WriteLine(dispSendBuf);
                    Console.WriteLine(dispMesgBuf);
                }
                else if (MsgType == MessageType.Sent ) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    string dispMesgBuf = (MessagesIndex + 1) + "  S<|" + messageBuffer + (activeSenderBuffer ? "" : "_");
                    freeLines = freeLines - (int)Math.Ceiling((decimal)dispMesgBuf.Length / (decimal)width);
                    Console.WriteLine(dispMesgBuf);
                }
                else if (MsgType == MessageType.System) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string dispMesgBuf = (MessagesIndex + 1) + " -N-|" + messageBuffer + (activeSenderBuffer ? "" : "_");
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
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow) {
                    MessagesIndex += (keyInfo.Key == ConsoleKey.UpArrow ? -1 : 1);
                    CorrectPointer();
                    if (MessagesIndex < MessagesBuffer.Count) {
                        senderBuffer = MessagesBuffer[MessagesIndex].Sender;
                        messageBuffer = MessagesBuffer[MessagesIndex].Text;
                        MsgType = MessagesBuffer[MessagesIndex].Type;
                    }
                    if (MsgType != MessageType.Received) { activeSenderBuffer = false; }
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
                }
                else if (keyInfo.Key == ConsoleKey.Backspace) {
                    int delNumber = 1;
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        activeBuffer = activeBuffer.TrimEnd();
                        int lastSpace = activeBuffer.LastIndexOf(" ", Math.Max(0, activeBuffer.Length));
                        delNumber = activeBuffer.Length - (lastSpace < 0 ? 0 : lastSpace + 1);
                    }
                    if (activeBuffer.Length >= delNumber) {
                        activeBuffer = activeBuffer.Substring(0, activeBuffer.Length - delNumber);
                    }
                }
                else if (char.IsControl(keyInfo.KeyChar)) {
                    // quite literally do NOTHING if it's a control character and is NOT covered by the cases above
                }
                else {
                    activeBuffer = activeBuffer.Insert(activeBuffer.Length, keyInfo.KeyChar.ToString());
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