# CompositeAreaManager
RimWorld Mod that allows composite Areas via a MapComponent CompositeAreaManager

- Adds a button to existing Manage Areas dialog for Composite area management
- Adds an additional option to all list with the "Manage areas ..." option
- Has a simple, row based entry for Composite areas
- Can build arbitrary trees of operations, using Union, Intersect, Invert, and Areas to construct a composite
- Composite Areas will rebuild themselves periodically (every 1000 ticks) and after adjusting them in the dialog
- Allows deleting and removing individual composite operations or entire composite area
- Basic cyclical reference checks
