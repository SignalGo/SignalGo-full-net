using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Client.PrioritySystem
{
    public class PriorityInfo
    {
        public bool IsFinished { get; set; }
        public Delegate PriorityMethod { get; set; }
        public PriorityAction PriorityAction { get; set; }

        public void Wait()
        {

        }

        public void Release()
        {

        }
    }
}
