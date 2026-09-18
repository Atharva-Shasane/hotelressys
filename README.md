# Hotel Reservation System

## How to Edit the UI
All the frontend code (HTML, CSS, and JavaScript) is located in one single file: `wwwroot/index.html`. You can edit this file to completely change the look and feel without needing to touch or understand the backend C# code!

## How to Run the Project Locally
1. Install MySQL and create a local database.
2. Update a file named `appsettings.json` in the root folder and add your connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=hoteldb;User=root;Password=your_mysql_password;"
     }
   }

## Commands for installation
dotnet tool install --global dotnet-ef
then
dotnet ef database update
