using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

// Alias our models to avoid conflicts with Microsoft.Azure.Cosmos.User
using HubUser  = WellbeingHub.Models.User;
using HubGroup = WellbeingHub.Models.Group;
using HubItem  = WellbeingHub.Models.MarketplaceItem;

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

            // Fresh DB name to avoid any containers previously created with the wrong PK
            var database = _client.CreateDatabaseIfNotExistsAsync("WellbeingHubDb2").Result;

            // All containers use lowercase /id as the partition key path
            _userContainer         = database.Database.CreateContainerIfNotExistsAsync("Users",            "/id").Result;
            _groupContainer        = database.Database.CreateContainerIfNotExistsAsync("Groups",           "/id").Result;
            _marketplaceContainer  = database.Database.CreateContainerIfNotExistsAsync("MarketplaceItems", "/id").Result;
        }

        // ---------- Users ----------
        public async Task AddUserAsync(HubUser user) =>
            await _userContainer.CreateItemAsync(user, new PartitionKey(user.id));

        public async Task<HubUser?> GetUserByEmailAsync(string email)
        {
            var q = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
                .WithParameter("@email", email);

            var it = _userContainer.GetItemQueryIterator<HubUser>(q);
            while (it.HasMoreResults)
            {
                var page = await it.ReadNextAsync();
                var match = page.FirstOrDefault();
                if (match != null) return match;
            }
            return null;
        }

        // ---------- Groups ----------
        public async Task AddGroupAsync(HubGroup group) =>
            await _groupContainer.CreateItemAsync(group, new PartitionKey(group.id));

        public async Task<List<HubGroup>> GetGroupsByLocationAsync(string location)
        {
            var q = new QueryDefinition("SELECT * FROM c WHERE c.Location = @location")
                .WithParameter("@location", location);

            var it = _groupContainer.GetItemQueryIterator<HubGroup>(q);
            var output = new List<HubGroup>();
            while (it.HasMoreResults)
            {
                var page = await it.ReadNextAsync();
                output.AddRange(page);
            }
            return output;
        }

        // ---------- Marketplace ----------
        public async Task AddMarketplaceItemAsync(HubItem item) =>
            await _marketplaceContainer.CreateItemAsync(item, new PartitionKey(item.id));

        public async Task<List<HubItem>> GetAllMarketplaceItemsAsync()
        {
            var q = new QueryDefinition("SELECT * FROM c");
            var it = _marketplaceContainer.GetItemQueryIterator<HubItem>(q);
            var output = new List<HubItem>();
            while (it.HasMoreResults)
            {
                var page = await it.ReadNextAsync();
                output.AddRange(page);
            }
            return output;
        }
    }
}
