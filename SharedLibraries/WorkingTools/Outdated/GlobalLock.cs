using System;
using System.Collections.Generic;
using System.Threading;

namespace WorkingTools.Classes
{
    public class KeyLock
    {
        private readonly object _lock = new object();

        private int _worked;
        public int Worked
        {
            get { return _worked; }
            set
            {
                lock (_lock)
                {
                    if (value == _worked) return;

                    if (value > 0) Idle.Reset();
                    if (value == 0) Idle.Set();
                    if (value < 0)
                        throw new Exception("число объектов использующих путь не может быть отрицательным, " +
                                            "видимо кто то лишний раз декрементировал значение, " +
                                            "стоит выяснить кто это был");
                    _worked = value;
                }
            }
        }

        public ManualResetEvent Idle { get; } = new ManualResetEvent(false);
    }

    public static class GlobalLock
    {
        private static Dictionary<string, KeyLock> Keys { get; } = new Dictionary<string, KeyLock>();

        public static void Set(string key)
        {
            KeyLock keyLock;
            lock (Keys)
                if (!Keys.ContainsKey(key)) Keys.Add(key, (keyLock = new KeyLock()));
                else keyLock = Keys[key];

            keyLock.Worked++;
        }

        public static void Reset(string key)
        {
            KeyLock keyLock;
            lock (Keys)
                if (!Keys.ContainsKey(key)) return;
                else keyLock = Keys[key];

            keyLock.Worked--;
        }

        public static void WaitAdnSet(string key)
        {
            KeyLock keyLock;
            lock (Keys)
                if (!Keys.ContainsKey(key)) return;
                else keyLock = Keys[key];


            lock (keyLock)
            {
                keyLock.Idle.WaitOne();
                keyLock.Worked++;
            }
        }

        public static void Wait(string key)
        {
            KeyLock keyLock;
            lock (Keys)
                if (!Keys.ContainsKey(key)) return;
                else keyLock = Keys[key];

            lock (keyLock) keyLock.Idle.WaitOne();
        }
    }
}