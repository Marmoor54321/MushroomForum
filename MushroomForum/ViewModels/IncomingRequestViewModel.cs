namespace MushroomForum.ViewModels
{
    public class IncomingRequestViewModel
    {
        public string Id { get; set; }              // ID zaproszenia
        public string SenderId { get; set; }        // ID użytkownika wysyłającego zaproszenie
        public string SenderUserName { get; set; }  // Nazwa użytkownika
        public string AvatarIcon { get; set; }      // Nazwa pliku z ikoną
    }
}
