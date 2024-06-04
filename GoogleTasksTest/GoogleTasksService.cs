using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;

namespace GoogleTasksTest;

public class InMemoryDataStore : IDataStore
{
	private readonly Dictionary<string, string> data = new Dictionary<string, string>();

	public System.Threading.Tasks.Task ClearAsync()
	{
		data.Clear();
		return System.Threading.Tasks.Task.CompletedTask;
	}

	public System.Threading.Tasks.Task DeleteAsync<T>(string key)
	{
		data.Remove(key);
		return System.Threading.Tasks.Task.CompletedTask;
	}

	public Task<T> GetAsync<T>(string key)
	{
		if (data.TryGetValue(key, out string value))
		{
			return System.Threading.Tasks.Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value));
		}
		return System.Threading.Tasks.Task.FromResult(default(T));
	}

	public System.Threading.Tasks.Task StoreAsync<T>(string key, T value)
	{
		data[key] = Newtonsoft.Json.JsonConvert.SerializeObject(value);
		return System.Threading.Tasks.Task.CompletedTask;
	}
}

//this works for desktop access
public class GoogleTasksService
{
	static string[] scopes = [TasksService.Scope.Tasks];
	static string ApplicationName = "CategoriesApp2";

	public static async Task<TasksService> GetTasksServiceAsync()
	{
		var clientSecrets = new ClientSecrets
		{
			//desktop
			ClientId = "49731328120-r4ust0vecq8em7nbi2mdcgvl1ohsckhi.apps.googleusercontent.com",
			ClientSecret = "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"
		};

		UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
			clientSecrets,
			scopes,
			"user",
			CancellationToken.None,
			new InMemoryDataStore()
		);

		return new TasksService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = credential,
			ApplicationName = ApplicationName,
		});
	}

	public static async Task<List<TaskList>> GetTasksAsync()
	{
		var service = await GoogleTasksService.GetTasksServiceAsync();

		var tasksListsRequest = service.Tasklists.List();
		// listele de tasks: Categoriile 
		var taskLists = (await tasksListsRequest.ExecuteAsync()).Items.ToList();

		//get tasks in a category
		var tasksInFamilyRequest = service.Tasks.List("RzJHYUFfSXpSenhFY1NqTQ");
		//task-urile din categoria Family
		var tasksInFamilyCateg = (await tasksInFamilyRequest.ExecuteAsync()).Items.ToList();

		return taskLists;
	}
}
