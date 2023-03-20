// Copyright (c) 2019 Google LLC.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using System.Net.WebSockets;
using DisasterPR.Server.Controllers;
using Google.Apis.Auth.OAuth2;
using KaLib.Utils;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DisasterPR.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<Server>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseWebSockets();
        app.UseRouting();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    public static string GetProjectId()
    {
        var envVar = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
        if (envVar != null)
        {
            return envVar;
        }
        // Use the service account credentials, if present.
        var googleCredential = GoogleCredential.GetApplicationDefault();
        if (googleCredential != null)
        {
            var credential = googleCredential.UnderlyingCredential;
            var serviceAccountCredential =
                credential as ServiceAccountCredential;
            if (serviceAccountCredential != null)
            {
                return serviceAccountCredential.ProjectId;
            }
        }
        try
        {
            // Query the metadata server.
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
            http.BaseAddress = new Uri(
                @"http://metadata.google.internal/computeMetadata/v1/project/");
            return http.GetStringAsync("project-id").Result;
        }
        catch (AggregateException e)
            when (e.InnerException is HttpRequestException)
        {
            throw new Exception("Could not find Google project id.  " +
                                "Run this application in Google Cloud or follow these " +
                                "instructions to run locally: " +
                                "https://cloud.google.com/docs/authentication/getting-started",
                e.InnerException);
        }
    }
    
    private async Task Echo(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}