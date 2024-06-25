CREATE TABLE User
(
    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
    Name  TEXT,
    Phone TEXT,
    Email TEXT
);

CREATE TABLE Term
(
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER,
    Name   TEXT,
    Start  DATETIME,
    End    DATETIME,
    Status INTEGER,
    FOREIGN KEY (UserId) REFERENCES User (Id)
);

CREATE TABLE Course
(
    Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    InstructorId            INTEGER,
    PerformanceAssessmentId INTEGER,
    ObjectiveAssessmentId   INTEGER,
    Name                    TEXT,
    Start                   DATETIME,
    End                     DATETIME,
    NotifyStart             BOOLEAN,
    NotifyEnd               BOOLEAN
);

CREATE TABLE TermCourse
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    TermId   INTEGER,
    CourseId INTEGER,
    FOREIGN KEY (TermId) REFERENCES Term (Id),
    FOREIGN KEY (CourseId) REFERENCES Course (Id)
);

CREATE TABLE Assessment
(
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    CourseId    INTEGER,
    Type        INTEGER,
    Name        TEXT,
    Start       DATETIME,
    End         DATETIME,
    NotifyStart BOOLEAN,
    NotifyEnd   BOOLEAN,
    FOREIGN KEY (CourseId) REFERENCES Course (Id)
);

CREATE TABLE Note
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    CourseId INTEGER,
    UserId   INTEGER,
    Value    TEXT,
    FOREIGN KEY (CourseId) REFERENCES Course (Id),
    FOREIGN KEY (UserId) REFERENCES User (Id)
);