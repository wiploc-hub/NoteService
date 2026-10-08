using Microsoft.Data.SqlClient;
using NoteServiceApi.Models;
using System.Net;

namespace NoteServiceApi.Service
{
    public interface INoteService
    {
        Task<MethodResponse<List<Owner>>> GetAllOwners();

        Task<MethodResponse<List<Note>>> GetAllNotes();
        Task<MethodResponse<Note>> GetNoteById(int noteId);
        Task<MethodResponse<List<Note>>> GetNotesForOwner(int ownerId);
        Task<MethodResponse<int>> AddNote(Note note);
        Task<MethodResponse<int>> UpdateNote(Note note);
        Task<MethodResponse<int>> DeleteNoteById(int noteId);
    }

    public class NoteService : INoteService
    {
        private IDatabaseService _databaseService = null!;

        public NoteService(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }


        public async Task<MethodResponse<List<Owner>>> GetAllOwners()
        {
            var response = new MethodResponse<List<Owner>>() { Success = true, Message = "", Result = null };
            try
            {
                var sql = $"SELECT * FROM [Owner] ORDER BY Id asc";
                var dbResp = await _databaseService.ExecuteReader<Owner>(sql);
                response.SetFailedIfSubFailed(dbResp);
                response.Result = dbResp.Result;
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }


        public async Task<MethodResponse<List<Note>>> GetAllNotes()
        {
            var response = new MethodResponse<List<Note>>() { Success = true, Message = "", Result = null };
            try
            {
                var sql = $"SELECT * FROM [Note] ORDER BY Id asc";
                var dbResp = await _databaseService.ExecuteReader<Note>(sql);
                response.SetFailedIfSubFailed(dbResp);
                response.Result = dbResp.Result;
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }

        public async Task<MethodResponse<Note>> GetNoteById(int noteId)
        {
            var response = new MethodResponse<Note>() { Success = true, Message = "", Result = null };
            try
            {
                var sql = $"SELECT * FROM [Note] WHERE Id = {noteId}";
                var dbResp = await _databaseService.ExecuteReader<Note>(sql);
                response.SetFailedIfSubFailed(dbResp);
                response.Result = dbResp.Result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }


        public async Task<MethodResponse<List<Note>>> GetNotesForOwner(int ownerId)
        {
            var response = new MethodResponse<List<Note>>() { Success = true, Message = "", Result = null };
            try
            {
                var sql = $"SELECT * FROM [Note] WHERE OwnerId = ({ownerId}) ORDER BY Id asc";
                var dbResp = await _databaseService.ExecuteReader<Note>(sql);
                response.SetFailedIfSubFailed(dbResp);
                response.Result = dbResp.Result;
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }


        private List<SqlParameter> BuildParametersFromNoteObject(Note note, bool includeId, bool includeCreated, bool includeModified, bool includeActive)
        {
            var sqlParams = new List<SqlParameter>();
            if (includeId)
            {
                sqlParams.Add(new SqlParameter("@Id", note.Id));
            }
            sqlParams.Add(new SqlParameter("@OwnerId", note.OwnerId));
            sqlParams.Add(new SqlParameter("@Title", note.Title));
            sqlParams.Add(new SqlParameter("@Text", note.Text));
            if (includeCreated)
            {
                sqlParams.Add(new SqlParameter("@CreatedBy", note.CreatedBy));
                sqlParams.Add(new SqlParameter("@CreatedOn", note.CreatedOn));
            }
            if (includeModified)
            {
                sqlParams.Add(new SqlParameter("@LastModifiedBy", note.LastModifiedBy));
                sqlParams.Add(new SqlParameter("@LastModified", note.LastModified));
            }
            if (includeActive)
            {
                sqlParams.Add(new SqlParameter("@Active", note.Active));
            }
            return sqlParams;
        }

        public async Task<MethodResponse<int>> AddNote(Note note)
        {
            var response = new MethodResponse<int>() { Success = true, Message = "", Result = 0 };
            try
            {
                /*
                        public int Id { get; set; }
                        public int OwnerId { get; set; }
                        public string Title { get; set; } = string.Empty;
                        public string Text { get; set; } = string.Empty;


                        public string CreatedBy { get; set; } = string.Empty;
                        public DateTime CreatedOn { get; set; }
                        public string LastModifiedBy { get; set; } = string.Empty;
                        public DateTime LastModified { get; set; }

                        public bool Active { get; set; } */


                if (note == null)
                {
                    response.Success = false;
                    response.Message = $"Note is null, cannot add";
                }
                else
                {
                    var sql = $"INSERT INTO [Note] ([OwnerId], [Title], [Text], [CreatedBy], [CreatedOn], [LastModifiedBy], [LastModified], [Active]) VALUES (@OwnerId, @Title, @Text, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModified, @Active)";
                    var sqlParams = BuildParametersFromNoteObject(note, false, true, true, true);
                    var dbResp = await _databaseService.ExecuteInsert(sql, sqlParams);
                    response.SetFailedIfSubFailed(dbResp);
                    response.Result = dbResp.Result;
                }
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }

        public async Task<MethodResponse<int>> UpdateNote(Note note)
        {
            var response = new MethodResponse<int>() { Success = true, Message = "", Result = 0 };
            try
            {
                if (note == null || note.Id == 0)
                {
                    response.Success = false;
                    response.Message = $"Note is null or ID not populated";
                }
                else
                {
                    var sql = $"UPDATE [Note] SET [OwnerId] = @OwnerId, [Title] = @Title, [Text] = @Text, [LastModifiedBy] = @LastModifiedBy, [LastModified] = @LastModified, [Active] =  @Active WHERE Id = {note.Id}";
                    var sqlParams = BuildParametersFromNoteObject(note, false, true, true, true);
                    var dbResp = await _databaseService.ExecuteNonQuery(sql, sqlParams);
                    response.SetFailedIfSubFailed(dbResp);
                    response.Result = dbResp.Result;
                }
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }


        public async Task<MethodResponse<int>> DeleteNoteById(int noteId)
        {
            var response = new MethodResponse<int>() { Success = true, Message = "", Result = 0 };
            try
            {
                if (noteId == 0)
                {
                    response.Success = false;
                    response.Message = $"No note ID sent in to delete";
                }
                else
                {
                    var sql = $"DELETE FROM [Note] WHERE Id = {noteId}";
                    var dbResp = await _databaseService.ExecuteNonQuery(sql, null);
                    response.SetFailedIfSubFailed(dbResp);
                    response.Result = dbResp.Result;
                }
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }

    }
}
