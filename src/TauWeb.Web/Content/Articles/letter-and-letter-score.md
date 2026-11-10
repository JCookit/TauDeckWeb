---
title: "Letter and Letter Score"
order: 120
status: pending
visible: true
---

## Can I Buy a Vowel

This is another idea borrowed from EverDeck, and one that ended up doing more work than I expected.

Every card already has a word, and surfacing the first letter of that word felt like an easy win. It sits in the bottom left, easy to find and easy to use. The distribution of letters roughly follows English frequency—there are a lot of E’s (15), fewer Z’s (2) —and that alone gives you something useful. You can immediately support spelling games, word games, or anything that cares about letter patterns.

That would have been enough.

But the natural extension is to assign a point value to each card based on that letter. Common letters get low values, rare letters get high values. Very similar to Scrabble. E, A, R land at 1. Q and Z land at 6.

The twist is that I didn’t want this to just be a lookup table.

If the deck is supposed to be reusable across different kinds of games, then the point values themselves should form a usable system, independent of the letters. You should be able to ignore the letters entirely and still have something meaningful.

So the goal became to shape the distribution of values across the deck.

Values run from 1 to 6, and the distribution is intentionally skewed. Half the deck is 1’s. Then a quarter is 2’s, and it tapers from there down to the 6’s. It’s not perfectly exponential, but it’s close enough to behave that way in practice.   \[insert chart\]

More importantly, that distribution is enforced per suit color. Each pair of suits (30 cards) has the same breakdown—exactly 15 cards with value 1, and the rest distributed the same way. That means no matter how you slice the deck, you get a consistent spread.

At that point, the system works two ways. You can use it as a letter-based scoring system, or you can ignore the letters and just treat it as a built-in 1–6 value distribution that’s already balanced.

Getting there meant the mapping from letter to value couldn’t be perfectly rigid. Most letters are fixed—E, A, R are always 1, Q and Z are always 6—but some of the mid-frequency letters (N, T, L, S, etc.) need a bit of flexibility. They might show up as a 1 or a 2 depending on where they land, just to keep the overall distribution balanced across suits. In practice, no letter ends up with more than two possible values, so it’s still predictable enough to reason about.   \[insert chart\]

Visually, this needed to be present but quiet.

The letter is already doing some work in the corner, so the point value needed to sit nearby without competing. A die ended up being a natural fit. It’s immediately recognizable as a number indicator, it scales well, and it can sit there without pulling too much attention. The fact that the distribution isn’t actually “fair” like a real die doesn’t really matter—the metaphor is strong enough.

The placement followed pretty naturally. Letter in the bottom left, die just above it. That keeps the entire system localized. Easy to find when you need it, easy to ignore when you don’t.

One small change fell out of that. Originally the system was 1–5. Once the die became the representation, moving to 1–6 just made more sense. It aligns with expectations, even if the underlying distribution is anything but uniform.

So now each card carries a letter, a point value tied to that letter, and a distribution that works both linguistically and mechanically. Like a lot of the systems in the deck, you can lean into it as much as you want, or not at all. It’s just there, ready to be used.

::: players-designers
## For players & designers:

The most direct use is for word and spelling games.

Because the letter distribution roughly follows English frequency, you can draw hands and form words without needing to rebalance anything. There are enough common letters to make it workable, and enough rare letters to make it interesting.

The point values layer on top of that naturally.

You can score words, compare hands, or just treat the cards as having an inherent value. Since the values are tied to the letters, it behaves similarly to systems people already understand.

Where it becomes more generally useful is ignoring the letters entirely.

The deck carries a built-in 1–6 value distribution that is consistent across suit colors. Half the cards are 1’s, then it tapers non-linearly from there. That gives you a predictable spread for any kind of scoring, drafting, or comparison-based mechanic.

Because that distribution is enforced per suit color, it also behaves well in subsets. If you’re only using part of the deck, or dividing it by suits, you still get the same shape.

You can also combine it with other systems.

For example, scoring within a suit, across spheres, or within a mode group all work without needing to adjust the values. The system is stable enough to layer without breaking.

Finally, there is a subtle bonus use. The dice are shaded into four groups (1s, 2s, 3–4s, 5–6s), which creates a secondary distribution. In cases where you just need tiers—more like poker chips—you can ignore the pips entirely and use the shading instead.
:::
