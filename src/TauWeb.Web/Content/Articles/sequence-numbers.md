---
title: "Sequence Numbers"
order: 150
status: pending
visible: true
---

## These tracks are single-file, to hide their numbers…

Sequence numbers showed up early, mostly as a way to keep the deck grounded.

At the simplest level, it’s just a way to uniquely identify every card. If a game needs ordering, indexing, or just a way to reference specific cards, having a number solves that cleanly.

---

## A Numbering Scheme

The scheme I ended up with was influenced by EverDeck, particularly the idea that the number itself can carry structure.

For ranks 1–10, the sequence number is aligned directly with suit and rank.

- the first two digits identify the suit  (00 to 09)
- the last digit identifies the rank  (0 to 9)

So for example, all cards of a given suit share the same prefix, and the last digit cycles 1 through 0 (for 10). That means you can look at the number and immediately recover both suit and rank without needing anything else on the card.

That’s already useful, but there was another constraint layered in.

For each color pair (30 cards), there are exactly three of each last digit.

So across those two suits:

- three cards ending in 1
- three ending in 2
- …
- three ending in 0

This isn’t obvious when looking at a single card, but across a set it becomes predictable.

---

## Fitting the Tarots

The Tarots were then slotted into this system.

Their sequence numbers are chosen so that the last two digits match their canonical Tarot numbering.

So:

- The Fool → 100 (Tarot 0)
- The World → 122 (Tarot XXII)

That anchors them into the deck in a way that’s consistent with their traditional ordering, while still fitting the overall structure.

---

## More Than an Identifier

At this point, sequence numbers are doing a few things at once:

- uniquely identifying cards
- encoding suit and rank (for ranks 1–10)
- providing a secondary distribution through the last digit
- and anchoring the Tarot ordering

It’s not something you need to use directly very often, but it’s there as a stable backbone if you do.

---

::: players-designers
## For Players and Designers

Sequence numbers are the most straightforward system to use.

If a game just needs an ordered deck, or a way to reference specific cards, you can use them directly. Shuffle, sort, deal—everything behaves the way you expect.

The alignment for ranks 1–10 makes them interchangeable with standard concepts.

You can treat the last digit as rank and the prefix as suit, which means you can reconstruct familiar mechanics using only the numbers if you want to. That can be useful in cases where you’re not relying on the visual layout.

The repeated last-digit distribution also has some practical uses.

Because each color pair has three of each last digit, you can build mechanics around matching or collecting by that digit. For example, sets of “same ending number” behave predictably, even when mixing suits.

The Tarot placement gives you another anchor.

If you’re using the Tarots, their sequence numbers already line up with their traditional ordering, so you can reference or sort them without needing a separate system.

Even if you ignore most of that, just having a consistent numbering scheme makes it easier to treat the deck as a structured set rather than a collection of unrelated cards.
:::
