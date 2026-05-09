from datetime import datetime, timedelta
from fastapi import FastAPI, Depends, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import OAuth2PasswordBearer
from jose import JWTError, jwt
from passlib.hash import bcrypt
from sqlalchemy.orm import Session, joinedload

from database import SessionLocal, engine, Base
import models
import schemas

Base.metadata.create_all(bind=engine)

app = FastAPI(title="Medical Appointment System API", version="1.0.0")
app.add_middleware(CORSMiddleware, allow_origins=["*"], allow_credentials=True, allow_methods=["*"], allow_headers=["*"])

SECRET_KEY = "change-this-medical-secret-key"
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 240
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/api/Auth/login")

def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

def create_access_token(data: dict):
    payload = data.copy()
    payload.update({"exp": datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)})
    return jwt.encode(payload, SECRET_KEY, algorithm=ALGORITHM)

def get_current_user(token: str = Depends(oauth2_scheme), db: Session = Depends(get_db)):
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        user_id = payload.get("id")
        if not user_id:
            raise HTTPException(status_code=401, detail="Invalid token.")
        user = db.query(models.User).filter(models.User.id == user_id).first()
        if not user:
            raise HTTPException(status_code=401, detail="User not found.")
        return user
    except JWTError:
        raise HTTPException(status_code=401, detail="Invalid token.")

def admin_required(current_user=Depends(get_current_user)):
    if current_user.role != "Admin":
        raise HTTPException(status_code=403, detail="Admin access required.")
    return current_user

def user_required(current_user=Depends(get_current_user)):
    if current_user.role != "User":
        raise HTTPException(status_code=403, detail="User access required.")
    return current_user

def add_audit(db: Session, user_id: int | None, action: str, details: str):
    db.add(models.AuditLog(userId=user_id, action=action, details=details))
    db.commit()

@app.get("/")
def home():
    return {"message": "Medical Appointment System API is running"}

@app.post("/api/Auth/register", response_model=schemas.UserResponse)
def register(dto: schemas.RegisterDto, db: Session = Depends(get_db)):
    if db.query(models.User).filter(models.User.username == dto.username).first():
        raise HTTPException(status_code=400, detail="Username already exists.")
    user = models.User(fullName=dto.fullName, username=dto.username, passwordHash=bcrypt.hash(dto.password), role="User")
    db.add(user); db.commit(); db.refresh(user)
    add_audit(db, user.id, "REGISTER", f"User {user.username} registered.")
    return user

@app.post("/api/Auth/create-admin", response_model=schemas.UserResponse)
def create_admin(dto: schemas.RegisterDto, db: Session = Depends(get_db)):
    if db.query(models.User).filter(models.User.username == dto.username).first():
        raise HTTPException(status_code=400, detail="Username already exists.")
    admin = models.User(fullName=dto.fullName, username=dto.username, passwordHash=bcrypt.hash(dto.password), role="Admin")
    db.add(admin); db.commit(); db.refresh(admin)
    add_audit(db, admin.id, "CREATE_ADMIN", f"Admin {admin.username} created.")
    return admin

@app.post("/api/Auth/login", response_model=schemas.TokenResponse)
def login(dto: schemas.LoginDto, db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.username == dto.username).first()
    if not user or not bcrypt.verify(dto.password, user.passwordHash):
        raise HTTPException(status_code=401, detail="Invalid username or password.")
    token = create_access_token({"id": user.id, "username": user.username, "role": user.role})
    return {"access_token": token, "token_type": "bearer", "id": user.id, "fullName": user.fullName, "username": user.username, "role": user.role}

@app.get("/api/Doctors", response_model=list[schemas.DoctorResponse])
def get_doctors(search: str = "", db: Session = Depends(get_db)):
    q = db.query(models.Doctor)
    if search:
        q = q.filter((models.Doctor.fullName.like(f"%{search}%")) | (models.Doctor.specialization.like(f"%{search}%")))
    return q.order_by(models.Doctor.id.desc()).all()

