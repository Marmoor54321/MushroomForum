using MushroomForum.Models;

namespace MushroomForum.Services
{
    public class CiekawostkiService
    {
        private readonly List<Ciekawostka> _ciekawostki = new()
        {
            new Ciekawostka { Tresc = "Zobaczcie jakiego grzyba znalazł Pan Mirosław! Robi wrażenieeee", Url = "/images/ciekawostki/1.jpg" },
            new Ciekawostka { Tresc = "Kara za przekroczenie limitu zbiorów!!!", Url = "/images/ciekawostki/2.jpg" },
            new Ciekawostka { Tresc = "Muchomor czerwony nie jest grzybem jadalnym!", Url = "/images/ciekawostki/3.jpg" },
            new Ciekawostka { Tresc = "Oto niezawodny sposób na rozróżnienie grzybów jadalnych i trujących!!!", Url = "/images/ciekawostki/4.jpg" },
            new Ciekawostka { Tresc = "Jak grzybiarze mogą pomóc dbac o środowisko?", Url = "/images/ciekawostki/5.jpg" },
            new Ciekawostka { Tresc = "Zbieranie napotkanych śmieci podczas grzybobrania to bardzo dobra ppraktyka :)", Url = "/images/ciekawostki/6.jpg" },
            new Ciekawostka { Tresc = "Spożywanie niektórych grzybów może mieć bardzo ciekawe efekty uboczne...", Url = "/images/ciekawostki/7.jpg" }
        };

        public IReadOnlyList<Ciekawostka> GetAll() => _ciekawostki;

        public Ciekawostka GetCiekawostkaNaDzien()
        {
            int total = _ciekawostki.Count;
            var today = DateTime.Today;
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            int weekOfYear = cal.GetWeekOfYear(today, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            int index = weekOfYear % total;
            return _ciekawostki[index];
        }
    }
}
