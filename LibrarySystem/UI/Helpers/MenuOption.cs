
namespace LibrarySystem.UI.Helpers
{
    public class MenuOption
    {
        public string Description { get; set; }
        public Action Action { get; set; }

        public MenuOption(string description, Action action)
        {
            Description = description;
            Action = action;
        }
    }
}
