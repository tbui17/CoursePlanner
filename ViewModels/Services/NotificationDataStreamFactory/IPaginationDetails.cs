using System.ComponentModel.DataAnnotations;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IPaginationDetails
{
    [Range(0, int.MaxValue)]
    int Index { get; }
    [Range(0, int.MaxValue)]
    int Count { get; }
    [Range(1, int.MaxValue)]
    int PageSize { get; }
}