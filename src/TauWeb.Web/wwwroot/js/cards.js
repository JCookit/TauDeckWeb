import { editionUrl, assetRoot, standardDeckGlyph, iconHalfGlyph, suitShapeGlyph, updateEditionLinks } from "./edition.js";
import { mobileSheet, phoneLayout, previewSheetLayout } from "./mobile-sheet.js";
const cards = JSON.parse(document.getElementById("cards-json")?.textContent ?? "[]");
const filterGroups = JSON.parse(document.getElementById("filter-groups-json")?.textContent ?? "[]");
const filterGroupsByValue = new Map(filterGroups.map(group => [group.filterValue, group]));
const params = new URLSearchParams(window.location.search);
const filters = {};
const noneValue = "__none__";
const noneFilterKeys = new Set(["sphereZodiac", "borderZodiac"]);
let sortKey = "deckOrder";
let sortDir = 1;
let selected = defaultSorted(cards)[0] ?? null;

const columns = [
  ["sequence", "#"],
  ["name", "Card"],
  ["suitName", "Suit"],
  ["rank", "Rank"],
  ["sphereNumber", "Sphere #"],
  ["sphere", "Sphere"],
  ["modeNumber", "Mode #"],
  ["mode", "Mode"],
  ["sphereZodiac", "Sphere zodiac"],
  ["firstLetter", "Letter"],
  ["letterScore", "Score"],
  ["linkWords", "Links"],
  ["linkInDegree", "In-links"],
  ["iconGroup", "Icon group"],
  ["partitionGroup", "Partition"],
  ["iconNumber", "Icon"],
  ["direction", "Dir"],
  ["totalBorderScore", "Border"],
  ["borderArrangement", "Arrangement"],
  ["borderZodiac", "Border zodiac"]
];

if (cards.some(card => card.iconHalf != null)) {
  columns.push(["iconHalf", "Icon half"], ["isStandardDeck", "Standard deck"]);
}

const filterElements = [...document.querySelectorAll("[data-filter]")];
const suitSelect = document.querySelector('[data-filter="suitName"]');
let combinedSuitOption;
const table = document.querySelector("[data-card-table]");
const grid = document.querySelector("[data-card-grid]");
const selectedPanel = document.querySelector("[data-selected-card]");
const count = document.querySelector("[data-result-count]");
const moreFilters = document.querySelector("[data-more-filters]");
const openFilters = document.querySelector("[data-open-filters]");
const showCards = document.querySelector("[data-show-cards]");
const filtersSheet = mobileSheet(document.querySelector(".filters"), "Card filters");
const cardSheet = mobileSheet(selectedPanel, "Card preview", { layout: previewSheetLayout, lightDismiss: true });
openFilters.addEventListener("click", () => filtersSheet.open(() => openFilters));
showCards.addEventListener("click", () => filtersSheet.close());

for (const element of filterElements) {
  const key = element.dataset.filter;
  populateSelect(element, key);
  element.value = params.get(key) ?? "";
  filters[key] = element.value;
  element.addEventListener("input", () => {
    if (element === suitSelect) {
      Object.assign(filters, suitChoice(element.value));
    } else {
      filters[key] = element.value;
    }
    updateUrl();
    render();
  });
}

for (const [key, value] of params.entries()) {
  if (!["edition", "sphere", "mode"].includes(key) && !(key in filters)) {
    filters[key] = value;
  }
}

updateFilterStates();

if (moreFilters && [...moreFilters.querySelectorAll("[data-filter]")].some(element => element.value)) {
  moreFilters.open = true;
}

for (const heading of document.querySelectorAll("[data-sort]")) {
  heading.addEventListener("click", () => {
    const key = heading.dataset.sort;
    sortDir = sortKey === key ? sortDir * -1 : 1;
    sortKey = key;
    render();
  });
}

document.querySelector("[data-clear-filters]")?.addEventListener("click", () => {
  for (const key of Object.keys(filters)) {
    filters[key] = "";
  }
  for (const element of filterElements) {
    element.value = "";
  }
  updateUrl();
  if (moreFilters) {
    moreFilters.open = false;
  }
  render();
});

updateEditionLinks();
render();

function populateSelect(element, key) {
  if (element.tagName !== "SELECT") {
    return;
  }

  for (const value of filterValues(key)) {
    element.append(new Option(filterLabel(key, value), value));
  }
}

