namespace LibTests.GridStateTests;

public record GridTestCase
{
    public required int Columns { get; set; }
    public ICollection<AutoGridStateSnapshot> Snapshots { get; set; } = [];

    public static IEnumerable<TestCaseData> GridStateTestCases()
    {
        var zeroColumnTestData = new GridTestCase
        {
            Columns = 0,
            Snapshots =
            [
                new(Count: 1,
                    Index: 0,
                    Column: 0,
                    Row: 0,
                    Rows: 1,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 2,
                    Index: 1,
                    Column: 0,
                    Row: 1,
                    Rows: 2,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 3,
                    Index: 2,
                    Column: 0,
                    Row: 2,
                    Rows: 3,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 4,
                    Index: 3,
                    Column: 0,
                    Row: 3,
                    Rows: 4,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 5,
                    Index: 4,
                    Column: 0,
                    Row: 4,
                    Rows: 5,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 6,
                    Index: 5,
                    Column: 0,
                    Row: 5,
                    Rows: 6,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 7,
                    Index: 6,
                    Column: 0,
                    Row: 6,
                    Rows: 7,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 8,
                    Index: 7,
                    Column: 0,
                    Row: 7,
                    Rows: 8,
                    ShouldAddRowDefinition: true
                ),
                new(Count: 9,
                    Index: 8,
                    Column: 0,
                    Row: 8,
                    Rows: 9,
                    ShouldAddRowDefinition: true
                ),
            ]
        };
        IEnumerable<GridTestCase> cases =
        [
            zeroColumnTestData,
            new() { Columns = 1, Snapshots = zeroColumnTestData.Snapshots },
            new()
            {
                Columns = 2,
                Snapshots =
                [
                    new(Count: 1,
                        Index: 0,
                        Column: 0,
                        Row: 0,
                        Rows: 1,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 2,
                        Index: 1,
                        Column: 1,
                        Row: 0,
                        Rows: 1,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 3,
                        Index: 2,
                        Column: 0,
                        Row: 1,
                        Rows: 2,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 4,
                        Index: 3,
                        Column: 1,
                        Row: 1,
                        Rows: 2,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 5,
                        Index: 4,
                        Column: 0,
                        Row: 2,
                        Rows: 3,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 6,
                        Index: 5,
                        Column: 1,
                        Row: 2,
                        Rows: 3,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 7,
                        Index: 6,
                        Column: 0,
                        Row: 3,
                        Rows: 4,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 8,
                        Index: 7,
                        Column: 1,
                        Row: 3,
                        Rows: 4,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 9,
                        Index: 8,
                        Column: 0,
                        Row: 4,
                        Rows: 5,
                        ShouldAddRowDefinition: true
                    )
                ]
            },
            new()
            {
                Columns = 3,
                Snapshots =
                [
                    new(Count: 1,
                        Index: 0,
                        Column: 0,
                        Row: 0,
                        Rows: 1,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 2,
                        Index: 1,
                        Column: 1,
                        Row: 0,
                        Rows: 1,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 3,
                        Index: 2,
                        Column: 2,
                        Row: 0,
                        Rows: 1,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 4,
                        Index: 3,
                        Column: 0,
                        Row: 1,
                        Rows: 2,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 5,
                        Index: 4,
                        Column: 1,
                        Row: 1,
                        Rows: 2,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 6,
                        Index: 5,
                        Column: 2,
                        Row: 1,
                        Rows: 2,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 7,
                        Index: 6,
                        Column: 0,
                        Row: 2,
                        Rows: 3,
                        ShouldAddRowDefinition: true
                    ),
                    new(Count: 8,
                        Index: 7,
                        Column: 1,
                        Row: 2,
                        Rows: 3,
                        ShouldAddRowDefinition: false
                    ),
                    new(Count: 9,
                        Index: 8,
                        Column: 2,
                        Row: 2,
                        Rows: 3,
                        ShouldAddRowDefinition: false
                    )
                ]
            }
        ];

        return cases.Select(x => new TestCaseData(x));
    }
}