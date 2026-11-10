---
title: "Borders"
order: 170
status: pending
visible: true
---

## Make a Run for the Border

I wanted something that wasn’t about sets, words, or grouping.

Up to this point, most systems were about partitioning the deck—suits, ranks, spheres, modes. Useful, but all fundamentally similar in how they behave.

I started thinking about spatial games instead. Tile-laying, adjacency, mapping. Things like Carcassonne, or other systems where how pieces sit next to each other matters.

Cards don’t naturally do that, but they do have edges.

And there is a strip of real estate between the corners that is usually unused.

---

## Starting with Edge Patterns

The initial idea was simple.

Some cards have borders, some don’t. Some have one side, some two, some all four. If you place cards next to each other, those borders could define how they connect.

That part was easy to assign, but that wasn’t enough. A system needed to do more than one thing to justify taking up space.

---

## Adding Point Values

So I added a point system.

Each border side would carry a value, and the sum of those values would give every card a “border score.”

That opens up a few possibilities.

You could restrict connections based on the edges themselves, but you could also use the total as a property of the card—something you can compare, sort, or score.

---

This is where it became more complicated.

If the system is going to be usable, the distribution has to be predictable.

I set a target range of 0 to 16:

- 0 → no borders
- 16 → all four borders, each with maximum value

That seemed like a reasonable span.

---

## Shaping the Distribution

It turns out that with 150 cards, you can get very close to a clean distribution.

You can assign:

- 1 card with 16
- 2 with 15
- 3 with 14
- …
- 16 with 1

which accounts for 136 cards, leaving 14 cards at 0.

It’s not perfect, but it’s very close to a linear distribution across 1–16.

That gives the total score a nice property—it behaves like a clean numeric scale.

---

## Assigning Edge Configurations

On top of that, you still have to assign actual border configurations.

Each side can have 1–4 points, which creates constraints:

- 0 points → no borders
- values above 12 → require all four borders

I also wanted the configurations themselves to be balanced.

You end up with:

- 14 cards with no borders
- 14 with all four
- 34 with one or three
- 54 with two

That distribution comes from the number of distinct configurations. There are more ways to have two borders than one or three—parallel sides, adjacent sides, and so on—so those naturally get more cards.

For example, there are six distinct “two-border” configurations, and each one ends up appearing exactly nine times.

---

Getting all of that to line up took a while.

Most of it was worked out in Excel—iterating on assignments until the point totals and configurations both balanced. The details aren’t obvious from a single card, but across the deck it forms some very regular patterns to discover.

---

## Keeping the Borders Subtle

Visually, this needed to stay subtle.

The borders sit at the edge of the card, and most of the time you’re not using them. So they needed to be visible when you look for them, but easy to ignore otherwise.

I tried a few things - simple lines with 'segments' for points; other ornate designs.   Inward-pointing triangles worked well.

- neutral grey for the structure
- overlayed suit-colored triangles for the points

That keeps the system readable without pulling attention away from the rest of the card.

The system was designed assuming cards are aligned the same way, but the triangles are uniform in size, so they still work if cards are rotated. It’s not perfect, but it doesn’t break.

---

## Adding Direction

The direction glyph came in as a small extension.

Once I was already thinking spatially, it made sense to add a notion of direction.  The deck already has natural 2, 3, 5 and 6 partitions (suits, ranks, spheres), and I had for a while wanted some way to suggest a 4-partitioning.  Four doesn’t divide cleanly into 150, but you can get close enough to make it useful.  Now I had more of an excuse to create that with up, down, left, right which now nicely complements spatial play.

A compass-style glyph was a natural fit.

---

## Showing the Total

The final addition was the ring around that compass.

That ring encodes the total border score. Instead of counting edges, you can read the value directly.

I wasn’t sure that would work at small scale, but it turns out you can still distinguish all 16 values. It’s a shortcut, not a replacement—you can still add the edge point values if you want—but it’s there.

---

At this point, borders are doing several things at once:

- defining adjacency and connection
- providing a numeric score
- contributing to spatial mechanics
- and adding a directional layer

It’s one of the denser systems on the card, but it earns its place.

---

::: players-designers
## For Players and Designers

The most direct use is adjacency.

You can place cards next to each other and require borders to match—either exactly or by some rule, like border points. That supports tile-laying or map-building mechanics in a way that most card decks don’t.

The point values add another layer.

You can restrict placement based on edge values, or use the total border score as an unrelated property of the card. That supports scoring, thresholds, or progression systems.

Because the scores form a near-linear distribution, they behave predictably.

The configurations also matter.

Since the different border patterns are evenly distributed, you can build mechanics around shape—cards with two borders, cards with corners, cards with opposite sides, and so on.

The direction glyph extends this into movement.

You can treat cards as having orientation, or require movement or placement in certain directions. It’s a small addition, but it fits naturally with the rest of the system.

And like most of the systems in the deck, borders layer well.

You can combine adjacency with suits, ranks, or spheres to create more constrained placement rules. Because the border system is orthogonal to those, it adds variety without needing additional components.
:::
