using System;
using System.Collections.Generic;
using System.Linq;
using BuildLib.Utils;
using Nuke.Common.IO;

namespace Entry.FileSystem;

[Ignore]
public class AndroidDirectoryManager(AbsolutePath solutionDirectory)
{
    public AbsolutePath OutputDirectory => solutionDirectory / "output";
    public AbsolutePath PackagePattern => OutputDirectory / "*.apk";
    public AbsolutePath BundlePattern => OutputDirectory / "*.apb";

    public IReadOnlyCollection<AbsolutePath> GetBundles()
    {
        return BundlePattern.GlobFiles();
    }

    public IReadOnlyCollection<AbsolutePath> GetPackages()
    {
        return PackagePattern.GlobFiles();
    }

    public IReadOnlyCollection<AbsolutePath> GetAndroidFiles()
    {
        return GetBundles().Concat(GetPackages()).ToList();
    }

    public IReadOnlyCollection<AbsolutePath> GetOrThrowAndroidFiles()
    {
        var res = GetBundles().Concat(GetPackages()).ToList();

        if (res.Count == 0)
        {
            throw new Exception("No Android files found.");
        }

        return res;
    }
}