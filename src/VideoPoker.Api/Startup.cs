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

                // Setup a default route that serves up metadata
                endpoints.MapGet("/", async context => {
                    var builder = new StringBuilder();
                    builder.AppendLine($"Time to Build Data: {this.AllHandsLoadTime.TotalSeconds.ToString("F2")} secs");
                    builder.AppendLine($"Is 64-bit: {System.Environment.Is64BitProcess}");
                    await context.Response.WriteAsync(builder.ToString());
                });

                // Setup a route to hit the Analyze API
                endpoints.MapGet("/analyze", async context => {

                    var builder = new StringBuilder();
                    var stopwatch = Stopwatch.StartNew();

                    var data = context.Request.Query["cards"].ToString();
                    var cards = data.Split(",");

                    var results = new ConcurrentDictionary<string, double>();

                    builder.AppendLine($"Processing checkpoint #1 at {stopwatch.ElapsedMilliseconds} ms.");
                    
                    Enumerable.Range(0, 32).AsParallel().ForAll(i => {

                        string holdPattern = Convert.ToString(i, 2).PadLeft(5, '0');

                        // If holding, must include that card. If discarding, must NOT include that card.
                        //List<Hand> matchingHands = (
                        //    from h in this.AllHands
                        //    where h.Contains(cards[0]) == (holdPattern.Substring(0, 1) == "1")
                        //       && h.Contains(cards[1]) == (holdPattern.Substring(1, 1) == "1")
                        //       && h.Contains(cards[2]) == (holdPattern.Substring(2, 1) == "1")
                        //       && h.Contains(cards[3]) == (holdPattern.Substring(3, 1) == "1")
                        //       && h.Contains(cards[4]) == (holdPattern.Substring(4, 1) == "1")
                        //    select h
                        //).ToList();

                        //var matchingHands = this.AllHands
                        //    .Where(h => h.Contains(cards[0]) == (holdPattern.Substring(0, 1) == "1"))
                        //    .Where(h => h.Contains(cards[1]) == (holdPattern.Substring(1, 1) == "1"))
                        //    .Where(h => h.Contains(cards[2]) == (holdPattern.Substring(2, 1) == "1"))
                        //    .Where(h => h.Contains(cards[3]) == (holdPattern.Substring(3, 1) == "1"))
                        //    .Where(h => h.Contains(cards[4]) == (holdPattern.Substring(4, 1) == "1"))
                        //    .ToList();

                        // Precalculate the hold pattern values
                        var hold0 = (holdPattern.Substring(0, 1) == "1");
                        var hold1 = (holdPattern.Substring(1, 1) == "1");
                        var hold2 = (holdPattern.Substring(2, 1) == "1");
                        var hold3 = (holdPattern.Substring(3, 1) == "1");
                        var hold4 = (holdPattern.Substring(4, 1) == "1");

                        var matchingHands = this.AllHands
                            .Where(h => h.Contains(cards[0]) == hold0)
                            .Where(h => h.Contains(cards[1]) == hold1)
                            .Where(h => h.Contains(cards[2]) == hold2)
                            .Where(h => h.Contains(cards[3]) == hold3)
                            .Where(h => h.Contains(cards[4]) == hold4)
                            .ToList();

                        double score = matchingHands.Average(x => x.Score);

                        bool added = results.TryAdd(holdPattern, score);
                    });

                    builder.AppendLine($"Processing checkpoint #2 at {stopwatch.ElapsedMilliseconds} ms.");

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
