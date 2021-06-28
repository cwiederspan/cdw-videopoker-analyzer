using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

using VideoPoker.Api.Models;
using VideoPoker.Api.Services;
using System.Text;
using System.Collections.Concurrent;

namespace VideoPoker.Api {

    public class Startup {

        public IConfiguration Configuration { get; }

        private List<Hand> AllHands { get; set; }

        private TimeSpan AllHandsLoadTime { get; set; }

        public Startup(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {

            var stopwatch = Stopwatch.StartNew();
            var scorer = new ScoringService();
            this.AllHands = scorer.ScoreAllHands(new StandardNineSixScoreSheet());
            this.AllHandsLoadTime = stopwatch.Elapsed;
            System.Diagnostics.Debug.WriteLine("Generating all hands took " + this.AllHandsLoadTime.TotalSeconds.ToString("F2") + " seconds!");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => {

                endpoints.MapGet("/", async context => {
                    var builder = new StringBuilder();
                    builder.AppendLine($"Time to Build Data: {this.AllHandsLoadTime.TotalSeconds.ToString("F2")} secs");
                    builder.AppendLine($"Is 64-bit: {System.Environment.Is64BitProcess}");
                    await context.Response.WriteAsync(builder.ToString());
                });



                // Setup a route for a GET to test the read/download performance
                endpoints.MapGet("/analyze", async context => {

                    var stopwatch = Stopwatch.StartNew();

                    var data = context.Request.Query["cards"].ToString();
                    var cards = data.Split(",");

                    var results = new ConcurrentDictionary<string, double>();

                    Enumerable.Range(0, 32).AsParallel().ForAll(i => {

                        string holdPattern = Convert.ToString(i, 2).PadLeft(5, '0');

                        List<Hand> matchingHands = (
                            from h in this.AllHands
                            where h.Contains(cards[0]) == (holdPattern.Substring(0, 1) == "1")
                               && h.Contains(cards[1]) == (holdPattern.Substring(1, 1) == "1")
                               && h.Contains(cards[2]) == (holdPattern.Substring(2, 1) == "1")
                               && h.Contains(cards[3]) == (holdPattern.Substring(3, 1) == "1")
                               && h.Contains(cards[4]) == (holdPattern.Substring(4, 1) == "1")
                            select h
                        ).ToList();

                        double score = matchingHands.Average(x => x.Score);

                        bool added = results.TryAdd(holdPattern, score);
                    });

                    /*
                    Parallel.For(0, 32,
                        i => {

                            string holdPattern = Convert.ToString(i, 2).PadLeft(5, '0');

                            List<Hand> matchingHands = (
                                from h in this.AllHands
                                where h.Contains(cards[0]) == (holdPattern.Substring(0, 1) == "1")
                                   && h.Contains(cards[1]) == (holdPattern.Substring(1, 1) == "1")
                                   && h.Contains(cards[2]) == (holdPattern.Substring(2, 1) == "1")
                                   && h.Contains(cards[3]) == (holdPattern.Substring(3, 1) == "1")
                                   && h.Contains(cards[4]) == (holdPattern.Substring(4, 1) == "1")
                                select h
                            ).ToList();

                            double score = matchingHands.Average(x => x.Score);

                            bool added = results.TryAdd(holdPattern, score);
                        }
                    );
                    */

                    var builder = new StringBuilder();
                    results.OrderByDescending(r => r.Value)
                        .Select(r => $"{data}_{r.Key}_{r.Value.ToString("F5")}")
                        .ToList()
                        .ForEach(s => builder.AppendLine(s));

                    builder.AppendLine();
                    builder.AppendLine($"Processing took {stopwatch.Elapsed.TotalSeconds.ToString("F2")} seconds.");

                    await context.Response.WriteAsync(builder.ToString());
                });
            });
        }
    }
}
