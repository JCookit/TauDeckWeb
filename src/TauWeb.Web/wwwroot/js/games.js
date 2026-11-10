const chips = Array.from(document.querySelectorAll("[data-zone]"));
const cells = Array.from(document.querySelectorAll("[data-zones]"));

function setActiveZone(zoneId) {
  chips.forEach((chip) => chip.classList.toggle("is-active", chip.dataset.zone === zoneId));
  cells.forEach((cell) => {
    const zones = (cell.dataset.zones || "").split(/\s+/);
    cell.classList.toggle("is-highlighted", zones.includes(zoneId));
  });
}

chips.forEach((chip) => {
  chip.addEventListener("click", () => {
    const isActive = chip.classList.contains("is-active");
    setActiveZone(isActive ? "" : chip.dataset.zone);
  });
});
