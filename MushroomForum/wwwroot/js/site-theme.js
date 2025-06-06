// Inicjalizacja Bootstrapowych dropdownów
var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
    return new bootstrap.Dropdown(dropdownToggleEl);
});

// Funkcja zmieniająca motyw
function toggleTheme() {
    const link = document.getElementById('themeStylesheet');
    const currentHref = link.getAttribute('href');
    const bannerImg = document.querySelector('.banner img');

    if (currentHref.includes('site.css') && !currentHref.includes('site2.css')) {
        link.setAttribute('href', '/css/site2.css');
        localStorage.setItem('theme', 'dark');
        if (bannerImg) {
            bannerImg.src = '/images/baner-dark.png';
        }
    } else {
        link.setAttribute('href', '/css/site.css');
        localStorage.setItem('theme', 'light');
        if (bannerImg) {
            bannerImg.src = '/images/baner-light.png';
        }
    }
}