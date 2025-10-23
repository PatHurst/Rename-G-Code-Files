Select DISTINCT
    Patterns.PrimaryProgram As [Program],
    Patterns.ID,
    Materials.ID,
    Materials.Name
From
    (Patterns Right Join
    Cuts On Patterns.ID = Cuts.PatternID) Inner Join
    Materials On Cuts.MaterialID = Materials.ID
Where
    Patterns.PrimaryProgram <> ''
Order By
    Materials.ID, Patterns.ID