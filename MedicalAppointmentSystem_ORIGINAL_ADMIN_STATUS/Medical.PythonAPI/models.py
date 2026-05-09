from sqlalchemy import Column, Integer, String, Date, ForeignKey, DateTime
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from database import Base

class User(Base):
    __tablename__ = "users"
    id = Column(Integer, primary_key=True, index=True)
    fullName = Column(String(100), nullable=False)
    username = Column(String(100), unique=True, nullable=False)
    passwordHash = Column(String(255), nullable=False)
    role = Column(String(20), nullable=False, default="User")
    createdAt = Column(DateTime, server_default=func.now())
    appointments = relationship("Appointment", back_populates="user")

class Doctor(Base):
    __tablename__ = "doctors"
    id = Column(Integer, primary_key=True, index=True)
    fullName = Column(String(100), nullable=False)
    specialization = Column(String(100), nullable=False)
    contactNumber = Column(String(50), nullable=False)
    createdAt = Column(DateTime, server_default=func.now())
    schedules = relationship("DoctorSchedule", back_populates="doctor")
    appointments = relationship("Appointment", back_populates="doctor")

class DoctorSchedule(Base):
    __tablename__ = "doctorschedules"
    id = Column(Integer, primary_key=True, index=True)
    doctorId = Column(Integer, ForeignKey("doctors.id"), nullable=False)
    scheduleDate = Column(Date, nullable=False)
    startTime = Column(String(50), nullable=False)
    endTime = Column(String(50), nullable=False)
    isAvailable = Column(Integer, nullable=False, default=1)
    createdAt = Column(DateTime, server_default=func.now())
    doctor = relationship("Doctor", back_populates="schedules")

class Appointment(Base):
    __tablename__ = "appointments"
    id = Column(Integer, primary_key=True, index=True)
    userId = Column(Integer, ForeignKey("users.id"), nullable=False)
    doctorId = Column(Integer, ForeignKey("doctors.id"), nullable=False)
    appointmentDate = Column(Date, nullable=False)
    reason = Column(String(255), nullable=False)
    status = Column(String(50), nullable=False, default="Pending")
    createdAt = Column(DateTime, server_default=func.now())
    user = relationship("User", back_populates="appointments")
    doctor = relationship("Doctor", back_populates="appointments")

class AuditLog(Base):
    __tablename__ = "auditlogs"
    id = Column(Integer, primary_key=True, index=True)
    userId = Column(Integer, nullable=True)
    action = Column(String(100), nullable=False)
    details = Column(String(255), nullable=False)
    createdAt = Column(DateTime, server_default=func.now())
