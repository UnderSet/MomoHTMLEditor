using System;
using System.Collections.Generic;
using System.Text;

namespace MomoHTMLEditor
{
    public enum MessageType { Received, Sent, System }
    public class Editor
    {
        private string? fileName; // handle this being NULL...OR ELSE
        private List<Message> MessagesBuffer = new List<Message>();
        public void Engine()
        {
            while (true)
            {
                Console.Clear();
                for (int i = 0; i < MessagesBuffer.Count; i++) {
                    Console.WriteLine($"{i} - {MessagesBuffer[i].Text}");
                }
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