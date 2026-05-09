MEDICAL APPOINTMENT SYSTEM - ORIGINAL CLEAN VERSION WITH ADMIN ACCEPT/CANCEL

Includes:
- Medical.PythonAPI: FastAPI backend with JWT, doctor CRUD, schedule CRUD, appointment booking, and admin appointment status control.
- Medical.WebApp: Laravel web app connected to FastAPI only.
- Medical.ConsoleApp: .NET 8 console app connected to FastAPI only.
- database/medical_appointment_system.sql: MySQL database setup.

Required:
1. XAMPP MySQL
2. Python 3.11+
3. PHP 8.2+
4. Composer
5. .NET 8 SDK

Recommended extraction path:
C:\Projects\MedicalSystem

DATABASE SETUP:
1. Start XAMPP MySQL.
2. Open http://localhost/phpmyadmin
3. Import database/medical_appointment_system.sql

RUN API:
cd C:\Projects\MedicalSystem\Medical.PythonAPI
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
uvicorn main:app --reload --port 8001
Open http://127.0.0.1:8001/docs

CREATE ADMIN:
POST /api/Auth/create-admin
{
  "fullName": "System Administrator",
  "username": "admin",
  "password": "admin123"
}

RUN LARAVEL:
cd C:\Projects\MedicalSystem\Medical.WebApp
copy .env.example .env
composer install
php artisan key:generate
php artisan config:clear
php artisan serve --port=8000
Open http://127.0.0.1:8000

RUN CONSOLE:
cd C:\Projects\MedicalSystem\Medical.ConsoleApp
dotnet run

RUN ORDER:
1. XAMPP MySQL
2. FastAPI
3. Laravel
4. Console App

ADMIN APPOINTMENT STATUS:
Admin Dashboard -> Appointments
Admin can click Accept, Reject, Cancel, or Complete.

Do not edit:
vendor/
node_modules/
bin/
obj/