@app.get("/api/Doctors/{doctor_id}", response_model=schemas.DoctorResponse)
def get_doctor(doctor_id: int, db: Session = Depends(get_db)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    return doctor

@app.post("/api/Doctors", response_model=schemas.DoctorResponse)
def create_doctor(dto: schemas.DoctorCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = models.Doctor(fullName=dto.fullName, specialization=dto.specialization, contactNumber=dto.contactNumber)
    db.add(doctor); db.commit(); db.refresh(doctor)
    add_audit(db, admin.id, "CREATE_DOCTOR", f"Doctor {doctor.fullName} created.")
    return doctor

@app.put("/api/Doctors/{doctor_id}")
def update_doctor(doctor_id: int, dto: schemas.DoctorCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    doctor.fullName = dto.fullName; doctor.specialization = dto.specialization; doctor.contactNumber = dto.contactNumber
    db.commit(); add_audit(db, admin.id, "UPDATE_DOCTOR", f"Doctor ID {doctor_id} updated.")
    return {"message": "Doctor updated successfully."}

@app.delete("/api/Doctors/{doctor_id}")
def delete_doctor(doctor_id: int, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    db.delete(doctor); db.commit(); add_audit(db, admin.id, "DELETE_DOCTOR", f"Doctor ID {doctor_id} deleted.")
    return {"message": "Doctor deleted successfully."}

@app.get("/api/DoctorSchedules", response_model=list[schemas.ScheduleResponse])
def get_schedules(db: Session = Depends(get_db)):
    return db.query(models.DoctorSchedule).options(joinedload(models.DoctorSchedule.doctor)).order_by(models.DoctorSchedule.id.desc()).all()

@app.get("/api/DoctorSchedules/{schedule_id}", response_model=schemas.ScheduleResponse)
def get_schedule(schedule_id: int, db: Session = Depends(get_db)):
    sched = db.query(models.DoctorSchedule).options(joinedload(models.DoctorSchedule.doctor)).filter(models.DoctorSchedule.id == schedule_id).first()
    if not sched:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    return sched

@app.post("/api/DoctorSchedules", response_model=schemas.ScheduleResponse)
def create_schedule(dto: schemas.ScheduleCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    if not db.query(models.Doctor).filter(models.Doctor.id == dto.doctorId).first():
        raise HTTPException(status_code=404, detail="Doctor not found.")
    sched = models.DoctorSchedule(doctorId=dto.doctorId, scheduleDate=dto.scheduleDate, startTime=dto.startTime, endTime=dto.endTime, isAvailable=1 if dto.isAvailable else 0)
    db.add(sched); db.commit(); db.refresh(sched); add_audit(db, admin.id, "CREATE_SCHEDULE", f"Schedule ID {sched.id} created.")
    return sched

@app.put("/api/DoctorSchedules/{schedule_id}")
def update_schedule(schedule_id: int, dto: schemas.ScheduleCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    sched = db.query(models.DoctorSchedule).filter(models.DoctorSchedule.id == schedule_id).first()
    if not sched:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    sched.doctorId=dto.doctorId; sched.scheduleDate=dto.scheduleDate; sched.startTime=dto.startTime; sched.endTime=dto.endTime; sched.isAvailable=1 if dto.isAvailable else 0
    db.commit(); add_audit(db, admin.id, "UPDATE_SCHEDULE", f"Schedule ID {schedule_id} updated.")
    return {"message": "Schedule updated successfully."}

@app.delete("/api/DoctorSchedules/{schedule_id}")
def delete_schedule(schedule_id: int, db: Session = Depends(get_db), admin=Depends(admin_required)):
    sched = db.query(models.DoctorSchedule).filter(models.DoctorSchedule.id == schedule_id).first()
    if not sched:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    db.delete(sched); db.commit(); add_audit(db, admin.id, "DELETE_SCHEDULE", f"Schedule ID {schedule_id} deleted.")
    return {"message": "Schedule deleted successfully."}

@app.get("/api/Appointments", response_model=list[schemas.AppointmentResponse])
def get_appointments(status: str = "", db: Session = Depends(get_db), admin=Depends(admin_required)):
    q = db.query(models.Appointment).options(joinedload(models.Appointment.user), joinedload(models.Appointment.doctor))
    if status:
        q = q.filter(models.Appointment.status == status)
    return q.order_by(models.Appointment.id.desc()).all()

@app.get("/api/Appointments/user/{user_id}", response_model=list[schemas.AppointmentResponse])
def get_user_appointments(user_id: int, db: Session = Depends(get_db), current_user=Depends(get_current_user)):
    if current_user.role == "User" and current_user.id != user_id:
        raise HTTPException(status_code=403, detail="You can only view your own appointments.")
    return db.query(models.Appointment).options(joinedload(models.Appointment.doctor)).filter(models.Appointment.userId == user_id).order_by(models.Appointment.id.desc()).all()

@app.post("/api/Appointments", response_model=schemas.AppointmentResponse)
def create_appointment(dto: schemas.AppointmentCreate, db: Session = Depends(get_db), current_user=Depends(user_required)):
    if dto.userId != current_user.id:
        raise HTTPException(status_code=403, detail="You can only create your own appointment.")
    if not db.query(models.Doctor).filter(models.Doctor.id == dto.doctorId).first():
        raise HTTPException(status_code=404, detail="Doctor not found.")
    existing = db.query(models.Appointment).filter(models.Appointment.doctorId == dto.doctorId, models.Appointment.appointmentDate == dto.appointmentDate, models.Appointment.status.in_(["Pending", "Approved"])).first()
    if existing:
        raise HTTPException(status_code=400, detail="Doctor already has an appointment on this date.")
    appt = models.Appointment(userId=current_user.id, doctorId=dto.doctorId, appointmentDate=dto.appointmentDate, reason=dto.reason, status="Pending")
    db.add(appt); db.commit(); db.refresh(appt); add_audit(db, current_user.id, "CREATE_APPOINTMENT", f"Appointment ID {appt.id} created.")
    return appt

@app.put("/api/Appointments/update/{appointment_id}")
def update_my_appointment(appointment_id: int, dto: schemas.AppointmentUpdate, db: Session = Depends(get_db), current_user=Depends(user_required)):
    appt = db.query(models.Appointment).filter(models.Appointment.id == appointment_id, models.Appointment.userId == current_user.id).first()
    if not appt:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    if appt.status not in ["Pending"]:
        raise HTTPException(status_code=400, detail="Only pending appointments can be updated.")
    appt.doctorId = dto.doctorId; appt.appointmentDate = dto.appointmentDate; appt.reason = dto.reason
    db.commit(); add_audit(db, current_user.id, "UPDATE_APPOINTMENT", f"Appointment ID {appointment_id} updated.")
    return {"message": "Appointment updated successfully."}

@app.put("/api/Appointments/cancel/{appointment_id}")
def user_cancel_appointment(appointment_id: int, db: Session = Depends(get_db), current_user=Depends(user_required)):
    appt = db.query(models.Appointment).filter(models.Appointment.id == appointment_id, models.Appointment.userId == current_user.id).first()
    if not appt:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    appt.status = "Cancelled"; db.commit(); add_audit(db, current_user.id, "USER_CANCEL_APPOINTMENT", f"Appointment ID {appointment_id} cancelled by user.")
    return {"message": "Appointment cancelled successfully."}

@app.put("/api/Admin/Appointments/{appointment_id}/status/{status}")
def admin_update_appointment_status(appointment_id: int, status: str, db: Session = Depends(get_db), admin=Depends(admin_required)):
    allowed = ["Pending", "Approved", "Rejected", "Completed", "Cancelled"]
    if status not in allowed:
        raise HTTPException(status_code=400, detail="Invalid status.")
    appt = db.query(models.Appointment).filter(models.Appointment.id == appointment_id).first()
    if not appt:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    appt.status = status; db.commit(); add_audit(db, admin.id, "ADMIN_UPDATE_STATUS", f"Appointment ID {appointment_id} set to {status}.")
    return {"message": f"Appointment status updated to {status}."}

@app.get("/api/Reports/summary")
def reports_summary(db: Session = Depends(get_db), admin=Depends(admin_required)):
    return {
        "users": db.query(models.User).count(),
        "doctors": db.query(models.Doctor).count(),
        "schedules": db.query(models.DoctorSchedule).count(),
        "appointments": db.query(models.Appointment).count(),
        "pending": db.query(models.Appointment).filter(models.Appointment.status == "Pending").count(),
        "approved": db.query(models.Appointment).filter(models.Appointment.status == "Approved").count(),
        "cancelled": db.query(models.Appointment).filter(models.Appointment.status == "Cancelled").count(),
    }

@app.get("/api/AuditLogs", response_model=list[schemas.AuditLogResponse])
def audit_logs(db: Session = Depends(get_db), admin=Depends(admin_required)):
    return db.query(models.AuditLog).order_by(models.AuditLog.id.desc()).limit(100).all()
