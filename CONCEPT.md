# Routing V3 — Technical Concept

## Overview

A routing engine for groupage carriers that finds optimal (lowest-cost) routes through a hub network. Given a shipment with origin, destination, and items, the engine returns all valid routing options as ordered legs with cost and ETA.

## Domain Model

### Shipment (API Input)
```
Shipment {
  Origin: Location
  Destination: Location
  Items: Item[]
  Attributes: string[]        // boolean flags: "ADR", "TAILLIFT_COLLECTION", "TAILLIFT_DELIVERY", "TEMPERATURE_CONTROLLED", "TIRES"
  MandateCode: string          // e.g. "AT-LNZ"
}

Location {
  Country: string              // ISO 3166-1 alpha-2
  PostalCode: string
  City: string
  AddressMatchCode?: string    // optional, for precise matching
}

Item {
  Amount: int
  Type: string                 // e.g. "PALLET", "PACKAGE"
  Weight: decimal              // kg
  Volume: decimal              // m³
  Length?: decimal
  Width?: decimal
  Height?: decimal
}
```

### Hub
```
Hub {
  Code: string                 // e.g. "AT-LNZ", "DE.AUR"
  Name: string
  Country: string
  Location?: GeoPoint          // for map display
}
```

### PostalCodeArea
```
PostalCodeArea {
  Id: int
  Country: string
  Pattern: string              // regex, e.g. "4.*" for AT-4*
  SubHubCode?: string          // for kilometer/pricing resolution
}
```

### Line
```
Line {
  Id: int
  Code: string                 // e.g. "NV AT.LNZ"
  Type: enum { Collection, Linehaul, Delivery }
  Origin: LineEndpoint
  Destination: LineEndpoint
  Attributes: string[]         // capabilities: "ADR", "TAILLIFT", etc.
  ValidMandates: string[]      // mandate codes this line serves
  Department: string
  Partner?: string             // e.g. "CTL"
  BaseSchedule: ScheduleRule[]
  PricingRef: string           // reference to pricing table
  PricingIncludedInDelivery: bool  // true = this leg costs 0, priced with delivery
}

LineEndpoint {
  HubCode?: string             // set if endpoint is a hub
  PostalCodeAreaId?: int       // set if endpoint is a postal code area
}

ScheduleRule {
  DaysOfWeek: DayOfWeek[]
  DepartureTime?: TimeOnly
  ArrivalTime?: TimeOnly
  ArrivalDayOffset: int        // 0 = same day, 1 = next day
}
```

### Schedule (Actual Executions)
```
ScheduleExecution {
  LineId: int
  Date: DateOnly
  DepartureTime: DateTime
  ArrivalTime: DateTime
  Cancelled: bool
}
```

### Routing Result (API Output)
```
RoutingResult {
  Options: RoutingOption[]     // ordered by total cost ascending
}

RoutingOption {
  Legs: RouteLeg[]
  TotalCost: decimal
  ETA: DateTime                // arrival at final destination
  DepartureTime: DateTime      // earliest departure
}

RouteLeg {
  Sequence: int
  Type: enum { Collection, Linehaul, Delivery }
  LineId: int
  LineCode: string
  Origin: string               // hub code or location description
  Destination: string
  DepartureTime: DateTime
  ArrivalTime: DateTime
  Cost: decimal
  Partner?: string
}
```

## Routing Algorithm

### Approach: Time-Aware Dijkstra with Cost Optimization

The network is modeled as a directed graph:
- **Nodes:** Hubs + virtual nodes for origin/destination postal code areas
- **Edges:** Lines (with schedule, cost, attributes, mandate constraints)

### Steps

1. **Input Validation** — Verify shipment has valid origin, destination, mandate, items
2. **Postal Code Resolution** — Match origin/destination postal codes to PostalCodeAreas (regex matching, multiple matches possible)
3. **Graph Construction** — Build subgraph of lines valid for the shipment's mandate and attributes
4. **Path Search** — Modified Dijkstra / BFS with:
   - **Cost** as primary weight
   - **Time constraints:** a leg can only be used if departure ≥ arrival of previous leg
   - **Max 6 hops**
   - **Attribute filtering:** every leg must satisfy shipment's required attributes
   - **Mandate filtering:** every leg must include shipment's mandate (hard constraint)
