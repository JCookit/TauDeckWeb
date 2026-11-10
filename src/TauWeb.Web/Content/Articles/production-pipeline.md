---
title: "Production Pipeline"
order: 250
status: pending
visible: true
---

## I’m a Real Boy!

This project ended up using a handful of tools, each doing a very specific job.

There wasn’t really a grand plan at the start. It evolved into a pipeline over time—mostly by finding the right tool for each problem and stitching them together.

---

## ChatGPT

This was effectively a constant design partner.

Not in the sense of “generate the answer,” but more like always having something to react to. I used it heavily as a critic:

- sanity-checking system ideas
- pressure-testing constraints
- pointing out edge cases
- suggesting alternatives I wouldn’t have thought of

It was also useful on the visual side. I could drop in screenshots and get feedback like:

- remove this line
- tighten spacing here
- that color is too strong
- this element is competing with that one

It helped frame tradeoffs in a way that made decisions easier to live with.

It wasn’t perfect. It leans a bit agreeable, and sometimes you have to push it to get a real critique. But overall it was extremely useful for iteration.

Early on, it also helped generate large search spaces—words, icon ideas, variations—which I then filtered down. It didn’t pick the answers, but it made exploration much faster.

Later, it could perform mechanical analysis and some solution-finding (balancing the icon groups for example was one it did.  I did most of the other systems balancing myself)

---

## Inkscape

This was the main drawing tool.

Everything visual started here—icons, glyphs, layouts. The style of the deck (simple line work, minimal shading) fit well with what Inkscape does well.

Nothing fancy, but it had everything needed:

- vector drawing
- alignment and spacing tools
- layering
- consistent export

When things were finalized, it could batch export PNGs cleanly, which fed directly into the next step.

---

## Word (the spec)

The main spec lived in a Word document.

This ended up being more important than expected. It was the place where all the rules, systems, and decisions were written down and reconciled.

It also helped when working with ChatGPT—having a stable spec meant the conversation could stay grounded instead of drifting.

Whenever something changed, it went back into the doc. Over time, that became the source of truth for the entire project.

---

## Excel

This is where most of the real work happened.

All the systems—suits, ranks, spheres, modes, links, borders—were worked out in spreadsheets.

It was used for:

- verifying distributions
- balancing constraints
- brute-forcing assignments
- checking counts and coverage
- visualizing patterns with conditional formatting

There were a lot of iterations here. Many ideas that seemed fine conceptually broke down when you actually tried to distribute them across 150 cards.

Eventually, everything collapsed into a single table—one row per card, with all attributes filled in. That table became the data source for rendering.

---

## nanDECK

nanDECK handled card generation.

Given the data table, it could render all cards programmatically. That was critical—there’s no realistic way to hand-place this many elements consistently across 150+ cards.

It worked well, but it’s a bit low-level:

- layout is precise but manual
- text (especially styled text) can be fiddly
- getting things pixel-perfect takes iteration

There was a fair amount of back-and-forth between nanDECK and the source data to get alignment, spacing, and layering right.

In the end, it outputs one PNG per card, ready for print.

---

## MakePlayingCards (MPC)

This is the final step—turning images into a physical product.

The PNGs are uploaded, mapped to card faces, and printed.

As of writing, the first prototype batch is still in progress. That will answer some open questions:

- how accurate the color reproduction is
- how the subtle backgrounds and shading come through
- whether any adjustments are needed for print

Ideally, it’s close enough. Color tuning at this stage is possible, but not something I’m eager to redo unless necessary.

---

## How It Fits Together

Roughly:

- ideas explored and critiqued (ChatGPT)
- visuals created (Inkscape)
- rules defined (Word)
- systems balanced (Excel)
- cards generated (nanDECK)
- cards produced (MPC)

Each tool solved a specific problem. None of them did everything, but together they covered the whole pipeline.

---

## Iteration Reality

The actual process wasn’t linear.

It looped constantly:

- adjust a system → rebalance in Excel
- tweak visuals → re-export assets
- shift layout → re-render in nanDECK
- reconsider rules → update spec

That cycle repeated a lot.

Most of the work wasn’t creating something new—it was refining something that almost worked.

---

## Where This Lands

The end result is a fairly mechanical pipeline:

data → render → print

But getting the data right—the structure, the balance, the constraints—that’s where most of the effort went.
