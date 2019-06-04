namespace SignalGo.Shared.Models
{
    /// <summary>
    /// details of parameter
    /// </summary>
    public class ServiceDetailsParameterInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// type of parameter
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// full type name of parameter
        /// </summary>
        public string FullTypeName { get; set; }
        /// <summary>
        /// value of parameter
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// is value json type
        /// </summary>
        public bool IsJson { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// example template of request data
        /// </summary>
        public string TemplateValue { get; set; }
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public ServiceDetailsParameterInfo Clone()
        {
            return new ServiceDetailsParameterInfo() { Id = Id, Comment = Comment, FullTypeName = FullTypeName, IsJson = IsJson, Name = Name, TemplateValue = TemplateValue, Type = Type, Value = Value, IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
