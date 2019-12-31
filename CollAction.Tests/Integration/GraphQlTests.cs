﻿using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using CollAction.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CollAction.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class GraphQlTests : IntegrationTestBase
    {
        [TestMethod]
        public Task TestProjectList()
            => WithTestServer(
                   async (scope, testServer) =>
                   {
                       const string QueryProjects = @"
                           query {
                               projects {
                                   id
                                   name
                                   descriptiveImage {
                                       filepath
                                   }
                               }
                           }";

                       HttpResponseMessage response = await PerformGraphQlQuery(testServer, QueryProjects, null);
                       string content = await response.Content.ReadAsStringAsync();
                       Assert.IsTrue(response.IsSuccessStatusCode, content);
                       JsonDocument result = JsonDocument.Parse(content);
                       Assert.ThrowsException<KeyNotFoundException>(() => result.RootElement.GetProperty("errors"), content);
                       JsonElement projects = result.RootElement.GetProperty("data").GetProperty("projects");
                       Assert.IsTrue(projects.GetArrayLength() > 0, content);

                       int projectId = projects.EnumerateArray().First().GetProperty("id").GetInt32();
                       const string QueryProject = @"
                           query($projectId : ID!) {
                               project(id: $projectId) {
                                   id
                                   name
                                   descriptiveImage {
                                       filepath
                                   }
                               }
                           }";
                       dynamic variables = new { projectId };
                       response = await PerformGraphQlQuery(testServer, QueryProject, variables);
                       content = await response.Content.ReadAsStringAsync();
                       Assert.IsTrue(response.IsSuccessStatusCode, content);
                       result = JsonDocument.Parse(content);
                       Assert.ThrowsException<KeyNotFoundException>(() => result.RootElement.GetProperty("errors"), content);
                       JsonElement project = result.RootElement.GetProperty("data").GetProperty("project");
                       Assert.AreEqual(projectId, project.GetProperty("id").GetInt32());
                   });

        [TestMethod]
        public Task TestAuthorization()
            => WithTestServer(
                   async (scope, testServer) =>
                   {
                       const string QueryProjects = @"
                           query {
                               projects {
                                   id
                                   name
                                   owner {
                                       id
                                   }
                               }
                           }";

                       HttpResponseMessage response = await PerformGraphQlQuery(testServer, QueryProjects, null);
                       string content = await response.Content.ReadAsStringAsync();
                       Assert.IsTrue(response.IsSuccessStatusCode, content);
                       JsonDocument result = JsonDocument.Parse(content);
                       Assert.IsNotNull(result.RootElement.GetProperty("errors"), content);

                       SeedOptions seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
                       using (var httpClient = testServer.CreateClient())
                       {
                           // Retry call as admin
                           httpClient.DefaultRequestHeaders.Add("Cookie", await GetAuthCookie(httpClient, seedOptions));
                           response = await PerformGraphQlQuery(httpClient, QueryProjects, null);
                           content = await response.Content.ReadAsStringAsync();
                           Assert.IsTrue(response.IsSuccessStatusCode, content);
                           result = JsonDocument.Parse(content);
                           Assert.ThrowsException<KeyNotFoundException>(() => result.RootElement.GetProperty("errors"), content);
                       }
                   });

        [TestMethod]
        public Task TestCreateProject()
            => WithTestServer(
                   async (scope, testServer) =>
                   {
                       string createProject = $@"
                           mutation {{
                               project {{
                                   createProject(project:
                                       {{
                                           name:""{Guid.NewGuid()}"",
                                           categoryId: 4,
                                           target: 55,
                                           proposal: ""44"",
                                           description: """",
                                           goal: ""dd"",
                                           creatorComments: ""dd"",
                                           start: ""4-4-2009"",
                                           end: ""4-4-2000"",
                                           descriptionVideoLink: ""https://youtube.com"",
                                           tags:[""{Guid.NewGuid()}"", ""a""]
                                       }}) {{
                                           id
                                       }}
                                   }}
                               }}";

                       SeedOptions seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
                       using (var httpClient = testServer.CreateClient())
                       {
                           httpClient.DefaultRequestHeaders.Add("Cookie", await GetAuthCookie(httpClient, seedOptions));
                           HttpResponseMessage response = await PerformGraphQlQuery(httpClient, createProject, null);
                           string content = await response.Content.ReadAsStringAsync();
                           JsonDocument result = JsonDocument.Parse(content);
                           Assert.IsTrue(response.IsSuccessStatusCode, content);
                           Assert.ThrowsException<KeyNotFoundException>(() => result.RootElement.GetProperty("errors"), content);
                       }
                   });

        private static async Task<HttpResponseMessage> PerformGraphQlQuery(TestServer testServer, string query, dynamic variables)
        {
            using (var httpClient = testServer.CreateClient())
            {
                return await PerformGraphQlQuery(httpClient, query, variables);
            }
        }

        private static async Task<HttpResponseMessage> PerformGraphQlQuery(HttpClient httpClient, string query, dynamic variables)
        {
            // Test with columns provided
            string jsonBody =
                JsonSerializer.Serialize(
                    new
                    {
                        Query = query,
                        Variables = variables
                    });
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using (content)
            {
                return await httpClient.PostAsync(new Uri("/graphql", UriKind.Relative), content, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}