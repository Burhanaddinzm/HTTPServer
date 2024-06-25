"use strict";

function rng() {
  const randomNumber = Math.floor(Math.random() * 255 + 1);
  return randomNumber;
}

function randomColor() {
  return `rgb(${rng()}, ${rng()}, ${rng()})`;
}

const btnElement = document.getElementById('btn');
const bodyElement = document.querySelector('body');

btnElement.addEventListener('click', () => {
    bodyElement.style.backgroundColor = randomColor();
});