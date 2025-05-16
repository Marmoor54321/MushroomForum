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

// Obrazek ludzika
const playerImage = new Image();
playerImage.src = "/js/textures/player.png"; // podaj ścieżkę do obrazka

// Funkcja rysująca ludzika
function drawPlayer() {
    if (playerImage.complete) {
        ctx.drawImage(playerImage, player.x, player.y, tileSize, tileSize);
    } else {
        // Jeżeli obrazek jeszcze się nie załadował, rysujemy niebieski kwadrat
        ctx.fillStyle = "blue";
        ctx.fillRect(player.x, player.y, tileSize, tileSize);
    }
}

// Funkcja rysująca planszę
function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    drawPlayer();
}

// Funkcja obsługująca ruchy gracza
function movePlayer(e) {
    switch (e.key) {
        case "ArrowUp":
            if (player.y > 0) player.y -= speed;
            break;
        case "ArrowDown":
            if (player.y < canvas.height - tileSize) player.y += speed;
            break;
        case "ArrowLeft":
            if (player.x > 0) player.x -= speed;
            break;
        case "ArrowRight":
            if (player.x < canvas.width - tileSize) player.x += speed;
            break;
    }
    draw();
}

// Nasłuchiwanie na strzałki
document.addEventListener("keydown", movePlayer);

// Po załadowaniu obrazka narysuj planszę
playerImage.onload = () => {
    draw();
};
