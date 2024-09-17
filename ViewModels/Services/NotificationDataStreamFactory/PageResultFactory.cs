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

        var filteredData = data.Notifications
            .AsParallel()
            .Where(filter)
            .OrderBy(x => x.Id)
            .ToList();

        var model = new PaginationModel
        {
            PageSize = data.InputData.PageSize,
            Index = data.Index,
            Count = filteredData.Count
        };

        var processedData = filteredData
            .Chunk(data.InputData.PageSize)
            .ElementAtOrDefault(model.Index) ?? [];

        var res = new PageResult(model, processedData);

        logger.LogDebug("Created {Data}", res);

        return res;
    }
}