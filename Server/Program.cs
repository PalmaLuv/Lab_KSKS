using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{

    class SettingServer
    {
        public void CleareDisplay() => Console.Clear();
        public void SetPixel(int x = 0, int y = 0, string color = "white")
        {
            SetColor(color);
            Console.SetCursorPosition(x, y);
            Console.Write("█");
            Console.ResetColor();
        }

        public void SetDrawEllipse(int x, int y, int radius_x, int radius_y, string color = "while")
        {
            int x0 = 0, y0 = radius_x + radius_y;
            int delta = 1 - 2 * (radius_x + radius_y) , error = 0;
            while (y0 >= x0)
            {
                SetPixel(x + x0, y + y0, color);
                SetPixel(x + x0, y - y0, color);
                SetPixel(x - x0, y + y0, color);
                SetPixel(x - x0, y - y0, color);
                SetPixel(x + y0, y + x0, color);
                SetPixel(x + y0, y - x0, color);
                SetPixel(x - y0, y + x0, color);
                SetPixel(x - y0, y - x0, color);
                error = 2 * (delta + y0) - 1;
                if (delta < 0 && error <= 0)
                {
                    delta += 2 * (x0++) + 1;
                    continue;
                }
                else if (delta > 0  && error > 0)
                {
                    delta -= 2 * (y0--) + 1;
                    continue;
                }
                delta += 2 * ((x0++) - (y0--));
            }
        }

        public void SetFillDrawEllipse(int x, int y, int radius_x, int radius_y, string color = "while")
        {
            int x0 = 0, y0 = radius_x + radius_y;
            int delta = 1 - 2 * (radius_x + radius_y), error = 0;
            while (y0 >= x0)
            {
                for (int i = x - x0; i < x + x0; i++)
                    for (int j = y - y0; j < y + y0; j++)
                        SetPixel(i, j, color);
                for (int i = x - y0; i < x + y0; i++)
                    for (int j = y - x0; j < y + x0; j++)
                        SetPixel(i, j, color);
                error = 2 * (delta + y0) - 1;
                if (delta < 0 && error <= 0)
                {
                    delta += 2 * (x0++) + 1;
                    continue;
                }
                else if (delta > 0 && error > 0)
                {
                    delta -= 2 * (y0--) + 1;
                    continue;
                }
                delta += 2 * ((x0++) - (y0--));
            }
        }

        public void SetDrawLine(int x0, int y0, int x1, int y1, string color = "white")
        {
            int delta_x = Math.Abs(x1 - x0), delta_y = Math.Abs(y1 - y0);
            int error = 0;
            int delta_err = (delta_y + 1);
            int y = y0 , diry = y1 - y0;
            diry = diry > 0 ? 1 : -1; 
            for (int x = x0; x < x1; x++)
            {
                SetPixel(x, y, color);
                error = error + delta_err; 
                if (error >= (delta_x + 1))
                {
                    y = y + diry;
                    error = error - (delta_x + 1);
                }
            }
        }

        public void SetFillDrawRectangle(int x, int y, int w, int h, string color = "white")
        {
            for (int i = 0; i < w + 1; i++)
                for (int j = 0; j < h; j++)
                {
                    SetPixel(i + x, j + y, color);
                }
        }

        public void SetDrawRectangle(int x, int y, int w, int h, string color = "white")
        {
            for (int i = 0; i < w + 1; i++)
                for (int j = 0; j < 2; j++)
                {
                    if (j == 0)
                        SetPixel(i + x, y, color);
                    else if (j == 1)
                        SetPixel(i + x, y + h, color);
                }

            for (int i = 0; i < h; i++)
                for (int j = 0; j < 2; j++)
                {
                    if (j == 0)
                        SetPixel(x, i + y, color);
                    else if (j == 1)
                        SetPixel(x + w, i + y, color);
                }
        }

        private void SetColor(string color)
        {
            Type type = typeof(ConsoleColor);
            foreach (var name in Enum.GetNames(type))
                if (color.ToLower() == name.ToLower())
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(type, name);
        }
    }

    class UDPServer
    {
        string serverIP = "127.0.0.1";

        int localPort  = 8262;
        int remotePort;

        public void StartServer()
        {

            var sender = new UdpClient();
            var receiver = new UdpClient(localPort);
            IPEndPoint remoteIP = null;

            try
            {
                Console.WriteLine($"The server is running, its IP : {serverIP} | localPort {localPort} |");
                while (true)
                {
                    byte[] remoteData = receiver.Receive(ref remoteIP);
                    remotePort = remoteIP.Port;
                    string message = Encoding.Unicode.GetString(remoteData);
                    byte[] localData =  Encoding.Unicode.GetBytes(GetCommand(message));
                    sender.Send(localData, localData.Length, remoteIP.Address.ToString(), remotePort);
                }
            }
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
            }
            finally
            {
                sender.Close();
                receiver.Close();
            }
        }

        string commandList = "\n-h --help\t- All available commands.\nclear | cleardisplay\t- Cleaning up the server console.\n" +
                             "drawpixel x y color\t- Draw a pixel in the server console.\n" +
                             "drawline x0 y0 x1 y1 color\t- Draw a line in the server console.\n" +
                             "drawrectangle x y w h color\t- Drawing a rectangle.\n" +
                             "fillrectangle x y w h color\t- Draw a filled rectangle\n" +
                             "drawellipse x y radius_x radius_y color\t- Drawing a ellipse.\n" +
                             "fillellipse x y radius_x radius_y color\t- Draw a filled ellipse.\n";
        string GetCommand(string command)
        {
            var ss = new SettingServer();
            string[] split = command.Split(new Char[] {'"',' '});
            if (split[0].ToLower() == "--help" || split[0].ToLower() == "-h")
            {
                return commandList;
            }
            else if (split[0].ToLower() == "clear" || split[0].ToLower() == "cleardisplay")
            {
                ss.CleareDisplay();
                return "Command completed.";
            }
            else if (split[0].ToLower() == "drawpixel")
            {
                if (split[1].ToLower() == "-h") 
                {
                    if (2 < split.Length && split[2].ToLower() == "color")
                    {
                        string ColorList = null;
                        Type type = typeof(ConsoleColor);
                        foreach (var name in Enum.GetNames(type))
                        {
                            ColorList += name + " ";
                        }
                        return ColorList;
                    }
                    return "drawpixel -m x y color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2], out _) && int.TryParse(split[3], out _))
                    {
                        if (4 < split.Length)
                        {
                            ss.SetPixel(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), split[4]);
                        }
                        else if (3 < split.Length)
                            ss.SetPixel(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            else if (split[0].ToLower() == "drawline")
            {
                if (split[1].ToLower() == "-h")
                {
                    if (2 < split.Length && split[2].ToLower() == "color")
                    {
                        string ColorList = null;
                        Type type = typeof(ConsoleColor);
                        foreach (var name in Enum.GetNames(type))
                        {
                            ColorList += name + " ";
                        }
                        return ColorList;
                    }
                    return "drawline -m x0 y0 x1 y1 color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2], out _) && int.TryParse(split[3], out _) && int.TryParse(split[4], out _) && int.TryParse(split[5], out _))
                    {
                        if (6 < split.Length)
                        {
                            ss.SetDrawLine(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), split[6]);
                        }
                        else if (5 < split.Length)
                            ss.SetDrawLine(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            else if (split[0].ToLower() == "drawrectangle")
            {
                if (split[1].ToLower() == "-h")
                {
                    if (2 < split.Length)
                        if (split[2].ToLower() == "color")
                        {
                            string ColorList = null;
                            Type type = typeof(ConsoleColor);
                            foreach (var name in Enum.GetNames(type))
                            {
                                ColorList += name + " ";
                            }
                            return ColorList;
                        }
                    return "drawrectangle -m x y w h color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2] , out _) && int.TryParse(split[3] , out _) && int.TryParse(split[4] , out _) && int.TryParse(split[5] , out _))
                    {
                        if (6 < split.Length)
                            ss.SetDrawRectangle(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), split[6]);
                        else if (5 < split.Length)
                            ss.SetDrawRectangle(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            else if (split[0].ToLower() == "fillrectangle")
            {
                if (split[1].ToLower() == "-h")
                {
                    if (2 < split.Length)
                        if (split[2].ToLower() == "color")
                        {
                            string ColorList = null;
                            Type type = typeof(ConsoleColor);
                            foreach (var name in Enum.GetNames(type))
                            {
                                ColorList += name + " ";
                            }
                            return ColorList;
                        }
                    return "fillrectangle -m x y w h color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2], out _) && int.TryParse(split[3], out _) && int.TryParse(split[4], out _) && int.TryParse(split[5], out _))
                    {
                        if (6 < split.Length)
                            ss.SetFillDrawRectangle(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), split[6]);
                        else if (5 < split.Length)
                            ss.SetFillDrawRectangle(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            else if (split[0].ToLower() == "drawellipse")
            {
                if (split[1].ToLower() == "-h")
                {
                    if (2 < split.Length)
                        if (split[2].ToLower() == "color")
                        {
                            string ColorList = null;
                            Type type = typeof(ConsoleColor);
                            foreach (var name in Enum.GetNames(type))
                            {
                                ColorList += name + " ";
                            }
                            return ColorList;
                        }
                    return "drawellipse -m x y radius_x radius_y color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2], out _) && int.TryParse(split[3], out _) && int.TryParse(split[4], out _) && int.TryParse(split[5], out _))
                    {
                        if (6 < split.Length)
                            ss.SetDrawEllipse(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), split[6]);
                        else if (5 < split.Length)
                            ss.SetDrawEllipse(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            else if (split[0].ToLower() == "fillellipse")
            {
                if (split[1].ToLower() == "-h")
                {
                    if (2 < split.Length)
                        if (split[2].ToLower() == "color")
                        {
                            string ColorList = null;
                            Type type = typeof(ConsoleColor);
                            foreach (var name in Enum.GetNames(type))
                            {
                                ColorList += name + " ";
                            }
                            return ColorList;
                        }
                    return "fillellipse -m x y radius_x radius_y color";
                }
                else if (split[1].ToLower() == "-m")
                {
                    if (int.TryParse(split[2], out _) && int.TryParse(split[3], out _) && int.TryParse(split[4], out _) && int.TryParse(split[5], out _))
                    {
                        if (6 < split.Length)
                            ss.SetFillDrawEllipse(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), split[6]);
                        else if (5 < split.Length)
                            ss.SetFillDrawEllipse(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]));
                        return "Command completed.";
                    }
                }
                return "Something went wrong.";
            }
            return "There is no such command.";
        }
    }

    class Programs
    {
        static void Main()
        {
            UDPServer server = new UDPServer();
            server.StartServer();
        }
    }
}