function filterValues(key) {
  const values = key === "sequenceDigit"
    ? []
    : [...new Set(cards.map(card => normalizedFilterValue(card, key)).filter(Boolean))].sort(compare);
  const groupValues = filterGroups
    .filter(group => group.filterKey === key || (key === "suitName" && group.filterKey === "suitShape"))
    .sort((a, b) => compare(a.sortOrder, b.sortOrder) || compare(a.name, b.name))
    .map(group => group.filterValue);

  return ["", ...values, ...groupValues];
}

function suitChoice(value) {
  return filterGroupsByValue.get(value)?.filterKey === "suitShape"
    ? { suitName: "", suitShape: value }
    : { suitName: value, suitShape: "" };
}

function filterLabel(key, value) {
  if (value === "") {
    return "Any";
  }

  if (value === noneValue) {
    return "None";
  }

  const group = filterGroupsByValue.get(value);
  if (group) {
    return `Group: ${group.name}`;
  }

  if (key === "isTarot" || key === "isStandardDeck") {
    return value === "true" ? "Yes" : "No";
  }

  if (key === "direction") {
    return directionDisplay(value);
  }

  if (key === "sphereNumber" || key === "modeNumber") {
    const card = cards.find(item => String(item[key]) === value);
    const name = key === "sphereNumber" ? card?.sphere : card?.mode;
    return name ? `${value}: ${name}` : value;
  }

  if (key === "iconNumber") {
    const card = cards.find(item => String(item.iconNumber) === value);
    return card ? `${value} ${card.iconName}` : value;
  }

  return value;
}

function filteredCards(overrides = {}) {
  const q = (filters.q ?? "").trim().toLowerCase();
  return cards.filter(card => {
    if (q) {
      const haystack = [
        card.name,
        card.word,
        card.suitName,
        card.rankTheme,
        card.sphere,
        card.sphereFullName,
        card.mode,
        card.sphereZodiac,
        card.borderZodiac,
        card.direction,
        card.linkWords,
        card.iconName,
        card.iconDescription,
        card.borderArrangement
      ].join(" ").toLowerCase();
      if (!haystack.includes(q)) {
        return false;
      }
    }

    return Object.entries({ ...filters, ...overrides }).every(([key, value]) => {
      if (!value || key === "q") {
        return true;
      }
      return matchesFilterValue(card, key, value);
    });
  });
}

function normalizedFilterValue(card, key) {
  if (key === "linkWord") {
    return "";
  }

  const value = card[key];
  if (noneFilterKeys.has(key)) {
    return value === null || value === undefined || value === "" ? noneValue : String(value);
  }
  return String(value ?? "");
}

function matchesFilterValue(card, key, value) {
  const group = filterGroupsByValue.get(String(value));
  if (group) {
    return matchesGroup(card, group);
  }

  if (key === "linkWord") {
    const expected = String(value).toLowerCase();
    return [card.linkWord1, card.linkWord2].some(link => String(link ?? "").toLowerCase() === expected);
  }

  return normalizedFilterValue(card, key) === String(value);
}

function matchesGroup(card, group) {
  const filter = group.filter ?? {};
  if (filter.hasBorderSide) {
    switch (String(filter.hasBorderSide).toLowerCase()) {
      case "top":
        return Number(card.borderTop) > 0;
      case "right":
        return Number(card.borderRight) > 0;
      case "bottom":
        return Number(card.borderBottom) > 0;
      case "left":
        return Number(card.borderLeft) > 0;
      default:
        return false;
    }
  }

  const value = normalizedFilterValue(card, filter.field);
  if (filter.equalTo) {
    return value.toLowerCase() === String(filter.equalTo).toLowerCase();
  }

  if (Array.isArray(filter.in) && filter.in.length) {
    return filter.in.some(item => value.toLowerCase() === String(item).toLowerCase());
  }

  return false;
}

function facetCount(key, value) {
  const overrides = key === "suitName" ? suitChoice(value) : { [key]: value };
  return filteredCards(overrides).length;
}

function render() {
  const visible = sortCards(filteredCards());
  count.textContent = visible.length.toString();
  const activeFilters = Object.values(filters).filter(Boolean).length;
  openFilters.textContent = activeFilters ? `Filters (${activeFilters})` : "Filters";
  showCards.textContent = `Show ${visible.length} ${visible.length === 1 ? "card" : "cards"}`;
  updateSelectOptions(visible.length);
  renderTable(visible);
  renderGrid(visible);
  if (selected && !visible.some(card => card.id === selected.id)) {
    selected = visible[0] ?? null;
  }
  updateFilterStates();
  renderSelected();
  if (!phoneLayout.matches) scrollSelectedIntoView();
}

