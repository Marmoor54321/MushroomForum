const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");
const tileSize = 40;

const maze = [
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
    [1, 0, 0, 0, 1, 0, 0, 0, 0, 1],
    [1, 0, 1, 0, 1, 0, 1, 1, 0, 1],
    [1, 0, 1, 0, 0, 0, 0, 1, 0, 1],
    [1, 0, 1, 1, 1, 1, 0, 1, 0, 1],
    [1, 0, 0, 0, 0, 1, 0, 1, 0, 1],
    [1, 1, 1, 1, 0, 1, 0, 1, 0, 1],
    [1, 0, 0, 1, 0, 0, 0, 0, 0, 1],
    [1, 0, 0, 0, 0, 1, 1, 1, 0, 1],
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
];

// Pozycja gracza w indeksach planszy
let player = { x: 1, y: 1 };
const playerImage = new Image();
playerImage.src = "/js/textures/player.png";
function drawMaze() {
    for (let y = 0; y < maze.length; y++) {
        for (let x = 0; x < maze[y].length; x++) {
            if (maze[y][x] === 1) {
                ctx.fillStyle = "black";
                ctx.fillRect(x * tileSize, y * tileSize, tileSize, tileSize);
            } else {
                ctx.fillStyle = "white";
                ctx.fillRect(x * tileSize, y * tileSize, tileSize, tileSize);
            }
        }
    }
}

function drawPlayer() {
    if (playerImage.complete) {
        ctx.drawImage(playerImage, player.x * tileSize, player.y * tileSize, tileSize, tileSize);
    } else {
        // Jeżeli obrazek jeszcze się nie załadował, rysujemy niebieski kwadrat
        ctx.fillStyle = "blue";
        ctx.fillRect(player.x, player.y, tileSize, tileSize);
    }
}

function draw() {
    drawMaze();
    drawPlayer();
}

function movePlayer(e) {
    let newX = player.x;
    let newY = player.y;

    switch (e.key) {
        case "ArrowUp":
            newY--;
            break;
        case "ArrowDown":
            newY++;
            break;
        case "ArrowLeft":
            newX--;
            break;
        case "ArrowRight":
            newX++;
            break;
    }

    // Sprawdzenie czy można iść (czy pole jest puste)
    if (maze[newY] && maze[newY][newX] === 0) {
        player.x = newX;
        player.y = newY;
    }

    draw();
}

document.addEventListener("keydown", movePlayer);

draw();