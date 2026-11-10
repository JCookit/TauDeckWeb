import { editionUrl, assetRoot, standardDeckGlyph, iconHalfGlyph, suitShapeGlyph } from "./edition.js";
import { mobileSheet, previewSheetLayout } from "./mobile-sheet.js";
const cards = JSON.parse(document.getElementById("cards-json")?.textContent ?? "[]");
const filterGroups = JSON.parse(document.getElementById("filter-groups-json")?.textContent ?? "[]");
const filterGroupsByValue = new Map(filterGroups.map(group => [group.filterValue, group]));
const noneValue = "__none__";
const noneFilterKeys = new Set(["sphereZodiac", "borderZodiac"]);
const zodiacSigns = ["Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"];
const planets = ["Mercury", "Venus", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto", "Moon", "Sun"];

const rowSelect = document.querySelector("[data-row-axis]");
const columnSelect = document.querySelector("[data-column-axis]");
const matrixHead = document.querySelector("[data-matrix-head]");
const matrixBody = document.querySelector("[data-matrix-body]");
const cardGrid = document.querySelector("[data-matrix-card-grid]");
const selectedPanel = document.querySelector("[data-matrix-selected-card]");
const cellTitle = document.querySelector("[data-cell-title]");
const cellCount = document.querySelector("[data-cell-count]");
const openBrowser = document.querySelector("[data-open-browser]");
const resultsSheet = mobileSheet(document.querySelector(".matrix-results"), "Matching cards");
const cardSheet = mobileSheet(selectedPanel, "Card preview", { layout: previewSheetLayout, lightDismiss: true });

const axes = buildAxes();
let rowAxis = axes.find(axis => axis.key === "suit") ?? axes[0];
let columnAxis = axes.find(axis => axis.key === "rank") ?? axes[1] ?? axes[0];
let selectedCell = null;
let selectedCard = null;

for (const axis of axes) {
  rowSelect.append(new Option(axis.name, axis.key));
  columnSelect.append(new Option(axis.name, axis.key));
}
rowSelect.value = rowAxis.key;
columnSelect.value = columnAxis.key;

rowSelect.addEventListener("input", () => {
  rowAxis = axisByKey(rowSelect.value);
  selectedCell = null;
  selectedCard = null;
  render();
});

columnSelect.addEventListener("input", () => {
  columnAxis = axisByKey(columnSelect.value);
  selectedCell = null;
  selectedCard = null;
  render();
});

document.querySelector("[data-swap-axes]")?.addEventListener("click", () => {
  [rowAxis, columnAxis] = [columnAxis, rowAxis];
  rowSelect.value = rowAxis.key;
  columnSelect.value = columnAxis.key;
  selectedCell = null;
  selectedCard = null;
  render();
});

document.querySelector("[data-clear-cell]")?.addEventListener("click", () => {
  selectedCell = null;
  selectedCard = null;
  renderSelection();
  renderMatrixSelection();
});

render();

function buildAxes() {
  return [
    directAxis("suit", "Suit", "suitName", card => card.suitName, card => card.suit),
    groupAxis("suit-colors", "Suit Colors", "suits", filterGroups
      .filter(group => group.systemKey === "suits" && group.filterKey === "suitName")
      .map(group => group.slug)),
    groupAxis("suit-shape", "Suit Shape", "suits", ["hard", "soft"]),
    groupAxis("sequence-digit", "Sequence Last Digit", "sequence-numbers"),
    directAxis("rank", "Rank", "rank", card => String(card.rank), card => card.rank),
    groupAxis("rank-bands", "Rank Bands", "ranks"),
    directAxis("sphere", "Sphere", "sphereNumber", card => card.sphereNumber, card => card.sphereNumber, (value, card) => `${value}: ${card.sphere}`),
    directAxis("mode", "Mode", "modeNumber", card => card.modeNumber, card => card.modeNumber, (value, card) => `${value}: ${card.mode}`),
    groupAxis("mode-families", "Mode Families", "modes"),
    directAxis("sphere-zodiac", "Sphere Zodiac", "sphereZodiac", card => normalizedFilterValue(card, "sphereZodiac"), card => zodiacSort(normalizedFilterValue(card, "sphereZodiac")), displayOrNone),
    groupAxis("sphere-zodiac-groups", "Sphere Zodiac Groups", "sphere-zodiac"),
    directAxis("direction", "Direction", "direction", card => card.direction, card => directionSort(card.direction), directionDisplay),
    directAxis("first-letter", "First Letter", "firstLetter", card => card.firstLetter, card => card.firstLetter),
    directAxis("letter-score", "Letter Score", "letterScore", card => String(card.letterScore), card => card.letterScore),
    directAxis("border-score", "Border Score", "totalBorderScore", card => String(card.totalBorderScore), card => card.totalBorderScore),
    directAxis("border-arrangement", "Border Arrangement", "borderArrangement", card => card.borderArrangement, card => borderArrangementSort(card.borderArrangement)),
    groupAxis("border-sides", "Border Sides", "border-arrangements", ["any-top", "any-right", "any-bottom", "any-left"]),
    groupAxis("border-count", "Border Count", "border-arrangements", ["one-border", "two-borders", "three-borders", "four-borders", "two-opposite-borders", "two-adjacent-borders"]),
    directAxis("border-zodiac", "Border Zodiac", "borderZodiac", card => normalizedFilterValue(card, "borderZodiac"), card => zodiacSort(normalizedFilterValue(card, "borderZodiac")), displayOrNone),
    groupAxis("border-zodiac-groups", "Border Zodiac Groups", "border-zodiac"),
    directAxis("tarot", "Tarot", "isTarot", card => String(Boolean(card.isTarot)), card => card.isTarot ? 0 : 1, value => value === "true" ? "Yes" : "No"),
    directAxis("icon-group", "Icon Group", "iconGroup", card => String(card.iconGroup), card => card.iconGroup),
    directAxis("partition", "Partition", "partitionGroup", card => String(card.partitionGroup), card => card.partitionGroup),
    ...(cards.some(card => card.iconHalf != null) ? [
      directAxis("icon-half", "Icon Half", "iconHalf", card => String(card.iconHalf), card => card.iconHalf),
      directAxis("standard-deck", "Standard Deck", "isStandardDeck", card => String(card.isStandardDeck), card => card.isStandardDeck ? 0 : 1, value => value === "true" ? "Yes" : "No")
    ] : []),
    directAxis("in-links", "In-links", "linkInDegree", card => String(card.linkInDegree), card => card.linkInDegree)
  ].filter(axis => axis.values.length > 0);
}

function directAxis(key, name, filterKey, valueSelector, sortSelector, labelSelector = value => value) {
  const byValue = new Map();
  for (const card of cards) {
    const value = String(valueSelector(card) ?? "");
    if (!value) {
      continue;
    }
    if (!byValue.has(value)) {
      byValue.set(value, {
        key: `${key}:${value}`,
        label: labelSelector(value, card),
        filterKey,
        filterValue: value,
        sortOrder: sortSelector(card),
        predicate: item => normalizedFilterValue(item, filterKey) === value
      });
    }
  }

  return {
    key,
    name,
    values: [...byValue.values()].sort((a, b) => compare(a.sortOrder, b.sortOrder) || compare(a.label, b.label))
  };
}

function groupAxis(key, name, systemKey, slugs = null) {
  const allowedSlugs = slugs ? new Set(slugs) : null;
  const values = filterGroups
    .filter(group => group.systemKey === systemKey && (!allowedSlugs || allowedSlugs.has(group.slug)))
    .sort((a, b) => compare(a.sortOrder, b.sortOrder) || compare(a.name, b.name))
    .map(group => ({
      key: group.filterValue,
      label: group.name,
      filterKey: group.filterKey,
      filterValue: group.filterValue,
      sortOrder: group.sortOrder,
      predicate: card => matchesGroup(card, group)
    }));
  if (key === "border-sides" || key === "border-count") {
    values.unshift({
      key: `${key}:none`,
      label: key === "border-sides" ? "None" : "0 borders",
      filterKey: "borderArrangement",
      filterValue: "None",
      sortOrder: -1,
      predicate: card => card.borderCount === 0
    });
  }
  return {
    key,
    name,
    values
  };
}

function render() {
  renderMatrix();
  renderSelection();
}

function renderMatrix() {
  const heatmap = matrixHeatmap();
  matrixHead.innerHTML = `<tr><th>${escapeHtml(rowAxis.name)} / ${escapeHtml(columnAxis.name)}</th>${columnAxis.values.map(value => `<th>${escapeHtml(value.label)}</th>`).join("")}<th class="matrix-total-heading">Total</th></tr>`;
  const rows = rowAxis.values.map(rowValue => {
    const row = document.createElement("tr");
    row.innerHTML = `<th>${escapeHtml(rowValue.label)}</th>${columnAxis.values.map(columnValue => cellHtml(rowValue, columnValue, "cell", heatmap)).join("")}${cellHtml(rowValue, null, "row-total", heatmap)}`;
    bindMatrixCells(row);
    return row;
  });

  const totalRow = document.createElement("tr");
  totalRow.className = "matrix-total-row";
  totalRow.innerHTML = `<th>Total</th>${columnAxis.values.map(columnValue => cellHtml(null, columnValue, "column-total", heatmap)).join("")}${cellHtml(null, null, "grand-total", heatmap)}`;
  bindMatrixCells(totalRow);

  matrixBody.replaceChildren(...rows, totalRow);
}

function cellHtml(rowValue, columnValue, kind = "cell", heatmap = null) {
  const matches = matchingCards(rowValue, columnValue);
  const count = matches.length;
  const selected = isSelectedCell(rowValue, columnValue);
  const classes = [
    "matrix-cell",
    kind !== "grand-total" ? "is-heatmapped" : "",
    kind !== "cell" ? "is-total" : "",
    kind === "grand-total" ? "is-grand-total" : "",
    count === 0 ? "is-empty" : "",
    selected ? "is-selected" : ""
  ].filter(Boolean).join(" ");
  const label = count === 1 ? "1 card" : `${count} cards`;
  const style = heatStyle(count, kind, heatmap);
  return `<td><button class="${classes}" type="button" data-row-value="${escapeAttr(rowValue?.key ?? "")}" data-column-value="${escapeAttr(columnValue?.key ?? "")}"${style}>${label}</button></td>`;
}

function bindMatrixCells(row) {
  for (const cell of row.querySelectorAll("[data-row-value]")) {
    cell.addEventListener("click", () => selectCell(
      rowAxis.values.find(value => value.key === cell.dataset.rowValue) ?? null,
      columnAxis.values.find(value => value.key === cell.dataset.columnValue) ?? null,
      cell
    ));
  }
}

function selectCell(row, column, trigger) {
  selectedCell = { row, column };
  const matches = matchingCards(row, column);
  selectedCard = defaultSorted(matches)[0] ?? null;
  renderMatrixSelection();
  renderSelection();
  resultsSheet.open(() => trigger);
}

function renderMatrixSelection() {
  for (const cell of matrixBody.querySelectorAll(".matrix-cell")) {
    const row = rowAxis.values.find(value => value.key === cell.dataset.rowValue) ?? null;
    const column = columnAxis.values.find(value => value.key === cell.dataset.columnValue) ?? null;
    cell.classList.toggle("is-selected", isSelectedCell(row, column));
  }
}

function renderSelection() {
  if (!selectedCell) {
    cellTitle.textContent = "Select a cell";
    cellCount.textContent = "0";
    openBrowser.href = editionUrl("/cards");
    openBrowser.setAttribute("aria-disabled", "true");
    cardGrid.replaceChildren();
    selectedPanel.innerHTML = "<p>Select a cell to inspect matching cards.</p>";
    return;
  }

  const matches = defaultSorted(matchingCards(selectedCell.row, selectedCell.column));
  cellTitle.textContent = selectionTitle(selectedCell.row, selectedCell.column);
  cellCount.textContent = matches.length.toString();
  const href = browserHref(selectedCell.row, selectedCell.column);
  openBrowser.href = href ?? editionUrl("/cards");
  openBrowser.setAttribute("aria-disabled", href ? "false" : "true");

  cardGrid.replaceChildren(...matches.map(card => {
    const button = document.createElement("button");
    button.type = "button";
    button.dataset.cardId = card.id;
    button.className = `thumb${selectedCard?.id === card.id ? " is-selected" : ""}`;
    button.innerHTML = `<img src="${card.thumbnailImage}" alt=""><span>${escapeHtml(card.name)}</span>`;
    button.addEventListener("click", () => {
      selectedCard = card;
      renderSelection();
      cardSheet.open(() => cardGrid.querySelector(".is-selected"));
    });
    return button;
  }));

  renderSelectedCard(matches);
}

function renderSelectedCard(matches) {
  if (!selectedCard) {
    selectedPanel.innerHTML = matches.length === 0
      ? "<p>No cards in this intersection.</p>"
      : "<p>Select a card to inspect it.</p>";
    return;
  }

  selectedPanel.innerHTML = `
    <img src="${selectedCard.displayImage}" alt="">
    <h2>${escapeHtml(selectedCard.name)}</h2>
    <div class="filter-actions" aria-label="Selected card filters">
      ${detailTile("Rank", selectedCard.rank, imageGlyph(selectedCard.glyphImages?.rank, "Rank", selectedCard.rank))}
      ${detailTile("Suit", selectedCard.suitName, imageGlyph(selectedCard.glyphImages?.suit, "Suit", selectedCard.suitName))}
      ${selectedCard.suitShape ? detailTile("Suit shape", selectedCard.suitShape, suitShapeGlyph(selectedCard.suitShape, selectedCard.color)) : ""}
      ${selectedCard.sequenceDigit ? detailTile("Last digit", "", textGlyph(selectedCard.sequenceDigit), selectedCard.sequenceDigit) : ""}
      ${detailTile("Sphere", `${selectedCard.sphere} (${selectedCard.sphereNumber})`, imageGlyph(selectedCard.glyphImages?.sphere, "Sphere", selectedCard.sphereNumber))}
      ${detailTile("Mode", `${selectedCard.mode} (${selectedCard.modeNumber})`, imageGlyph(selectedCard.glyphImages?.mode, "Mode", selectedCard.modeNumber))}
      ${detailTile("Zodiac", displayIfAny(selectedCard.sphereZodiac), imageOrEmpty(selectedCard.glyphImages?.zodiac, "Zodiac"), displayOrNone(selectedCard.sphereZodiac))}
      ${selectedCard.firstLetter ? detailTile("Letter", "", textGlyph(selectedCard.firstLetter)) : ""}
      ${detailTile("Score", selectedCard.letterScore, imageGlyph(selectedCard.glyphImages?.letterScore, "Letter score", selectedCard.letterScore))}
      ${detailTile("Border", selectedCard.totalBorderScore, imageGlyph(selectedCard.glyphImages?.borderScore, "Border score", selectedCard.totalBorderScore))}
      ${detailTile("Sides", "", borderGlyph(selectedCard), selectedCard.borderArrangement)}
      ${detailTile("B. Zodiac", displayIfAny(selectedCard.borderZodiac), imageOrEmpty(selectedCard.glyphImages?.borderZodiac, "Border zodiac"), displayOrNone(selectedCard.borderZodiac))}
      ${detailTile("Dir", directionDisplay(selectedCard.direction), imageGlyph(selectedCard.glyphImages?.direction, "Direction", directionDisplay(selectedCard.direction)))}
      ${detailTile("Part", selectedCard.partitionGroup, partitionGlyph(selectedCard.partitionGroup, selectedCard.iconGroup))}
      ${selectedCard.iconHalf != null ? detailTile("Half", selectedCard.iconHalf, iconHalfGlyph(selectedCard.glyphImages?.iconHalf)) : ""}
      ${selectedCard.isStandardDeck != null ? detailTile("Standard", selectedCard.isStandardDeck ? "Yes" : "No", standardDeckGlyph(selectedCard.isStandardDeck)) : ""}
      ${detailTile("In-links", selectedCard.linkInDegree, inLinkGlyph(selectedCard.linkInDegree))}
      ${detailTile("Group", selectedCard.iconGroup, imageGlyph(selectedCard.glyphImages?.iconGroup, "Icon group", selectedCard.iconGroup))}
      ${detailTile("Tarot", selectedCard.isTarot ? "Yes" : "No", selectedCard.isTarot ? tauGlyph() : "")}
      ${detailTile("Icon", `${selectedCard.iconNumber} ${selectedCard.iconName}`, imageGlyph(selectedCard.glyphImages?.icon, "Icon", selectedCard.iconNumber))}
    </div>
    <a class="button primary" href="${editionUrl(`/cards/${selectedCard.slug}`)}">Open detail</a>`;
}

function matchingCards(rowValue, columnValue) {
  return cards.filter(card => (!rowValue || rowValue.predicate(card)) && (!columnValue || columnValue.predicate(card)));
}

function matrixHeatmap() {
  const interiorCounts = [];
  const totalCounts = [];

  for (const rowValue of rowAxis.values) {
    for (const columnValue of columnAxis.values) {
      interiorCounts.push(matchingCards(rowValue, columnValue).length);
    }
    totalCounts.push(matchingCards(rowValue, null).length);
  }

  for (const columnValue of columnAxis.values) {
    totalCounts.push(matchingCards(null, columnValue).length);
  }

  return {
    interior: heatRange(interiorCounts),
    totals: heatRange(totalCounts)
  };
}

function heatRange(values) {
  return {
    min: Math.min(...values),
    max: Math.max(...values)
  };
}

function heatStyle(count, kind, heatmap) {
  if (!heatmap || kind === "grand-total") {
    return "";
  }

  const range = kind === "cell" ? heatmap.interior : heatmap.totals;
  const ratio = range.min === range.max ? 0.5 : (count - range.min) / (range.max - range.min);
  const hue = kind === "cell" ? 8 + (122 * ratio) : 42 + (145 * ratio);
  const saturation = kind === "cell" ? 66 : 54;
  const lightness = 94 - (15 * ratio);
  return ` style="--heat-bg: hsl(${hue.toFixed(1)} ${saturation}% ${lightness.toFixed(1)}%);"`;
}

function browserHref(rowValue, columnValue) {
  if (!rowValue && !columnValue) {
    return editionUrl("/cards");
  }

  if (!rowValue || !columnValue) {
    const value = rowValue ?? columnValue;
    const params = new URLSearchParams();
    params.set(value.filterKey, value.filterValue);
    return editionUrl(`/cards?${params}`);
  }

  if (rowValue.filterKey === columnValue.filterKey && rowValue.filterValue !== columnValue.filterValue) {
    return null;
  }

  const params = new URLSearchParams();
  params.set(rowValue.filterKey, rowValue.filterValue);
  params.set(columnValue.filterKey, columnValue.filterValue);
  return editionUrl(`/cards?${params}`);
}

function isSelectedCell(rowValue, columnValue) {
  if (!selectedCell) {
    return false;
  }

  return (selectedCell?.row?.key ?? "") === (rowValue?.key ?? "")
    && (selectedCell?.column?.key ?? "") === (columnValue?.key ?? "");
}

function selectionTitle(rowValue, columnValue) {
  if (rowValue && columnValue) {
    return `${rowValue.label} × ${columnValue.label}`;
  }

  if (rowValue) {
    return `${rowValue.label} row total`;
  }

  if (columnValue) {
    return `${columnValue.label} column total`;
  }

  return "Grand total";
}

function axisByKey(key) {
  return axes.find(axis => axis.key === key) ?? axes[0];
}

function normalizedFilterValue(card, key) {
  const value = card[key];
  if (noneFilterKeys.has(key)) {
    return value === null || value === undefined || value === "" ? noneValue : String(value);
  }
  return String(value ?? "");
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

function detailTile(caption, display, visual, labelDisplay = display) {
  const label = `${caption}: ${labelDisplay}`;
  return `<span class="filter-action" role="group" aria-label="${escapeAttr(label)}" title="${escapeAttr(label)}"><span class="filter-action-caption">${escapeHtml(caption)}</span><span class="filter-action-visual">${visual}</span><span class="filter-action-value">${escapeHtml(display)}</span></span>`;
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
  return `<span class="partition-art" aria-hidden="true"><span class="partition-row">${Array.from({ length: 5 }, (_, index) => {
    const state = index < filledCount ? "1" : "0";
    return `<img src="${assetRoot}/glyphs/partition${family}_${state}.webp" alt="">`;
  }).join("")}</span></span>`;
}

function displayOrNone(value) {
  return value === null || value === undefined || value === "" || value === noneValue ? "None" : value;
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

function defaultSorted(items) {
  return [...items].sort((a, b) => compare(a.suit, b.suit) || compare(a.rank, b.rank));
}

function directionSort(value) {
  return { n: 0, e: 1, s: 2, w: 3 }[String(value).toLowerCase()] ?? 99;
}

function borderArrangementSort(value) {
  if (value === "None") {
    return 0;
  }
  return String(value).split("+").length * 10
    + (String(value).includes("T") ? 1 : 0)
    + (String(value).includes("R") ? 2 : 0)
    + (String(value).includes("B") ? 4 : 0)
    + (String(value).includes("L") ? 8 : 0);
}

function zodiacSort(value) {
  if (value === noneValue) {
    return 999;
  }
  const zodiacIndex = zodiacSigns.indexOf(value);
  if (zodiacIndex >= 0) {
    return zodiacIndex;
  }
  const planetIndex = planets.indexOf(value);
  return planetIndex >= 0 ? 100 + planetIndex : 500;
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
