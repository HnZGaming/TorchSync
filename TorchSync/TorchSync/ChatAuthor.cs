using System.Xml.Serialization;
using Torch;

namespace TorchSync
{
    public class ChatAuthor : ViewModel
    {
        string _name;

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }
    }
}