using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner
{
    public partial class App : Application
    {
        public App()
        {
            var file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            if (file.Exists)
            {
                file.Delete();
                using var db = new LocalDbCtx();
                db.Database.EnsureDeleted();
                db.Database.Migrate();
            }
            else
            {
                using var db = new LocalDbCtx();
                db.Database.EnsureDeleted();
                db.Database.Migrate();
            }
            InitializeComponent();
           
            

            MainPage = new AppShell();
        }
    }
}
