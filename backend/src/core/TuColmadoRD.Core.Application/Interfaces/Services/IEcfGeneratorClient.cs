using System.Threading.Tasks;

namespace TuColmadoRD.Core.Application.Interfaces.Services;

public interface IEcfGeneratorClient
{
    Task<string> GenerateXmlAsync(object payload);
}