function sortCards(items) {
  if (sortKey === "deckOrder") {
    return defaultSorted(items);
  }

  return [...items].sort((a, b) => compare(a[sortKey], b[sortKey]) * sortDir || compareDeckOrder(a, b));
}

function defaultSorted(items) {
  return [...items].sort(compareDeckOrder);
}

function compareDeckOrder(a, b) {
  return compare(a.suit, b.suit) || compare(a.rank, b.rank);
}

function renderTable(visible) {
  table.replaceChildren(...visible.map(card => {
    const row = document.createElement("tr");
    row.dataset.cardId = card.id;
    row.className = selected?.id === card.id ? "is-selected" : "";
    row.innerHTML = columns.map(([key]) => `<td>${escapeHtml(displayValue(card, key))}</td>`).join("");
    row.addEventListener("click", () => selectCard(card));
    return row;
  }));
}

function displayValue(card, key) {
  if (key === "iconNumber") {
    return card.iconNumber ? `${card.iconNumber} ${card.iconName}` : "";
  }

  if (key === "direction") {
    return directionDisplay(card.direction);
  }

  if (key === "isStandardDeck") return card.isStandardDeck ? "Yes" : "No";
  return card[key] ?? "";
}

function renderGrid(visible) {
  grid.replaceChildren(...visible.map(card => {
    const button = document.createElement("button");
    button.type = "button";
    button.dataset.cardId = card.id;
    button.className = `thumb${selected?.id === card.id ? " is-selected" : ""}`;
    button.innerHTML = `<img src="${card.thumbnailImage}" alt=""><span>${escapeHtml(card.name)}</span>`;
    if (phoneLayout.matches) button.firstElementChild.loading = "lazy";
    button.addEventListener("click", () => selectCard(card));
    return button;
  }));
}

function selectCard(card) {
  selected = card;
  render();
  cardSheet.open(() => grid.querySelector(".is-selected"));
}

function renderSelected() {
  if (!selected) {
    selectedPanel.innerHTML = "<p>No cards match the current filters.</p>";
    return;
  }

  selectedPanel.innerHTML = `
    <img src="${selected.displayImage}" alt="">
    <h2>${escapeHtml(selected.name)}</h2>
    <div class="filter-actions" aria-label="Selected card filters">
      ${filterAction("rank", selected.rank, "Rank", selected.rank, imageGlyph(selected.glyphImages?.rank, "Rank", selected.rank))}
      ${filterAction("suitName", selected.suitName, "Suit", selected.suitName, imageGlyph(selected.glyphImages?.suit, "Suit", selected.suitName))}
      ${selected.suitShape ? filterAction("suitShape", `group:suits:${selected.suitShape.toLowerCase()}`, "Suit shape", selected.suitShape, suitShapeGlyph(selected.suitShape, selected.color)) : ""}
      ${selected.sequenceDigit ? filterAction("sequenceDigit", `group:sequence-numbers:ending-${selected.sequenceDigit}`, "Last digit", "", textGlyph(selected.sequenceDigit), selected.sequenceDigit) : ""}
      ${filterAction("sphereNumber", selected.sphereNumber, "Sphere", `${selected.sphere} (${selected.sphereNumber})`, imageGlyph(selected.glyphImages?.sphere, "Sphere", selected.sphereNumber))}
      ${filterAction("modeNumber", selected.modeNumber, "Mode", `${selected.mode} (${selected.modeNumber})`, imageGlyph(selected.glyphImages?.mode, "Mode", selected.modeNumber))}
      ${filterAction("sphereZodiac", filterValue(selected.sphereZodiac), "Zodiac", displayIfAny(selected.sphereZodiac), imageOrEmpty(selected.glyphImages?.zodiac, "Zodiac"), displayOrNone(selected.sphereZodiac))}
      ${selected.firstLetter ? filterAction("firstLetter", selected.firstLetter, "Letter", "", textGlyph(selected.firstLetter)) : ""}
      ${filterAction("letterScore", selected.letterScore, "Score", selected.letterScore, imageGlyph(selected.glyphImages?.letterScore, "Letter score", selected.letterScore))}
      ${filterAction("totalBorderScore", selected.totalBorderScore, "Border", selected.totalBorderScore, imageGlyph(selected.glyphImages?.borderScore, "Border score", selected.totalBorderScore))}
      ${filterAction("borderArrangement", selected.borderArrangement, "Sides", "", borderGlyph(selected))}
      ${filterAction("borderZodiac", filterValue(selected.borderZodiac), "B. Zodiac", displayIfAny(selected.borderZodiac), imageOrEmpty(selected.glyphImages?.borderZodiac, "Border zodiac"), displayOrNone(selected.borderZodiac))}
      ${filterAction("direction", selected.direction, "Dir", directionDisplay(selected.direction), imageGlyph(selected.glyphImages?.direction, "Direction", directionDisplay(selected.direction)), directionDisplay(selected.direction))}
      ${filterAction("partitionGroup", selected.partitionGroup, "Part", selected.partitionGroup, partitionGlyph(selected.partitionGroup, selected.iconGroup))}
      ${filterAction("linkInDegree", selected.linkInDegree, "In-links", selected.linkInDegree, inLinkGlyph(selected.linkInDegree))}
      ${selected.iconHalf != null ? filterAction("iconHalf", selected.iconHalf, "Half", selected.iconHalf, iconHalfGlyph(selected.glyphImages?.iconHalf)) : ""}
      ${selected.isStandardDeck != null ? filterAction("isStandardDeck", selected.isStandardDeck, "Standard", selected.isStandardDeck ? "Yes" : "No", standardDeckGlyph(selected.isStandardDeck)) : ""}
      ${filterAction("iconGroup", selected.iconGroup, "Group", selected.iconGroup, imageGlyph(selected.glyphImages?.iconGroup, "Icon group", selected.iconGroup))}
      ${filterAction("isTarot", selected.isTarot, "Tarot", selected.isTarot ? "Yes" : "No", selected.isTarot ? tauGlyph() : "")}
      ${filterAction("iconNumber", selected.iconNumber, "Icon", `${selected.iconNumber} ${selected.iconName}`, imageGlyph(selected.glyphImages?.icon, "Icon", selected.iconNumber))}
    </div>
    <a class="button primary" href="${editionUrl(`/cards/${selected.slug}`)}">Open detail</a>`;

  for (const link of selectedPanel.querySelectorAll("[data-quick-filter]")) {
    link.addEventListener("click", () => {
      const key = link.dataset.key;
      const value = link.dataset.value;
      const element = document.querySelector(`[data-filter="${key}"]`);
      const nextValue = filters[key] === value ? "" : value;
      if (nextValue && (key === "suitName" || key === "suitShape")) {
        Object.assign(filters, suitChoice(nextValue));
      } else {
        if (element) element.value = nextValue;
        filters[key] = nextValue;
      }
      updateUrl();
      render();
      if (phoneLayout.matches) {
        cardSheet.close(() => openFilters);
        window.scrollTo(0, 0);
      } else {
        cardSheet.close();
      }
    });
  }
}

