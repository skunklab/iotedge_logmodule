using System;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap
{
    public interface ICoapRequestDispatch : IDisposable
    {
        string Identity { set; }
        Task<CoapMessage> PostAsync(CoapMessage message);

        Task<CoapMessage> GetAsync(CoapMessage message);

        Task<CoapMessage> PutAsync(CoapMessage message);

        Task<CoapMessage> DeleteAsync(CoapMessage message);

        Task<CoapMessage> ObserveAsync(CoapMessage message);        
    }
}
