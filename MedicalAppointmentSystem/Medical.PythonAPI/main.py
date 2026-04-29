from datetime import datetime, timedelta
from fastapi import FastAPI, Depends, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import OAuth2PasswordBearer
from fastapi.responses import JSONResponse
from fastapi.exceptions import RequestValidationError
from jose import JWTError, jwt
from sqlalchemy.orm import Session, joinedload
from passlib.hash import bcrypt

from database import SessionLocal, engine, Base
import models
import schemas

Base.metadata.create_all(bind=engine)

app = FastAPI(title="Medical Appointment System API", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

SECRET_KEY = "change-this-secret-key-in-production"
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 120

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/api/Auth/login")


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def create_access_token(data: dict):
    payload = data.copy()
    expire = datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    payload.update({"exp": expire})
    return jwt.encode(payload, SECRET_KEY, algorithm=ALGORITHM)


def get_current_user(token: str = Depends(oauth2_scheme), db: Session = Depends(get_db)):
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        user_id = payload.get("id")
        if user_id is None:
            raise HTTPException(status_code=401, detail="Invalid token.")
        user = db.query(models.User).filter(models.User.id == user_id).first()
        if user is None:
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


def add_audit_log(db: Session, user_id: int | None, action: str, details: str):
    log = models.AuditLog(userId=user_id, action=action, details=details)
    db.add(log)
    db.commit()


@app.exception_handler(RequestValidationError)
async def validation_exception_handler(request, exc):
    return JSONResponse(status_code=422, content={"message": "Validation error.", "errors": exc.errors()})


@app.get("/")
def home():
    return {"message": "Medical Appointment System API is running"}


@app.post("/api/Auth/register", response_model=schemas.UserResponse)
def register(dto: schemas.RegisterDto, db: Session = Depends(get_db)):
    existing_user = db.query(models.User).filter(models.User.username == dto.username).first()
    if existing_user:
        raise HTTPException(status_code=400, detail="Username already exists.")
    user = models.User(fullName=dto.fullName, username=dto.username, passwordHash=bcrypt.hash(dto.password), role="User")
    db.add(user)
    db.commit()
    db.refresh(user)
    add_audit_log(db, user.id, "REGISTER", f"User {user.username} registered.")
    return user


@app.post("/api/Auth/create-admin", response_model=schemas.UserResponse)
def create_admin(dto: schemas.RegisterDto, db: Session = Depends(get_db)):
    existing_user = db.query(models.User).filter(models.User.username == dto.username).first()
    if existing_user:
        raise HTTPException(status_code=400, detail="Username already exists.")
    admin = models.User(fullName=dto.fullName, username=dto.username, passwordHash=bcrypt.hash(dto.password), role="Admin")
    db.add(admin)
    db.commit()
    db.refresh(admin)
    add_audit_log(db, admin.id, "CREATE_ADMIN", f"Admin {admin.username} created.")
    return admin


@app.post("/api/Auth/login", response_model=schemas.TokenResponse)
def login(dto: schemas.LoginDto, db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.username == dto.username).first()
    if not user or not bcrypt.verify(dto.password, user.passwordHash):
        raise HTTPException(status_code=401, detail="Invalid username or password.")
    token = create_access_token({"id": user.id, "username": user.username, "role": user.role})
    return {"access_token": token, "token_type": "bearer", "id": user.id, "fullName": user.fullName, "username": user.username, "role": user.role}


@app.get("/api/Doctors", response_model=list[schemas.DoctorResponse])
def get_doctors(search: str | None = None, db: Session = Depends(get_db)):
    query = db.query(models.Doctor)
    if search:
        pattern = f"%{search}%"
        query = query.filter((models.Doctor.fullName.like(pattern)) | (models.Doctor.specialization.like(pattern)))
    return query.all()


@app.get("/api/Doctors/{doctor_id}", response_model=schemas.DoctorResponse)
def get_doctor(doctor_id: int, db: Session = Depends(get_db)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    return doctor


@app.post("/api/Doctors", response_model=schemas.DoctorResponse)
def create_doctor(dto: schemas.DoctorCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = models.Doctor(fullName=dto.fullName, specialization=dto.specialization, contactNumber=dto.contactNumber)
    db.add(doctor)
    db.commit()
    db.refresh(doctor)
    add_audit_log(db, admin.id, "CREATE_DOCTOR", f"Doctor ID {doctor.id} created.")
    return doctor


@app.put("/api/Doctors/{doctor_id}")
def update_doctor(doctor_id: int, dto: schemas.DoctorCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    doctor.fullName = dto.fullName
    doctor.specialization = dto.specialization
    doctor.contactNumber = dto.contactNumber
    db.commit()
    add_audit_log(db, admin.id, "UPDATE_DOCTOR", f"Doctor ID {doctor.id} updated.")
    return {"message": "Doctor updated successfully."}


@app.delete("/api/Doctors/{doctor_id}")
def delete_doctor(doctor_id: int, db: Session = Depends(get_db), admin=Depends(admin_required)):
    doctor = db.query(models.Doctor).filter(models.Doctor.id == doctor_id).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    db.delete(doctor)
    db.commit()
    add_audit_log(db, admin.id, "DELETE_DOCTOR", f"Doctor ID {doctor_id} deleted.")
    return {"message": "Doctor deleted successfully."}


@app.get("/api/DoctorSchedules", response_model=list[schemas.ScheduleResponse])
def get_schedules(doctorId: int | None = None, scheduleDate: str | None = None, db: Session = Depends(get_db)):
    query = db.query(models.DoctorSchedule).options(joinedload(models.DoctorSchedule.doctor))
    if doctorId:
        query = query.filter(models.DoctorSchedule.doctorId == doctorId)
    if scheduleDate:
        query = query.filter(models.DoctorSchedule.scheduleDate == scheduleDate)
    return query.all()


@app.get("/api/DoctorSchedules/{schedule_id}", response_model=schemas.ScheduleResponse)
def get_schedule(schedule_id: int, db: Session = Depends(get_db)):
    schedule = db.query(models.DoctorSchedule).options(joinedload(models.DoctorSchedule.doctor)).filter(models.DoctorSchedule.id == schedule_id).first()
    if not schedule:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    return schedule


@app.post("/api/DoctorSchedules", response_model=schemas.ScheduleResponse)
def create_schedule(dto: schemas.ScheduleCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    if dto.startTime24 >= dto.endTime24:
        raise HTTPException(status_code=400, detail="Start time must be earlier than end time.")
    doctor = db.query(models.Doctor).filter(models.Doctor.id == dto.doctorId).first()
    if not doctor:
        raise HTTPException(status_code=404, detail="Doctor not found.")
    overlap = db.query(models.DoctorSchedule).filter(
        models.DoctorSchedule.doctorId == dto.doctorId,
        models.DoctorSchedule.scheduleDate == dto.scheduleDate,
        models.DoctorSchedule.startTime24 < dto.endTime24,
        models.DoctorSchedule.endTime24 > dto.startTime24,
    ).first()
    if overlap:
        raise HTTPException(status_code=400, detail="Schedule overlaps with an existing schedule.")
    schedule = models.DoctorSchedule(doctorId=dto.doctorId, scheduleDate=dto.scheduleDate, startTime=dto.startTime, endTime=dto.endTime, startTime24=dto.startTime24, endTime24=dto.endTime24, isAvailable=1 if dto.isAvailable else 0)
    db.add(schedule)
    db.commit()
    db.refresh(schedule)
    add_audit_log(db, admin.id, "CREATE_SCHEDULE", f"Schedule ID {schedule.id} created.")
    return schedule


@app.put("/api/DoctorSchedules/{schedule_id}")
def update_schedule(schedule_id: int, dto: schemas.ScheduleCreate, db: Session = Depends(get_db), admin=Depends(admin_required)):
    schedule = db.query(models.DoctorSchedule).filter(models.DoctorSchedule.id == schedule_id).first()
    if not schedule:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    if dto.startTime24 >= dto.endTime24:
        raise HTTPException(status_code=400, detail="Start time must be earlier than end time.")
    overlap = db.query(models.DoctorSchedule).filter(
        models.DoctorSchedule.id != schedule_id,
        models.DoctorSchedule.doctorId == dto.doctorId,
        models.DoctorSchedule.scheduleDate == dto.scheduleDate,
        models.DoctorSchedule.startTime24 < dto.endTime24,
        models.DoctorSchedule.endTime24 > dto.startTime24,
    ).first()
    if overlap:
        raise HTTPException(status_code=400, detail="Schedule overlaps with an existing schedule.")
    schedule.doctorId = dto.doctorId
    schedule.scheduleDate = dto.scheduleDate
    schedule.startTime = dto.startTime
    schedule.endTime = dto.endTime
    schedule.startTime24 = dto.startTime24
    schedule.endTime24 = dto.endTime24
    schedule.isAvailable = 1 if dto.isAvailable else 0
    db.commit()
    add_audit_log(db, admin.id, "UPDATE_SCHEDULE", f"Schedule ID {schedule.id} updated.")
    return {"message": "Schedule updated successfully."}


@app.delete("/api/DoctorSchedules/{schedule_id}")
def delete_schedule(schedule_id: int, db: Session = Depends(get_db), admin=Depends(admin_required)):
    schedule = db.query(models.DoctorSchedule).filter(models.DoctorSchedule.id == schedule_id).first()
    if not schedule:
        raise HTTPException(status_code=404, detail="Schedule not found.")
    db.delete(schedule)
    db.commit()
    add_audit_log(db, admin.id, "DELETE_SCHEDULE", f"Schedule ID {schedule_id} deleted.")
    return {"message": "Schedule deleted successfully."}


@app.get("/api/Appointments", response_model=list[schemas.AppointmentResponse])
def get_appointments(status: str | None = None, appointmentDate: str | None = None, db: Session = Depends(get_db), admin=Depends(admin_required)):
    query = db.query(models.Appointment).options(joinedload(models.Appointment.user), joinedload(models.Appointment.doctor))
    if status:
        query = query.filter(models.Appointment.status == status)
    if appointmentDate:
        query = query.filter(models.Appointment.appointmentDate == appointmentDate)
    return query.all()


@app.get("/api/Appointments/{appointment_id}", response_model=schemas.AppointmentResponse)
def get_appointment(appointment_id: int, db: Session = Depends(get_db), current_user=Depends(get_current_user)):
    appointment = db.query(models.Appointment).options(joinedload(models.Appointment.user), joinedload(models.Appointment.doctor)).filter(models.Appointment.id == appointment_id).first()
    if not appointment:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    if current_user.role != "Admin" and appointment.userId != current_user.id:
        raise HTTPException(status_code=403, detail="Access denied.")
    return appointment


@app.get("/api/Appointments/user/{user_id}", response_model=list[schemas.AppointmentResponse])
def get_user_appointments(user_id: int, db: Session = Depends(get_db), current_user=Depends(get_current_user)):
    if current_user.role != "Admin" and current_user.id != user_id:
        raise HTTPException(status_code=403, detail="Access denied.")
    return db.query(models.Appointment).options(joinedload(models.Appointment.doctor)).filter(models.Appointment.userId == user_id).all()


@app.post("/api/Appointments", response_model=schemas.AppointmentResponse)
def create_appointment(dto: schemas.AppointmentCreate, db: Session = Depends(get_db), current_user=Depends(user_required)):
    if dto.userId != current_user.id:
        raise HTTPException(status_code=403, detail="You can only create your own appointment.")
    schedule = db.query(models.DoctorSchedule).filter(
        models.DoctorSchedule.doctorId == dto.doctorId,
        models.DoctorSchedule.scheduleDate == dto.appointmentDate,
        models.DoctorSchedule.startTime24 <= dto.appointmentTime,
        models.DoctorSchedule.endTime24 > dto.appointmentTime,
        models.DoctorSchedule.isAvailable == 1,
    ).first()
    if not schedule:
        raise HTTPException(status_code=400, detail="Doctor is not available at this date and time.")
    existing = db.query(models.Appointment).filter(
        models.Appointment.doctorId == dto.doctorId,
        models.Appointment.appointmentDate == dto.appointmentDate,
        models.Appointment.appointmentTime == dto.appointmentTime,
        models.Appointment.status != "Cancelled",
    ).first()
    if existing:
        raise HTTPException(status_code=400, detail="This doctor is already booked at this time.")
    appointment = models.Appointment(userId=current_user.id, doctorId=dto.doctorId, appointmentDate=dto.appointmentDate, appointmentTime=dto.appointmentTime, reason=dto.reason, status="Pending")
    db.add(appointment)
    db.commit()
    db.refresh(appointment)
    add_audit_log(db, current_user.id, "CREATE_APPOINTMENT", f"Appointment ID {appointment.id} created.")
    return appointment


@app.put("/api/Appointments/update/{appointment_id}")
def update_my_appointment(appointment_id: int, dto: schemas.AppointmentUpdate, db: Session = Depends(get_db), current_user=Depends(user_required)):
    appointment = db.query(models.Appointment).filter(models.Appointment.id == appointment_id, models.Appointment.userId == current_user.id).first()
    if not appointment:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    existing = db.query(models.Appointment).filter(
        models.Appointment.id != appointment_id,
        models.Appointment.doctorId == dto.doctorId,
        models.Appointment.appointmentDate == dto.appointmentDate,
        models.Appointment.appointmentTime == dto.appointmentTime,
        models.Appointment.status != "Cancelled",
    ).first()
    if existing:
        raise HTTPException(status_code=400, detail="Doctor already booked at this time.")
    appointment.doctorId = dto.doctorId
    appointment.appointmentDate = dto.appointmentDate
    appointment.appointmentTime = dto.appointmentTime
    appointment.reason = dto.reason
    db.commit()
    add_audit_log(db, current_user.id, "UPDATE_APPOINTMENT", f"Appointment ID {appointment.id} updated.")
    return {"message": "Appointment updated successfully."}


@app.put("/api/Appointments/cancel/{appointment_id}")
def cancel_appointment(appointment_id: int, db: Session = Depends(get_db), current_user=Depends(user_required)):
    appointment = db.query(models.Appointment).filter(models.Appointment.id == appointment_id, models.Appointment.userId == current_user.id).first()
    if not appointment:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    appointment.status = "Cancelled"
    db.commit()
    add_audit_log(db, current_user.id, "CANCEL_APPOINTMENT", f"Appointment ID {appointment.id} cancelled.")
    return {"message": "Appointment cancelled successfully."}


@app.delete("/api/Appointments/{appointment_id}")
def delete_appointment(appointment_id: int, db: Session = Depends(get_db), admin=Depends(admin_required)):
    appointment = db.query(models.Appointment).filter(models.Appointment.id == appointment_id).first()
    if not appointment:
        raise HTTPException(status_code=404, detail="Appointment not found.")
    db.delete(appointment)
    db.commit()
    add_audit_log(db, admin.id, "DELETE_APPOINTMENT", f"Appointment ID {appointment_id} deleted.")
    return {"message": "Appointment deleted successfully."}
