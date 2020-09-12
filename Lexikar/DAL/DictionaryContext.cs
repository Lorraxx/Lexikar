using Lexikar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lexikar.DAL
{
    public class DictionaryContext : DbContext
    {
        public DictionaryContext(DbContextOptions<DictionaryContext> options) : base(options)
        {
        }
        public DictionaryContext()
        {
        }

        public DbSet<Translation> Translations { get; set; }
        public DbSet<DescText> DescTexts { get; set; }
        public DbSet<CorpusSource> CorpusSources { get; set; }
        public DbSet<CorpusFile> CorpusFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CorpusFile>()
                .HasMany(c => c.CorpusSources)
                .WithOne(e => e.CorpusFile)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
