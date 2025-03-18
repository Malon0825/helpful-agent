using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nova_log.DataAccess
{
    public class GitHubIssueCreator
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public GitHubIssueCreator()
        {
            _token = SecretsConfiguration.GetGithubToken();
            string appname = SecretsConfiguration.GetAppName();
            string appVersion = SecretsConfiguration.GetAppVersion();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appname, appVersion));

        }

        public async Task<string> CreateIssueAsync(string repositoryId, string title, string body)
        {
            // GraphQL mutation to create an issue
            var mutation = @"
                mutation CreateIssue($repositoryId: ID!, $title: String!, $body: String) {
                  createIssue(input: {repositoryId: $repositoryId, title: $title, body: $body}) {
                    issue {
                      id
                      number
                      url
                    }
                  }
                }";

            // Variables for the mutation
            var variables = new
            {
                repositoryId,
                title,
                body
            };

            // Create the request content
            var request = new
            {
                query = mutation,
                variables
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            // Send the request to GitHub's GraphQL endpoint
            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        // Helper method to get repository ID from owner and name
        public async Task<string> GetRepositoryIdAsync(string owner, string name)
        {
            var query = @"
                query($owner: String!, $name: String!) {
                  repository(owner: $owner, name: $name) {
                    id
                  }
                }";

            var variables = new
            {
                owner,
                name
            };

            var request = new
            {
                query,
                variables
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse the response to extract the repository ID
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("repository")
                .GetProperty("id")
                .GetString();
        }

        public async Task<string> AddIssueToProjectAsync(string projectId, string contentId)
        {
            var mutation = @"
                mutation($projectId: ID!, $contentId: ID!) {
                  addProjectV2ItemById(input: {projectId: $projectId, contentId: $contentId}) {
                    item {
                      id
                    }
                  }
                }";

            var variables = new
            {
                projectId,
                contentId
            };

            var request = new
            {
                query = mutation,
                variables
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        public async Task<string> UpdateAssigneeAsync(string contentId, string assigneeId)
        {
            // Define the GraphQL mutation.
            // Note: We declare $assigneeId as String! now to match the expected type.
            var mutation = @"
            mutation($assignableId: ID!, $assigneeIds: [ID!]!) {
                                  addAssigneesToAssignable(input: { assignableId: $assignableId, assigneeIds: $assigneeIds }) {
                                    assignable {
                                      ... on Issue {
                                        id
                                        assignees(first: 10) {
                                          nodes {
                                            login
                                          }
                                        }
                                      }
                                    }
                                  }
                                }";

            // Set up the variables.
            var variables = new
            {
                assignableId = contentId, 
                assigneeIds = new[] { assigneeId } 
            };

            // Construct the request payload.
            var request = new
            {
                query = mutation,
                variables
            };

            // Serialize the request content.
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            // Post the request.
            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        public async Task<string> UpdateStatusAsync(string projectId, string itemId, string fieldId, string statusOptionId)
        {
            // Define the GraphQL mutation.
            // Note: We declare $assigneeId as String! now to match the expected type.
            var mutation = @"
                            mutation {
                              updateProjectV2ItemFieldValue(
                                input: {
                                  projectId: """ + projectId + @"""
                                  itemId: """ + itemId + @"""
                                  fieldId: """ + fieldId + @"""
                                  value: { singleSelectOptionId: """ + statusOptionId + @""" }
                                }
                              ) {
                                projectV2Item {
                                  id
                                }
                              }
                            }";

            // Construct the request payload.
            var request = new
            {
                query = mutation
            };

            // Serialize the request content.
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            // Post the request.
            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        public async Task<string> UpdateDatesAsync(string itemId, string fieldId, string dateValue, string projectId)
        {
            var mutation = @"
                            mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $dateValue: Date!) {
                              updateProjectV2ItemFieldValue(input: {
                                projectId: $projectId,
                                itemId: $itemId,
                                fieldId: $fieldId,
                                value: { date: $dateValue } 
                              }) {
                                projectV2Item {
                                  id
                                }
                              }
                            }";

            var variables = new
            {
                itemId = itemId,
                fieldId = fieldId,
                projectId = projectId,
                dateValue = dateValue
            };

            // Construct the request payload.
            var request = new
            {
                query = mutation,
                variables
            };

            // Serialize the request content.
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            // Post the request.
            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        // Get the ID of an organization project (for projects that belong to organizations rather than users)
        public async Task<string> GetOrgProjectIdAsync(string orgName, string projectNumber)
        {
            var query = @"
                query($organization: String!, $number: Int!) {
                  organization(login: $organization) {
                    projectV2(number: $number) {
                      id
                    }
                  }
                }";

            var variables = new
            {
                organization = orgName,
                number = int.Parse(projectNumber)
            };

            var request = new
            {
                query,
                variables
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse the response to extract the project ID
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("organization")
                .GetProperty("projectV2")
                .GetProperty("id")
                .GetString();
        }

        public async Task<string> GetUserIdAsync(string username)
        {
            var query = @"
        query($username: String!) {
          user(login: $username) {
            id
          }
        }";

            var variables = new { username };

            var request = new
            {
                query,
                variables
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(responseBody);
            return jsonDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetString();
        }

        public async Task<string> GetAllFieldsAsJson(string projectV2Id)
        {
            string query = @"
                {
                  node(id: """ + projectV2Id + @""") {
                    ... on ProjectV2 {
                      fields(first: 50) {
                        nodes {
                          ... on ProjectV2SingleSelectField {
                            id
                            name
                          }
                          ... on ProjectV2Field {
                            id
                            name
                          }
                        }
                      }
                    }
                  }
                }";

            var requestBody = new { query };
            var json = JsonSerializer.Serialize(requestBody);

            var response = await _httpClient.PostAsync("https://api.github.com/graphql",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw API Response: " + responseString);

            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseString);

                if (!doc.RootElement.TryGetProperty("data", out JsonElement dataElement) ||
                    !dataElement.TryGetProperty("node", out JsonElement nodeElement) ||
                    !nodeElement.TryGetProperty("fields", out JsonElement fieldsElement) ||
                    !fieldsElement.TryGetProperty("nodes", out JsonElement nodesElement))
                {
                    Console.WriteLine("Error: Unexpected JSON structure.");
                    return "{}"; // Return empty JSON if parsing fails
                }

                // Create a dictionary to hold field names and IDs
                var fieldsDict = new Dictionary<string, string>();

                foreach (var field in nodesElement.EnumerateArray())
                {
                    string id = field.GetProperty("id").GetString();
                    string name = field.GetProperty("name").GetString();
                    fieldsDict[name] = id;
                }

                // Convert dictionary to JSON and return
                return JsonSerializer.Serialize(fieldsDict, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
                return "{}"; // Return empty JSON if an error occurs
            }
        }

        public async Task<string> GetGithubRepoId(string repoOwner)
        {
            string query = @"
                {
                  repositoryOwner(login: """ + repoOwner + @""") {
                    repositories(first: 100) {
                      nodes {
                        id
                        name
                        url
                        description
                        isPrivate
                        createdAt
                      }
                    }
                  }
                }";

            // Prepare the request body
            var requestBody = new { query };
            var json = JsonSerializer.Serialize(requestBody);

            // Send the request to GitHub GraphQL API
            var response = await _httpClient.PostAsync(
                "https://api.github.com/graphql",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            // Read the response as a string
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw API Response: " + responseString);

            // Parse the JSON response
            using JsonDocument doc = JsonDocument.Parse(responseString);
            if (!doc.RootElement.TryGetProperty("data", out JsonElement dataElement) ||
                !dataElement.TryGetProperty("repositoryOwner", out JsonElement ownerElement) ||
                !ownerElement.TryGetProperty("repositories", out JsonElement reposElement) ||
                !reposElement.TryGetProperty("nodes", out JsonElement nodesElement))
            {
                Console.WriteLine("Error: Unexpected JSON structure.");
                return "{}"; // Return empty JSON in case of error
            }

            // Extract repository data and return as JSON
            var repositories = nodesElement.EnumerateArray();
            var repoList = new JsonElement[repositories.Count()];
            int index = 0;

            foreach (var repo in repositories)
            {
                repoList[index++] = repo;
            }

            return JsonSerializer.Serialize(repoList, new JsonSerializerOptions { WriteIndented = true });
        }


        // Extract issue ID from CreateIssueAsync response
        public string ExtractIssueId(string responseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("createIssue")
                .GetProperty("issue")
                .GetProperty("id")
                .GetString();
        }

        // Extract issue number from CreateIssueAsync response
        public int ExtractIssueNumber(string responseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("createIssue")
                .GetProperty("issue")
                .GetProperty("number")
                .GetInt32();
        }

        // Extract issue URL from CreateIssueAsync response
        public string ExtractIssueUrl(string responseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("createIssue")
                .GetProperty("issue")
                .GetProperty("url")
                .GetString();
        }
    }
}