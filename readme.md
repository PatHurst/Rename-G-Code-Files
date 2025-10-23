
# Rename G Code Files Utility  

AUTHOR: PATRICK HURST  

Rename G Code Files is a simple Utility I built for moving and renaming G Code Files in CABINET VISION.  
It is compatible with 2023 (Access) and 2024 (SQL local).  

At this point it does the following:  

Determines the running version of S2M center or CV  

Determines the Job File Path and Job Name  

Moves all the G code files to a folder within the CVJ file's folder renaming them to include their material name  

Moves the .snc or .snx file to the CVJ's folder  


## Todo

- [ ] Rewrite in a more functional style  
- [ ] Return Dataset from Database Read Method  
- [ ] Rewrite using more advanced C# features
- [ ] Implement better logging; log various levels: Info, Warning, Error