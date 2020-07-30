using System;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class ParameterReferenceInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Comment { get; set; }

        public ParameterReferenceInfo Clone()
        {
            return new ParameterReferenceInfo()
            {
                Name = Name,
                TypeName = TypeName,
                Comment = Comment
            };
        }
    }
}
