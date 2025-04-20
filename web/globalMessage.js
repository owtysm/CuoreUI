const message = "10'000 DOWNLOADS !!!";

let isChristmas = new Date().getMonth() === 11; // thanks javascript for offsetting months by -1!!! (AGAIN)

const snowflakes = [
    "./assets/christmas/snowflakes/1.png",
    "./assets/christmas/snowflakes/2.png",
    "./assets/christmas/snowflakes/3.png"
];

function createSnowflake() {
    let snowflakeOverlay = document.querySelector('.snowflake-overlay');

    if (!snowflakeOverlay) {
        snowflakeOverlay = document.createElement('div');
        snowflakeOverlay.className = 'snowflake-overlay';
        document.body.appendChild(snowflakeOverlay);
    }

    const snowflake = document.createElement('img');
    snowflake.classList.add('snowflake');

    const randomIndex = Math.floor(Math.random() * snowflakes.length);
    snowflake.src = snowflakes[randomIndex];

    const size = Math.random() * 30 + 10; // min 10
    snowflake.style.width = `${size}px`;
    snowflake.style.height = `${size}px`;

    const position = Math.random() * 100;
    snowflake.style.left = `${position}vw`;

    const animationDuration = 0.75 + Math.random() * 4;
    snowflake.style.animationDuration = `${animationDuration}s`;

    const animationDelay = Math.random() * 0.5;
    snowflake.style.animationDelay = `${animationDelay}s`;

    // add snowflake
    snowflakeOverlay.appendChild(snowflake);

    // delete every snowflake after some time
    setTimeout(() => {
        snowflake.remove();
    }, animationDuration * 2000);
}

document.addEventListener('DOMContentLoaded', () => {
    const messageLabel = document.querySelector('#globalmessage');
    if (messageLabel) {
        messageLabel.textContent = message;
    }

    loadHolidayEffect();
});

function loadHolidayEffect()
{
    if (isChristmas)
    {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.type = 'text/css';
        link.href = './assets/christmas/style.css';
        document.head.appendChild(link);

        setInterval(createSnowflake, 300);
    }
}

// for anyone that wants to enable the visuals when it is not christmas :)
// from console ofc
function enableChristmasMode()
{
    isChristmas = true;
    loadHolidayEffect();
}
