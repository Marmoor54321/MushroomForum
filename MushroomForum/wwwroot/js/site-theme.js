// Inicjalizacja Bootstrapowych dropdownów
var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
    return new bootstrap.Dropdown(dropdownToggleEl);
});

// Funkcja zmieniająca motyw
function toggleTheme() {
    const link = document.getElementById('themeStylesheet');
    const currentHref = link.getAttribute('href');

    if (currentHref.includes('site.css')) {
        link.setAttribute('href', '/css/site2.css');
        localStorage.setItem('theme', 'dark');
    } else {
        link.setAttribute('href', '/css/site.css');
        localStorage.setItem('theme', 'light');
    }
}

// Po załadowaniu strony ustawiamy zapamiętany motyw
window.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme');
    const link = document.getElementById('themeStylesheet');

    if (savedTheme === 'dark') {
        link.setAttribute('href', '/css/site2.css');
    }
});