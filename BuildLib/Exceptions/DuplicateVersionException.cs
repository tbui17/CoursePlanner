using System.Data;
using BuildLib.CloudServices.AzureBlob;

namespace BuildLib.Exceptions;

public class DuplicateVersionException(string message) : DataException(message)
{
    public static DuplicateVersionException Create(IEnumerable<IBlob> data)
    {
        return new DuplicateVersionException(
            $"Found multiple files with the same version: {data.Select(x => new { x.Name, x.Version }).ToArray()}"
        )
        {
            Data = { ["Data"] = data }
        };
    }
}