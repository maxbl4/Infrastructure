using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class AsyncInitTests: IDisposable
    {
        private IHost host;

        [Fact]
        public async Task Can_load_repos()
        {
            var r = await GetRepos();
            r.Should().NotBeEmpty();
            r[0].Id.Should().BeGreaterThan(0);
            r[0].Name.Should().NotBeEmpty();
            r[0].FullName.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task Init()
        {
            host = new HostBuilder()
                .ConfigureWebHostDefaults(x =>
                {
                    x.UseStartup<Startup>();
                })
                .Build();
            await host.StartAsync();
            var opts = host.Services.GetService<IOptions<RepoOptions>>();
            opts.Value.Repos.Should().HaveCountGreaterThan(2);
            await Task.Delay(1000);
            await host.StopAsync();
        }

        public static async Task<List<GithubRepo>> GetRepos()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
            var response = await http.GetAsync<List<GithubRepo>>("https://api.github.com/users/maxbl4/repos");
            return response;
        }

        public void Dispose()
        {
            host?.Dispose();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RepoOptions>(x =>
            {
                x.Repos = AsyncInitTests.GetRepos().GetAwaiter().GetResult();
            });
            services.AddMvc();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(x =>
            {
                x.MapControllers();
            });
        }
    }

    public class RepoOptions
    {
        public List<GithubRepo> Repos { get; set; }
    }

    public class GithubRepo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }
    }
}