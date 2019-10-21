using Graph.Community;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChaosHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener.Init();

            GetAllUsers().GetAwaiter().GetResult();

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        public static async Task GetAllUsers()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder.Create("9914776b-f46e-41a9-a8ee-56af1134e379")  // This is a throwaway ClientId
                .WithTenantId("d5fe491b-5987-4770-a68f-477c204cd1ca")  // This is a demo tenant.
                .Build();
            var authProvider = new InteractiveAuthenticationProvider(app, new string[] { "User.Read", "User.Read.All" });

            // Get the standard middleware for Graph  (Auth, Redirect, Retry, Compression)
            var handlers = GraphClientFactory.CreateDefaultHandlers(authProvider);

            // Add a trace handler, and ChaosHandler
            handlers.Add(new GraphTraceHandler());
            handlers.Add(new ChaosHandler());

            // Create a customized HttpClient based on these handlers
            HttpClient client = GraphClientFactory.Create(handlers);

            // Create a GraphServiceClient based on the HttpClient
            var gc = new GraphServiceClient(client);

            // Get all the user Ids
            var users = await gc.Users.Request().Select(u => u.Id).GetAsync();

            // Iterate over that paged list
            var iterator = PageIterator<User>.CreatePageIterator(gc, users, (u) =>
            {
                // Get User details
                gc.Users[u.Id].Request().GetAsync().GetAwaiter().GetResult();
                
                Console.WriteLine(u.Id);
                return true;
            });

            await iterator.IterateAsync();

        }
    }



}
