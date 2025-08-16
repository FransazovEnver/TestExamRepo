using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Channels;
using TestExamBackEnd.Models;

namespace TestExamBackEnd
{
    public class StorySpoilerTests
    {
        //Variables
        private RestClient client;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api ";
        private const string userName = "testuserenver";
        private const string passWord = "123456";
        private static string CreateStoryId;
        [OneTimeSetUp]
        // Mein setup
        public void Setup()
        {
            string jwtToken = GetJwtToken(userName, passWord);
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }
        //Request to get Token
        private string GetJwtToken(string userName, string passWord)
        {
            RestClient authClient = new RestClient(BaseUrl);
            var request = new RestRequest("User/Authentication");
            request.AddJsonBody(new
            {
                userName,
                passWord
            });
         //Get Token 
            var response = authClient.Execute(request, Method.Post);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("AccessToken is null or empty");
                }
                return token;

            }
            else
            {
                throw new InvalidOperationException($"Authentication failed: {response.StatusCode} - {response.Content}");
            }
        }
        //Test 
        [Test, Order(1)]
        public void Create_NewStorySpoiler_ShouldSuccseed()
        {
            var story = new StoryDTO()
            {
                Title = "New story",
                Description = "New Description",
                Url = ""
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "The response code is fail");

            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
            CreateStoryId = content.GetProperty("storyId").GetString() ?? string.Empty;

            Assert.That(CreateStoryId, Is.Not.Null.Or.Empty, "Story Id should not be null or empty");

            Assert.That(content.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));



        }

        [Test, Order(2)]
        public void Edit_StorySpoilerCreatedShuoldSuccseed()
        {

            var editstory = new StoryDTO
            {
                Title = "Edited Story",
                Description = "Edit",
                Url = ""
            };

            var request = new RestRequest($"/Story/Edit/{CreateStoryId}", Method.Put);
            request.AddQueryParameter("storyId", CreateStoryId);
            request.AddJsonBody(editstory);
            var response = client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status should be OK");
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void Get_AllStorySpoilers()
        {
            var request = new RestRequest("/Story/All");

            var response = client.Execute(request, Method.Get);
            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is fail");
            Assert.That(responseDataArray.Length, Is.GreaterThan(0), "Response Data is null");

            CreateStoryId = responseDataArray[responseDataArray.Length - 1].StoryId;
        }

        [Test, Order(5)]
        public void Create_StorySpoilerWithoutTilteAndDescription()
        {
            var story = new
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Edit_NonExistingStorySpoiler()
        {
            var fakeStoryId = "FakeOne";
            var fakeStory = new
            {
                Title = "Fake Test",
                Description = "Fake test",

            };
            var request = new RestRequest($"/Story/Edit/{fakeStoryId}", Method.Put);

            request.AddJsonBody(fakeStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("No spoilers..."));

        }

        [Test, Order(7)]
        public void Delete_NonExistingStorySpoiler()
        {
            string fakeID = "1234";

            var request = new RestRequest($"/Story/Delete/{fakeID}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }



        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}