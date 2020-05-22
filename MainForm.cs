using System;
using System.Runtime.InteropServices;
using ConsoleAPI;

namespace 爬虫
{
    class MainForm
    {
        #region Win API
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        const int STD_OUTPUT_HANDLE = -11;
        #endregion

        public delegate bool SetConsoleDisplayMode(IntPtr hOut, int dwNewMode, out int lpdwOldMode);

        static void Main(string[] args)
        {
            DllInvoke dll = new DllInvoke("kernel32.dll");
            //标准输出句柄
            IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);

            //调用Win API,设置屏幕最大化
            SetConsoleDisplayMode s = (SetConsoleDisplayMode)dll.Invoke("SetConsoleDisplayMode", typeof(SetConsoleDisplayMode));
            //全屏
            //s(hOut, 1, out int dwOldMode);
            Console.Title = "论坛博客爬虫";
            Console.WindowWidth = 150;
            //Console.WindowHeight = 34;

            Run();
        }

        private static void Run()
        {
            try
            {
                var resultNumber = 0;
                PrintMenu();
                var number = Console.ReadLine();
                switch (number)
                {
                    case "1":
                        resultNumber = new CnblogsUpdater().CnRun();
                        break;
                    case "2":
                        resultNumber = new PoJieUpdater().Run();
                        break;
                    default:
                        return;
                }
                if (resultNumber == 1)
                {
                    Run();
                }
                if (resultNumber == 0)
                {
                    PrintReStart("请按Enter键返回菜单,按其它键退出程序：");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("解析出错：" + ex);
                Console.ReadKey();
            }
        }

        //主菜单
        private static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("                                                                   『简单爬虫』");
            Console.WriteLine("                                                              ________________________");
            Console.WriteLine("                                                             |                        |");
            Console.WriteLine("                                                             |  请输入对应编号选择    |");
            Console.WriteLine("                                                             |  1.获取博客园数据      |");
            Console.WriteLine("                                                             |  2.获取吾爱破解数据    |");
            Console.WriteLine("                                                             |  按回车键退出程序      |");
            Console.WriteLine("                                                             |________________________|");
        }

        private static void PrintReStart(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg);
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Run();
            }
        }

        private static void ReStart()
        {
            PrintReStart("现在，请您按Enter键返回菜单,按其它键退出程序：");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Run();
            }
        }
    }
}
