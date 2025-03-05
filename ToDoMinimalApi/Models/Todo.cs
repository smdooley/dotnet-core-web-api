namespace ToDoMinimalApi.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }

        // The secret field needs to be hidden from this app, but an administrative app could choose to expose it.
        public string? Secret { get; set; }
    }
}