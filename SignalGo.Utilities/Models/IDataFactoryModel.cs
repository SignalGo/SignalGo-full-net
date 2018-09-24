using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Models
{
    public interface IDataFactoryModel
    {
         object[] ConstructorParameterValues { get; set; }
    }
}
