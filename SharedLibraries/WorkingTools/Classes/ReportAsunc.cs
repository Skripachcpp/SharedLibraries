using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkingTools.Classes
{
    /// <summary>
    /// Тип сообщения
    /// </summary>
    public enum Rt
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
    }

    public class RtMsg
    {
        public string Message { get; set; }
        public Rt Type { get; set; }
    }

    public class ReportAsunc
    {
        protected Action<string, Rt> _report;
        protected ReportAsunc() { }

        public ReportAsunc(Action<string> report) : this((st, rt) => report(st)) { }
        public ReportAsunc(Action<string, Rt> report)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            _report = report;
        }

        private readonly Queue<RtMsg> OutputValues = new Queue<RtMsg>();

        private bool _ran = false;
        private Task _task;
        public virtual void Send(string value = null, Rt type = Rt.Debug)
        {
            if (_report == null)
                return;

            lock (OutputValues)
            {
                OutputValues.Enqueue(new RtMsg() {Message = value, Type = type});

                if (_ran) return;
                else
                {
                    _ran = true;

                    _task = new Task(() =>
                    {
                        while (true)
                        {
                            RtMsg v;
                            lock (OutputValues)
                            {
                                if (OutputValues.Count > 0)
                                {
                                    v = OutputValues.Dequeue();
                                }
                                else
                                {
                                    _ran = false;
                                    break;
                                }
                            }
                            
                            _report(v.Message, v.Type);
                        }
                    });
                }

                _task.Start();
            }
        }

        public void Wait()
        {
            lock (OutputValues) if (!_ran) return;
            _task?.Wait();
        }
    }
}
