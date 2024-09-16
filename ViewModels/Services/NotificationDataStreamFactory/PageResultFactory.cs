using Lib.Attributes;
using Microsoft.Extensions.Logging;

namespace ViewModels.Services.NotificationDataStreamFactory;

[Inject]
public class PageResultFactory(ILogger<PageResultFactory> logger)
{
    public IPageResult Create(ReturnedData data)
    {
        var model = new PaginationModel
        {
            Count = data.Notifications.Count,
            Index = data.Index,
            PageSize = data.InputData.PageSize
        };
        logger.LogDebug("Creating PageResult for {data}", data);
        var factory = new NotificationDataFilterFactory(data.InputData);
        var filter = factory.CreateFilter();
        var dataProvider = new DataProcessingService(model, data.Notifications, filter);

        return new PageResult(model, dataProvider);
    }
}