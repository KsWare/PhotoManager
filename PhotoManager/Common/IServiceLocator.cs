using System.Collections.Generic;

namespace KsWare.PhotoManager.Common
{
    public interface IServiceLocator
    {
	    T GetInstance<T>();

	    T GetInstance<T, T1>(string p1Name, T1 p1);

        T GetInstance<T, T1, T2>(string p1Name, T1 p1, string p2Name, T2 p2);

        T GetInstance<T>(IDictionary<string, object> parameter);

        T CreateInstanceDirect<T>(params object[] args) ;
    }
}