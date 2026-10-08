namespace NoteServiceWeb;


public interface IApiClient
{
    Task<List<Note>> GetNotes();
}

public class ApiClient(HttpClient httpClient) : IApiClient
{

    public async Task<List<Note>> GetNotes()
    {
        var list = new List<Note>();
        await foreach (var nextItem in httpClient.GetFromJsonAsAsyncEnumerable<Note>("https://localhost:44386/notes"))
        {
            if (nextItem != null)
                list.Add(nextItem);
        }

        return list;
    }

}
