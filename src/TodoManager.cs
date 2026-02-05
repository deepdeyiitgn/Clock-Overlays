using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Thread-safe manager for to-do items.
    /// Encapsulates all business logic for CRUD operations on TodoItem objects.
    /// Does not expose UI objects or dependencies.
    /// Designed for WinForms UI thread usage.
    /// </summary>
    public class TodoManager
    {
        private readonly List<TodoItem> todos;
        private readonly object syncLock = new object();

        /// <summary>
        /// Initializes a new instance of the TodoManager class with an empty to-do list.
        /// </summary>
        public TodoManager()
        {
            todos = new List<TodoItem>();
        }

        /// <summary>
        /// Initializes a new instance of the TodoManager class with an existing list of to-do items.
        /// Creates a copy of the input list to prevent external mutation.
        /// </summary>
        /// <param name="initialTodos">The initial collection of to-do items. Can be null (treated as empty).</param>
        public TodoManager(IEnumerable<TodoItem> initialTodos)
        {
            todos = initialTodos != null ? new List<TodoItem>(initialTodos) : new List<TodoItem>();
        }

        /// <summary>
        /// Adds a new to-do item with the given title.
        /// </summary>
        /// <param name="title">The title of the to-do item. Validation is caller's responsibility.</param>
        /// <returns>The created TodoItem with auto-generated ID and CreatedAt timestamp.</returns>
        /// <exception cref="ArgumentNullException">Thrown if title is null.</exception>
        public TodoItem AddTodo(string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            var newTodo = new TodoItem
            {
                Title = title,
                IsCompleted = false
            };

            lock (syncLock)
            {
                todos.Add(newTodo);
            }

            return newTodo;
        }

        /// <summary>
        /// Toggles the completion status of a to-do item by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the to-do item to toggle.</param>
        /// <returns>True if the item was found and toggled; false if the item was not found.</returns>
        public bool ToggleTodo(Guid id)
        {
            lock (syncLock)
            {
                var todo = todos.FirstOrDefault(t => t.Id == id);
                if (todo == null)
                    return false;

                todo.IsCompleted = !todo.IsCompleted;
                return true;
            }
        }

        /// <summary>
        /// Removes a to-do item by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the to-do item to remove.</param>
        /// <returns>True if the item was found and removed; false if the item was not found.</returns>
        public bool RemoveTodo(Guid id)
        {
            lock (syncLock)
            {
                var todo = todos.FirstOrDefault(t => t.Id == id);
                if (todo == null)
                    return false;

                todos.Remove(todo);
                return true;
            }
        }

        /// <summary>
        /// Gets a read-only snapshot of all to-do items.
        /// Safe for external iteration without lock contention.
        /// </summary>
        /// <returns>An immutable copy of the current to-do list.</returns>
        public IReadOnlyList<TodoItem> GetAllTodos()
        {
            lock (syncLock)
            {
                return todos.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the count of to-do items currently managed.
        /// </summary>
        /// <returns>The number of to-do items.</returns>
        public int Count
        {
            get
            {
                lock (syncLock)
                {
                    return todos.Count;
                }
            }
        }
    }
}
