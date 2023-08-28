using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace StockExchangeClient
{
    class Client
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("localhost", 3000);
            Console.WriteLine("Connected to server.");

            NetworkStream stream = client.GetStream();

            // Request all packets
            byte[] streamAllPacketsPayload = CreateRequestPayload(1, 0);
            stream.Write(streamAllPacketsPayload, 0, streamAllPacketsPayload.Length);

            List<Dictionary<string, object>> collectedPackets = new List<Dictionary<string, object>>();

            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Process the received data and extract packets
                byte[] receivedData = new byte[bytesRead];
                Array.Copy(buffer, receivedData, bytesRead);

                ProcessReceivedData(receivedData, collectedPackets);
            }

            stream.Close();
            client.Close();
            Console.WriteLine("Connection closed.");

            // Create and store JSON output file
            string jsonOutput = JsonConvert.SerializeObject(collectedPackets, Formatting.Indented);
            File.WriteAllText("collected_packets.json", jsonOutput);
            Console.WriteLine("JSON file 'collected_packets.json' created and stored.");
        }

        static void ProcessReceivedData(byte[] data, List<Dictionary<string, object>> collectedPackets)
        {
            
            int offset = 0;
            while (offset < data.Length)
            {
                string symbol = System.Text.Encoding.ASCII.GetString(data, offset, 4);
                char buySellIndicator = (char)data[offset + 4];
                int quantity = BitConverter.ToInt32(data, offset + 5);
                int price = BitConverter.ToInt32(data, offset + 9);
                int packetSequence = BitConverter.ToInt32(data, offset + 13);

                Dictionary<string, object> packet = new Dictionary<string, object>
                {
                    { "symbol", symbol },
                    { "buySellIndicator", buySellIndicator },
                    { "quantity", quantity },
                    { "price", price },
                    { "packetSequence", packetSequence }
                };

                collectedPackets.Add(packet);

                offset += 17;
            }
        }

        static byte[] CreateRequestPayload(int callType, int resendSeq)
        {
            byte[] payload = new byte[2];
            payload[0] = (byte)callType;
            payload[1] = (byte)resendSeq;
            return payload;
        }
    }
}

