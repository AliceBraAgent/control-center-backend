# Routing V3 — Decisions & Clarifications

## Algorithm
- **Optimal routing by lowest cost** (not greedy)
- Returns all valid routing options ordered by minimum cost
- Product selection is done by TMS — engine just returns options
- Multiple linehaul hops allowed (e.g. Hub A → Hub B → Hub C)

## Time & ETA
- Routing is **time-aware** (considers schedules, departure/arrival times)
- **ETA calculation is in scope**

## Attributes
- Always **boolean flags** (no value-based attributes)
- Mandate filtering is a **hard constraint** (must match, not preference)

## Cost
- Cost is **per leg**
- Exception: sometimes 1 cost covers 2 legs (Linehaul + Delivery priced together)
- Pricing tables based on **zones/kilometers** and **kilogram/pallets**
- Pricing model details to be investigated separately
- Combined pricing (LH+Delivery): linehaul leg priced at 0 or flagged "pricing included in delivery line"

## Graph Search
- **Max 6 hops** (bounds search space)
- Multiple valid routing options possible (overlapping postal code areas both returned)

## Data Volume
- ~400 lines, few thousand postal code areas
- **In-memory graph** (no graph DB needed)

## Sub-Hubs & Partner Data
- Sub-hub is used for **distance/kilometer calculation** only
- Defined in line configuration via uploaded file (European postcode → sub-hub mapping)
- Mapping data **comes from partners**

## Scope
- **Greenfield** build (no V1/V2 legacy)
- Primary API consumer: **TMS Desktop App**
