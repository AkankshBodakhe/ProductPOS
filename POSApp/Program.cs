using POSLibrary.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace POSApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Tell EF how to initialize the DB
            Database.SetInitializer(new CreateDatabaseIfNotExists<POSDbContext>());

            // Force DB creation + add a test user
            using (var context = new POSDbContext())
            {
                context.Database.Initialize(force: true);

               
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Frm_Pos());
        }
    }
}
