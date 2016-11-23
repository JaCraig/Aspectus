﻿using FileCurator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Aspectus.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public class TestingDirectoryFixture : IDisposable
    {
        public TestingDirectoryFixture()
        {
            Canister.Builder.CreateContainer(new List<ServiceDescriptor>(),
                typeof(TestingDirectoryFixture).GetTypeInfo().Assembly,
                typeof(Aspectus).GetTypeInfo().Assembly,
                typeof(FileInfo).GetTypeInfo().Assembly);
            new DirectoryInfo(@".\Testing").Create();
            new DirectoryInfo(@".\App_Data").Create();
            new DirectoryInfo(@".\Logs").Create();
        }

        public void Dispose()
        {
            new DirectoryInfo(@".\Testing").Delete();
            new DirectoryInfo(@".\App_Data").Delete();
            new DirectoryInfo(@".\Logs").Delete();
        }
    }
}