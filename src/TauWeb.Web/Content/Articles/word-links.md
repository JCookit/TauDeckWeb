---
title: "Word Links"
order: 160
status: pending
visible: true
---

## Words Deserve Friends too

From the start, I knew I didn’t want a single naked word sitting on each card.

It works, but it felt like a missed opportunity. If the card is only carrying one word, then all of the interpretation has to come from that one thing. It’s fine, but it’s thin.

---

## Trying Flavor Text

The earliest attempt was flavor text.

Short micro-quotes, one or two lines, meant to give the word some context. I had AI generate a first batch, and they were… fine. They read like fortune cookies—coherent, vaguely meaningful, but generic.

I also considered pulling from public-domain sources—Shakespeare, the Bible, that sort of thing. That solved the copyright problem cleanly, but it didn’t match the voice of the deck. It felt like borrowing tone instead of building one.

---

## Trying Word Clusters

Next attempt was “word clusters.”

Instead of sentences, just a handful of additional words—four to six—that relate to the main word, and of course all unique across the deck. I ended up curating a list of around 900 words to make that work. It was interesting, but it didn’t really solve the problem.

---

What kept coming up was that all of these additions helped the *word*, but only the word.

They didn’t interact with anything else on the card. They didn’t provide any structure or gameplay value. And on a card where space is already tight, that wasn’t enough. The question became whether something could do both.

---

## Connecting the Cards

The answer was to make those extra words come from other cards.

Instead of adding new vocabulary, each card references two other words that already exist in the deck.

That does two things at once.

On the narrative side, it gives a lens. If you draw a card like **Pulse**, that’s pretty open, but if it links to **Breath** and **Tide**, that starts to suggest something—rhythm, ebb and flow, things that rise and fall. It doesn’t define the meaning, but it nudges it.

On the gameplay side, it creates connections. The cards are no longer isolated; they form a graph.

---

## Choosing the Link Constraints

The next step was figuring out how many links, and what constraints to put on them.

Two felt like the right balance—enough to create structure, not so many that it overwhelms the card.

From there, I set a few rules:

- every card must be the target of at least one link
- no card can link to its icon pair
- every link should make some kind of narrative sense (similar, opposite, suggestive, etc.)
- minimize simple back-and-forth pairs

With those in place, it was possible to build a workable set.

---

That set ended up having some useful properties.

Most cards have two incoming links. Some have one. A small number are more “connected”—four or even five incoming links.

That’s indicated on the card with small tick marks between the two words. It’s subtle, but it gives you a sense of how central a card is in the network.

There are also some direct cycles—cards that point to each other—which show up in about 60 cases.

---

## Longer Cycles

The more interesting structure is in longer cycles.

Starting from any card, you can follow links and eventually loop back around. Some of those chains are short; others run through 5–15 cards before closing. That wasn’t strictly required, but it fell out of the constraints, and it’s one of the more interesting emergent properties of the system.

---

## Keeping Links Secondary

Visually, this is all kept secondary.

Small text, reduced emphasis, subtle markers. It’s there if you want it, but it doesn’t compete with the main elements of the card.

---

::: players-designers
## For Players and Designers

The simplest use is following links.

You can start at a card and move to one of its linked cards, then continue from there. That gives you a natural way to traverse the deck—useful for movement, chaining, or progression mechanics. Because every card has at least one incoming link, you don’t get dead ends.

You can also build around the structure of the graph.

Cards with more incoming links are more “central.” That can be used for scoring, targeting, or as special objectives. The tick marks give you that information without needing to track it separately.

The cycles are where it gets more interesting.

You can construct loops of cards and use those as paths, sets, or objectives. Short cycles are easier to find; longer ones require more exploration. That supports different kinds of gameplay, from quick matching to longer-form search or planning.

\[note:  add some tables here of the data\]

Links also layer well with other systems.

You might follow links but restrict by suit, or require transitions across spheres or modes. Because the connections cut across everything else, they create combinations that don’t show up in other partitions.

Even without explicit mechanics, the links give you a way to move through the deck that isn’t based on position, suit, or rank. It’s a different kind of structure, and it holds together on its own.
:::