5. **Cost Calculation** — For each valid path, sum leg costs from pricing tables. Handle combined pricing (LH + Delivery) by zeroing the flagged leg.
6. **ETA Calculation** — Chain schedule times through the route to compute departure and arrival at each leg.
7. **Result Ordering** — Return all valid options sorted by total cost ascending.

### Complexity
- ~400 lines, few thousand postal code areas
- In-memory graph, no external DB needed for routing computation
- Graph rebuilt on line/schedule data changes (or periodically)

## API Design

### POST /api/v3/routing/calculate
Request: `Shipment` object
Response: `RoutingResult` object

### GET /api/v3/routing/lines
List/filter lines. Query params: mandate, type, hub, attributes.

### POST /api/v3/routing/lines
Create a new line.

### PUT /api/v3/routing/lines/{id}
Update a line.

### DELETE /api/v3/routing/lines/{id}
Delete a line.

### GET /api/v3/routing/hubs
List hubs.

### CRUD for: Hubs, PostalCodeAreas, Mandates, Attributes, Schedules

### POST /api/v3/routing/postal-code-areas/import
Upload European postcode → sub-hub mapping file.

## Frontend Pages

### Line Management
- Table view of all lines with filtering (mandate, type, hub, department)
- Create/edit line form with schedule rules, attributes, mandate assignment
- Pricing reference assignment

### Hub Management
- CRUD for hubs with geo coordinates

### Postal Code Area Management
- Table with country, pattern, sub-hub
- Bulk import from CSV/Excel (partner data)

### Route Simulator
- Input: origin, destination, items, attributes, mandate
- Output: list of routing options with legs, cost, ETA
- Map visualization showing the route through hubs

### Schedule Management
- Calendar view of line executions
- Override/cancel specific dates

## Project Structure

### Backend (.NET 8)
```
RoutingV3/
├── Domain/
│   ├── Models/           # Shipment, Line, Hub, PostalCodeArea, etc.
│   ├── Enums/            # LegType, DayOfWeek
│   └── ValueObjects/     # Location, GeoPoint
├── Engine/
│   ├── RoutingEngine.cs          # Main orchestrator
│   ├── GraphBuilder.cs           # Builds in-memory graph from lines
│   ├── PathFinder.cs             # Dijkstra/BFS with constraints
│   ├── CostCalculator.cs         # Pricing resolution
│   ├── EtaCalculator.cs          # Schedule-based ETA
│   └── PostalCodeMatcher.cs      # Regex matching for postal codes
├── Api/
│   ├── Controllers/
│   │   ├── RoutingController.cs
│   │   ├── LinesController.cs
│   │   ├── HubsController.cs
│   │   └── PostalCodeAreasController.cs
│   └── Dtos/             # Request/Response DTOs
├── Data/
│   ├── RoutingDbContext.cs
│   ├── Repositories/
│   └── Migrations/
└── Services/
    ├── LineService.cs
    ├── ImportService.cs   # Postal code file import
    └── GraphCacheService.cs  # Keeps in-memory graph fresh
```

### Frontend (React + shadcn/ui)
```
src/features/routing-v3/
├── api/                  # API client functions
├── components/
│   ├── LineTable.tsx
│   ├── LineForm.tsx
│   ├── HubTable.tsx
│   ├── RouteSimulator.tsx
│   ├── RouteResultCard.tsx
│   ├── RouteMap.tsx
│   └── ScheduleCalendar.tsx
├── hooks/                # React Query hooks
├── types/                # TypeScript interfaces
└── pages/
    ├── LinesPage.tsx
    ├── HubsPage.tsx
    ├── PostalCodeAreasPage.tsx
    ├── SchedulesPage.tsx
    └── SimulatorPage.tsx
```

## Implementation Phases

### Phase 1: Core Engine + API
- Domain models, DB schema, migrations
- Postal code matching
- Graph builder + path finder (cost-optimized, time-aware)
- Cost & ETA calculation
- Routing API endpoint
- Line/Hub/PostalCodeArea CRUD APIs
- Postal code import endpoint

### Phase 2: Frontend Management
- Line management (table, form, CRUD)
- Hub management
- Postal code area management + import
- Schedule management

### Phase 3: Route Simulator
- Simulator UI with input form
- Route results display
- Map visualization

## Open Items
- Pricing table structure (to be investigated separately)
- Exact CSV/Excel format for postal code import
- Map provider choice (Leaflet/Mapbox/Google Maps)
