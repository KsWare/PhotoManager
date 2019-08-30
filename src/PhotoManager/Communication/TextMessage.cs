using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace KsWare.PhotoManager.Communication
{
	public class TextMessage : IMessage
	{
		public string Text { get; set; }
		public IDictionary Properties { get; }
	}
}
