# Medical Appointment System - Windows Forms Desktop App

This is a Windows Forms desktop client that connects to the same FastAPI API used by your Laravel web app.

Architecture:

```text
Windows Forms App -> FastAPI API -> MySQL Database
Laravel Web App   -> FastAPI API -> MySQL Database
```

The WinForms app does not connect directly to MySQL. Login, registration, doctors, schedules, appointments, reports, and password hashing are handled by the API.

## Requirements

- Windows 10/11
- Visual Studio 2022
- .NET 8 Desktop Development workload
- Your FastAPI backend running on port 8001
- XAMPP MySQL with your `medical_appointment_system.sql` imported

## Run the API first

Open PowerShell in your `Medical.PythonAPI` folder:

```powershell
pip install -r requirements.txt
python -m uvicorn main:app --reload --port 8001
```

Check Swagger:

```text
http://127.0.0.1:8001/docs
```

## Run the WinForms app

1. Open `MedicalAppointment.WinForms.sln` in Visual Studio.
2. Restore/build the project.
3. Press F5.
4. Keep the API URL as `http://127.0.0.1:8001` on the login screen.

## Included Forms

- LoginForm
- RegisterForm
- UserDashboardForm
- DoctorsForm
- CreateAppointmentForm
- MyAppointmentsForm
- EditAppointmentForm
- AdminDashboardForm
- ManageDoctorsForm
- ManageSchedulesForm
- ManageAppointmentsForm
- ReportsForm

## API endpoints used

- POST `/api/Auth/login`
- POST `/api/Auth/register`
- GET `/api/Doctors`
- POST `/api/Doctors`
- PUT `/api/Doctors/{id}`
- DELETE `/api/Doctors/{id}`
- GET `/api/DoctorSchedules`
- POST `/api/DoctorSchedules`
- PUT `/api/DoctorSchedules/{id}`
- DELETE `/api/DoctorSchedules/{id}`
- GET `/api/Appointments`
- GET `/api/Appointments/user/{user_id}`
- POST `/api/Appointments`
- PUT `/api/Appointments/update/{id}`
- PUT `/api/Appointments/cancel/{id}`
- PUT `/api/Admin/Appointments/{id}/status/{status}`
- GET `/api/Reports/summary`

## Notes

- User login opens the Patient dashboard.
- Admin login opens the Admin dashboard.
- User appointments can only be edited/cancelled when Pending.
- Reports include a custom doughnut chart drawn in WinForms without extra packages.
- The design is clean blue/white medical style using standard WinForms controls so it opens without requiring Guna UI2. You can later replace controls with Guna UI2 if you want.

## Layout Update
This version uses a fullscreen responsive layout for smaller laptop screens, including 12.5-inch displays.

Design updates included:
- Maximized startup forms
- Fixed 230px sidebar with full main content area
- Responsive dashboard cards using TableLayoutPanel
- Modernized DataGridView styling
- Better spacing and non-overlapping dashboard sections
- Updated Login/Register centered cards
- Reports page with fullscreen doughnut chart layout

Run order:
1. Start XAMPP MySQL.
2. Run your FastAPI API on port 8001.
3. Open the solution in Visual Studio and run the WinForms app.
