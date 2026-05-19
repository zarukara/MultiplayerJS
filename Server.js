const dgram = require("dgram");

const server = dgram.createSocket("udp4");

const PORT = 7777;

const TICK_RATE = 20;
const TICK_DELTA_TIME = 1 / TICK_RATE;

const PLAYER_SPEED = 5;
const PLAYER_HP = 3;

const BULLET_SPEED = 8;
const BULLET_LIFE_TIME = 2;
const HIT_RADIUS = 0.5;

const players = {};
const bullets = {};

const spawnPoints = [
    { x: -4, y: 0 },
    { x: 4, y: 0 }
];

server.on("listening", () => {
    console.log("Server started on port " + PORT);
});

server.on("message", (msg, rinfo) => {
    const data = JSON.parse(msg.toString());

    if (!players[data.id]) {
        const playerCount = Object.keys(players).length;
        const spawn = spawnPoints[playerCount % spawnPoints.length];

        players[data.id] = {
            id: data.id,
            x: spawn.x,
            y: spawn.y,
            inputX: 0,
            inputY: 0,
            hp: PLAYER_HP,
            isDead: false,
            address: rinfo.address,
            port: rinfo.port
        };

        console.log("Player joined: " + data.id);
    }

    const player = players[data.id];

    player.address = rinfo.address;
    player.port = rinfo.port;

    if (data.type === "input") {
        handleInput(player, data);
    }

    if (data.type === "shoot") {
        handleShoot(player, data);
    }

    if (data.type === "restart") {
        restartPlayer(player);
    }
});

function handleInput(player, data) {
    if (player.isDead) {
        player.inputX = 0;
        player.inputY = 0;
        return;
    }

    player.inputX = data.inputX;
    player.inputY = data.inputY;
}

function handleShoot(player, data) {
    if (player.isDead) {
        return;
    }

    const length = Math.sqrt(data.dirX * data.dirX + data.dirY * data.dirY);

    if (length <= 0) {
        return;
    }

    const dirX = data.dirX / length;
    const dirY = data.dirY / length;

    const bulletId = "bullet_" + Date.now() + "_" + Math.random();

    bullets[bulletId] = {
        id: bulletId,
        ownerId: player.id,
        x: player.x,
        y: player.y,
        dirX: dirX,
        dirY: dirY,
        lifeTime: BULLET_LIFE_TIME
    };

    console.log("Player shoot: " + player.id);
}

function restartPlayer(player) {
    const playerIds = Object.keys(players);
    const playerIndex = playerIds.indexOf(player.id);
    const spawn = spawnPoints[playerIndex % spawnPoints.length];

    player.x = spawn.x;
    player.y = spawn.y;
    player.inputX = 0;
    player.inputY = 0;
    player.hp = PLAYER_HP;
    player.isDead = false;

    console.log("Player restarted: " + player.id);
}

function updatePlayers() {
    for (let id in players) {
        const player = players[id];

        if (player.isDead) {
            continue;
        }

        player.x += player.inputX * PLAYER_SPEED * TICK_DELTA_TIME;
        player.y += player.inputY * PLAYER_SPEED * TICK_DELTA_TIME;
    }
}

function updateBullets() {
    for (let id in bullets) {
        const bullet = bullets[id];

        bullet.x += bullet.dirX * BULLET_SPEED * TICK_DELTA_TIME;
        bullet.y += bullet.dirY * BULLET_SPEED * TICK_DELTA_TIME;

        bullet.lifeTime -= TICK_DELTA_TIME;

        if (bullet.lifeTime <= 0) {
            delete bullets[id];
            continue;
        }

        checkBulletHit(id, bullet);
    }
}

function checkBulletHit(bulletId, bullet) {
    for (let playerId in players) {
        const player = players[playerId];

        if (player.id === bullet.ownerId) {
            continue;
        }

        if (player.isDead) {
            continue;
        }

        const dx = player.x - bullet.x;
        const dy = player.y - bullet.y;

        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance <= HIT_RADIUS) {
            player.hp -= 1;

            delete bullets[bulletId];

            console.log("Player hit: " + player.id + " HP: " + player.hp);

            if (player.hp <= 0) {
                player.isDead = true;
                player.inputX = 0;
                player.inputY = 0;

                console.log("Player lost: " + player.id);
            }

            break;
        }
    }
}

function sendState() {
    const state = JSON.stringify({
        type: "state",
        players: players,
        bullets: bullets
    });

    for (let id in players) {
        server.send(
            state,
            players[id].port,
            players[id].address
        );
    }
}

function gameLoop() {
    updatePlayers();
    updateBullets();
    sendState();
}

setInterval(gameLoop, 1000 / TICK_RATE);

server.bind(PORT);