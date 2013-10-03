using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringTableEditorModel
{
    public interface IModel : IEnumerable<StringTableSet>
    {
        #region Package
        /// <summary>
        /// When true, indicates that the model has been changed.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Create a new instance of the model.
        /// </summary>
        void New();

        /// <summary>
        /// Load an existing instance of the model from the supplied <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">A file containing a serialised instance of the model.</param>
        void Open(string filename);

        /// <summary>
        /// Release resources associated with the current model.
        /// </summary>
        void Close();

        /// <summary>
        /// Commit the current model to the file from which it was loaded.
        /// </summary>
        void Save();

        /// <summary>
        /// Commit the current model to an arbitrary filename
        /// </summary>
        /// <param name="filename">A file to contain a serialised instance of the current model.</param>
        void SaveAs(string filename);

        /// <summary>
        /// Raised when the model's Dirty status changes.
        /// </summary>
        event EventHandler IsDirtyChanged;

        /// <summary>
        /// Determines whether the provided <paramref name="filename"/> is valid
        /// for opening.
        /// </summary>
        /// <param name="filename">The filename to test.</param>
        /// <param name="save">True if validating a filename to save to.</param>
        /// <param name="reason">The reason the filename is not valid.</param>
        /// <returns>True if the model will be able to open the filename.</returns>
        bool IsValidFilename(string filename, bool save, out string reason);

        /// <summary>
        /// The current filename (or null, if no current filename).
        /// </summary>
        string Filename { get; }
        #endregion

        /// <summary>
        /// Retrieve the list of languages in a given string table set.
        /// </summary>
        /// <param name="iid">The string table set identifier.</param>
        /// <returns>The list of languages in the given string table set.</returns>
        IEnumerable<Language> Languages(ulong iid);

        /// <summary>
        /// Add a new StringTableSet with the given iid to the model.
        /// </summary>
        /// <param name="iid">The iid to use for the new StringTableSet.</param>
        void Add(ulong iid);

        /// <summary>
        /// Access the model by iid.
        /// </summary>
        /// <param name="iid">The iid of the StringTableSet to access.</param>
        /// <returns>A reference to a StringTableSet with the request iid, or null if not present in the model.</returns>
        StringTableSet this[ulong iid] { get; }
    }
}
