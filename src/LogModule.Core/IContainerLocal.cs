using System.Threading.Tasks;

namespace LogModule
{
    public interface IContainerLocal : IContainerRemote
    {
        Task<byte[]> GetFile(string path, string filename);
        Task WriteFile(string path, string filename, byte[] body, bool append = false, int maxSize = 0);
        
    }
}
