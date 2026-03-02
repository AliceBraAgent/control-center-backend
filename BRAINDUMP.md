# Routing V3

For our customers operating as groupage carriers we need to support a routing engine that will allow them to define a set of rules that will be used to determine the best route for a given shipment. A shipment can be a single shipment or a group of shipments. A shipment has a origin and a destination. A shipment has a list of packages. A package has a weight and a volume.

## Route Definition

A route is defined by a list of legs through the system. a leg has a origin and a destination. For each leg we need to calculate the cost of the leg. basically there are three types of legs:

1. Collection leg: from the origin of the shipment to the first hub
2. Linehaul leg: from one hub to another hub.
3. Delivery leg: from the last hub to the destination of the shipment

A route usually has 1 Collection Leg, 0 or more Linehaul Legs and 1 Delivery Leg. In some cases there is might be no collection (when origin is a hub) and no delivery (when destination is a hub).

## Cost Calculation

For each leg we need to calculate the cost of the leg. The cost is calculated based on the weight and volume of the packages.

## Leg Types

### Collection Leg
A collection leg is a leg that is used to collect packages from a customer. It has a origin and a destination. The origin is the customer location and the destination is the first hub.

### Linehaul Leg
A linehaul leg is a leg that is used to transport packages from one hub to another hub. It has a origin and a destination. The origin is the first hub and the destination is the last hub.

### Delivery Leg
A delivery leg is a leg that is used to deliver packages to a customer. It has a origin and a destination. The origin is the last hub and the destination is the customer location.

## Shipment

A shipment is a group of packages that are transported together. A shipment has a origin and a destination. A shipment has a list of packages. A package has a weight and a volume.

- Origin (Country, Postal Code, City, Optional: AdressMatchCode)
- Destination (Country, Postal Code, City, Optional: AdressMatchCode)
- List of Items (Amount, Type, Weight, Volume, Dimensions)

## Line

A Line is a scheduled connection between two hubs (Linehaul) or a hub and postal code area (Collection/Delivery)

- Origin (Hub or Postal Code Area)
- Destination (Hub or Postal Code Area)
- Base-Schedule (Days of week, times)
- Cost (Weight, Volume, Dimensions)

Postal Code Areas should be defined by Country + Postcode regex (e.g. DK 2xxx).

A Line might be valid for multiple mandates.

A Line might have multiple costs depending on the weight, volume and dimensions of the packages.

### Schedule

A Schedule defines the executions of a Line. The Line itself has a Base-Schedule that defines the days of the week and times when the line is available. The Schedule then defines the actual executions of the line (incl. specific dates, times, etc.).

## Attributes

Attributes are used to define special needs for a shipment or a leg. For example a shipment might need a tail lift at the destination. This can be defined as an attribute on the shipment. A leg might need to be driven by a driver with a specific license. This can be defined as an attribute on the leg.

Examples:
- Collection with Taillift: Collection Leg must have Taillift
- Delivery with Taillift: Delivery Leg must have Taillift
- ADR: All legs need to support ADR
- Temperature Controlled: All legs need to support temperature controlled transport
- Tires: All legs need to support tires

## Mandates

A Mandate is a branch of our customer. Shipments belong to a mandate. Legs are assigned to a mandate. A Leg might be valid for multiple mandates but not all.

- E.G. "SLW" is a mandate that has special network for tires and is not mixed with other mandates.

## Routing Engine

The routing engine is used to calculate the best route for a given shipment. The routing engine is a rule-based engine that uses the attributes of the shipment and the legs to calculate the best route. The routing engine is a greedy algorithm that tries to find the best route for the shipment.

## Example Lines:

### NV AT.LNZ
- Mandate: AT-LNZ
- Department: Rollfuhr
- AT-4* -> AT-LNZ:
  - Origin: AT-4* (Collection)
  - Destination: AT-LNZ (Hub)
  - Attributes: ADR
- AT-LNZ -> AT-4*:
  - Origin: AT-LNZ (Hub)
  - Destination: AT-4* (Delivery)
  - Attributes: ADR
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: ILV-Pricing

### NV AT.WND
- Mandate: AT-WND
- Department: Rollfuhr
- AT-1* -> AT-WND:
  - Origin: AT-1* (Collection)
  - Destination: AT-WND (Hub)
  - Attributes: ADR
  - Monday-Friday: Collection 08:00-17:00 / Arrival 18:00
- AT-WND -> AT-1*:
  - Origin: AT-WND (Hub)
  - Destination: AT-1* (Delivery)
  - Attributes: ADR
  - Monday-Friday: Leaving 06:00 / Arrival 08:00-17:00
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: ILV-Pricing

### AT.LNZ-AT.WND
- Mandate: AT-LNZ
- Department: EAS
- AT-LNZ -> AT-WND:
  - Origin: AT-LNZ (Hub)
  - Destination: AT-WND (Hub)
  - Attributes: ADR
  - Monday-Friday: Departure 18:00 / Arrival 04:00+1
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: ILV-Pricing

### AT.WND-AT.LNZ
- Mandate: AT-WND
- Department: EAS
- AT-WND -> AT-LNZ:
  - Origin: AT-WND (Hub)
  - Destination: AT-LNZ (Hub)
  - Attributes: ADR
  - Monday-Friday: Departure 18:00 / Arrival 04:00+1
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: ILV-Pricing

### AT.LNZ-DE.AUR
- Mandate: AT-LNZ
- Department: EAS
- AT-LNZ -> DE.AUR:
  - Origin: AT-LNZ (Hub)
  - Destination: DE.AUR (Hub)
  - Attributes: ADR
  - Monday-Friday: Departure 18:00 / Arrival 01:00+1
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: AT.LNZ-DE.AUR Linehaul Pricing
- Partner: CTL

### DE.AUR-DE*
- Mandate: AT-LNZ
- Department: EAS
- DE.AUR -> DE-4*:
  - Origin: DE.AUR (Hub)
  - Destination: DE-4* (Delivery)
  - Via: DE-41379 (Sub-Hub)
  - Attributes: ADR
  - Monday-Friday: Departure 01:00 / Arrival 08:00-17:00
- DE.AUR -> DE-1*:
  - Origin: DE.AUR (Hub)
  - Destination: DE-1* (Delivery)
  - Via: DE-10115 (Sub-Hub)
  - Attributes: ADR
  - Monday-Friday: Departure 01:00 / Arrival 08:00-17:00
- Valid for Mandate: AT-LNZ, AT-WND, AT-SEI, AT-GRZ, AT-KLG, AT-IBK
- Pricing: DE.AUR-DE* Linehaul Pricing (Calculated from Sub-Hub to Destination)
- Partner: CTL
- There is a list with all european postal code areas and their sub-hubs.

# Multiple Options

- A shipment can have multiple routing options
- A routing option is a list of legs that are assigned to the shipment
- A routing option has a cost that is calculated based on the legs
- A routing option has a priority that is used to determine the best route

# Frontend

- The Frontend should allow to manage Lines, Schedules, Attributes, Mandates, Departments, Pricing, Sub-Hubs, Postal Code Areas, etc.
- The Frontend should allow to simulate a route for a given shipment and show the cost of the route.
- The Frontend should show a route on a map.

# API

- The API should allow to get Routing Options (Legs, Cost, Priority, ETA, etc.) for a given shipment
