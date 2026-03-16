# RSQL Filtering Guide

## Overview

RSQL (Resource Search Query Language) filtering in the AEC Data Model API enables precise querying of element data across APS/ACC projects. This guide provides comprehensive documentation for constructing effective filters when using `GetElementsWithFilter` and other query operations.

**Key uses:**
- Filter elements by category, type, or properties
- Query by numeric ranges (area, volume, length)
- Combine multiple conditions for complex scenarios
- Validate element properties for QA/QC workflows
- Support data exchange operations with filtered element sets

**Important:** All filters must include the Element Context requirement: `'property.name.Element Context'==Instance`

---

## RSQL Syntax Fundamentals

### Property path structure

Properties are accessed using the dot notation: `property.name.X` where X is the property name.

```
property.name.category         // Built-in category property
property.name.Length           // Built-in numeric property
'property.name.Element Name'   // Property with spaces in name
'property.name.Element Context'// Element Context (always required)
```

### Quoting rules (critical for correctness)

**Property names:**
- With spaces: MUST be quoted with single quotes
  - `'property.name.Element Context'` ✓
  - `property.name.Element Context` ✗
- Without spaces: NO quotes needed
  - `property.name.category` ✓
  - `'property.name.category'` ✗ (unnecessary)

**Values:**
- Multi-word values: MUST be quoted
  - `'Pipe Fittings'` ✓
  - `'Store Front Double Door'` ✓
  - `Pipe Fittings` ✗
- Single-word values: NO quotes needed
  - `Instance` ✓
  - `'Instance'` ✗ (unnecessary)
- Numeric/boolean values: NEVER quote
  - `100` ✓
  - `'100'` ✗

### Operator spacing

RSQL operators must have NO spaces around them:

```
property.name.category=contains=doors     // Correct
property.name.category = contains = doors // Wrong
'property.name.Element Context'==Instance // Correct (no spaces)
'property.name.Element Context' == Instance // Wrong
```

### Data type handling

- **Strings:** Case-insensitive by default (use `=caseSensitive=` if needed)
- **Numbers:** Floats require decimal digits (10.0, not 10 for float fields)
- **Booleans:** Use lowercase `true` or `false`
- **DateTime:** ISO 8601 format (YYYY-MM-DDTHH:mm:ss)

---

## Complete Operator Reference

### String operators

**Equality (case-insensitive):**
```
property.name.category==doors
property.name.Material=='Cast Iron'
```

**Case-sensitive equality:**
```
property.name.description=caseSensitive='HVAC Zone A'
```

**Not equal:**
```
property.name.category!=windows
```

**Contains (substring match):**
```
property.name.category=contains=pipes        // Matches "Pipes", "Pipe Fittings", "Pipe Curves"
property.name.description=contains='HVAC'   // Case-insensitive substring
```

**Starts with:**
```
property.name.description=startsWith='Fire Rating'
property.name.ElementName=startsWith=EXT
```

**Ends with:**
```
property.name.description=endsWith='mm'
property.name.Type=endsWith='-Steel'
```

**In list (multiple values):**
```
property.name.category=in=('doors','windows','walls')
property.name.Type=in=('Structural','Non-bearing')
```

### Numeric operators

**Basic comparisons:**
```
property.name.Length>100           // Greater than
property.name.Area<500             // Less than
property.name.Volume>=1000         // Greater than or equal
property.name.Load<=50.5           // Less than or equal
property.name.Height==10.5         // Equal to
property.name.Quantity!=0          // Not equal
```

**Range queries:**
```
property.name.Area>100 and property.name.Area<500    // Between 100 and 500
property.name.Cost>=10000 and property.name.Cost<=50000
property.name.Length<10             // Open-ended ranges
property.name.Depth>0.5
```

### Boolean operators

```
property.name.IsExternal==true
property.name.FireRated!=false
```

### DateTime operators

Requires ISO 8601 format (YYYY-MM-DDTHH:mm:ss):

```
property.name.CreatedDate>2024-01-01T00:00:00
property.name.InstalledDate<2024-12-31T23:59:59
property.name.ReviewDate==2024-06-15T14:30:00
```

### Null/empty value handling

To filter for elements with missing or null properties:

```
property.name.AssetTag==''           // Empty string values
not(property.name.Manufacturer)      // Null values (property doesn't exist)
```

---

## Compound operators

### AND (all conditions must be true)

```
property.name.category=contains=doors and 'property.name.Element Context'==Instance
property.name.category=contains=pipes and property.name.Material=='Copper'
property.name.Area>100 and property.name.Area<500 and property.name.category=contains=walls
```

### OR (any condition can be true)

```
property.name.category=contains=windows or property.name.category=contains=doors
property.name.Material=='Steel' or property.name.Material=='Concrete'
```

### Grouping with parentheses

```
(property.name.category=contains=pipes or property.name.category=contains=ducts) and property.name.Material=='Copper'
property.name.category=contains=walls and (property.name.FireRating=='1HR' or property.name.FireRating=='2HR')
```

