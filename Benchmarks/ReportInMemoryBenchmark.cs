using BenchmarkDotNet.Attributes;
using Lib.Models;
using Lib.Services.ReportService;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class ReportInMemoryBenchmark
{
    private List<DateTimeRange> _dateTimeRanges = [];


    [GlobalSetup]
    public void GlobalSetup()
    {
        var util = new GlobalSetupUtil();
        _dateTimeRanges = util.CreateDateTimeRanges().ToList();
    }

    [Benchmark(Baseline = true)]
    public int NoMultiThreading()
    {
        List<DurationReportFactory> reports = [
            new(){Entities = _dateTimeRanges},
            new(){Entities = _dateTimeRanges},
            new(){Entities = _dateTimeRanges}
        ];

        return reports.Select(x => x.ToData()).Sum(x => x.TotalItems);
    }

    [Benchmark]
    public int Multithreading()
    {
        List<DurationReportFactory> reports = [
            new(){Entities = _dateTimeRanges},
            new(){Entities = _dateTimeRanges},
            new(){Entities = _dateTimeRanges}
        ];

        return reports.AsParallel().Select(x => x.ToData()).Sum(x => x.TotalItems);
    }


}
