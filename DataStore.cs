using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

// Alias our models to avoid conflicts with Microsoft.Azure.Cosmos.User
using HubUser = WellbeingHub.Models.User;
using HubGroup = WellbeingHub.Models.Group;
using HubItem = WellbeingHub.Models.MarketplaceItem;

namespace WellbeingHub
{
    public class DataStore
    {
        private readonly CosmosClient _client;
        private readonly Container _userContainer;
        private readonly Container _groupContainer;
        private readonly Container _marketplaceContainer;

        public DataStore(CosmosClient client)
        {
            _client = client;
            var database = _client.CreateDatabaseIfNotExistsAsync("WellbeingHubDb").Result;
            _userContainer = database.Database.CreateContainerIfNotExistsAsync("Users", "/id").Result;
            _groupContainer = database.Database.CreateContainerIfNotExistsAsync("Groups", "/id").Result;
            _marketplaceContainer = database.Database.CreateContainerIfNotExistsAsync("MarketplaceItems", "/id").Result;
        }

        public async Task AddUserAsync(HubUser user) =>
            await _userContainer.CreateItemAsync(user, new PartitionKey(user.id.ToString()));

        public async Task<HubUser?> GetUserByEmailAsync(string email)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
                .WithParameter("@email", email);
            var iterator = _userContainer.GetItemQueryIterator<HubUser>(query);
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                var match = results.FirstOrDefault();
                if (match != null) return match;
            }
            return null;
        }

        public async Task AddGroupAsync(HubGroup group) =>
            await _groupContainer.CreateItemAsync(group, new PartitionKey(group.id.ToString()));

        public async Task<List<HubGroup>> GetGroupsByLocationAsync(string location)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Location = @location")
                .WithParameter("@location", location);
            var iterator = _groupContainer.GetItemQueryIterator<HubGroup>(query);
            var output = new List<HubGroup>();
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                output.AddRange(page);
            }
            return output;
        }

        public async Task AddMarketplaceItemAsync(HubItem item) =>
            await _marketplaceContainer.CreateItemAsync(item, new PartitionKey(item.id.ToString()));

        public async Task<List<HubItem>> GetAllMarketplaceItemsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _marketplaceContainer.GetItemQueryIterator<HubItem>(query);
            var output = new List<HubItem>();
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                output.AddRange(page);
            }
            return output;
        }
    }
}
