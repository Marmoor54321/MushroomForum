const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

// Rozmiar klocka
const tileSize = 40;

// Pozycja klocka (gracza)
let player = {
    x: 0,
    y: 0
};

// Prędkość ruchu
const speed = tileSize;

// Funkcja rysująca klocka
function drawPlayer() {
    ctx.fillStyle = "blue"; // Kolor klocka
    ctx.fillRect(player.x, player.y, tileSize, tileSize); // Rysowanie klocka
}

// Funkcja rysująca planszę
function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height); // Czyści planszę
    drawPlayer(); // Rysuje klocka
}

// Funkcja obsługująca ruchy gracza
function movePlayer(e) {
    switch (e.key) {
        case "ArrowUp":
            if (player.y > 0) player.y -= speed; // Ruch w górę
            break;
        case "ArrowDown":
            if (player.y < canvas.height - tileSize) player.y += speed; // Ruch w dół
            break;
        case "ArrowLeft":
            if (player.x > 0) player.x -= speed; // Ruch w lewo
            break;
        case "ArrowRight":
            if (player.x < canvas.width - tileSize) player.x += speed; // Ruch w prawo
            break;
    }
    draw(); // Po ruchu rysujemy planszę na nowo
}

// Nasłuchiwanie na strzałki
document.addEventListener("keydown", movePlayer);

// Inicjalizacja gry
draw();
