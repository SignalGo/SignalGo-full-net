using SignalGo.Shared.Helpers;

namespace SignalGo.Shared.Models
{
    public class ModelDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of model
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// name and namce space of class
        /// </summary>
        public string FullNameSpace { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// json template of model
        /// </summary>
        public string JsonTemplate { get; set; }
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// type of object
        /// </summary>
        public SerializeObjectType ObjectType { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public ModelDetailsInfo Clone()
        {
            return new ModelDetailsInfo() { Id = Id, ObjectType = ObjectType, Name = Name, Comment = Comment, FullNameSpace = FullNameSpace, JsonTemplate = JsonTemplate, IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
