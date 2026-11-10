---
title: "Partition Rail"
order: 190
status: pending
visible: true
---

## Now We’re Off the Rails

This one came from a practical concern.

With 75 icon pairs in the deck, a full memory-style game is probably too much. It’s not that it wouldn’t work, but it would be long and hard to manage. It felt like the deck needed a way to break itself into smaller, usable pieces.

So the idea was to divide the deck into sub-decks.

---

## Smaller Matching Decks

A natural target was three groups of 25 icon pairs, which is 50 cards each.

That’s a manageable size, and it lines up cleanly with the overall structure. It’s easy enough to just pick 25 pairs at random and call it done, but that wouldn’t be very interesting.

The question was whether those groups could be structured.

---

## Balancing the Groups

Each icon pair already spans two suit colors.

So if you’re picking 25 pairs, you’re implicitly selecting 50 cards across the deck. The question was whether those could be chosen so that each group of 50 had a balanced representation of colors.

It turns out they can.

Each partition can be constructed to contain exactly 10 cards of each color, and that holds across all three groups. That alone makes them much more usable, since you’re not accidentally biasing one subset.

---

From there it was natural to ask how far that could be pushed.

It turns out the same partitions can also be arranged to include exactly two cards of each mode.

That was about the limit of what could be cleanly balanced. Beyond that, the partitions still have a reasonably even spread of spheres, ranks, and other systems, but those aren’t perfectly constrained.

So each partition ends up with:

- 25 icon pairs (50 cards)
- 10 cards of each color
- 2 cards of each mode

That’s a useful amount of structure for something that is still just a subset of the deck.

---

## A Separate 1–5 Score

Separately, there was one more idea I wanted to include.

A simple scoring system from 1 to 5, evenly distributed across the deck. Each suit would have three of each value, and each rank would have two. Nothing fancy—just a clean, predictable set of values that could be used in a lot of contexts.

---

The obvious approach would be to show both of these systems independently.

But at this point, combining systems was usually the better answer.

So the partitioning and the scoring became a single visual element.

---

## Combining the Signals

The solution was a small rail at the bottom of the card.

It contains five symbols. The number of filled symbols indicates the score from 1 to 5.

At the same time, the symbols themselves come in three distinct forms, which correspond to the three partitions. That makes it easy to sort cards visually into their groups without needing to reference anything else.

---

## Placing the Rail

Placed at the bottom center, below the word, and colored using the rank band, it balances the rank rail above.

At this point the card has a kind of vertical symmetry—functional elements framing the more narrative center—and the partition rail fits into that without drawing attention to itself.

---

## V2 Update: Three Signals in One Rail

The account above describes the rail’s original two functions. In v2, the lower rail carries three distinct signals:

- The number of filled symbols gives the 1–5 [partition value](/systems/partitions?edition=v2).
- The symbol shape identifies one of the three [icon groups](/systems/icon-groups?edition=v2).
- The end bars identify one of the two [icon halves](/systems/icon-halves?edition=v2).

Each icon half contains one card from every icon pair, giving a 75-card deck of unique pictures. The 50-card groups described above are called Icon Groups on the website; they are separate from both the 1–5 partition value and Icon Half.

---

::: players-designers
## For Players and Designers

The most direct use is splitting the deck.

You can take one partition and get a balanced 50-card subset without needing to curate anything. Because each partition has equal color distribution and a consistent spread of modes, they behave well as independent decks.

The scoring layer is also immediately usable.

You can treat the filled symbols as a 1–5 value and use that for scoring, drafting, or comparison. Since it’s evenly distributed across suits and ranks, it behaves predictably in most contexts.

The partitions also support filtering.

You can play within a single partition, combine two, or require cards from different partitions. Because the groups are visually distinct, that’s easy to manage during play.

And like the other systems, it layers.

You might build sets within a partition, or require specific scores within a subset. Because the partitioning is orthogonal to most other systems, it creates combinations that don’t show up naturally elsewhere in the deck.
:::
