---
title: "Rank Rail"
order: 180
status: pending
visible: true
---

## Riding the Rails

The rank rail came from wanting more flexibility out of rank.

At this point, rank already had a number and a glyph, and that covers most uses. But I kept coming back to edge cases—games that used zero, or even negative numbers. It’s niche, but once I started thinking about it, it stuck.

For a while I tried the obvious approach, which was just putting more numbers on the card. That worked mechanically, but it didn’t look right. It felt bolted on.

---

The alternative is to just expect players to map things in their head.

“15 is actually 0,” or “this glyph means something else in this game.” That works too, but I didn’t like that being the only option, especially for something as central as rank.

---

## One Glyph with More Than One Meaning

The idea then was whether a single glyph could carry more than one interpretation.

Not in a clever or abstract way, but in something that still felt natural. If rank is already important enough to take up space, then it might justify taking a bit more.

That’s where the rank rail came from.

---

The rail encodes the rank in a fixed width.

That part was important—whatever it was, it needed to be consistent so it didn’t draw attention to itself.

The pattern uses two elements:

- thicker segments for 5
- thinner segments for 1

So the total is easy to read if you need it, but you don’t have to.

---

## Two Values at Once

The more interesting part is that it encodes two values at the same time.

The top of the rail reads as one number, and the bottom reads as another.

So a card can show 1 on top and 14 on the bottom at the same time. You can interpret that as positive/negative, forward/backward, or however a game wants to use it.

There are also other patterns in the shape if you look for them, but that wasn’t the primary goal.

---

## A Frame for the Card

If that was all it did, it probably wouldn’t have made the cut.

But once I started placing it on the card—above the icon—it ended up solving another problem.

It created a visual separation.

Up to that point, the card had a lot of functional elements. The rail gave the center a bit of structure, and it introduced something that felt more decorative, almost mythic, without actually adding noise.

Colored with the rank bands, it started to feel like it belonged.

And when paired with the partition rail below, it framed the icon in a way that made the whole center section feel intentional.

In v2, that lower rail carries three signals: the filled-symbol [partition value](/systems/partitions?edition=v2), the symbol shape for [Icon Groups](/systems/icon-groups?edition=v2), and the end bars for [Icon Halves](/systems/icon-halves?edition=v2).

\[insert examples here\]

---

::: players-designers
## For Players and Designers

The most direct use is treating the rail as an alternate rank value.

You can read the top or the bottom depending on the game. That supports things like positive/negative values, reversed ordering, or dual scoring systems without needing to redefine the deck.

Because the encoding is consistent, it’s also something players can learn if they want to. The 5-and-1 structure is simple enough that the values are recoverable without much effort.

The dual encoding opens up a few more possibilities.

You can compare top vs bottom, require one or the other, or build mechanics around switching between them. That’s useful for games that want state changes or inversion without introducing new components.

Even if you ignore the numeric side, the rail still acts as a stable visual anchor.

It separates the card into regions and reinforces the rank bands, which can be useful when combining with other systems like partitioning or progression.
:::
