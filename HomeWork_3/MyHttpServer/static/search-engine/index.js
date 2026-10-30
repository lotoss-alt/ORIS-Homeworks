const searchForm = document.querySelector('.search');
const searchInput = searchForm.querySelector('.search__input');
const clockElement = document.querySelector('.greeting__time');

function updateClock() {
    if (!clockElement) return;
    const now = new Date();
    clockElement.textContent = now.toLocaleTimeString('ru-RU', { hour12: false });
}

updateClock();
setInterval(updateClock, 1000);

searchForm.addEventListener('submit', () => {
    setTimeout(() => {
        searchInput.value = '';
        searchInput.blur();
    }, 10);
});