### NOT (negation)

```
not(property.name.category=contains=doors)
not(property.name.Material=='Wood')
'property.name.Element Context'!=Instance  // Also valid negation
```

### Complex conditions

```
(property.name.category=contains=pipes or property.name.category=contains=ducts) and (property.name.Material=='Copper' or property.name.Material=='PVC') and property.name.Length>100
```

---

## Real-world filtering scenarios

### Architecture: Doors and windows by location and type

**Use case:** Find all exterior doors on ground level (Level 1).

```
property.name.category=contains=doors and 'property.name.Element Context'==Instance and property.name.Level=='Level 1' and property.name.IsExterior==true
```

**Use case:** Find double-hung windows with specific dimensions.

```
property.name.category=contains=windows and 'property.name.Element Context'==Instance and property.name.Type=='Double Hung' and property.name.Width>=3.0 and property.name.Height>=4.0
```

**Use case:** Doors with fire ratings for code compliance check.

```
property.name.category=contains=doors and 'property.name.Element Context'==Instance and (property.name.FireRating=='1HR' or property.name.FireRating=='2HR') and property.name.Material=='Steel'
```

**Use case:** Count window openings by frame type and location.

```
property.name.category=contains=windows and 'property.name.Element Context'==Instance and property.name.Level=in=('Level 1','Level 2','Level 3') and property.name.FrameType=='Aluminum'
```

### Architecture: Walls by construction and material

**Use case:** Exterior walls with specific acoustic ratings.

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.IsExterior==true and property.name.AcousticRating>50
```

**Use case:** Interior partition walls less than 4 inches thick.

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.IsExterior==false and property.name.Thickness<0.33
```

**Use case:** Walls by construction type for phasing analysis.

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and (property.name.Type=contains='Drywall' or property.name.Type=contains='Masonry') and property.name.ConstructionPhase=='Phase 1'
```

### MEP: Pipes by system, size, and material

**Use case:** Find all cold water pipes larger than 2 inches.

```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.System=='Cold Water' and property.name.Diameter>2.0
```

**Use case:** Hot water piping with insulation requirements.

```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.System=='Hot Water' and property.name.Insulation==true and property.name.InsulationThickness>=1.0
```

**Use case:** Drain/waste/vent pipes by material type.

```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.System=contains=DWV and (property.name.Material=='PVC' or property.name.Material=='Cast Iron')
```

**Use case:** High-temperature steam pipes with specific pressure ratings.

```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.System=='Steam' and property.name.PressureRating>=150 and property.name.Temperature>=250
```

### MEP: HVAC equipment and systems

**Use case:** HVAC units by zone and capacity.

```
property.name.category=contains='Mechanical Equipment' and 'property.name.Element Context'==Instance and property.name.EquipmentType=='Air Handler' and property.name.Capacity>10000 and property.name.Zone=contains='Zone'
```

**Use case:** Thermostats and controls by floor.

```
property.name.category=contains='Generic Model' and 'property.name.Element Context'==Instance and property.name.Type=='Thermostat' and property.name.Level=in=('Level 1','Level 2','Level 3','Level 4')
```

**Use case:** Equipment requiring maintenance scheduling.

```
property.name.category=contains='Mechanical Equipment' and 'property.name.Element Context'==Instance and property.name.MaintenanceRequired==true and property.name.LastServiceDate<2023-12-31T00:00:00
```

### MEP: Electrical fixtures and circuits

**Use case:** Lighting fixtures by circuit load.

```
property.name.category=contains=Light and 'property.name.Element Context'==Instance and property.name.Load<20 and property.name.Circuit=startsWith='LT'
```

**Use case:** Emergency/exit lighting requiring backup power.

```
property.name.category=contains=Light and 'property.name.Element Context'==Instance and (property.name.Type='Exit Light' or property.name.Type='Emergency') and property.name.BackupPowerRequired==true
```

### Structural: Columns by load and material

**Use case:** Steel columns with specific strength requirements.

```
property.name.category=contains=columns and 'property.name.Element Context'==Instance and property.name.Material=='Steel' and property.name.LoadCapacity>=500 and property.name.Section=contains='W'
```

**Use case:** Reinforced concrete columns by size range.

```
property.name.category=contains=columns and 'property.name.Element Context'==Instance and property.name.Material=='Concrete' and property.name.Width>=18 and property.name.Depth>=18 and property.name.ReinforcementType=contains='Rebar'
```

### Structural: Foundations with embedment specifications

**Use case:** Spread footings with minimum embedment depth.

```
property.name.category=contains='Structural Foundation' and 'property.name.Element Context'==Instance and property.name.Type=='Spread Footing' and property.name.EmbedmentDepth>=4.0
```

**Use case:** Pile foundations by capacity and depth.

```
property.name.category=contains='Structural Foundation' and 'property.name.Element Context'==Instance and property.name.Type=='Pile' and property.name.Capacity>=1000 and property.name.Length>50
```

### QA/QC: Validation and completeness checks

**Use case:** Find elements missing required fire rating properties.

```
property.name.category=contains=doors and 'property.name.Element Context'==Instance and not(property.name.FireRating)
```

**Use case:** Elements with empty or missing assembly descriptions.

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and (property.name.Assembly=='' or not(property.name.Assembly))
```

