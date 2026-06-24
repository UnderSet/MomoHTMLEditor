using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                Console.WriteLine("[ESC] Menu [TAB] Sender/Message [ALT]+[UP/DOWN] Move Message");
                Console.WriteLine("Type normally to enter text");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Index {MessagesIndex + 1} | {fileName}");

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
                        Console.WriteLine("[New Message...]");
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
}