using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ProjektGrupowy
{
    class Program
    {

        class Terminal
        {
            public String IPAddress { get; }
            public String Username { get; }
            public String Password { get; }
            
            public Terminal(String[] Line)
            {
                this.IPAddress = Line[0];
                if (Line.Length == 2)
                {
                    this.Username = "admin";
                    this.Password = Line[1];
                }
                else
                {
                    this.Username = Line[1];
                    this.Password = Line[2];

                }
            } 
                       
        }
        
        private static string Parser(String input, int mode)
        {
            String output = null;
            string[] lines = input.Replace("\r", "").Split('\n');

            if (mode == 1)
            {
                bool flag = false;
                
                foreach (String line in lines)
                {
                    if (line.Contains("#") && flag) flag = false;
                    if (line.StartsWith("Device ID") || flag)
                    {
                        flag = true;
                        output = output + line + "\r\n";
                    }
                }
            }
            if (mode == 2)
            {
                String temp = null;
                
                foreach(String line in lines)
                {
                    if (line.StartsWith("cisco"))
                    {
                        temp = line;
                    }
                }
                temp = temp.Substring(6);
                char[] t = temp.ToCharArray();
                int i = 0;
                int index = 0;
                do
                {
                    if (t[index] == '-') i++;
                    index++;
                } while (i != 2);
                char[] o = new char[index];
                for (int j = 0; j < index-1; j++) o[j] = t[j];
                temp = new string(o);
                output = temp;
            }
            return output;
        }

        static void Main(string[] args)
        {
            List<Terminal> TerminalsList = new List<Terminal>();
            Console.WriteLine("Welcome.");
            try
            {
                Console.WriteLine("Reading input file...");
                String[] input = System.IO.File.ReadAllLines(@"input.txt");

                foreach (String line in input)
                {
                    String[] TerminalInfo = line.Split(' ');
                    TerminalsList.Add(new Terminal(TerminalInfo));
                }
                Console.WriteLine();
                Console.WriteLine("Initiating program for IP addresses:");
                Console.WriteLine();
                foreach (Terminal term in TerminalsList)
                {
                    System.Console.WriteLine(term.IPAddress);
                }

                String timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                foreach (Terminal term in TerminalsList)
                {
                    String IP = term.IPAddress;
                    TcpClient tcpClient;
                    Console.Write("Trying to connect to IP address: " + IP + "    ");
                    try
                    {
                        tcpClient = new TcpClient(IP, 23);
                        Console.WriteLine("Connected");
                        NetworkStream netStream = tcpClient.GetStream();

                        Byte[] sendBytes = Encoding.UTF8.GetBytes(term.Password + "\r\nenable\r\n" + term.Password + "\r\nterminal length 0\r\nshow version\r\nshow cdp neighbor\r\n");

                        netStream.Write(sendBytes, 0, sendBytes.Length);
                        byte[] bytes = new byte[50000];

                        System.Threading.Thread.Sleep(2000);
                        netStream.Read(bytes, 0, 50000);

                        String version = Parser(Encoding.UTF8.GetString(bytes), 2);
                        String parsed = Parser(Encoding.UTF8.GetString(bytes),1);
                        tcpClient.Close();
                        System.IO.File.AppendAllText(@"output" + timestamp + ".txt", term.IPAddress + " - "+ version + "\r\n" + parsed + "\r\n****************************************************************************\r\n");
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        Console.WriteLine("Could not connect.");
                        System.IO.File.AppendAllText(@"output" + timestamp + ".txt", term.IPAddress + "\r\n" + "Not Connected" + "\r\n****************************************************************************\r\n");
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Output saved to file output" + timestamp + ".txt");
                Console.WriteLine();
                
            }
            catch(System.IO.FileNotFoundException e)
            {
                Console.WriteLine("Could not read input file 'input.txt'");
            }
            Console.WriteLine("Program finished. Press any key to exit.");
            Console.ReadKey();
            }
    }
}