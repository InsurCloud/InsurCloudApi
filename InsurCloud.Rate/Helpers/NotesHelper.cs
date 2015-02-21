using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;

namespace Helpers
{
    public static class NotesHelper
    {
        public static bool HasErrorNotes(List<clsBaseNote> notes, out string msg)
        {
            msg = string.Empty;            
            foreach (clsBaseNote note in notes)
            {
                if (note.SourceCode.ToUpper().Trim() == "NEI" || note.SourceCode.ToUpper().Trim() == "IER")
                {               
                    msg = (msg == string.Empty) ? note.NoteText : string.Concat(msg, " -- ", note.NoteText);
                }
            }
            if (msg != string.Empty)
            {
                return true;
            }
            return false;
        }
        
        public static void RemoveNotes(List<clsBaseNote> notes, string sourceCode)
        {
            for (int i = notes.Count - 1; i > 0; i--)
            {
                if (notes[i].SourceCode.ToUpper().Trim() == sourceCode.ToUpper().Trim())
                {
                    notes.RemoveAt(i);
                }
            }
        }

        public static void AddNote(clsPolicyPPA pol, string noteText, string noteDesc, string sourceCode)
        {
            bool addNote = true;
            //Check to make sure the note doesn't already exist
            foreach (clsBaseNote note in pol.Notes)
            {
                if (note.NoteDesc.ToUpper().Trim() == noteDesc.ToUpper().Trim() && note.SourceCode.ToUpper().Trim() == sourceCode.ToUpper().Trim())
                {
                    addNote = false;
                    break;
                }
            }

            if (addNote)
            {
                clsBaseNote note = new clsBaseNote();
                note.NoteText = noteText;
                note.NoteDesc = noteDesc;
                note.SourceCode = sourceCode;
                note.SystemTS = DateTime.Now;
                note.UserID = pol.CallingSystem;
                note.IndexNum = pol.Notes.Count + 1;
                note.IsNew = true;
                pol.Notes.Add(note);
            }
        }

        public static clsBaseNote FindNoteByDescriptionOnly(clsPolicyPPA pol, string noteDesc)
        {
            foreach (clsBaseNote note in pol.Notes)
            {
                if (note.NoteDesc.ToUpper() == noteDesc.ToUpper())
                {
                    return note;
                }
            }
            return null;
        }
        public static clsBaseNote FindNote(clsPolicyPPA pol, string sourceCode, string noteDesc = ""){
            foreach (clsBaseNote note in pol.Notes)
            {
                if (note.SourceCode.ToUpper() == sourceCode.ToUpper() && (noteDesc == "" || note.NoteDesc.ToUpper() == noteDesc.ToUpper()))
                {
                    return note;
                }
            }
            return null;
        }
    }
}
