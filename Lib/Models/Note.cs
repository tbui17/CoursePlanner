﻿using Lib.Interfaces;

namespace Lib.Models;

public class Note : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;
}