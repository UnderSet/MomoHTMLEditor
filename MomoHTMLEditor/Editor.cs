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
        private MessageType Type = MessageType.Received;
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
                Console.Clear();
                //for (int i = 0; i < MessagesBuffer.Count; i++) {
                //    Console.WriteLine($"{i} - {MessagesBuffer[i].Text}");
                //}
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[ESC] Menu [TAB] Sender/Message [ALT]+[UP/DOWN] Move Message");
                Console.WriteLine("Type normally to enter text");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Index {MessagesIndex} | {fileName}");
                Console.ResetColor();
                Console.WriteLine(senderBuffer);
                Console.WriteLine(messageBuffer);
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter) {
                    if (MessagesIndex < MessagesBuffer.Count) {
                        MessagesBuffer[MessagesIndex].Sender = senderBuffer;
                        MessagesBuffer[MessagesIndex].Text = messageBuffer;
                    }
                    else {
                        MessagesBuffer.Add(new Message {
                            Sender = senderBuffer,
                            Text = messageBuffer,
                            Type = MessageType.Sent });
                        MessagesIndex += 1;
                        messageBuffer = "";
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape) {
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Tab) {
                    activeSenderBuffer = !activeSenderBuffer;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow) {
                    MessagesIndex += (keyInfo.Key == ConsoleKey.UpArrow ? -1 : 1);
                    CorrectPointer();
                    if (MessagesIndex < MessagesBuffer.Count) {
                        senderBuffer = MessagesBuffer[MessagesIndex].Sender;
                        messageBuffer = MessagesBuffer[MessagesIndex].Text;
                    }
                }
                //else if (keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.RightArrow) {
                //}
                else if (keyInfo.Key == ConsoleKey.Backspace) {
                    int delNumber = 1;
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        activeBuffer = activeBuffer.TrimEnd();
                        int lastSpace = activeBuffer.LastIndexOf(" ", Math.Max(0, activeBuffer.Length));
                        delNumber = activeBuffer.Length - (lastSpace < 0 ? 0 : lastSpace + 1);
                    }
                    //else {

                    //}

                    //activeBuffer.Remove(activeBuffer.Length - 1, 1);
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