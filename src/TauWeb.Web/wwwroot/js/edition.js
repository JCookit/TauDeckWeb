export const edition = document.body.dataset.edition;
export const assetRoot = document.body.dataset.assetRoot;

export function editionUrl(path) {
  const url = new URL(path, window.location.origin);
  url.searchParams.set("edition", edition);
  return `${url.pathname}${url.search}${url.hash}`;
}

export function standardDeckGlyph(value) {
  return value ? '<span class="standard-deck-mark" aria-hidden="true"></span>' : "";
}

export function suitShapeGlyph(shape, color) {
  const shapeClass = shape === "Hard" ? "hard" : "soft";
  const suitColor = ["Black", "Red", "Blue", "Yellow", "Green"].includes(color) ? color : "";
  return `<span class="suit-shape-mark ${shapeClass}" data-suit-color="${suitColor}" aria-hidden="true"></span>`;
}

export function iconHalfGlyph(image) {
  const src = String(image ?? "").replaceAll("&", "&amp;").replaceAll('"', "&quot;").replaceAll("<", "&lt;");
  return `<span class="icon-half-pair" aria-hidden="true"><img class="icon-half-bar" src="${src}" alt=""><img class="icon-half-bar is-mirrored" src="${src}" alt=""></span>`;
}

export function updateEditionLinks() {
  for (const link of document.querySelectorAll("[data-switch-edition]")) {
    const url = new URL(window.location.href);
    const target = link.dataset.switchEdition;
    url.searchParams.set("edition", target);
    if (target === "v1") {
      url.searchParams.delete("iconHalf");
      url.searchParams.delete("isStandardDeck");
    }
    link.href = `${url.pathname}${url.search}`;
  }
}
