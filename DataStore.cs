using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using WellbeingHub.Models;

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

        public async Task AddUserAsync(User user) =>
            await _userContainer.CreateItemAsync(user, new PartitionKey(user.Id.ToString()));

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email").WithParameter("@email", email);
            var iterator = _userContainer.GetItemQueryIterator<User>(query);
            var results = await iterator.ReadNextAsync();
            return results.FirstOrDefault();
        }

        public async Task AddGroupAsync(Group group) =>
            await _groupContainer.CreateItemAsync(group, new PartitionKey(group.Id.ToString()));

        public async Task<List<Group>> GetGroupsByLocationAsync(string location)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Location = @location").WithParameter("@location", location);
            var iterator = _groupContainer.GetItemQueryIterator<Group>(query);
            var results = await iterator.ReadNextAsync();
            return results.ToList();
        }

        public async Task AddMarketplaceItemAsync(MarketplaceItem item) =>
            await _marketplaceContainer.CreateItemAsync(item, new PartitionKey(item.Id.ToString()));

        public async Task<List<MarketplaceItem>> GetAllMarketplaceItemsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _marketplaceContainer.GetItemQueryIterator<MarketplaceItem>(query);
            var results = await iterator.ReadNextAsync();
            return results.ToList();
        }
    }
}