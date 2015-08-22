# LD33
Mattias Fagerlunds Ludum Dare 33 Entry

Commit early, commit often!

## On Flow Field navigation
For this LD I'm again trying Flow Field navigation to facilitate many agents heading for the same target areas.

The Flow Fields are designed as follows;

## Cells
Cells are numbered [0,0]-[Size.x,Size.y]. Each cell is *centered* on the coordinate, so cell [0,0] goes from [-gridSize/2,-gridSize/2]  to [gridSize/2,gridSize/2]. Cell [x,y] [x-gridSize/2,y-gridSize/2]  to [x+gridSize/2,y+gridSize/2].



## NavigationField
Contains a field called costs, which is computed using Physics2D and area intersection. If anything from the "Buildings" layer is found to be colliding with a cell in the field, the cell is set to a PositiveInfinity cost.

This field should be identical for all 
* agent types 
* factions
* destinations

## PotentialField
Contains a potential field for a given target area. A higher potential means a better location, a lower potential means a worse location when it comes to pathing.

This field will be used to compute "Flows" going from low potential to higher potential.

If a cell is blocked, the potential will be NegativeInfinity. This information is collected from the NavigationField.

For easy navigation, go towards the lowest potential neighbour you can find!

The field is first populated with the target positions and their potential (typically 100 or some such).

This field will be different for each target area, because the 

## FlowField
The FlowField is part of the PotentialField as it's just the direction of potentials.