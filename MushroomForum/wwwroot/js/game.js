const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");
const tileSize = 40;
const rows = 10;
const cols = 10;

let player = { x: 1, y: 1 };
let mushrooms = [];
let score = 0;

const map = [
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
    [1, 0, 0, 0, 1, 0, 0, 0, 0, 1],
    [1, 0, 1, 0, 1, 0, 1, 1, 0, 1],
    [1, 0, 1, 0, 0, 0, 0, 1, 0, 1],
    [1, 0, 1, 1, 1, 1, 0, 1, 0, 1],
    [1, 0, 0, 0, 0, 1, 0, 1, 0, 1],
    [1, 1, 1, 1, 0, 1, 0, 1, 0, 1],
    [1, 0, 0, 1, 0, 1, 0, 0, 0, 1],
    [1, 0, 0, 0, 0, 0, 0, 1, 0, 1],
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1]
];

function drawMap() {
    for (let y = 0; y < rows; y++) {
        for (let x = 0; x < cols; x++) {
            if (map[y][x] === 1) {
                ctx.fillStyle = "#3b5d3b";
                ctx.fillRect(x * tileSize, y * tileSize, tileSize, tileSize);
            }
        }
    }
}

function drawPlayer() {
    ctx.fillStyle = "blue";
    ctx.beginPath();
    ctx.arc(
        player.x * tileSize + tileSize / 2,
        player.y * tileSize + tileSize / 2,
        tileSize / 2 - 4,
        0,
        Math.PI * 2
    );
    ctx.fill();
}

function drawMushrooms() {
    ctx.fillStyle = "red";
    mushrooms.forEach(m => {
        ctx.beginPath();
        ctx.arc(
            m.x * tileSize + tileSize / 2,
            m.y * tileSize + tileSize / 2,
            6,
            0,
            Math.PI * 2
        );
        ctx.fill();
    });
}

function placeMushrooms() {
    mushrooms = [];
    for (let i = 0; i < 10; i++) {
        let x, y;
        do {
            x = Math.floor(Math.random() * cols);
            y = Math.floor(Math.random() * rows);
        } while (map[y][x] === 1 || (x === player.x && y === player.y));
        mushrooms.push({ x, y });
    }
}

function update() {
    // Check if player is on a mushroom
    mushrooms = mushrooms.filter(m => {
        if (m.x === player.x && m.y === player.y) {
            score++;
            document.getElementById("score").innerText = score;
            return false;
        }
        return true;
    });

    if (mushrooms.length === 0) placeMushrooms();
}

function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    drawMap();
    drawPlayer();
    drawMushrooms();
}

function gameLoop() {
    update();
    draw();
    requestAnimationFrame(gameLoop);
}

document.addEventListener("keydown", (e) => {
    let newX = player.x;
    let newY = player.y;

    if (e.key === "ArrowUp") newY--;
    if (e.key === "ArrowDown") newY++;
    if (e.key === "ArrowLeft") newX--;
    if (e.key === "ArrowRight") newX++;

    if (map[newY][newX] === 0) {
        player.x = newX;
        player.y = newY;
    }
});

placeMushrooms();
gameLoop();