function updateFilterStates() {
  for (const element of filterElements) {
    element.classList.toggle("is-filtered", Boolean(element.value));
  }

  if (moreFilters && [...moreFilters.querySelectorAll("[data-filter]")].some(element => element.value)) {
    moreFilters.open = true;
  }
}

function updateSelectOptions(resultCount) {
  combinedSuitOption?.remove();
  for (const element of filterElements) {
    if (element.tagName !== "SELECT") {
      continue;
    }

    const key = element.dataset.filter;
    for (const option of element.options) {
      const optionCount = facetCount(key, option.value);
      option.textContent = `${filterLabel(key, option.value)} (${optionCount})`;
      option.disabled = false;
    }
  }

  if (filters.suitName && filters.suitShape) {
    // Matrix links can carry both constraints. Show their intersection as the current choice.
    const suit = filterLabel("suitName", filters.suitName);
    const shape = filterGroupsByValue.get(filters.suitShape)?.name ?? filters.suitShape;
    combinedSuitOption = new Option(`${suit} · ${shape} (${resultCount})`, "combined-suit", false, true);
    combinedSuitOption.disabled = true;
    suitSelect.add(combinedSuitOption);
  } else {
    suitSelect.value = filters.suitName || filters.suitShape || "";
  }
}

function filterAction(key, value, caption, display, visual, labelDisplay = display) {
  if (value === null || value === undefined || value === "") {
    return "";
  }

  const isActive = filters[key] === String(value);
  const label = `${caption}: ${labelDisplay}`;
  return `<button class="filter-action${isActive ? " is-filter-active" : ""}" type="button" data-quick-filter data-key="${key}" data-value="${escapeAttr(value)}" aria-label="${escapeAttr(label)}" aria-pressed="${isActive ? "true" : "false"}" title="${escapeAttr(label)}"><span class="filter-action-caption">${escapeHtml(caption)}</span><span class="filter-action-visual">${visual}</span><span class="filter-action-value">${escapeHtml(display)}</span></button>`;
}

