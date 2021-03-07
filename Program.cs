using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// Virtual Key Codes:
// https://docs.microsoft.com/ru-ru/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN

namespace Test
{
    class Program
    {
        public static int[] TIMINGS = new int[] { 20, 75, 2, 6 };

        [DllImport("user32.dll", SetLastError = true)] static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")] static extern short VkKeyScan(char ch);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        
        // before start exe, i copying text from speedtyper.dev
        [STAThread]
        static int Main(string[] args)
        {
            // if in clipboard not text we exiting from app
            if(!Clipboard.ContainsText()) return -1;

            // we waiting 4 seconds
            for(int i = 3; i >= 0; i--)
            {
                Console.WriteLine($"{i}...");
                Thread.Sleep(1000);
            }

            var rng = new Random();

            // splitting text into lines, with lines it easier to Trim every line
            var lines = Clipboard.GetText().Split('\n');
            
            foreach(var line in lines)
            {
                // so we trimming the string
                var str = line.Trim();
                
                for(int i = 0; i < str.Length; i++)
                {
                    char ch = str[i];
                    PressKey(ch);
                    // Act like a human
                    Thread.Sleep(rng.Next(TIMINGS[0], TIMINGS[1]) * (ch==' '?rng.Next(TIMINGS[2],TIMINGS[3]):1));

                    // 5% to make a mistake and other checks
                    if(i < str.Length - 3 && str[i + 1] != str[i + 2] && rng.NextDouble() < 0.05)
                    {
                        int typedCount = 0;
                        // we skipping 1 letter and typing next lettering in line
                        for(int j = i + 2; j < str.Length && rng.NextDouble() < 0.5 && typedCount < 8; j++)
                        {
                            PressKey(str[j]);
                            Thread.Sleep(rng.Next(TIMINGS[0], TIMINGS[1]));
                            typedCount++;
                        }

                        // pressing backspaces to delete
                        for(int j = 0; j < typedCount; j++)
                        {
                            keybd_event(0x08, 0, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event(0x08, 0, KEYEVENTF_KEYUP, 0);
                            Thread.Sleep(rng.Next(TIMINGS[0], TIMINGS[1]));
                        }
                    }
                }

                // pressing enter for new line
                keybd_event(0x0D, 0, KEYEVENTF_EXTENDEDKEY, 0);
                keybd_event(0x0D, 0, KEYEVENTF_KEYUP, 0);
                Thread.Sleep(rng.Next(TIMINGS[0], TIMINGS[1]) * rng.Next(TIMINGS[2],TIMINGS[3]));
            }

            return 0;
        }

        private static void PressKey(char ch)
        {
            var vk = VkKeyScan(ch);
            var vkCode = vk & 0xff;
            var shift = (vk & 0x100) > 0;
            
            if(shift) keybd_event(0xA1, 0, KEYEVENTF_EXTENDEDKEY, 0);

            keybd_event((byte)vkCode, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event((byte)vkCode, 0, KEYEVENTF_KEYUP, 0);

            if(shift) keybd_event(0xA1, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}