MEDICAL APPOINTMENT SYSTEM

System parts:
1. Medical.PythonAPI - Python FastAPI API
2. Medical.WebApp - Laravel Web App source files
3. Medical.ConsoleApp - .NET Console App
4. database.sql - MySQL database and tables

REQUIRED INSTALLATIONS
- XAMPP with MySQL
- Python 3.11+
- PHP 8.2+
- Composer
- Laravel installer or Composer create-project
- .NET 8 SDK

DATABASE SETUP
1. Open XAMPP Control Panel.
2. Start Apache and MySQL.
3. Open http://localhost/phpmyadmin.
4. Click SQL.
5. Copy and run the contents of database.sql.

FASTAPI SETUP
Open PowerShell:
cd C:\Users\HP\MedicalAppointmentSystem\Medical.PythonAPI
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
uvicorn main:app --reload --port 8001

Open FastAPI Swagger:
http://localhost:8001/docs

Create admin account in Swagger:
POST /api/Auth/create-admin
Body:
{
  "fullName": "System Administrator",
  "username": "admin",
  "password": "admin123"
}

LARAVEL WEB APP SETUP
This ZIP includes the Laravel source files you need to place into a Laravel project.
If Medical.WebApp is not a full Laravel installation yet, run:
cd C:\Users\HP\MedicalAppointmentSystem
composer create-project laravel/laravel Medical.WebApp_Full

Then copy these folders/files from this ZIP's Medical.WebApp into Medical.WebApp_Full:
- app/Http/Controllers
- routes/web.php
- resources/views
- .env.example

Rename .env.example to .env or copy API_BASE_URL into your existing .env:
API_BASE_URL=http://localhost:8001/api

Then run:
cd C:\Users\HP\MedicalAppointmentSystem\Medical.WebApp_Full
composer install
php artisan key:generate
php artisan config:clear
php artisan cache:clear
php artisan route:clear
php artisan serve --port=8000

Open:
http://127.0.0.1:8000

CONSOLE APP SETUP
Open PowerShell:
cd C:\Users\HP\MedicalAppointmentSystem\Medical.ConsoleApp
dotnet restore
dotnet run

RUN ORDER
1. Start XAMPP MySQL.
2. Run Python FastAPI on port 8001.
3. Run Laravel Web App on port 8000.
4. Run .NET Console App.

LOGIN
Admin:
Username: admin
Password: admin123

Patient:
Register from Laravel Web App or Console App.

NOTES
- API base URL is http://localhost:8001/api.
- MySQL user is root with no password.
- FastAPI creates tables automatically, but database.sql is included for a clean manual setup.
- Laravel vendor folder is not included to keep the ZIP small. Run composer install/create-project as instructed.