function imageGlyph(image, label, fallback = "") {
  return image
    ? `<img src="${escapeAttr(image)}" alt="${escapeAttr(label)}">`
    : textGlyph(fallback);
}

function imageOrEmpty(image, label) {
  return image ? `<img src="${escapeAttr(image)}" alt="${escapeAttr(label)}">` : "";
}

function textGlyph(value) {
  return `<span class="filter-action-text">${escapeHtml(value)}</span>`;
}

function tauGlyph() {
  return `<span class="filter-action-text tau-glyph">τ</span>`;
}

function directionDisplay(value) {
  return String(value ?? "").toUpperCase();
}

function inLinkGlyph(value) {
  const count = Number(value) || 0;
  return `<span class="in-link-count-art" aria-hidden="true">${Array.from({ length: count }, () => `<img src="${assetRoot}/glyphs/indegree.webp" alt="">`).join("")}</span>`;
}

function partitionGlyph(value, iconGroup) {
  const filledCount = Number(value) || 0;
  const family = Math.min(Math.max(Number(iconGroup) || 1, 1), 3);
  return `<span class="partition-art" aria-hidden="true">${Array.from({ length: 1 }, () => `<span class="partition-row">${Array.from({ length: 5 }, (_, index) => {
    const state = index < filledCount ? "1" : "0";
    return `<img src="${assetRoot}/glyphs/partition${family}_${state}.webp" alt="">`;
  }).join("")}</span>`).join("")}</span>`;
}

function filterValue(value) {
  return value === null || value === undefined || value === "" ? noneValue : value;
}

function displayOrNone(value) {
  return value === null || value === undefined || value === "" ? "None" : value;
}

function displayIfAny(value) {
  return value === null || value === undefined || value === "" ? "" : value;
}

function borderGlyph(card) {
  const classes = [
    "border-mini",
    card.borderTop > 0 ? "has-top" : "",
    card.borderRight > 0 ? "has-right" : "",
    card.borderBottom > 0 ? "has-bottom" : "",
    card.borderLeft > 0 ? "has-left" : ""
  ].filter(Boolean).join(" ");

  return `<span class="${classes}"><span class="chevrons top"></span><span class="chevrons right"></span><span class="chevrons bottom"></span><span class="chevrons left"></span></span>`;
}

function scrollSelectedIntoView() {
  if (!selected) {
    return;
  }

  const selector = `[data-card-id="${selected.id}"]`;
  scrollWithin(document.querySelector(".table-wrap"), document.querySelector(`tbody ${selector}`), document.querySelector(".card-table thead"));
  grid.querySelector(selector)?.scrollIntoView({ block: "nearest", inline: "nearest" });
}

function scrollWithin(container, item, header = null) {
  if (!container || !item) {
    return;
  }

  const margin = 8;
  const containerRect = container.getBoundingClientRect();
  const itemRect = item.getBoundingClientRect();
  const headerHeight = header?.getBoundingClientRect().height ?? 0;
  const topLimit = containerRect.top + headerHeight + margin;
  const bottomLimit = containerRect.bottom - margin;

  if (itemRect.top < topLimit) {
    container.scrollTop -= topLimit - itemRect.top;
  } else if (itemRect.bottom > bottomLimit) {
    container.scrollTop += itemRect.bottom - bottomLimit;
  }
}

function quickFilter(key, value, label) {
  if (value === null || value === undefined || value === "") {
    return "";
  }
  return `<button class="pill" type="button" data-quick-filter data-key="${key}" data-value="${escapeAttr(value)}">${label}</button>`;
}

function updateUrl() {
  const next = new URLSearchParams();
  for (const [key, value] of Object.entries(filters)) {
    if (value) {
      next.set(key, value);
    }
  }
  const query = next.toString();
  history.replaceState(null, "", editionUrl(query ? `/cards?${query}` : "/cards"));
  updateEditionLinks();
}

function compare(a, b) {
  const aNumber = Number(a);
  const bNumber = Number(b);
  if (!Number.isNaN(aNumber) && !Number.isNaN(bNumber)) {
    return aNumber - bNumber;
  }

  return String(a ?? "").localeCompare(String(b ?? ""), undefined, { numeric: true });
}

function escapeHtml(value) {
  const div = document.createElement("div");
  div.textContent = String(value ?? "");
  return div.innerHTML;
}

function escapeAttr(value) {
  return escapeHtml(value).replaceAll('"', "&quot;");
}
