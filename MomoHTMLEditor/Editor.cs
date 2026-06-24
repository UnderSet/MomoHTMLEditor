using System;
using System.Collections.Generic;
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
                Console.WriteLine("[ESC] Menu [TAB] Sender/Message");
                Console.WriteLine("Type normally to enter text");
                Console.WriteLine(senderBuffer);
                Console.WriteLine(messageBuffer);
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter) {
                    MessagesBuffer.Add(new Message {
                        Sender = "",
                        Text = "",
                        Type = MessageType.Sent });
                }
                else if ( keyInfo.Key == ConsoleKey.Escape ) {
                    break;
                }
                else if ( keyInfo.Key == ConsoleKey.Tab)
                {
                    activeSenderBuffer = !activeSenderBuffer;
                }
                else if (char.IsControl(keyInfo.KeyChar)) {
                    // quite literally do NOTHING if it's a control character and is NOT covered by the cases above
                }
                else {
                    activeBuffer = activeBuffer.Insert(activeBuffer.Length, keyInfo.KeyChar.ToString());
                }
            }
        }
    }
    public class Message
    {
        public required string Text { get; set; }
        public required string Sender { get; set; }
        public MessageType Type { get; set; }
    }
}