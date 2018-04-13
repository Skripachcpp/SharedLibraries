using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTools.Extensions;

namespace SL.ConsoleWriter
{
    public class Meny
    {
        public static void WriteMenu(
            string n1, Action a1,
            string n2 = null, Action a2 = null,
            string n3 = null, Action a3 = null,
            string n4 = null, Action a4 = null,
            string n5 = null, Action a5 = null,
            string n6 = null, Action a6 = null,
            string n7 = null, Action a7 = null,
            string n8 = null, Action a8 = null,
            string n9 = null, Action a9 = null
        )
        {
            var items = new string[9];
            items[0] = n1;
            items[1] = n2;
            items[2] = n3;
            items[3] = n4;
            items[4] = n5;
            items[5] = n6;
            items[6] = n7;
            items[7] = n8;
            items[8] = n9;

            var key = WriteMenu(items);

            if (key == 1) a1?.Invoke();
            else if (key == 2) a2?.Invoke();
            else if (key == 3) a3?.Invoke();
            else if (key == 4) a4?.Invoke();
            else if (key == 5) a5?.Invoke();
            else if (key == 6) a6?.Invoke();
            else if (key == 7) a7?.Invoke();
            else if (key == 8) a8?.Invoke();
            else if (key == 9) a9?.Invoke();
        }


        public static void WriteMenu(params MenyItem[] items) => WriteMenu(true, items);
        public static void WriteMenu(bool? autoRedraw, params MenyItem[] items)
        {
            var displeyValues = items.Select(a => a.DispleyValue).ToArray();
            var key = WriteMenu(autoRedraw, null, displeyValues);

            if (key == null) return;
            items.Gv(key.Value - 1)?.Action?.Invoke();
        }


        public static int? WriteMenu(params string[] items) => WriteMenu(null, null, items);
        public static int? WriteMenu(bool? autoRedraw, int? selectItem, string[] items)
        {
            if (autoRedraw == null) autoRedraw = true;

            var index = 1;
            foreach (var item in items)
            {
                if (item != null)
                {
                    Console.ForegroundColor = selectItem == index ? ConsoleColor.Yellow : ConsoleColor.White;
                    Console.WriteLine(str.Join(". ", index, item));
                }

                index++;
            }

            if (selectItem == null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("выбирите пункт меню: ");

                Console.ForegroundColor = ConsoleColor.White;

                while (true)
                {
                    if (selectItem == null)
                    {
                        var key = Console.ReadKey();
                        selectItem = key.KeyChar.ToString().AsInt();
                    }
                    else
                    {
                        break;
                    }
                }

                

                if (autoRedraw.Value)
                {
                    Console.Clear();
                    WriteMenu(autoRedraw, selectItem, items);
                }
                else
                {
                    Console.WriteLine();
                }
            }

            Console.ForegroundColor = ConsoleColor.White;

            return selectItem;
        }

        public static bool Query(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine();
            Console.Write(message + " ");

            while (true)
            {
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.Y || key == ConsoleKey.D1 || key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return true;
                }

                if (key == ConsoleKey.N || key == ConsoleKey.D2 || key == ConsoleKey.D0 || key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return false;
                }

                Console.Write(" y/n? ");
            }
        }
    }

    public static class Report
    {
        public static void WriteLine(string text = null, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
    }

    public class MenyItem
    {
        public MenyItem() {}
        public MenyItem(string displeyValue, Action action) { DispleyValue = displeyValue; Action = action; }

        public string DispleyValue { get; set; }
        public Action Action { get; set; }
    }
}
