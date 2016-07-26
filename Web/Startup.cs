using Microsoft.Owin;
using Owin;
using Hangfire;
using MandraSoft.PokemonGo.Web.Jobs;
using System.Linq;
[assembly: OwinStartup(typeof(MandraSoft.PokemonGo.Web.Startup))]

namespace MandraSoft.PokemonGo.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("PokemonGoDb");
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new [] { new MyRestrictiveAuthorizationFilter() }
            });
            RecurringJob.AddOrUpdate("Dump Queue Encounters", () => ScanningJobs.DumpToDbAndLiveDbAndPurge(), Cron.Minutely);
            //RecurringJob.AddOrUpdate("Purge Expired Pokemons from RAM", () => ScanningJobs.PurgeExpiredPokemons(), Cron.Minutely);
            //var jobCountersParis = 50;
            //for (var i = 0; i < 100; i++)
            //    RecurringJob.RemoveIfExists(i.ToString());
            //var iTotal = 0;
            //for (var i = 0; i < jobCountersParis; i++)
            //{
            //    RecurringJob.AddOrUpdate(iTotal.ToString(), () => ScanningJobs.ScanAllArea(48.899352, 2.260842, 48.818289, 2.454376, jobCountersParis, i, "Client"), Cron.Minutely);
            //    iTotal++;
            //}
            //var jobCountersNico = 10;
            //for (var i = 0 ; i < jobCountersNico; i++)
            //{
            //    RecurringJob.AddOrUpdate(iTotal.ToString(), () => ScanningJobs.ScanAllParisArea(47.924124, 1.985559, 47.898075, 1.893441, jobCountersNico, i,"ClientNico"), Cron.Minutely());
            //    iTotal++;
            //}
            //var jobCountersRichard = 5;
            //for (var i = 0; i < jobCountersRichard; i++)
            //{
            //    RecurringJob.AddOrUpdate(iTotal.ToString(), () => ScanningJobs.ScanAllParisArea(48.883994, 2.660292, 48.859859, 2.619515, jobCountersRichard, i,"ClientRichard"), Cron.Minutely());
            //    iTotal++;
            //}
            app.UseHangfireServer(new BackgroundJobServerOptions() { WorkerCount = 2 });
            //Hangfire.RecurringJob.AddOrUpdate("Update Paris 1", () => ScanningJobs.ScanAllPokemonsForAddress("1er arrondissement,75001  Paris"), Cron.MinuteInterval(1));
            // Pour plus d'informations sur la façon de configurer votre application, consultez http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
