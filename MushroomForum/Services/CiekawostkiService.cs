namespace MushroomForum.Services
{
    public class CiekawostkiService
    {
        private readonly List<string> ciekawostki = new List<string>
    {
        "Grzyby tworzą złożone sieci mikoryzowe z drzewami, które pomagają im w wymianie składników odżywczych.",
        "Niektóre gatunki grzybów są bioluminescencyjne i świecą w ciemności.",
        "Największy organizm świata to grzyb, którego podziemna sieć pokrywa kilka kilometrów kwadratowych.",
        "Niektóre grzyby potrafią rozkładać plastik i inne trudne do strawienia materiały.",
        // dodaj więcej ciekawostek...
    };

        public IReadOnlyList<string> GetAll() => ciekawostki;
        public string GetCiekawostkaNaDzien()
        {
            int total = ciekawostki.Count;
            var today = DateTime.Today;
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            int weekOfYear = cal.GetWeekOfYear(today, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            int index = weekOfYear % total;
            return ciekawostki[index];
        }
    }

}
