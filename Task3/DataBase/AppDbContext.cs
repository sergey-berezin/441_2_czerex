using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataBase
{
    public class AppDbContext: DbContext
    {
        public DbSet<TabText> TabTexts { get; set; }
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
           => o.UseSqlite($"Data Source=\"D:\\Daria\\Task2\\Task2\\DataBase\\AppLibrary.db\"");
     
    }
}