**Use case:** Identify cost estimates not yet assigned.

```
'property.name.Element Context'==Instance and property.name.EstimatedCost==0 and not(property.name.CostCode)
```

**Use case:** Find elements assigned to wrong phase/scope.

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.ConstructionPhase=='Phase 2' and property.name.ScheduledStartDate<2024-03-01T00:00:00
```

### Quantity takeoff: Material calculations

**Use case:** Total wall area for finish estimation (walls between 100 and 1000 sq ft).

```
property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.Area>100 and property.name.Area<1000
```

**Use case:** Multi-material takeoff for cost estimation.

```
(property.name.category=contains=walls or property.name.category=contains=floors or property.name.category=contains=ceilings) and 'property.name.Element Context'==Instance and (property.name.Material==Concrete or property.name.Material=='Steel Studs')
```

**Use case:** Pipe material summary for procurement.

```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and (property.name.Material=='Copper' or property.name.Material=='PVC' or property.name.Material=='Steel') and property.name.Length>0
```

---

## Performance and best practices

### Filter construction order

For optimal query performance, structure filters in this order:

1. **Category first:** Narrows down the broadest dataset
2. **Element Context requirement:** Always include early
3. **Type/family criteria:** Further narrows scope
4. **Property ranges:** Apply after narrowing
5. **Additional properties:** Applied last to already-filtered set

**Good order:**
```
property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.Material=='Copper' and property.name.Diameter>1.0
```

**Less efficient order:**
```
property.name.Diameter>1.0 and property.name.Material=='Copper' and property.name.category=contains=pipes and 'property.name.Element Context'==Instance
```

### Query optimization tips

- **Use specific categories** instead of broad searches when possible
- **Combine conditions** with AND to filter early rather than post-processing results
- **Limit numeric ranges** to only the necessary bounds
- **Test filters** with element group counts before building complex expressions
- **Avoid negative queries** (NOT conditions) on large datasets when possible

### Common pitfalls

- **Forgetting Element Context:** Every filter must include `'property.name.Element Context'==Instance`
- **Case sensitivity mismatches:** Remember string matching is case-insensitive by default
- **Missing quotes on multi-word values:** `'Pipe Fittings'` not `Pipe Fittings`
- **Spaces around operators:** Use `==` not `== ` or ` ==`
- **Numeric precision:** Float fields need decimal points (10.0 not 10)

---

## Troubleshooting

### Invalid filter returned no results

**Possible causes:**
- Missing or incorrect Element Context condition
- Typo in category name (check against available categories in your model)
- Quote mismatch (property name not quoted when it has spaces)
- Case sensitivity issue with multi-word category values

**Solution:** Simplify the filter step-by-step. Start with just:
```
'property.name.Element Context'==Instance and property.name.category=contains=doors
```

Then add conditions one at a time to identify which condition is causing issues.

### Unexpected results

**Possible causes:**
- Category contains match is too broad (e.g., `pipes` matches "Pipes", "Pipe Fittings", "Pipe Curves")
- Operator precedence confusion with AND/OR
- Property not available in this element group (check property definitions)

**Solution:** Use `GetPropertyDefinitionsByElementGroup` to verify available properties before filtering.

### Filter syntax error

**Possible causes:**
- Spaces around operators
- Missing quotes on property names with spaces
- Incorrect quoting on values
- Invalid operator for data type

**Solution:** Review syntax rules above. Common errors:
- `property.name.category == doors` → Remove spaces: `property.name.category==doors`
- `property.name.Element Name=='Wall 1'` → Quote property: `'property.name.Element Name'=='Wall 1'`
- `property.name.Material='Steel'` → Use ==: `property.name.Material=='Steel'`

### Special characters in values

To match values containing quotes or special characters, escape with backslash:

```
property.name.Description=='Value with \"quotes\"'
property.name.Type=='Type\\With\\Backslash'
```

---

## Summary table: Operators by data type

| Data type | Operators | Example |
|-----------|-----------|---------|
| String | ==, =caseSensitive=, !=, =contains=, =startsWith=, =endsWith=, =in= | `property.name.Material=='Steel'` |
| Numeric | ==, !=, <, >, <=, >= | `property.name.Area>100` |
| Boolean | ==, != | `property.name.IsExternal==true` |
| DateTime | ==, !=, <, >, <=, >= | `property.name.Date>2024-01-01T00:00:00` |
| Compound | and, or, not() | `(cond1 or cond2) and cond3` |
