using Lib.Attributes;
using Microsoft.Extensions.Logging;

namespace ViewModels.Services.NotificationDataStreamFactory;

[Inject]
public class PageResultFactory(ILogger<PageResultFactory> logger)
{
    public IPageResult Create(NotificationsInputData data)
    {
        logger.LogDebug("Processing {Data}", data);
        var filter = new NotificationDataFilterFactory(data.InputData).CreateFilter();

        var model = new PaginationModel
        {
            Count = data.Notifications.Count,
            Index = data.Index,
            PageSize = data.InputData.PageSize
        };

        var processedData = data.Notifications
            .AsParallel()
            .Where(filter)
            .OrderBy(x => x.Id)
            .Chunk(model.PageSize)
            .ElementAtOrDefault(model.Index) ?? [];

        var res = new PageResult(model, processedData);

        logger.LogDebug("Created {Data}", res);

        return res;
    }
}