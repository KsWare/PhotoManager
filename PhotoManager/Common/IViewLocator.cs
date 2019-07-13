using System;
using System.Windows;

namespace KsWare.PhotoManager.Common
{
    public interface IViewLocator
    {
        UIElement GetOrCreateViewType(Type viewType);
    }
}