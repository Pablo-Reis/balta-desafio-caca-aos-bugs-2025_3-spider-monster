using BugStore.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace BugStore.Test.Data
{
    internal class FaultyDbContext(DbContextOptions options) : AppDbContext(options)
    {

        public override int SaveChanges()
            => throw new Exception("Simulated DB failure");
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new Exception("Simulated DB failure");
    }
}
