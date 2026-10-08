using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NoteServiceApi.Models;
using NoteServiceApi.Service;

namespace NoteServiceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly ILogger<NoteController> _logger;
        private INoteService _noteService;

        public NoteController(ILogger<NoteController> logger, INoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        } 


        [HttpGet("AllNotes")]
        public async Task<ActionResult<MethodResponse<List<Note>>>> GetAllNotes()
        {
            var resp = await _noteService.GetAllNotes();
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }


        [HttpGet("GetNotesForOwner/{ownerId:int}")]
        public async Task<ActionResult<MethodResponse<List<Note>>>> GetForOwner(int ownerId)
        {
            var resp = await _noteService.GetNotesForOwner(ownerId);
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }

        [HttpGet("GetNoteById/{noteId:int}")]
        public async Task<ActionResult<MethodResponse<Note>>> GetById(int noteId)
        {
            var resp = await _noteService.GetNoteById(noteId);
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }

        [HttpPost("add")]
        public async Task<ActionResult<MethodResponse<int>>> Add([FromBody] Note note)
        {
            var resp = await _noteService.AddNote(note);
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }


        [HttpPut("update/{ownerId:int}")]
        public async Task<ActionResult<MethodResponse<int>>> Update([FromBody] Note note)        
        {
            var resp = await _noteService.UpdateNote(note);
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }

        [HttpDelete("delete/{ownerId:int}")]
        public async Task<ActionResult<MethodResponse<int>>> DeleteById([FromBody] Note note)
        {
            var resp = await _noteService.UpdateNote(note);
            if (!resp.Success)
                return BadRequest(resp.Message);
            else
                return resp;
        }

    }
}
