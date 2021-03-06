﻿namespace Unosquare.Labs.EmbedIO.Tests
{
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Unosquare.Labs.EmbedIO.Modules;
    using Unosquare.Labs.EmbedIO.Tests.TestObjects;

    [TestFixture]
    public class CorsModuleTest
    {
        protected WebServer WebServer;
        protected string WebServerUrl = Resources.GetServerAddress();
        protected TestConsoleLog Logger = new TestConsoleLog();
        protected object TestObj = new { Message = "OK" };

        [SetUp]
        public void Init()
        {
            WebServer = new WebServer(WebServerUrl, Logger)
                .EnableCors(
                    "http://client.cors-api.appspot.com,http://unosquare.github.io,http://run.plnkr.co",
                    "content-type",
                    "post,get");

            WebServer.RegisterModule(new WebApiModule());
            WebServer.Module<WebApiModule>().RegisterController<TestController>();
            WebServer.RegisterModule(new FallbackModule((ws, ctx) =>
            {
                return ctx.JsonResponse(TestObj);
            }));
            WebServer.RunAsync();
        }

        [Test]
        public async Task GetFallback()
        {
            var webClient = new HttpClient();

            var jsonBody = await webClient.GetStringAsync(WebServerUrl + "invalidpath");

            var jsonFormatting = Formatting.None;
#if DEBUG
            jsonFormatting = Formatting.Indented;
#endif
            Assert.AreEqual(JsonConvert.SerializeObject(TestObj, jsonFormatting), jsonBody, "Same content");
        }

        [Test]
        public async Task PreFlight()
        {
            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + TestController.GetPath);
            request.Headers[Constants.HeaderOrigin] = "http://unosquare.github.io";
            request.Headers[Constants.HeaderAccessControlRequestMethod] = "post";
            request.Headers[Constants.HeaderAccessControlRequestHeaders] = "content-type";
            request.Method = "OPTIONS";

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }
        }

        [TearDown]
        public void Kill()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            WebServer.Dispose();
        }
    }
}