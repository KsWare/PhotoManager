using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KsWare.PhotoManager.Communication
{
	public class ImageLoadedMessage : IMessage
	{
		public ImageLoadedMessage(ImageSource imageSource)
		{
			Properties = new Dictionary<string, object> {{nameof(ImageSource), imageSource}};
		}

		public IDictionary Properties { get; }

		public BitmapSource ImageSource => (BitmapSource)Properties[nameof(ImageSource)];
	}
}
