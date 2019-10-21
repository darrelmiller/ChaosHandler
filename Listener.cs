using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChaosHandler
{
    public static class Listener
    {
        static IDisposable networkSubscription;
        static IDisposable chaosSubscription;
        static IDisposable traceSubscription;
        static IDisposable listenerSubscription = DiagnosticListener.AllListeners.Subscribe(delegate (DiagnosticListener listener)
        {
            //////We get a callback of every Diagnostics Listener that is active in the system(past present or future)
            //if (listener.Name == "System.Net.Http.Desktop")
            //{
            //    lock (listener)
            //    {
            //        if (networkSubscription != null)
            //            networkSubscription.Dispose();

            //        networkSubscription = listener.Subscribe((KeyValuePair<string, object> evnt) =>
            //            Console.WriteLine("From Listener {0} Received Event {1} with payload {2}",
            //            listener.Name, evnt.Key, evnt.Value.ToString()));
            //    }
            //}

            if (listener.Name == "Microsoft.Graph.ChaosHandler")
            {
                lock (listener)
                {
                    if (chaosSubscription != null)
                        chaosSubscription.Dispose();

                    chaosSubscription = listener.Subscribe((KeyValuePair<string, object> evnt) => {
                        var oldcolor = Console.ForegroundColor;
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            var response = (HttpResponseMessage)evnt.Value;
                            
                            Console.WriteLine("{0}: Event {1} with response {2}",
                            listener.Name, evnt.Key, response.StatusCode);
                            Console.WriteLine($"Waiting for {response.Headers.RetryAfter.Delta} secs");

                        } catch (Exception ex)
                        {
                            Console.WriteLine($"Listener failure: {ex.Message}");
                        } finally
                        {
                            Console.ForegroundColor = oldcolor;
                        }
                });

                }
            }
            if (listener.Name == "Microsoft.Graph.GraphTraceHandler")
            {
                lock (listener)
                {
                    if (traceSubscription != null)
                        traceSubscription.Dispose();

                    traceSubscription = listener.Subscribe((KeyValuePair<string, object> evnt) => {
                        var oldcolor = Console.ForegroundColor;
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            switch (evnt.Key)
                            {
                                case "MicrosoftGraphCall.Start":
                                    dynamic payload = evnt.Value;
                                    var request = payload.request as HttpRequestMessage;
                                    Console.WriteLine($"{listener.Name}: {request.Method} {request.RequestUri.OriginalString}");
                                    break;
                                case "MicrosoftGraphCall.Stop":
                                    dynamic stopPayload = evnt.Value;
                                    var response = stopPayload.response as HttpResponseMessage;

                                    Console.WriteLine($"{listener.Name}: {response.StatusCode} Duration: {Activity.Current.Duration}");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Listener failure: {ex.Message}");
                        }
                        finally
                        {
                            Console.ForegroundColor = oldcolor;
                        }

                });

                }
            }

        });

        public static void Init() { 
        }
    }
}
